using System.Reflection;
using System.Text;
using Avalonia.Platform.Storage;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Styling;
using Avalonia.Threading;
using Redes.Knowledge.Domain;
using Redes.Knowledge.Infrastructure;
using Redes.Knowledge.Infrastructure.Capturas;
using Redes.Knowledge.Visualization;

namespace Redes.Knowledge.App;

/// <summary>
/// Ventana principal:
/// - Barra superior: búsqueda global, filtros Familia/Estado, tema, comparador.
/// - Sidebar: grupos de familias (dropdowns de navegación) con ANCHURA IDÉNTICA (fija),
///   altura uniforme (acordeón) y filtro rápido para localizarlos.
/// - Zoom global del tamaño de letra con Ctrl+Scroll.
/// </summary>
public partial class MainWindow : Window
{
    private const double ZoomMin = 0.7, ZoomMax = 3.0;
    private const double ListaAltura = 240;
    private const double ListaAnchura = 306; // idéntica para todos los grupos (340 - márgenes - scrollbar)

    // Mínimo horizontal de la ventana según los diagramas visibles: la redimensión por
    // borde no puede dejar un diagrama más estrecho que su ancho natural. Componentes
    // fijos de la UI desde el borde izquierdo hasta el diagrama: grip de borde (6) +
    // sidebar (360) + márgenes de la ficha (28) + scrollbar vertical (~17) + grip (6).
    private const double AnchoFijoRedimension = 417;
    private const double MinAnchoVentana = 960;   // piso absoluto (también en el axaml)
    private const double MaxAnchoVentana = 1400;  // tope: no romper pantallas normales a zoom alto

    private readonly Dictionary<string, Protocol> _protocolos = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, Protocol> _normalizados = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, IReadOnlyList<Field>> _camposPorAcronimo = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, string> _pduPorAcronimo = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, string> _cifradoPorAcronimo = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, CatalogJson.NotaFuente> _notasFuentes = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, FichaPrioritaria> _fichas = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, string> _familias = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<Relationship> _relaciones = new();
    private readonly SqliteProtocolRepository _repo;
    private readonly SqliteServiceRepository _servicios;
    private readonly SqliteSearchEngine _busqueda;
    private bool _cargando;
    private Protocol? _seleccionado;
    private double _zoom = 1.0;

    // Base de fuente EFECTIVA del contenido (capturada al abrir la ventana): el zoom se
    // aplica como multiplicador real (reflow), así a 100 % el aspecto es el del tema.
    // Valores provisionales razonables: Opened los sobrescribe con los del tema aplicado.
    private double _fuenteBase = 13;
    private double _tituloBase = 14;

    // Evita recursión al sincronizar el selector de protocolo con RenderFicha.
    private bool _sincronizandoSelector;

    // Última posición del puntero (coordenadas de la ventana) para el cierre por geometría
    // de los popups de información (sin parpadeo).
    private Point _ultimaPosPuntero = new(-1, -1);

    /// <summary>Diagramas de la ficha actual (cache para la exportación D4-3).</summary>
    private List<(string Titulo, DiagramDocument Doc, IReadOnlyList<NodoGrafo>? Nodos, IReadOnlyDictionary<string, string?>? Abrir)> _docsActuales = new();

    /// <summary>Paquetes de la captura abierta (vista D6).</summary>
    private List<PcapPacket> _paquetesCaptura = new();
    private string _rutaCaptura = "";

    /// <summary>Deduplicación fina IANA (D2-2): sinónimos agrupados + vínculos al catálogo.</summary>
    private ResultadoDedup _dedup = new(new Dictionary<string, string>(), Array.Empty<ServicioCanonico>(), 0, 0);

    public MainWindow()
    {
        InitializeComponent();

        // Logo de la aplicación (embebido): icono de ventana/taskbar.
        try
        {
            using var stream = AssetLoader.Open(
                new Uri("avares://NetProtocol/Assets/Logo_NetProtocol.png"));
            Icon = new WindowIcon(new Bitmap(stream));
        }
        catch
        {
            Icon = null; // si no se pudiera decodificar, se usa el del ejecutable
        }

        var raiz = RaizDatos();
        // Base de datos local en un directorio con permiso de escritura (%LOCALAPPDATA%):
        // junto al ejecutable (Program Files) NO se puede escribir como usuario normal.
        var localApp = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var dirDb = string.IsNullOrEmpty(localApp)
            ? Path.Combine(raiz, "FASE-II-DISENO", "run")
            : Path.Combine(localApp, "NetProtocol");
        Directory.CreateDirectory(dirDb);
        var store = new SqliteKnowledgeStore($"Data Source={Path.Combine(dirDb, "knowledge.db")};Pooling=False");

        _repo = new SqliteProtocolRepository(store);
        _servicios = new SqliteServiceRepository(store);
        _busqueda = new SqliteSearchEngine(store);

        var importados = DatasetBootstrap.EnsureProtocolos(store,
            Path.Combine(raiz, "FASE-03-INVENTARIO", "F3-Protocolos.json"));
        // D2-2: deduplicación fina IANA en memoria (sinónimos agrupados + vínculos al catálogo).
        _dedup = ServiciosDedup.Agrupar(_servicios.Todos());
        foreach (var p in _repo.GetAll()) _protocolos[p.Id.Value] = p;
        foreach (var p in _protocolos.Values)
        {
            _normalizados[Normalizar(p.Acronimo)] = p;
            _normalizados[Normalizar(p.Nombre)] = p;
        }
        foreach (var kv in CatalogJson.CargarNotasFuenteF3(
                     Path.Combine(raiz, "FASE-03-INVENTARIO", "F3-Protocolos.json")))
            _notasFuentes[kv.Key] = kv.Value;
        foreach (var kv in CatalogJson.CargarFamiliasF3(
                     Path.Combine(raiz, "FASE-03-INVENTARIO", "F3-Protocolos.json")))
            _familias[kv.Key] = kv.Value;

        // Catálogos de la Fase I para las vistas avanzadas (D5): campos PDU (F5) para
// cualquier protocolo del catálogo que los tenga catalogados (no solo los 6 iniciales).
        var f5 = Path.Combine(raiz, "FASE-05-MENSAJERIA", "F5-Campos-PDU.json");
        foreach (var p in _protocolos.Values)
        {
            var campos = CatalogJson.CargarCamposF5(f5, p.Acronimo);
            if (campos.Count > 0) _camposPorAcronimo[p.Acronimo] = campos;
            var pdu = CatalogoExploracion.ObtenerPduF5(f5, p.Acronimo);
            if (pdu is not null) _pduPorAcronimo[p.Acronimo] = pdu;
        }
        _cifradoPorAcronimo = CatalogoExploracion.CargarSeguridadF6(
            Path.Combine(raiz, "FASE-06-SEGURIDAD", "F6-Seguridad-Protocolos.json"), "cifrado")
            .ToDictionary(e => e.Key, e => e.Value, StringComparer.OrdinalIgnoreCase);
        _relaciones.AddRange(CatalogoExploracion.CargarRelacionesF4(
            Path.Combine(raiz, "FASE-04-PROFUNDIZACION", "F4-Matriz-Encapsulacion.json")));
        foreach (var kv in CatalogoExploracion.CargarFichasF4(
                     Path.Combine(raiz, "FASE-04-PROFUNDIZACION", "F4-Fichas-Prioritarias.json")))
            _fichas[kv.Key] = kv.Value;

        CargarFiltros();
        CargarComparador();
        CargarSelectorProtocolo();
        ReconstruirNavegacion();
        // Ficha inicial: primer protocolo con relaciones catalogadas (TCP) para mostrar
        // el grafo F4 al abrir; si no existe, el primero del catálogo.
        var inicial = _protocolos.Values
            .FirstOrDefault(p => p.Acronimo == "TCP")
            ?? _protocolos.Values
                .OrderByDescending(p => GrafoRelaciones.Vecinos1Salto(p.Acronimo, _relaciones).Count)
                .ThenBy(p => p.Acronimo, StringComparer.OrdinalIgnoreCase)
                .FirstOrDefault();
        RenderFicha(inicial);

        StatusText.Text = $"Dataset: {_protocolos.Count} protocolos · {_servicios.Contar()} servicios IANA · " +
                          $"{_relaciones.Count} relaciones F4 · importados: {importados} · zoom {_zoom * 100:0}% (Ctrl+Scroll)";

        SearchButton.Click += (_, _) => EjecutarBusqueda();
        SearchBox.KeyDown += (_, e) => { if (e.Key == Key.Enter) EjecutarBusqueda(); };
        ThemeButton.Click += (_, _) => AlternarTema();
        CompareButton.Click += (_, _) => CompararConReferencia();
        VolverFichaButton.Click += (_, _) => VolverALaFicha();
        LegendButton.Click += (_, _) => MostrarLeyenda();
        AboutButton.Click += (_, _) => MostrarAcercaDe();
        FilterFamilia.SelectionChanged += (_, _) => { if (!_cargando) ReconstruirNavegacion(); };
        FilterEstado.SelectionChanged += (_, _) => { if (!_cargando) ReconstruirNavegacion(); };
        NavFilter.TextChanged += (_, _) => { if (!_cargando) ReconstruirNavegacion(); };
        // Selector de protocolo: seleccionar en el desplegable abre su ficha (navegación
        // directa), y la marca de "protocolo en pantalla" se sincroniza sola en RenderFicha.
        ProtocolSelector.SelectionChanged += (_, _) =>
        {
            if (_cargando || _sincronizandoSelector) return;
            if (ProtocolSelector.SelectedItem is Protocol p) RenderFicha(p);
        };
        ExportFormat.ItemsSource = new[] { "SVG", "PNG", "PDF" };
        ExportFormat.SelectedIndex = 0;
        ExportButton.Click += async (_, _) => await ExportarDiagramasAsync();
        AbrirCapturaButton.Click += async (_, _) => await AbrirCapturaAsync();
        MuestraButton.Click += (_, _) => GenerarMuestra();
        // Tooltip de "Muestra de prueba": el contenido (frase + carpeta REAL de capturas) y
        // Popups de información de los botones de acción (Comparar, Exportar, Abrir captura,
        // Muestra de prueba), en sustitución de los tooltips nativos: estilo cristalino y
        // permanencia por geometría (ver ConfigurarPopupsBotones).
        ConfigurarPopupsBotones();
        CerrarCapturaButton.Click += (_, _) => CerrarCaptura();
        ListaPaquetes.SelectionChanged += (_, _) =>
        {
            if (ListaPaquetes.SelectedItem is ListBoxItem { Tag: int idx } &&
                idx >= 0 && idx < _paquetesCaptura.Count)
                DetalleCaptura.Text = DetalleDe(_paquetesCaptura[idx]);
        };

        // Zoom del contenido por REFLOW (diseño UX adoptado): se escala el FontSize real
        // de la ficha/paneles y el FactorZoom de los diagramas, en lugar de un
        // RenderTransform global. El texto re-envuelve contra el ancho del panel (solo
        // crece en altura) → la barra vertical del ScrollViewer basta y nunca hay texto
        // inaccesible a la derecha. La shell (barras y sidebar) queda fija a 100 %.
        AddHandler(InputElement.PointerWheelChangedEvent,
            (_, e) =>
            {
                if ((e.KeyModifiers & KeyModifiers.Control) == 0) return;
                _zoom = Math.Clamp(_zoom + (e.Delta.Y > 0 ? 0.1 : -0.1), ZoomMin, ZoomMax);
                AplicarZoomContenido();
                StatusText.Text = $"Zoom del contenido: {_zoom * 100:0}% — Ctrl+Scroll ajusta el tamaño de letra (reflow, sin desbordes)";
                e.Handled = true;
            },
            RoutingStrategies.Bubble,
            handledEventsToo: true);

        // Captura la base de fuente EFECTIVA del contenido una vez la ventana está en el
        // árbol (el tema la ha aplicado): a zoom 100 % el aspecto no cambia respecto al
        // montaje anterior. Hasta que se dispare, AplicarZoomContenido no hace nada.
        Opened += (_, _) =>
        {
            _fuenteBase = double.IsNaN(DetailText.FontSize) || DetailText.FontSize <= 0 ? 13 : DetailText.FontSize;
            var tituloBase = double.IsNaN(DiagramTitle.FontSize) || DiagramTitle.FontSize <= 0 ? _fuenteBase : DiagramTitle.FontSize;
            _tituloBase = Math.Max(_fuenteBase, tituloBase);
            AplicarZoomContenido();
            if (IsLoaded) StatusText.Text = $"Dataset: {_protocolos.Count} protocolos · {_servicios.Contar()} servicios IANA · zoom del contenido {_zoom * 100:0}% (Ctrl+Scroll)";
        };

        // Última posición del puntero (coordenadas de la ventana), compartida por todos
        // los popups de información para decidir el cierre por geometría (sin parpadeo).
        AddHandler(InputElement.PointerMovedEvent,
            (_, e) => _ultimaPosPuntero = e.GetPosition(this),
            RoutingStrategies.Bubble,
            handledEventsToo: true);
    }

    /// <summary>Crea el popup de información "cristalino" con el que se sustituyen los
    /// tooltips de los botones de acción (el ToolTip nativo se cierra al salir del botón y
    /// no permite enlaces). Estilo profesional: acrílico oscuro translúcido (blur real de
    /// Avalonia), borde claro, esquinas redondeadas y ancho limitado al de la ventana actual
    /// (el texto largo se envuelve en multilínea y nunca se escapa por el borde).
    /// Permanencia SIN PARPADEO: el cierre se decide por GEOMETRÍA (posición del puntero
    /// contra los rects del botón y del popup), no por PointerEntered/Exited (que al abrir
    /// el overlay disparan eventos espurios y provocan ciclos de abrir/cerrar). La posición
    /// la mantiene el PointerMoved global (_ultimaPosPuntero).
    /// Si se pasa <paramref name="alPulsar"/>, se engancha al contenido y cierra el popup.</summary>
    private void CrearPopupInfo(Control anfitrion, Control contenido, Action? alPulsar = null)
    {
        // Cristal acrílico: el ExperimentalAcrylicBorder aporta el fondo translúcido con blur;
        // un Border exterior añade borde, esquinas, padding y el límite de ancho.
        var borde = new Border
        {
            // Transparente para recibir punteros en toda su área (el acrílico lo pinta).
            Background = Brushes.Transparent,
            BorderBrush = RecursoPopupBrush("tBordePopup", "#55FFFFFF"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(1),
            MaxWidth = Math.Max(320, Bounds.Width - 48),
            Child = new ExperimentalAcrylicBorder
            {
                Material = new ExperimentalAcrylicMaterial
                {
                    BackgroundSource = AcrylicBackgroundSource.Digger,
                    TintColor = RecursoPopupColor("tFondoPopup", "#22252A"),
                    TintOpacity = 0.85,
                    MaterialOpacity = 0.75
                },
                CornerRadius = new CornerRadius(7),
                Padding = new Thickness(10, 7),
                Child = contenido
            }
        };

        var popup = new Popup
        {
            PlacementTarget = anfitrion,
            Placement = PlacementMode.Bottom,
            VerticalOffset = 4,
            // SIN light-dismiss: con él, el primer clic sobre el botón lo consume el propio
            // popup y el botón no recibe la pulsación. El cierre lo gestionamos nosotros
            // (geometría del botón + flags de entrada/salida del popup).
            IsLightDismissEnabled = false,
            Child = borde
        };
        // El Popup debe estar en el árbol visual de la ventana para poder abrirse.
        if (Content is Panel raiz && !raiz.Children.Contains(popup))
            raiz.Children.Add(popup);

        // Permanencia SIN PARPADEO: el estado "puntero dentro" combina
        // (a) el rect del BOTÓN en coordenadas de la ventana (misma capa, fiable) y
        // (b) los flags sobrePopup del propio popup (sus eventos de puntero sí llegan,
        // aunque viva en otra capa visual; un TranslatePoint entre capas NO es fiable).
        // Al salir del botón o del popup se programa un cierre diferido que solo se
        // ejecuta si la posición real no está sobre ninguno de los dos.
        var sobreBoton = false;
        var sobrePopup = false;

        bool PunteroDentro()
        {
            var vacio = new Rect(0, 0, 0, 0);
            var botonRect = anfitrion.TranslatePoint(new Point(0, 0), this) is { } pb
                ? new Rect(pb, anfitrion.Bounds.Size)
                : vacio;
            return sobrePopup || botonRect.Contains(_ultimaPosPuntero);
        }

        void ProgramarCierre()
        {
            var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(400) };
            timer.Tick += (_, _) =>
            {
                timer.Stop();
                if (!PunteroDentro()) popup.IsOpen = false;
            };
            timer.Start();
        }

        anfitrion.PointerEntered += (_, _) => { sobreBoton = true; popup.IsOpen = true; };
        anfitrion.PointerExited += (_, _) => { sobreBoton = false; ProgramarCierre(); };
        // Al pulsar el botón se cierra el popup sin consumir el clic: el botón sigue
        // ejecutando su acción normal (Comparar, Exportar, Abrir captura, Muestra…).
        anfitrion.PointerPressed += (_, _) => popup.IsOpen = false;
        borde.PointerEntered += (_, _) => { sobrePopup = true; popup.IsOpen = true; };
        borde.PointerExited += (_, _) => { sobrePopup = false; ProgramarCierre(); };
        if (alPulsar is not null)
            contenido.PointerPressed += (_, _) =>
            {
                popup.IsOpen = false;
                alPulsar();
            };
    }

    // Colores del popup cristalino (legibles en tema claro y oscuro). Viven como TOKENS
    // permanentes en App.axaml; aquí se resuelven con respaldo al valor actual del token
    // (si la resolución de recursos fallara, el popup sigue como estaba).
    private static IBrush TextoPopup => RecursoPopupBrush("tTextoPopup", "#F2F2F2");
    private static IBrush EnlacePopup => RecursoPopupBrush("tEnlacePopup", "#6CBAFF");

    private static IBrush RecursoPopupBrush(string clave, string respaldoHex)
    {
        if (Application.Current?.TryGetResource(clave, null, out var v) is true && v is IBrush br) return br;
        return new SolidColorBrush(Color.Parse(respaldoHex));
    }

    private static Color RecursoPopupColor(string clave, string respaldoHex)
    {
        if (Application.Current?.TryGetResource(clave, null, out var v) is true && v is SolidColorBrush sc) return sc.Color;
        return Color.Parse(respaldoHex);
    }

    /// <summary>Configura los popups de información de los 4 botones de acción (Comparar,
    /// Exportar, Abrir captura, Muestra de prueba), sustituyendo a los tooltips nativos.</summary>
    private void ConfigurarPopupsBotones()
    {
        CrearPopupInfo(CompareButton, new TextBlock
        {
            Text = "Compara el protocolo actual con otro del catálogo (referencia seleccionable). " +
                   "Pulsa «✕ Volver a la ficha» para salir.",
            TextWrapping = TextWrapping.Wrap,
            Foreground = TextoPopup
        });

        CrearPopupInfo(ExportButton, new TextBlock
        {
            Text = "Exporta los diagramas de la ficha actual (pila, grafo, cabecera) al formato elegido: SVG, PNG o PDF.",
            TextWrapping = TextWrapping.Wrap,
            Foreground = TextoPopup
        });

        CrearPopupInfo(AbrirCapturaButton, new TextBlock
        {
            Text = "Abre una ventana del explorador para que selecciones el archivo .cap de captura de paquetes con PCAP/PCAPNG y luego muestra sus detalles en pantalla.",
            TextWrapping = TextWrapping.Wrap,
            Foreground = TextoPopup
        });

        // Muestra de prueba: texto + HIPERVÍNCULO a la carpeta/captura real (abre el
        // explorador con el archivo seleccionado: explorer /select en Windows, open -R en
        // macOS, xdg-open en Linux).
        var carpeta = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "NetProtocol", "capturas");
        var capturaMasReciente = Directory.Exists(carpeta)
            ? Directory.GetFiles(carpeta, "*.pcap")
                .OrderByDescending(f => File.GetLastWriteTimeUtc(f))
                .FirstOrDefault()
            : null;
        var ruta = capturaMasReciente ?? carpeta;

        var intro = new TextBlock
        {
            Text = "Genera una captura sintética determinista (28 protocolos F5). Para localizar la captura:",
            TextWrapping = TextWrapping.Wrap,
            Foreground = TextoPopup
        };
        var enlace = new TextBlock
        {
            Text = ruta,
            TextWrapping = TextWrapping.Wrap,
            TextDecorations = TextDecorations.Underline,
            Foreground = EnlacePopup,
            Cursor = new Cursor(StandardCursorType.Hand)
        };
        var contenidoMuestra = new StackPanel { Spacing = 2, Children = { intro, enlace } };
        CrearPopupInfo(MuestraButton, contenidoMuestra, () => AbrirCapturaEnExplorador(ruta));
    }

    /// <summary>Abre el explorador de archivos en la carpeta de la ruta dada y selecciona
    /// el archivo (Windows: explorer /select; macOS: open -R; Linux: xdg-open del propio
    /// archivo).</summary>
    private void AbrirCapturaEnExplorador(string ruta)
    {
        try
        {
            var psi = new System.Diagnostics.ProcessStartInfo();
            if (OperatingSystem.IsWindows())
            {
                psi.FileName = "explorer.exe";
                psi.Arguments = $"/select,\"{ruta}\"";
            }
            else if (OperatingSystem.IsMacOS())
            {
                psi.FileName = "open";
                psi.Arguments = $"-R \"{ruta}\"";
            }
            else
            {
                psi.FileName = "xdg-open";
                psi.Arguments = $"\"{Path.GetDirectoryName(ruta) ?? ruta}\"";
            }
            psi.UseShellExecute = true;
            System.Diagnostics.Process.Start(psi);
        }
        catch (Exception ex)
        {
            if (IsLoaded) StatusText.Text = $"No se pudo abrir el explorador: {ex.Message}";
        }
    }

    /// <summary>Aplica el zoom por reflow: fuente real del contenido + factor de los
    /// diagramas. El texto con TextWrapping=Wrap se re-envuelve contra el ancho del
    /// panel (nunca desborda a la derecha); los diagramas se re-renderizan con su
    /// FactorZoom (unidad), y su ScrollViewer horizontal aparece solo si desbordan.</summary>
    private void AplicarZoomContenido()
    {
        if (_fuenteBase <= 0) return; // la ventana aún no está abierta (base no capturada)

        DetailText.FontSize = _fuenteBase * _zoom;
        DiagramTitle.FontSize = _tituloBase * _zoom;
        ResumenCaptura.FontSize = _fuenteBase * _zoom;
        DetalleCaptura.FontSize = _fuenteBase * _zoom;
        ListaPaquetes.FontSize = _fuenteBase * _zoom;

        // Los diagramas se reconstruyen con el nuevo factor SOLO si se están mostrando en
        // este momento (ficha con diagramas en pantalla): en Leyenda/Acerca de/Comparador
        // el panel está vacío a propósito y no debe repoblarse al hacer zoom; con una
        // captura abierta el panel está oculto y cada texto de captura ya escaló su fuente.
        if (!PanelCaptura.IsVisible && _seleccionado is not null && DiagramPanel.Children.Count > 0)
        {
            var vecinos = GrafoRelaciones.Vecinos1Salto(_seleccionado.Acronimo, _relaciones);
            RenderDiagramas(_seleccionado, vecinos);
        }
    }

    private void CargarFiltros()
    {
        _cargando = true;
        FilterFamilia.Items.Add("Todas las familias");
        foreach (FamiliaProtocolo f in Enum.GetValues<FamiliaProtocolo>()) FilterFamilia.Items.Add(f.ToString());
        FilterFamilia.SelectedIndex = 0;

        FilterEstado.Items.Add("Todos los estados");
        foreach (LifecycleState e in Enum.GetValues<LifecycleState>()) FilterEstado.Items.Add(e.ToString());
        FilterEstado.SelectedIndex = 0;
        _cargando = false;
    }

    /// <summary>Selector de la referencia de comparación: permite comparar el protocolo
    /// seleccionado contra cualquier protocolo del catálogo (TCP por defecto).</summary>
    private void CargarComparador()
    {
        _cargando = true;
        foreach (var p in _protocolos.Values
                     .OrderBy(p => p.Acronimo, StringComparer.OrdinalIgnoreCase))
            CompareTarget.Items.Add(p);
        // Muestra "ACR · Nombre" en el desplegable (ancho suficiente para los 113).
        // Muestra "ACR · Nombre" en el desplegable con ancho FIJO (300 px): la ventanita de la
// lista no crece con el texto; el texto de cada ítem solo se desplaza (carrusel) cuando
// el puntero está sobre ese ítem.
// - ItemTemplate: se aplica a los ítems del POPUP (carrusel bajo el puntero).
// - SelectionBoxItemTemplate: se aplica al RECUADRO del selector (texto estático,
//   sin animación; elimina los caracteres extra del recuadro).
// IMPORTANTE: el data template se invoca con null al reciclar contenedores
// del popup al abrir; sin la guarda, p.Acronimo lanza NullReferenceException.
        CompareTarget.ItemTemplate = new FuncDataTemplate<Protocol>((p, _) =>
            p is null ? null : new MarqueeTextBlock
            {
                Width = 300,
                Text = $"{p.Acronimo} · {p.Nombre}",
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
            });
        CompareTarget.SelectionBoxItemTemplate = new FuncDataTemplate<Protocol>((p, _) =>
            p is null ? null : new TextBlock
            {
                Text = $"{p.Acronimo} · {p.Nombre}",
                TextTrimming = TextTrimming.CharacterEllipsis,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
            });
        CompareTarget.SelectedItem = _protocolos.Values.FirstOrDefault(p => p.Acronimo == "TCP");
        _cargando = false;
    }

    /// <summary>Selector de protocolo de la barra superior (junto a la búsqueda): referencia
    /// permanente de en qué protocolo estamos y vía de navegación directa. Mismo patrón que
    /// la casilla "Comparar con:": ancho FIJO (300 px) y carrusel del nombre completo en el
    /// desplegable (solo al pasar el ratón por un ítem).</summary>
    private void CargarSelectorProtocolo()
    {
        _cargando = true;
        foreach (var p in _protocolos.Values
                     .OrderBy(p => p.Acronimo, StringComparer.OrdinalIgnoreCase))
            ProtocolSelector.Items.Add(p);
        // Ítems del desplegable: "ACR · Nombre" con carrusel bajo el puntero (ancho fijo).
        ProtocolSelector.ItemTemplate = new FuncDataTemplate<Protocol>((p, _) =>
            p is null ? null : new MarqueeTextBlock
            {
                Width = 300,
                Text = $"{p.Acronimo} · {p.Nombre}",
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
            });
        // Recuadro del selector: texto estático con elipsis (sin animación, sin duplicados).
        ProtocolSelector.SelectionBoxItemTemplate = new FuncDataTemplate<Protocol>((p, _) =>
            p is null ? null : new TextBlock
            {
                Text = $"{p.Acronimo} · {p.Nombre}",
                TextTrimming = TextTrimming.CharacterEllipsis,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
            });
        _cargando = false;
    }

    private IEnumerable<Protocol> Activos
    {
        get
        {
            IEnumerable<Protocol> lista = _protocolos.Values;
            if (FilterFamilia.SelectedIndex > 0 &&
                Enum.TryParse<FamiliaProtocolo>(FilterFamilia.SelectedItem?.ToString(), out var familia))
                lista = lista.Where(p => p.Familia == familia);
            if (FilterEstado.SelectedIndex > 0 &&
                Enum.TryParse<LifecycleState>(FilterEstado.SelectedItem?.ToString(), out var estado))
                lista = lista.Where(p => p.Estado == estado);
            return lista;
        }
    }

    private void ReconstruirNavegacion()
    {
        _cargando = true;
        NavPanel.Children.Clear();

        var texto = (NavFilter.Text ?? "").Trim().ToLowerInvariant();
        IEnumerable<IGrouping<FamiliaProtocolo, Protocol>> grupos = Activos
            .GroupBy(p => p.Familia)
            .OrderBy(g => g.Key.ToString());
        if (texto.Length > 0)
        {
            // Si el texto es un ACRÓNIMO DE FAMILIA exacto (ROUT, SEG, SYNC…), se filtra
            // SOLO por esa familia: no se arrastran otras familias cuyo nombre de protocolo
            // contenga el patrón (p. ej. "ROUT" ya no incluye SEG por el "Routing" de GRE,
            // ni "SYNC" incluye HIST por el "Asynchronous" de ATM). Para cualquier otro
            // texto se mantiene la búsqueda por protocolo (acrónimo/nombre).
            var familiaExacta = Enum.GetValues<FamiliaProtocolo>()
                .Cast<FamiliaProtocolo?>()
                .FirstOrDefault(f => f.ToString() is { } nombre && nombre.Equals(texto, StringComparison.OrdinalIgnoreCase));
            if (familiaExacta is { } fam)
            {
                grupos = grupos.Where(g => g.Key == fam);
            }
            else
            {
                grupos = grupos.Where(g =>
                    g.Key.ToString().ToLowerInvariant().Contains(texto) ||
                    g.Any(p => p.Acronimo.ToLowerInvariant().Contains(texto) ||
                               p.Nombre.ToLowerInvariant().Contains(texto)));
            }
        }

        var expanders = new List<Expander>();
        foreach (var g in grupos)
        {
            var lista = new ListBox { Height = ListaAltura, Width = ListaAnchura }; // anchura idéntica en todos
            foreach (var p in g.OrderBy(p => p.Acronimo, StringComparer.OrdinalIgnoreCase))
            {
                var item = new ListBoxItem { Tag = p, Content = ItemProtocolo(p) };
                lista.Items.Add(item);
            }
            lista.SelectionChanged += (_, _) =>
            {
                if (lista.SelectedItem is ListBoxItem sel && sel.Tag is Protocol proto) RenderFicha(proto);
            };

            var exp = new Expander
            {
                Header = $"{g.Key} ({g.Count()})",
                Content = lista,
                IsExpanded = false, // inicio con todos los desplegables colapsados
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                Margin = new Thickness(0, 0, 0, 6),
                Padding = new Thickness(4)
            };
            exp.Expanded += (_, _) => CerrarOtros(exp, expanders);
            expanders.Add(exp);
            NavPanel.Children.Add(exp);
        }
        _cargando = false;

        if (IsLoaded)
            StatusText.Text = $"Visible: {Activos.Count()} de {_protocolos.Count} protocolos · {_servicios.Contar()} servicios IANA · zoom {_zoom * 100:0}%";
    }

    private static void CerrarOtros(Expander abierto, IEnumerable<Expander> todos)
    {
        foreach (var e in todos)
        {
            if (!ReferenceEquals(e, abierto) && e.IsExpanded) e.IsExpanded = false;
        }
    }

    private void EjecutarBusqueda()
    {
        var q = SearchBox.Text?.Trim();
        if (string.IsNullOrWhiteSpace(q)) { ReconstruirNavegacion(); return; }

        var encontrados = _busqueda.Search(q, 100)
            .Select(h => _protocolos.TryGetValue(h.Urn, out var p) ? p : null)
            .Where(p => p is not null)
            .Cast<Protocol>()
            .ToList();

        if (encontrados.Count == 0)
        {
            // D2-2: el término puede ser un nombre de servicio IANA (con sinónimos agrupados).
            if (MostrarServicioIana(q)) return;
            DetailText.Text = "Sin resultados para la búsqueda.";
            StatusText.Text = $"Búsqueda \"{q}\": 0 resultados.";
            return;
        }

        NavPanel.Children.Clear();
        var lista = new ListBox { Height = ListaAltura, Width = ListaAnchura };
        foreach (var p in encontrados.OrderBy(p => p.Acronimo, StringComparer.OrdinalIgnoreCase))
            lista.Items.Add(new ListBoxItem { Content = ItemProtocolo(p), Tag = p });
        lista.SelectionChanged += (_, _) =>
        {
            if (lista.SelectedItem is ListBoxItem sel && sel.Tag is Protocol proto) RenderFicha(proto);
        };
        NavPanel.Children.Add(new Expander
        {
            Header = $"Resultados ({encontrados.Count})",
            Content = lista,
            IsExpanded = true,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            Margin = new Thickness(0, 0, 0, 6),
            Padding = new Thickness(4)
        });
        StatusText.Text = $"Búsqueda \"{q}\": {encontrados.Count} resultados · zoom {_zoom * 100:0}%";
        RenderFicha(encontrados[0]);
    }

    private void RenderFicha(Protocol? p)
    {
        _seleccionado = p;

        // Navegar a un protocolo (búsqueda, selector, sidebar, grafo…) cierra la captura
        // en pantalla: la ficha vuelve a ocupar el panel central y no queda texto de
        // captura mezclado bajo los diagramas.
        CerrarCapturaSiAbierta();

        if (p is null) { DetailText.Text = "Seleccione un protocolo."; return; }

        // Sincroniza el selector de la barra superior con el protocolo en pantalla (navegues
        // como navegues: sidebar, búsqueda, grafo, comparador). El flag evita que este set
        // dispare SelectionChanged → RenderFicha (recursión).
        _sincronizandoSelector = true;
        ProtocolSelector.SelectedItem = p;
        _sincronizandoSelector = false;

        // Al volver a una ficha real, la comparación deja de estar en pantalla: se oculta
        // su botón de salida (si estaba visible).
        VolverFichaButton.IsVisible = false;

        DetailText.TextAlignment = TextAlignment.Left; // "Acerca de" centra el texto; al volver, izquierda.

        _notasFuentes.TryGetValue(p.Acronimo, out var nf);
        var nota = string.IsNullOrWhiteSpace(nf?.Nota) ? null : nf.Nota;
        var fuenteF3 = string.IsNullOrWhiteSpace(nf?.Fuente) || nf.Fuente == "pendiente" ? null : nf.Fuente;
        _fichas.TryGetValue(p.Acronimo, out var ficha);
        var vecinos = GrafoRelaciones.Vecinos1Salto(p.Acronimo, _relaciones);

        var sb = new StringBuilder();
        sb.AppendLine($"=== {p.Nombre} ({p.Acronimo}) ===");

        // La ficha prioritaria F4 (si existe) aporta los 18 campos; el resto se completa
        // con los catálogos F3/F5/F6/IANA. Solo se imprime una línea si hay dato:
        // los campos sin dato no se muestran (lagunas registradas en bitácora/backlog).
        void Linea(int n, string etiqueta, string? valor)
        {
            if (string.IsNullOrWhiteSpace(valor)) return;
            var num = n <= 9 ? $" {n}" : n.ToString();
            sb.AppendLine($"{num}. {etiqueta,-22}: {valor}");
        }

        Linea(1, "Identidad", $"{p.Nombre} · {p.Acronimo} · familia {p.Familia} [F3]");
        Linea(2, "Estado", ficha?.Campo(2) ?? $"{p.Estado} (catálogo F3, 2026-08-26)");
        Linea(3, "Finalidad", ficha?.Campo(3));
        Linea(4, "Encapsulación",
            ficha?.Campo(4) ?? (vecinos.Count > 0 ? string.Join(", ", vecinos.Select(EtiquetaVecino)) : null));
        Linea(5, "Capas", p.Capas ?? ficha?.Campo(5));
        var puertos = PuertosDe(p.Acronimo);
        Linea(6, "Transp./dir", ficha?.Campo(6)
            ?? (puertos != "—" ? $"{puertos} [IANA]" : null));
        var pdu = _pduPorAcronimo.TryGetValue(p.Acronimo, out var pduV) ? pduV : ficha?.Campo(7);
        Linea(7, "PDU", pdu);
        Linea(8, "Mensajes", ficha?.Campo(8));
        Linea(9, "Campos", ficha?.Campo(9));
        Linea(10, "Secuencia", ficha?.Campo(10));
        Linea(11, "Addressing", ficha?.Campo(11));
        Linea(12, "Routing", ficha?.Campo(12));

        var cifrado = _cifradoPorAcronimo.TryGetValue(p.Acronimo, out var cf) ? cf : null;
        Linea(13, "Seguridad", cifrado is not null ? $"cifrado: {cifrado} [F6]" : ficha?.Campo(13));
        Linea(14, "QoS/rendim.", ficha?.Campo(14));
        Linea(15, "Observabilidad", ficha?.Campo(15));
        Linea(16, "Interoperabilidad", ficha?.Campo(16));
        Linea(17, "Implementaciones", ficha?.Campo(17));
        Linea(18, "Fuentes", ficha?.Campo(18) ?? fuenteF3);

        if (!string.IsNullOrWhiteSpace(nota)) sb.AppendLine($"      Nota/F3 .....: {nota}");

        if (ficha is null && vecinos.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("--- Vecinos a 1 salto (grafo F4) ---");
            foreach (var v in vecinos) sb.AppendLine($"  • {EtiquetaVecino(v)}");
        }

        if (_camposPorAcronimo.TryGetValue(p.Acronimo, out var campos) && campos.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("--- Campos catalogados (F5) ---");
            foreach (var f in campos)
                sb.AppendLine($"  {f.Nombre}: offset {f.OffsetBits?.ToString() ?? "var"} · {f.LongitudBits?.ToString() ?? "var"} bits · {f.Tipo}{(f.Obligatorio ? "" : " (opt.)")}");
        }

        DetailText.Text = sb.ToString();
        RenderDiagramas(p, vecinos);
    }

    /// <summary>Diagramas de arquitectura del protocolo: pila de encapsulación (F4),
    /// grafo de vecinos (F4) y wire format de la cabecera (F5).</summary>
    private void RenderDiagramas(Protocol p, IReadOnlyList<Vecino> vecinos)
    {
        DiagramPanel.IsVisible = true; // la vista de captura lo oculta; al volver se restaura
        DiagramPanel.Children.Clear();
        var docs = new List<(string Titulo, DiagramDocument Doc, IReadOnlyList<NodoGrafo>? Nodos, IReadOnlyDictionary<string, string?>? Abrir)>();

        // 1) Pila de encapsulación: cadena de "corre sobre" desde el medio hacia el protocolo.
        var pila = PilaDeEncapsulacion(p.Acronimo);
        if (pila is not null)
            docs.Add(( "Pila de encapsulación (F4)", pila, null, null));

        // 2) Grafo de vecinos a 1 salto. NAVEGABLE (D5-1): cada nodo recuerda el acrónimo
        //    al que lleva y un clic lo selecciona (RenderFicha), recomponiendo el grafo
        //    alrededor del protocolo pulsado.
        if (vecinos.Count > 0)
        {
            var nodos = new List<(string Nodo, string Etiqueta)>
            {
                (p.Acronimo, p.Acronimo)
            };
            // Clave de nodo -> acrónimo del protocolo a abrir (null si el vecino no está
            // en el catálogo: nodo visible pero no navegable).
            var abrirPorClave = new Dictionary<string, string?>(StringComparer.Ordinal);
            void RegistrarNodo(string clave, string? acronimo)
            {
                // Registro la clave tal cual y normalizada: la semilla usa el acrónimo
                // crudo y los vecinos usan su clave normalizada.
                abrirPorClave[clave] = acronimo;
                abrirPorClave[Normalizar(clave)] = acronimo;
            }

            RegistrarNodo(p.Acronimo, p.Acronimo);
            foreach (var v in vecinos)
            {
                var clave = Normalizar(v.Nombre);
                nodos.Add((clave, EtiquetaNodo(v)));
                var abrir = _normalizados.TryGetValue(clave, out var proto) ? proto.Acronimo : null;
                RegistrarNodo(clave, abrir);
            }
            var aristas = new List<(string A, string B, string Etiqueta)>();
            foreach (var v in vecinos)
                aristas.Add((p.Acronimo, Normalizar(v.Nombre), TipoRelacion(v.Tipo)));
            var (doc, nodosRect) = Layouts.GrafoConNodos(
                $"Vecinos de {p.Acronimo}", p.Acronimo, nodos, aristas,
                mostrarEtiquetasAristas: false);

            // Leyenda con el color de cada tipo presente, al pie del diagrama.
            var tipos = vecinos.Select(v => v.Tipo).Distinct().ToList();
            if (tipos.Count > 0)
            {
                var extra = new List<Primitive>();
                double x = 14, y = doc.Height - 22;
                foreach (var t in tipos)
                {
                    var etq = TipoRelacion(t);
                    var color = DiagramView.ColorDeTipo(etq) ?? "#334155";
                    extra.Add(new Primitive(PrimitiveKind.Rect, x, y, 12, 12, "", color, "#334155"));
                    extra.Add(new Primitive(PrimitiveKind.Text, x + 16, y, 0, 0, etq, "#0f172a"));
                    x += 18 + etq.Length * 7;
                }
                doc = doc with { Items = doc.Items.Concat(extra).ToList() };
            }
            docs.Add(("Grafo de vecinos a 1 salto (F4) — clic en un nodo para navegar",
                doc, nodosRect, abrirPorClave));
        }

        // 3) Wire format de la cabecera desde F5 (offset/longitud conocidos).
        if (_camposPorAcronimo.TryGetValue(p.Acronimo, out var campos) && campos.Count > 0)
        {
            var wire = campos
                .Where(c => c.OffsetBits.HasValue && c.LongitudBits.HasValue)
                .Select(c => new WireField(c.Nombre, c.OffsetBits!.Value, c.LongitudBits!.Value))
                .ToList();
            if (wire.Count > 0)
                docs.Add(("Wire format de la cabecera (F5)", Layouts.WireFormat(
                    $"Cabecera {p.Acronimo} — campos F5", wire), null, null));
        }

        // Cache para la exportación D4-3 (se exportan exactamente estos diagramas).
        _docsActuales = docs;

        // La redimensión por borde debe permitir ver COMPLETO el diagrama más ancho de la
        // ficha (la cabecera wire format es la más ancha de las tres: 688 px frente al
        // grafo 600 y a la pila 480). El mínimo se calcula sobre el ancho natural del
        // documento por el zoom actual, con piso 960 y tope 1400 (a zoom alto el diagrama
        // sigue teniendo su scroll horizontal propio, y la ventana no se vuelve imposible
        // de encoger en pantallas normales).
        if (docs.Count > 0)
        {
            var anchoMax = docs.Max(d => d.Doc.Width) * _zoom;
            MinWidth = Math.Clamp(AnchoFijoRedimension + anchoMax, MinAnchoVentana, MaxAnchoVentana);
        }
        else
        {
            MinWidth = MinAnchoVentana;
        }

        if (docs.Count == 0)
        {
            DiagramTitle.IsVisible = false;
            return;
        }

        DiagramTitle.IsVisible = true;
        foreach (var (titulo, doc, nodosGrafo, abrir) in docs)
        {
            var panel = new StackPanel { Spacing = 4 };
            // Título hereda el color del tema (sin color fijo), es seleccionable y escala
            // con el zoom de reflow (misma base de fuente que DiagramTitle).
            panel.Children.Add(new SelectableTextBlock
            {
                Text = titulo,
                FontWeight = Avalonia.Media.FontWeight.SemiBold,
                FontSize = _tituloBase * _zoom
            });
            var view = new DiagramView
            {
                Document = doc,
                Nodos = nodosGrafo,
                // Zoom del diagrama como UNIDAD (reflow de texto + factor propio):
                // geometría y texto escalan de verdad vía el contexto de dibujo.
                FactorZoom = _zoom
            };
            // Navegación del grafo (D5-1): un clic en un nodo abre su ficha.
            if (abrir is not null)
                view.NodoPulsado += clave => NavegarGrafo(clave, abrir);
            // Cada diagrama con su propio scroll horizontal: solo aparece cuando el
            // diagrama escalado desborda el ancho del panel (el contenido de texto no
            // necesita barra horizontal gracias al reflow; los diagramas, de geometría
            // fija, sí pueden necesitarla a zoom alto).
            panel.Children.Add(new ScrollViewer
            {
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                VerticalScrollBarVisibility = ScrollBarVisibility.Disabled,
                Content = view
            });
            DiagramPanel.Children.Add(panel);
        }
    }

    /// <summary>Navegación del grafo navegable (D5-1): traduce la clave de un nodo pulsado
    /// al acrónimo del protocolo y re-renderiza su ficha (el grafo se recompone alrededor
    /// del protocolo pulsado). Nodos sin catálogo (clave sin acrónimo) no navegan.</summary>
    private void NavegarGrafo(string clave, IReadOnlyDictionary<string, string?> abrirPorClave)
    {
        if (!abrirPorClave.TryGetValue(clave, out var acronimo) || acronimo is null) return;
        if (!_normalizados.TryGetValue(Normalizar(acronimo), out var proto)) return;
        RenderFicha(proto);
        if (IsLoaded)
            StatusText.Text = $"Grafo: {proto.Acronimo} · {_relaciones.Count} relaciones F4 · zoom {_zoom * 100:0}%";
    }

    /// <summary>Exporta los diagramas de la ficha actual al formato elegido (D4-3):
    /// SVG (vectorial), PNG (raster del mismo renderer) o PDF (vectorial mínimo).
    /// Nombres: NetProtocol-&lt;acrónimo&gt;-&lt;tipo&gt;.&lt;ext&gt;.</summary>
    private async Task ExportarDiagramasAsync()
    {
        if (_seleccionado is null || _docsActuales.Count == 0)
        {
            if (IsLoaded) StatusText.Text = "Nada que exportar: abre la ficha de un protocolo con diagramas.";
            return;
        }

        var opciones = new FolderPickerOpenOptions
        {
            Title = "Carpeta de destino para exportar los diagramas",
            AllowMultiple = false
        };
        var carpetas = await StorageProvider.OpenFolderPickerAsync(opciones);
        if (carpetas.Count == 0 || carpetas[0].Path is not { } uri) return;
        var carpeta = uri.LocalPath;

        var formato = (ExportFormat.SelectedItem as string) ?? "SVG";
        var ext = formato switch { "PNG" => ".png", "PDF" => ".pdf", _ => ".svg" };
        var acronimo = _seleccionado.Acronimo;
        var ok = 0;
        foreach (var (_, doc, _, _) in _docsActuales)
        {
            var nombre = SanearNombre($"{acronimo}-{doc.Tipo}") + ext;
            var ruta = Path.Combine(carpeta, nombre);
            try
            {
                switch (formato)
                {
                    case "PNG":
                        // PNG a 2× (alta resolución, nítido para impresión/documentos).
                        var png = DiagramExporter.Png(doc, 2.0);
                        if (!DiagramExporter.EsPngValido(png))
                        {
                            if (IsLoaded) StatusText.Text = $"Exportación PNG inválida para {nombre}.";
                            return;
                        }
                        await File.WriteAllBytesAsync(ruta, png);
                        break;
                    case "PDF":
                        await File.WriteAllBytesAsync(ruta, DiagramExporter.Pdf(doc));
                        break;
                    default:
                        await File.WriteAllTextAsync(ruta, DiagramExporter.Svg(doc));
                        break;
                }
                ok++;
            }
            catch (Exception ex)
            {
                if (IsLoaded) StatusText.Text = $"Error exportando {nombre}: {ex.Message}";
                return;
            }
        }
        if (IsLoaded)
            StatusText.Text = $"Exportados {ok} diagrama(s) de {acronimo} como {formato} en {carpeta}";
    }

    /// <summary>Nombre de archivo seguro (solo letras, dígitos, '-', '_' y '.').</summary>
    private static string SanearNombre(string s)
        => string.Concat(s.Select(c => char.IsLetterOrDigit(c) || c is '-' or '_' or '.' ? c : '_'));

    // ── Vista de captura (D6-1/D6-2) ──────────────────────────────────────────────────

    private async Task AbrirCapturaAsync()
    {
        var opciones = new FilePickerOpenOptions
        {
            Title = "Abrir captura (PCAP / PCAPNG)",
            AllowMultiple = false,
            FileTypeFilter = new[]
            {
                new FilePickerFileType("Capturas PCAP/PCAPNG") { Patterns = new[] { "*.pcap", "*.pcapng" } }
            }
        };
        var archivos = await StorageProvider.OpenFilePickerAsync(opciones);
        if (archivos.Count == 0 || archivos[0].Path is not { } uri) return;
        try { MostrarCaptura(PcapCaptureReader.Abrir(uri.LocalPath), uri.LocalPath); }
        catch (Exception ex) { if (IsLoaded) StatusText.Text = $"No se pudo abrir la captura: {ex.Message}"; }
    }

    /// <summary>Genera una muestra sintética determinista (F5) y la vuelca a un .pcap (D6).</summary>
    private void GenerarMuestra()
    {
        // Muestra completa determinista: una trama por cada uno de los 28 protocolos F5.
        var captura = PcapSintetico.GenerarTodas();
        var capturas = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "NetProtocol", "capturas");
        var ruta = Path.Combine(capturas, $"NetProtocol-muestra-{DateTime.Now:yyyyMMdd-HHmmss}.pcap");
        try
        {
            PcapWriter.EscribirAArchivo(ruta, captura);
            MostrarCaptura(captura, ruta);
            if (IsLoaded)
                StatusText.Text = $"Muestra sintética (ETH/IPv4/IPv6/TCP/UDP/DNS/ICMP) volcada en {ruta} · click en un paquete para su detalle F5";
        }
        catch (Exception ex)
        {
            if (IsLoaded) StatusText.Text = $"No se pudo volcar la muestra: {ex.Message}";
        }
    }

    private void MostrarCaptura(PcapCapture captura, string ruta)
    {
        _paquetesCaptura = captura.Paquetes.ToList();
        _rutaCaptura = ruta;
        DetailText.IsVisible = false;
        DiagramTitle.IsVisible = false;
        DiagramPanel.IsVisible = false;
        PanelCaptura.IsVisible = true;
        MinWidth = MinAnchoVentana; // la vista de captura no tiene diagramas
        VolverFichaButton.IsVisible = false; // la captura no es una comparación

        ResumenCaptura.Text =
            $"{Path.GetFileName(ruta)} · linktype {captura.LinkType} ({(captura.EsEthernet ? "Ethernet" : "otro")}) · {_paquetesCaptura.Count} paquetes";
        ListaPaquetes.Items.Clear();
        for (var i = 0; i < _paquetesCaptura.Count; i++)
            ListaPaquetes.Items.Add(new ListBoxItem { Content = ResumenPaquete(i), Tag = i });
        ListaPaquetes.SelectedIndex = 0;
    }

    /// <summary>Cierra la captura en pantalla si está abierta (idempotente): restaura la
    /// visibilidad de la ficha y limpia el estado de captura. Se llama desde RenderFicha
    /// (toda navegación a un protocolo cierra la captura) y desde las vistas textuales
    /// (Comparador/Leyenda/Acerca de), que escriben en DetailText (oculto con captura).</summary>
    private void CerrarCapturaSiAbierta()
    {
        if (!PanelCaptura.IsVisible) return;
        PanelCaptura.IsVisible = false;
        DetailText.IsVisible = true;
        DiagramPanel.IsVisible = true;
        DiagramTitle.IsVisible = true;
        _paquetesCaptura.Clear();
        _rutaCaptura = "";
    }

    private void CerrarCaptura()
    {
        CerrarCapturaSiAbierta();
        if (_seleccionado is not null) RenderFicha(_seleccionado);
        if (IsLoaded) StatusText.Text = $"Captura cerrada · {_protocolos.Count} protocolos · zoom {_zoom * 100:0}%";
    }

    private string ResumenPaquete(int i)
    {
        var p = _paquetesCaptura[i];
        var d = PcapDissector.Disectar(p.Data);
        var proto = d.EsTcp ? "TCP"
            : d.ProtocoloIp == 17 ? "UDP"
            : d.ProtocoloIp == 1 ? "ICMP"
            : d.EtherType == 0x86DD ? "IPv6"
            : d.EsIpv4 ? "IPv4"
            : d.ProtocoloIp is { } pp ? $"IP proto {pp}" : "otro";
        var dir = d.IpOrigen is null
            ? ""
            : $"{d.IpOrigen}{(d.PuertoOrigen is { } po ? $":{po}" : "")} → {d.IpDestino}{(d.PuertoDestino is { } pd ? $":{pd}" : "")}";
        return $"#{i + 1} · {dir} · {proto} · {p.Data.Length} B";
    }

    private string DetalleDe(PcapPacket p)
    {
        var f = p.Data;
        var d = PcapDissector.Disectar(f);
        var sb = new StringBuilder();
        sb.AppendLine($"=== Paquete de {f.Length} bytes ===");
        if (d.EsEthernet)
        {
            sb.AppendLine($"Ethernet II: {Mac(f, 0)} → {Mac(f, 6)} · EtherType 0x{(d.EtherType ?? 0):X4}");
            if (d.EsIpv4) sb.AppendLine($"IPv4: {d.IpOrigen} → {d.IpDestino} · TTL {f[22]} · proto {d.ProtocoloIp}");
            if (d.EsTcp) sb.AppendLine($"TCP: {d.PuertoOrigen} → {d.PuertoDestino}");
            if (d.EtherType == 0x86DD) sb.AppendLine("IPv6: capas y campos en el detalle F5");
        }
        sb.AppendLine();
        // Cadena de capas detectada → validación del layout F5 de cada una (D6-2/L-004).
        foreach (var c in PcapDissector.DisectarCapas(f))
            sb.AppendLine(ValidarCapaTexto(c.AcronimoF5,
                f.AsSpan(c.InicioBytes, c.LongitudBytes).ToArray(), c.BaseBits));
        return sb.ToString();
    }

    /// <summary>Valida el buffer contra el layout F5 de un protocolo (D6-2/L-004).</summary>
    private string ValidarCapaTexto(string acronimoF5, byte[] bufer, int baseBits)
    {
        var campos = CamposF5De(acronimoF5);
        if (campos is null || campos.Count == 0)
            return $"   {acronimoF5}: sin layout F5 en el catálogo";
        var definidos = campos
            .Where(f => f.OffsetBits.HasValue)
            .Select(f => new CampoDefinido(f.OffsetBits!.Value, f.LongitudBits, f.Nombre))
            .ToList();
        var resultado = PcapDissector.Validar(bufer, definidos, baseBits);
        var resumen = PcapDissector.Resumen(acronimoF5, resultado);
        var detalles = string.Join(", ", resultado.Select(c => $"{c.Nombre}={c.ValorHex}"));
        return $"   {acronimoF5} [{resumen}] · {detalles}";
    }

    private IReadOnlyList<Field>? CamposF5De(string acronimoF5)
    {
        if (_camposPorAcronimo.TryGetValue(acronimoF5, out var campos)) return campos;
        var clave = _camposPorAcronimo.Keys.FirstOrDefault(k =>
            string.Equals(k, acronimoF5, StringComparison.OrdinalIgnoreCase) ||
            (acronimoF5 == "IPv4" && k.Equals("IP", StringComparison.OrdinalIgnoreCase)));
        return clave is null ? null : _camposPorAcronimo[clave];
    }

    /// <summary>Servicio IANA agrupado: nombre canónico + sus puertos distintos (puerto, transporte).</summary>
    /// <summary>Entity-linking visible (D2-2): si el término es un servicio IANA agrupado,
    /// muestra sus puertos y, si tiene vínculo curado, abre el protocolo del catálogo.</summary>
    private bool MostrarServicioIana(string q)
    {
        var qn = ServiciosDedup.Normalizar(q);
        var svc = _dedup.Servicios.FirstOrDefault(s => ServiciosDedup.Normalizar(s.Nombre) == qn);
        if (svc is null) return false;

        var acronimo = VinculoServicios.AcronimoDe(svc.Nombre);
        _normalizados.TryGetValue(ServiciosDedup.Normalizar(acronimo ?? svc.Nombre), out var proto);

        var sb = new StringBuilder();
        sb.AppendLine($"=== Servicio IANA: {svc.Nombre} ===");
        sb.AppendLine($"Puertos registrados: {string.Join(", ", svc.Puertos.Select(p => $"{p.Puerto}/{p.Transporte}"))}");
        if (proto is not null)
            sb.AppendLine($"Protocolo vinculado del catálogo: {proto.Nombre} ({proto.Acronimo}) [vínculo curado IANA→F3]");
        DetailText.TextAlignment = TextAlignment.Left;
        DetailText.Text = sb.ToString();
        DiagramTitle.IsVisible = false;
        DiagramPanel.IsVisible = true;
        DiagramPanel.Children.Clear();
        if (proto is not null) RenderFicha(proto);
        if (IsLoaded)
            StatusText.Text = $"Servicio IANA \"{q}\" → {svc.Nombre} · {svc.Puertos.Count} puerto(s)";
        return true;
    }

    private static string Mac(byte[] b, int off)
        => string.Join(":", Enumerable.Range(off, 6).Select(i => b[i].ToString("X2")));

    /// <summary>Cadena "X corre sobre Y corre sobre Z…" desde el protocolo hacia el medio
    /// (usando las relaciones F4), invertida para dibujar de abajo (medio) hacia arriba.</summary>
    private DiagramDocument? PilaDeEncapsulacion(string acronimo)
    {
        var cadena = new List<string> { acronimo };
        var visto = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { acronimo };
        var actual = acronimo;
        const int maxProfundidad = 8;
        for (var i = 0; i < maxProfundidad; i++)
        {
            var rel = _relaciones.FirstOrDefault(r =>
                Normalizar(GrafoRelaciones.EntidadDe(r.Origen.Value)) == Normalizar(actual) &&
                r.Tipo == RelacionTipo.CorreSobre);
            if (rel is null) break;
            var siguiente = GrafoRelaciones.EntidadDe(rel.Destino.Value);
            if (!visto.Add(siguiente)) break; // evita ciclos
            cadena.Add(siguiente);
            actual = siguiente;
        }
        if (cadena.Count < 2) return null;

        // Mostrar nombres legibles (acrónimo F3 si se resuelve; si no, entidad normalizada).
        var capas = cadena.Select(c => _normalizados.TryGetValue(Normalizar(c), out var pr)
                ? $"{pr.Acronimo} — {pr.Nombre}"
                : c)
            .ToList();
        capas.Reverse(); // medio abajo, protocolo arriba
        // Sin título interno (el panel ya lo muestra como cabecera) y sin la etiqueta
        // "encapsulación" repetida en cada conexión (texto redundante, estilo grafo).
        return Layouts.Pila($"Pila de encapsulación de {acronimo} (F4)", capas,
            mostrarTitulo: false, mostrarEtiquetasEnlace: false);
    }

    private string EtiquetaNodo(Vecino v)
        => _normalizados.TryGetValue(Normalizar(v.Nombre), out var proto)
            ? $"{proto.Acronimo}"
            : v.Nombre;

    /// <summary>Puertos IANA deduplicados y enlazados al protocolo (D2-2): unión de
    /// (a) servicios cuyo nombre normalizado coincide con el acrónimo y (b) servicios cuyo
    /// vínculo curado IANA→F3 apunta al protocolo (p. ej. HTTP recoge http, www-http, http-alt).</summary>
    private string PuertosDe(string acronimo)
    {
        var vistos = new SortedSet<(int Puerto, string Transporte)>();
        var acrNorm = ServiciosDedup.Normalizar(acronimo);
        foreach (var s in _dedup.Servicios)
        {
            var coincideExacto = ServiciosDedup.Normalizar(s.Nombre) == acrNorm;
            var vinculado = VinculoServicios.AcronimoDe(s.Nombre);
            var coincideVinculo = vinculado is not null &&
                string.Equals(vinculado, acronimo, StringComparison.OrdinalIgnoreCase);
            if (coincideExacto || coincideVinculo)
                foreach (var p in s.Puertos) vistos.Add(p);
        }
        return vistos.Count == 0
            ? "—"
            : string.Join(", ", vistos.Select(v => $"{v.Puerto}/{v.Transporte}"));
    }

    private void CompararConReferencia()
    {
        // Restaura la alineación izquierda por si "Acerca de" la dejó centrada.
        DetailText.TextAlignment = TextAlignment.Left;
        CerrarCapturaSiAbierta(); // la comparación es una vista de ficha: sale de la captura
        if (_seleccionado is null) return;
        // Referencia elegida por el usuario (ComboBox "Comparar con:"); TCP por defecto,
        // pero cualquier protocolo del catálogo es válido.
        var referencia = CompareTarget.SelectedItem as Protocol
            ?? _protocolos.Values.FirstOrDefault(p => p.Acronimo == "TCP");
        if (referencia is null) return;

        // La comparación es textual: oculta los diagramas individuales del protocolo.
        DiagramPanel.Children.Clear();
        _docsActuales.Clear();
        DiagramTitle.IsVisible = false;
        MinWidth = MinAnchoVentana; // sin diagramas, la ventana vuelve a su mínimo base
        VolverFichaButton.IsVisible = true; // la comparación queda "en pantalla"; hay salida explícita

        var filas = ProtocoloComparador.Comparar(
            new[] { _seleccionado, referencia },
            (nombre, lim) => _servicios.PorNombre(nombre, lim),
            _pduPorAcronimo,
            _cifradoPorAcronimo,
            _fichas,
            _relaciones);

        const int anchoCelda = 36;

        // Celda de ancho fijo: trunca con "…" si el valor no cabe (la tabla nunca desborda).
        static string Celda(string s, int ancho)
            => s.Length <= ancho ? s.PadRight(ancho) : s[..(ancho - 1)] + "…";

        // Tabla transpuesta: filas = aspectos cortos, columnas = protocolos.
        var a = filas.First(f => f.Protocolo == _seleccionado.Acronimo);
        var b = filas.First(f => f.Protocolo == referencia.Acronimo);
        var cortos = new[]
        {
            ("Familia", a.Familia, b.Familia),
            ("Estado", a.Estado, b.Estado),
            ("Capas", a.Capas, b.Capas),
            ("PDU", a.Pdu, b.Pdu),
            ("Puertos (IANA)", a.Puertos, b.Puertos),
            ("Cifrado (F6)", a.Cifrado, b.Cifrado)
        };
        // Campos largos: fuera de la tabla, en líneas envueltas por protocolo.
        var largos = new[]
        {
            ("Finalidad", Acortar(a.Finalidad), Acortar(b.Finalidad)),
            ("Encapsulación", Acortar(a.Encapsulacion), Acortar(b.Encapsulacion))
        };

        var sb = new StringBuilder();
        sb.AppendLine($"=== Comparador: {a.Protocolo} vs {b.Protocolo} ===");
        sb.AppendLine();
        sb.AppendLine($"{"Aspecto",-16}| {Celda(a.Protocolo, anchoCelda)} | {Celda(b.Protocolo, anchoCelda)}");
        sb.AppendLine(new string('-', 17 + 3 + anchoCelda + 3 + anchoCelda));
        foreach (var (nombre, va, vb) in cortos)
        {
            sb.Append($"{nombre,-16}| {Celda(va, anchoCelda)} | {Celda(vb, anchoCelda)}");
            if (va == vb && va != "—") sb.AppendLine("  (= igual)");
            else sb.AppendLine();
        }
        sb.AppendLine();
        foreach (var (nombre, va, vb) in largos)
        {
            sb.AppendLine($"{nombre} — {a.Protocolo}: {va ?? "—"}");
            sb.AppendLine($"{nombre} — {b.Protocolo}: {vb ?? "—"}");
            sb.AppendLine();
        }
        sb.AppendLine("Fuentes: F3 (familia/estado/capas/puertos IANA), " +
                      "F4 (finalidad/encapsulación por ficha y grafo), F5 (PDU), F6 (cifrado).");
        DetailText.Text = sb.ToString();
    }

    /// <summary>Salida explícita de la vista de comparación: restaura la ficha del
    /// protocolo previo (la comparación nunca cambia _seleccionado, solo oculta la ficha).
    /// El botón "✕ Volver a la ficha" solo es visible mientras la comparación está en
    /// pantalla.</summary>
    private void VolverALaFicha()
    {
        if (_seleccionado is not null)
            RenderFicha(_seleccionado); // vuelve a la ficha del protocolo que se comparaba
        else
        {
            DetailText.Text = "Seleccione un protocolo.";
            DiagramPanel.IsVisible = true;
        }
        // RenderFicha ya oculta el botón; por si _seleccionado fuera null, se oculta aquí.
        VolverFichaButton.IsVisible = false;
    }

    private void TitleBar_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        // Arrastre de la ventana desde la barra de título personalizada (WindowDecorations=None).
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            BeginMoveDrag(e);
    }

    // Grips de redimensión (WindowDecorations=None): el marco del sistema no existe, así
    // que sin esto la ventana no se puede redimensionar por el borde. Cada grip de borde/
    // esquina del axaml llama a BeginResizeDrag con su dirección; MinWidth/MinHeight de la
    // ventana (960×600) fija el límite inferior. El zoom (Ctrl+Scroll) es independiente.
    private void Grip_N(object? sender, PointerPressedEventArgs e) => IniciarRedimension(WindowEdge.North, e);
    private void Grip_S(object? sender, PointerPressedEventArgs e) => IniciarRedimension(WindowEdge.South, e);
    private void Grip_W(object? sender, PointerPressedEventArgs e) => IniciarRedimension(WindowEdge.West, e);
    private void Grip_E(object? sender, PointerPressedEventArgs e) => IniciarRedimension(WindowEdge.East, e);
    private void Grip_NW(object? sender, PointerPressedEventArgs e) => IniciarRedimension(WindowEdge.NorthWest, e);
    private void Grip_NE(object? sender, PointerPressedEventArgs e) => IniciarRedimension(WindowEdge.NorthEast, e);
    private void Grip_SW(object? sender, PointerPressedEventArgs e) => IniciarRedimension(WindowEdge.SouthWest, e);
    private void Grip_SE(object? sender, PointerPressedEventArgs e) => IniciarRedimension(WindowEdge.SouthEast, e);

    private void IniciarRedimension(WindowEdge borde, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            BeginResizeDrag(borde, e);
    }

    private void Minimize_Click(object? sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

    private void Maximize_Click(object? sender, RoutedEventArgs e)
        => WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;

    private void Close_Click(object? sender, RoutedEventArgs e) => Close();

    private void AlternarTema()
    {
        var app = Application.Current;
        if (app is null) return;
        app.RequestedThemeVariant = app.RequestedThemeVariant == ThemeVariant.Dark
            ? ThemeVariant.Light
            : ThemeVariant.Dark;
        // Los diagramas usan paleta por tema (DiagramView): se repintan los que estén
        // en pantalla con la nueva paleta (AplicarZoomContenido repuebla solo si el panel
        // tiene diagramas visibles y no hay captura abierta — mismo guard que el zoom).
        AplicarZoomContenido();
    }

    /// <summary>Leyenda de las 13 familias: acrónimo → descripción escueta + ejemplos del catálogo.</summary>
    private void MostrarLeyenda()
    {
        // Alineación izquierda siempre: "Acerca de" centra el texto y hay que restaurarla.
        DetailText.TextAlignment = TextAlignment.Left;
        CerrarCapturaSiAbierta(); // la leyenda es una vista de ficha: sale de la captura
        // Visto textual: oculta los diagramas del protocolo previamente seleccionado.
        DiagramPanel.Children.Clear();
        _docsActuales.Clear();
        DiagramTitle.IsVisible = false;
        MinWidth = MinAnchoVentana; // sin diagramas, la ventana vuelve a su mínimo base
        VolverFichaButton.IsVisible = false; // la leyenda no es una comparación

        var sb = new StringBuilder();
        sb.AppendLine("=== Leyenda de familias de protocolos ===");
        sb.AppendLine();
        sb.AppendLine("Cada familia agrupa protocolos con un propósito/ámbito común. Acrónimo — descripción (ejemplos del catálogo):");
        sb.AppendLine();

        foreach (var familia in Enum.GetValues<FamiliaProtocolo>()
                     .Cast<FamiliaProtocolo>()
                     .OrderBy(f => f.ToString(), StringComparer.OrdinalIgnoreCase))
        {
            var acronimo = familia.ToString();
            var descripcion = _familias.TryGetValue(acronimo, out var d) ? d : "[descripción en F3]";
            var ejemplos = _protocolos.Values
                .Where(p => p.Familia == familia)
                .OrderBy(p => p.Acronimo, StringComparer.OrdinalIgnoreCase)
                .Take(5)
                .Select(p => p.Acronimo);
            var ejemploTexto = ejemplos.Count() == 0 ? "" : $" — ej.: {string.Join(", ", ejemplos)}";
            sb.AppendLine($"  {acronimo}: {descripcion}{ejemploTexto}");
        }

        sb.AppendLine();
        sb.AppendLine($"Fuente: catálogo F3 (campo 'familias') · {_protocolos.Count} protocolos en {_familias.Count} familias.");
        DetailText.Text = sb.ToString();

        if (IsLoaded)
            StatusText.Text = $"Leyenda de familias · zoom {_zoom * 100:0}%";
    }

    /// <summary>Sección "Acerca de" con los datos de la aplicación, centrados y formateados.</summary>
    private void MostrarAcercaDe()
    {
        // Visto textual: oculta los diagramas del protocolo previamente seleccionado.
        CerrarCapturaSiAbierta(); // "Acerca de" es una vista de ficha: sale de la captura
        DiagramPanel.Children.Clear();
        _docsActuales.Clear();
        DiagramTitle.IsVisible = false;
        MinWidth = MinAnchoVentana; // sin diagramas, la ventana vuelve a su mínimo base
        VolverFichaButton.IsVisible = false; // "Acerca de" no es una comparación

        var sb = new StringBuilder();
        sb.AppendLine();
        sb.AppendLine("=== Acerca de ===");
        sb.AppendLine();
        sb.AppendLine("Nombre de la aplicación: Net Protocol");
        sb.AppendLine();
        sb.AppendLine("Versión: " + (Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "1.0.0"));
        sb.AppendLine();
        sb.AppendLine("Autor: Marcos Calabrés Ibáñez");
        sb.AppendLine();
        sb.AppendLine("Email: marcoscalabresibaniez@gmail.com");
        sb.AppendLine();
        sb.AppendLine();
        sb.AppendLine("© Todos los derechos reservados");
        DetailText.Text = sb.ToString();
        DetailText.TextAlignment = TextAlignment.Center;

        if (IsLoaded)
            StatusText.Text = "Acerca de · zoom " + $"{_zoom * 100:0}%";
    }

    private static string Normalizar(string s)
        => string.Concat(s.Where(char.IsLetterOrDigit)).ToLowerInvariant();

    /// <summary>Ítem de lista con ajuste de línea: acrónimo · nombre completo envuelto
    /// (sin límite de líneas ni trimming: el texto se ajusta, nunca se recorta).</summary>
    private static Control ItemProtocolo(Protocol p)
    {
        var panel = new StackPanel { Spacing = 1 };
        panel.Children.Add(new TextBlock
        {
            Text = $"{p.Acronimo} · {p.Nombre}",
            TextWrapping = TextWrapping.Wrap
        });
        panel.Children.Add(new TextBlock
        {
            Text = $"{p.Familia} · {p.Estado}",
            FontSize = 11,
            Opacity = 0.65
        });
        return panel;
    }

    /// <summary>Acorta un valor largo para las líneas de comparación (los campos
    /// largos se muestran por protocolo, envueltos, sin desbordar la pantalla).</summary>
    private static string? Acortar(string? s) =>
        string.IsNullOrWhiteSpace(s) || s == "—" ? null
        : s.Length <= 180 ? s
        : s[..177] + "…";

    /// <summary>Nombre legible de un vecino del grafo F4: resuelve la entidad normalizada de la
    /// URN contra el catálogo F3 (acrónimo o nombre) y muestra "ACR · Nombre (tipo)".</summary>
    private string EtiquetaVecino(Vecino v)
    {
        var nombre = _normalizados.TryGetValue(Normalizar(v.Nombre), out var proto)
            ? $"{proto.Acronimo} · {proto.Nombre}"
            : v.Nombre;
        return $"{nombre} ({TipoRelacion(v.Tipo)})";
    }

    private static string TipoRelacion(RelacionTipo t) => t switch
    {
        RelacionTipo.Encapsula => "encapsula",
        RelacionTipo.CorreSobre => "corre sobre",
        RelacionTipo.DependeDe => "depende de",
        RelacionTipo.EsVersionDe => "es versión de",
        RelacionTipo.SustituyeA => "sustituye a",
        RelacionTipo.Implementa => "implementa",
        _ => "documenta"
    };

    /// <summary>
    /// Raíz de datos. En el modo instalado (release) los catálogos viajan junto al
    /// ejecutable en "datos/..." (el csproj los copia al publicar) y esa carpeta es la
    /// raíz; en desarrollo se sube desde el directorio del exe hasta la raíz del repo.
    /// </summary>
    private static string RaizDatos()
    {
        var bundled = Path.Combine(AppContext.BaseDirectory, "datos");
        if (Directory.Exists(Path.Combine(bundled, "FASE-03-INVENTARIO")))
            return bundled;
        var d = new DirectoryInfo(AppContext.BaseDirectory);
        while (d is not null && !File.Exists(Path.Combine(d.FullName, "PLANREDES.md")))
            d = d.Parent;
        return d?.FullName
            ?? throw new InvalidOperationException("No se encontró la raíz del repositorio.");
    }
}