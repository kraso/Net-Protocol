using System.Reflection;
using System.Text;
using Avalonia.Platform.Storage;
using Avalonia;
using Avalonia.Automation;
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
using Redes.Knowledge.App.ViewModels;
using Redes.Knowledge.Infrastructure;
using Redes.Knowledge.Infrastructure.Capturas;
using Redes.Knowledge.Infrastructure.Updates;
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
    private const double ListaAltura = 240;
    private const double ListaAnchura = 306; // idéntica para todos los grupos (340 - márgenes - scrollbar)

    // Mínimo horizontal de la ventana según los diagramas visibles: la redimensión por
    // borde no puede dejar un diagrama más estrecho que su ancho natural. Componentes
    // fijos de la UI desde el borde izquierdo hasta el diagrama: grip de borde (6) +
    // sidebar (360) + márgenes de la ficha (28) + scrollbar vertical (~17) + grip (6).
    private const double AnchoFijoRedimension = 417;
    private const double MinAnchoVentana = 960;   // piso absoluto (también en el axaml)
    private const double MaxAnchoVentana = 1400;  // tope: no romper pantallas normales a zoom alto

    private readonly CatalogoViewModel _catalogo;

    // Estado de presentación (zoom, selección, captura) sin dependencia de Avalonia.
    private readonly PresentacionViewModel _presentacion = new();
    private readonly SqliteProtocolRepository _repo;
    private readonly SqliteServiceRepository _servicios;
    private readonly SqliteSearchEngine _busqueda;
    private bool _cargando;

    // Evita recursión al sincronizar el selector de protocolo con RenderFicha.
    private bool _sincronizandoSelector;

    /// <summary>Vista textual en pantalla: el cambio de idioma la repinta en vez de navegar.</summary>
    private enum Vista { Ficha, Leyenda, Acerca, Comparacion }

    private Vista _vista = Vista.Ficha;

    // Última posición del puntero (coordenadas de la ventana) para el cierre por geometría
    // de los popups de información (sin parpadeo).
    private Point _ultimaPosPuntero = new(-1, -1);

    // Supervisión GLOBAL de los popups de información: cierre determinista por deadline
    // (5 s desde que el popup se ABRE). Los re-enters espurios de X11 ya no pueden reiniciar
    // un temporizador por-popup (eso dejaba los tooltips abiertos mucho tiempo y de forma
    // errática); la deadline solo se fija en la transición cerrado→abierto.
    private readonly DispatcherTimer _supervisorTooltips;
    private readonly List<SupertipPopup> _vigilados = new();

    /// <summary>Estado de un popup de información vigilado por el supervisor global.</summary>
    private sealed class SupertipPopup
    {
        public required Popup Popup { get; init; }
        public DateTime AbiertoDesde { get; set; }
        public DateTime NoReabrirHasta { get; set; } = DateTime.MinValue;
    }

    /// <summary>Diagramas de la ficha actual (cache para la exportación D4-3).</summary>
    private List<(string Titulo, DiagramDocument Doc, IReadOnlyList<NodoGrafo>? Nodos, IReadOnlyDictionary<string, string?>? Abrir)> _docsActuales = new();

    public MainWindow()
    {
        InitializeComponent();

        // Idioma de la interfaz: NETPROTOCOL_IDIOMA manda; si no, LANG/LC_ALL; español
        // por defecto. AplicarIdioma vuelca el catálogo en los controles con nombre.
        Textos.Detectar();
        AplicarIdioma();

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
        // Catálogos F3–F6 e índices de consulta: estado y lógica pura en el ViewModel,
        // sin dependencia de Avalonia (ver ViewModels/CatalogoViewModel.cs).
        _catalogo = CatalogoViewModel.Cargar(raiz, _repo.GetAll(), _servicios.Todos());

        CargarFiltros();
        CargarComparador();
        CargarSelectorProtocolo();
        ReconstruirNavegacion();
        // Ficha inicial: primer protocolo con relaciones catalogadas (TCP) para mostrar
        // el grafo F4 al abrir; si no existe, el primero del catálogo.
        var inicial = _catalogo.Protocolos.Values
            .FirstOrDefault(p => p.Acronimo == "TCP")
            ?? _catalogo.Protocolos.Values
                .OrderByDescending(p => GrafoRelaciones.Vecinos1Salto(p.Acronimo, _catalogo.Relaciones).Count)
                .ThenBy(p => p.Acronimo, StringComparer.OrdinalIgnoreCase)
                .FirstOrDefault();
        RenderFicha(inicial);

        StatusText.Text = Textos.F("estado.dataset", _catalogo.Protocolos.Count, _servicios.Contar(), _catalogo.Relaciones.Count, importados, _presentacion.ZoomTexto);

        SearchButton.Click += (_, _) => EjecutarBusqueda();
        SearchBox.KeyDown += (_, e) => { if (e.Key == Key.Enter) EjecutarBusqueda(); };
        ThemeButton.Click += (_, _) => AlternarTema();
        LanguageButton.Click += (_, _) => AlternarIdioma();
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
        // cierre determinista por deadline (ver ConfigurarPopupsBotones).
        _supervisorTooltips = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
        _supervisorTooltips.Tick += (_, _) =>
        {
            foreach (var v in _vigilados)
            {
                if (!v.Popup.IsOpen) continue;
                if (DateTime.UtcNow - v.AbiertoDesde > TimeSpan.FromSeconds(5))
                {
                    v.Popup.IsOpen = false;
                    // Gracia anti-reabrir: en X11 el desmapeo del overlay re-emite enters
                    // espurios; ignorarlos 1,2 s para no volver a abrir el popup al cerrarlo.
                    v.NoReabrirHasta = DateTime.UtcNow.AddMilliseconds(1200);
                }
            }
        };
        _supervisorTooltips.Start();
        ConfigurarPopupsBotones();
        CerrarCapturaButton.Click += (_, _) => CerrarCaptura();
        ListaPaquetes.SelectionChanged += (_, _) =>
        {
            if (ListaPaquetes.SelectedItem is ListBoxItem { Tag: int idx } &&
                _presentacion.SeleccionarPaquete(idx) &&
                _presentacion.PaqueteActual is { } paquete)
                DetalleCaptura.Text = DetalleDe(paquete);
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
                AplicarPasoZoom(e.Delta.Y > 0 ? PresentacionViewModel.ZoomPaso : -PresentacionViewModel.ZoomPaso);
                e.Handled = true;
            },
            RoutingStrategies.Bubble,
            handledEventsToo: true);

        // Captura la base de fuente EFECTIVA del contenido una vez la ventana está en el
        // árbol (el tema la ha aplicado): a zoom 100 % el aspecto no cambia respecto al
        // montaje anterior. Hasta que se dispare, AplicarZoomContenido no hace nada.
        Opened += (_, _) =>
        {
            _presentacion.FijarBases(DetailText.FontSize, DiagramTitle.FontSize);
            AplicarZoomContenido();
            if (IsLoaded) StatusText.Text = Textos.F("estado.dataset.zoom", _catalogo.Protocolos.Count, _servicios.Contar(), _presentacion.ZoomTexto);
            _ = ComprobarActualizacionesAsync();
        };

        // Última posición del puntero (coordenadas de la ventana), compartida por todos
        // los popups de información para decidir el cierre por geometría (sin parpadeo).
        AddHandler(InputElement.PointerMovedEvent,
            (_, e) => _ultimaPosPuntero = e.GetPosition(this),
            RoutingStrategies.Bubble,
            handledEventsToo: true);
    }

    /// <summary>
    /// Comprobación de actualizaciones (P1): opcional y no bloqueante. Si hay una versión
    /// más reciente se muestra un aviso bajo la barra de estado; cualquier fallo (sin red,
    /// timeout, API caída) se ignora en silencio porque la app es local-first.
    /// </summary>
    private async Task ComprobarActualizacionesAsync()
    {
        try
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version ?? new System.Version(0, 0, 0);
            using var http = GitHubUpdateChecker.CrearCliente();
            var resultado = await new GitHubUpdateChecker(http).CheckAsync(version).ConfigureAwait(true);
            if (!resultado.HayActualizacion) return;
            UpdateText.Text = Textos.F("estado.actualizacion", resultado.VersionDisponible, resultado.UrlDescarga);
            UpdateText.IsVisible = true;
        }
        catch
        {
            // La comprobación de actualizaciones nunca debe afectar al arranque ni a la UI.
        }
    }

    /// <summary>Crea el popup de información "cristalino" con el que se sustituyen los
    /// tooltips de los botones de acción (el ToolTip nativo se cierra al salir del botón y
    /// no permite enlaces). Estilo profesional: acrílico oscuro translúcido (blur real de
    /// Avalonia), borde claro, esquinas redondeadas y ancho limitado al de la ventana actual
    /// (el texto largo se envuelve en multilínea y nunca se escapa por el borde).
    /// Permanencia SIN PARPADEO: el cierre se decide por GEOMETRÍA (posición del puntero
    /// contra el rect del botón), no por PointerEntered/Exited del overlay (que al abrir
    /// disparan eventos espurios). La posición la mantiene el PointerMoved global
    /// (_ultimaPosPuntero). Además hay un AUTO-CIERRE por tiempo INCONDICIONAL (igual que
    /// el ShowDuration del ToolTip nativo): en Linux/X11 los eventos del overlay no llegan
    /// de forma fiable, así que a los 6 s el popup se cierra siempre (con una breve gracia
    /// anti-reabrir para no dejarlo abierto de nuevo por los re-enter espurios al desmapear).
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

        // CIERRE determinista por DEADLINE (supervisor global, ver _supervisorTooltips):
        // el popup se cierra 5 s después de ABRIRSE, sin depender de flags ni eventos del
        // overlay. En Linux/X11 los PointerEntered autobiográficos del botón/popup se
        // re-emiten sin orden (reiniciaban un temporizador por-popup → cierres a los 30-60 s
        // y erráticos); aquí la deadline se fija solo en la transición cerrado→abierto, así
        // que ningún re-enter la puede prolongar. El cierre por geometría (salir del botón)
        // sigue como atajo cuando los eventos SÍ llegan (Windows).
        var vig = new SupertipPopup { Popup = popup };
        _vigilados.Add(vig);

        void OcultarPopup()
        {
            popup.IsOpen = false;
        }

        void AbrirSiProcede()
        {
            if (DateTime.UtcNow < vig.NoReabrirHasta) return;
            var yaAbierto = popup.IsOpen;
            popup.IsOpen = true;
            if (!yaAbierto) vig.AbiertoDesde = DateTime.UtcNow;
        }

        bool PunteroDentro()
        {
            var vacio = new Rect(0, 0, 0, 0);
            var botonRect = anfitrion.TranslatePoint(new Point(0, 0), this) is { } pb
                ? new Rect(pb, anfitrion.Bounds.Size)
                : vacio;
            return botonRect.Contains(_ultimaPosPuntero);
        }

        void ProgramarCierre()
        {
            var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(400) };
            timer.Tick += (_, _) =>
            {
                timer.Stop();
                if (!PunteroDentro()) OcultarPopup();
            };
            timer.Start();
        }

        anfitrion.PointerEntered += (_, _) => AbrirSiProcede();
        anfitrion.PointerExited += (_, _) => ProgramarCierre();
        // Al pulsar el botón se cierra el popup sin consumir el clic: el botón sigue
        // ejecutando su acción normal (Comparar, Exportar, Abrir captura, Muestra…).
        anfitrion.PointerPressed += (_, _) => OcultarPopup();
        borde.PointerEntered += (_, _) => AbrirSiProcede();
        borde.PointerExited += (_, _) => ProgramarCierre();
        if (alPulsar is not null)
            contenido.PointerPressed += (_, _) =>
            {
                OcultarPopup();
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
            if (IsLoaded) StatusText.Text = Textos.F("estado.explorador.error", ex.Message);
        }
    }

    /// <summary>Aplica el zoom por reflow: fuente real del contenido + factor de los
    /// diagramas. El texto con TextWrapping=Wrap se re-envuelve contra el ancho del
    /// panel (nunca desborda a la derecha); los diagramas se re-renderizan con su
    /// FactorZoom (unidad), y su ScrollViewer horizontal aparece solo si desbordan.</summary>
    private void AplicarZoomContenido()
    {
        if (!_presentacion.BasesFijadas) return; // la ventana aún no está abierta (base no capturada)

        DetailText.FontSize = _presentacion.FuenteContenido;
        DiagramTitle.FontSize = _presentacion.FuenteTitulo;
        ResumenCaptura.FontSize = _presentacion.FuenteContenido;
        DetalleCaptura.FontSize = _presentacion.FuenteContenido;
        ListaPaquetes.FontSize = _presentacion.FuenteContenido;

        // Los diagramas se reconstruyen con el nuevo factor SOLO si se están mostrando en
        // este momento (ficha con diagramas en pantalla): en Leyenda/Acerca de/Comparador
        // el panel está vacío a propósito y no debe repoblarse al hacer zoom; con una
        // captura abierta el panel está oculto y cada texto de captura ya escaló su fuente.
        if (!PanelCaptura.IsVisible && _presentacion.Seleccionado is not null && DiagramPanel.Children.Count > 0)
        {
            var vecinos = GrafoRelaciones.Vecinos1Salto(_presentacion.Seleccionado.Acronimo, _catalogo.Relaciones);
            RenderDiagramas(_presentacion.Seleccionado, vecinos);
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
        foreach (var p in _catalogo.Protocolos.Values
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
        CompareTarget.SelectedItem = _catalogo.Protocolos.Values.FirstOrDefault(p => p.Acronimo == "TCP");
        _cargando = false;
    }

    /// <summary>Selector de protocolo de la barra superior (junto a la búsqueda): referencia
    /// permanente de en qué protocolo estamos y vía de navegación directa. Mismo patrón que
    /// la casilla "Comparar con:": ancho FIJO (300 px) y carrusel del nombre completo en el
    /// desplegable (solo al pasar el ratón por un ítem).</summary>
    private void CargarSelectorProtocolo()
    {
        _cargando = true;
        foreach (var p in _catalogo.Protocolos.Values
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
            FamiliaProtocolo? familia = FilterFamilia.SelectedIndex > 0 &&
                Enum.TryParse<FamiliaProtocolo>(FilterFamilia.SelectedItem?.ToString(), out var f) ? f : null;
            LifecycleState? estado = FilterEstado.SelectedIndex > 0 &&
                Enum.TryParse<LifecycleState>(FilterEstado.SelectedItem?.ToString(), out var e) ? e : null;
            return _catalogo.Activos(familia, estado);
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
            StatusText.Text = Textos.F("estado.visible", Activos.Count(), _catalogo.Protocolos.Count, _servicios.Contar(), _presentacion.ZoomTexto);
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
            .Select(h => _catalogo.Protocolos.TryGetValue(h.Urn, out var p) ? p : null)
            .Where(p => p is not null)
            .Cast<Protocol>()
            .ToList();

        if (encontrados.Count == 0)
        {
            // D2-2: el término puede ser un nombre de servicio IANA (con sinónimos agrupados).
            if (MostrarServicioIana(q)) return;
            DetailText.Text = Textos.T("estado.sin.resultados");
            StatusText.Text = Textos.F("estado.busqueda.cero", q);
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
        StatusText.Text = Textos.F("estado.busqueda", q, encontrados.Count, _presentacion.ZoomTexto);
        RenderFicha(encontrados[0]);
    }

    private void RenderFicha(Protocol? p)
    {
        _presentacion.Seleccionar(p);
        _vista = Vista.Ficha;

        // Navegar a un protocolo (búsqueda, selector, sidebar, grafo…) cierra la captura
        // en pantalla: la ficha vuelve a ocupar el panel central y no queda texto de
        // captura mezclado bajo los diagramas.
        CerrarCapturaSiAbierta();

        if (p is null) { DetailText.Text = Textos.T("estado.seleccione"); return; }

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

        _catalogo.Notas.TryGetValue(p.Acronimo, out var nf);
        var nota = string.IsNullOrWhiteSpace(nf?.Nota) ? null : nf.Nota;
        var fuenteF3 = string.IsNullOrWhiteSpace(nf?.Fuente) || nf.Fuente == "pendiente" ? null : nf.Fuente;
        _catalogo.Fichas.TryGetValue(p.Acronimo, out var ficha);
        var vecinos = GrafoRelaciones.Vecinos1Salto(p.Acronimo, _catalogo.Relaciones);

        var modelo = new FichaTextoModelo
        {
            Protocolo = p,
            Ficha = ficha,
            Campos = _catalogo.Campos.TryGetValue(p.Acronimo, out var camposF5) ? camposF5 : Array.Empty<Field>(),
            Vecinos = vecinos.Select(EtiquetaVecino).ToArray(),
            Pdu = _catalogo.Pdu.TryGetValue(p.Acronimo, out var pduV) ? pduV : ficha?.Campo(7),
            Cifrado = _catalogo.Cifrado.TryGetValue(p.Acronimo, out var cf) ? cf : null,
            Puertos = PuertosDe(p.Acronimo),
            Nota = nota,
            FuenteF3 = fuenteF3
        };

        DetailText.Text = FichaTexto.Render(modelo);
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
                var abrir = _catalogo.Normalizados.TryGetValue(clave, out var proto) ? proto.Acronimo : null;
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
        if (_catalogo.Campos.TryGetValue(p.Acronimo, out var campos) && campos.Count > 0)
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
            var anchoMax = docs.Max(d => d.Doc.Width) * _presentacion.Zoom;
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
                FontSize = _presentacion.FuenteTitulo
            });
            var view = new DiagramView
            {
                Document = doc,
                Nodos = nodosGrafo,
                // Zoom del diagrama como UNIDAD (reflow de texto + factor propio):
                // geometría y texto escalan de verdad vía el contexto de dibujo.
                FactorZoom = _presentacion.Zoom
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
        if (!_catalogo.Normalizados.TryGetValue(Normalizar(acronimo), out var proto)) return;
        RenderFicha(proto);
        if (IsLoaded)
            StatusText.Text = Textos.F("estado.grafo", proto.Acronimo, _catalogo.Relaciones.Count, _presentacion.ZoomTexto);
    }

    /// <summary>Exporta los diagramas de la ficha actual al formato elegido (D4-3):
    /// SVG (vectorial), PNG (raster del mismo renderer) o PDF (vectorial mínimo).
    /// Nombres: NetProtocol-&lt;acrónimo&gt;-&lt;tipo&gt;.&lt;ext&gt;.</summary>
    private async Task ExportarDiagramasAsync()
    {
        if (_presentacion.Seleccionado is null || _docsActuales.Count == 0)
        {
            if (IsLoaded) StatusText.Text = Textos.T("estado.nada.exportar");
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
        var acronimo = _presentacion.Seleccionado.Acronimo;
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
                            if (IsLoaded) StatusText.Text = Textos.F("estado.export.png.invalida", nombre);
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
                if (IsLoaded) StatusText.Text = Textos.F("estado.export.error", nombre, ex.Message);
                return;
            }
        }
        if (IsLoaded)
            StatusText.Text = Textos.F("estado.export.ok", ok, acronimo, formato, carpeta);
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
        catch (Exception ex) { if (IsLoaded) StatusText.Text = Textos.F("estado.captura.error", ex.Message); }
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
                StatusText.Text = Textos.F("estado.muestra.ok", ruta);
        }
        catch (Exception ex)
        {
            if (IsLoaded) StatusText.Text = Textos.F("estado.muestra.error", ex.Message);
        }
    }

    private void MostrarCaptura(PcapCapture captura, string ruta)
    {
        _presentacion.AbrirCaptura(captura.Paquetes, ruta);
        DetailText.IsVisible = false;
        DiagramTitle.IsVisible = false;
        DiagramPanel.IsVisible = false;
        PanelCaptura.IsVisible = true;
        MinWidth = MinAnchoVentana; // la vista de captura no tiene diagramas
        VolverFichaButton.IsVisible = false; // la captura no es una comparación

        ResumenCaptura.Text =
            $"{Path.GetFileName(ruta)} · linktype {captura.LinkType} ({(captura.EsEthernet ? "Ethernet" : "otro")}) · {_presentacion.TotalPaquetes} paquetes";
        ListaPaquetes.Items.Clear();
        for (var i = 0; i < _presentacion.TotalPaquetes; i++)
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
        _presentacion.CerrarCaptura();
    }

    private void CerrarCaptura()
    {
        CerrarCapturaSiAbierta();
        if (_presentacion.Seleccionado is not null) RenderFicha(_presentacion.Seleccionado);
        if (IsLoaded) StatusText.Text = Textos.F("estado.captura.cerrada", _catalogo.Protocolos.Count, _presentacion.ZoomTexto);
    }

    private string ResumenPaquete(int i)
    {
        var p = _presentacion.Paquetes[i];
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
        if (_catalogo.Campos.TryGetValue(acronimoF5, out var campos)) return campos;
        var clave = _catalogo.Campos.Keys.FirstOrDefault(k =>
            string.Equals(k, acronimoF5, StringComparison.OrdinalIgnoreCase) ||
            (acronimoF5 == "IPv4" && k.Equals("IP", StringComparison.OrdinalIgnoreCase)));
        return clave is null ? null : _catalogo.Campos[clave];
    }

    /// <summary>Servicio IANA agrupado: nombre canónico + sus puertos distintos (puerto, transporte).</summary>
    /// <summary>Entity-linking visible (D2-2): si el término es un servicio IANA agrupado,
    /// muestra sus puertos y, si tiene vínculo curado, abre el protocolo del catálogo.</summary>
    private bool MostrarServicioIana(string q)
    {
        var qn = ServiciosDedup.Normalizar(q);
        var svc = _catalogo.Dedup.Servicios.FirstOrDefault(s => ServiciosDedup.Normalizar(s.Nombre) == qn);
        if (svc is null) return false;

        var acronimo = VinculoServicios.AcronimoDe(svc.Nombre);
        _catalogo.Normalizados.TryGetValue(ServiciosDedup.Normalizar(acronimo ?? svc.Nombre), out var proto);

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
            StatusText.Text = Textos.F("estado.iana", q, svc.Nombre, svc.Puertos.Count);
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
            var rel = _catalogo.Relaciones.FirstOrDefault(r =>
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
        var capas = cadena.Select(c => _catalogo.Normalizados.TryGetValue(Normalizar(c), out var pr)
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
        => _catalogo.Normalizados.TryGetValue(Normalizar(v.Nombre), out var proto)
            ? $"{proto.Acronimo}"
            : v.Nombre;

    /// <summary>Puertos IANA deduplicados y enlazados al protocolo (D2-2): unión de
    /// (a) servicios cuyo nombre normalizado coincide con el acrónimo y (b) servicios cuyo
    /// vínculo curado IANA→F3 apunta al protocolo (p. ej. HTTP recoge http, www-http, http-alt).</summary>
    private string PuertosDe(string acronimo) => _catalogo.PuertosDe(acronimo);

    private void CompararConReferencia()
    {
        // Restaura la alineación izquierda por si "Acerca de" la dejó centrada.
        DetailText.TextAlignment = TextAlignment.Left;
        CerrarCapturaSiAbierta(); // la comparación es una vista de ficha: sale de la captura
        _vista = Vista.Comparacion;
        if (_presentacion.Seleccionado is null) return;
        // Referencia elegida por el usuario (ComboBox "Comparar con:"); TCP por defecto,
        // pero cualquier protocolo del catálogo es válido.
        var referencia = CompareTarget.SelectedItem as Protocol
            ?? _catalogo.Protocolos.Values.FirstOrDefault(p => p.Acronimo == "TCP");
        if (referencia is null) return;

        // La comparación es textual: oculta los diagramas individuales del protocolo.
        DiagramPanel.Children.Clear();
        _docsActuales.Clear();
        DiagramTitle.IsVisible = false;
        MinWidth = MinAnchoVentana; // sin diagramas, la ventana vuelve a su mínimo base
        VolverFichaButton.IsVisible = true; // la comparación queda "en pantalla"; hay salida explícita

        var filas = ProtocoloComparador.Comparar(
            new[] { _presentacion.Seleccionado, referencia },
            (nombre, lim) => _servicios.PorNombre(nombre, lim),
            _catalogo.Pdu,
            _catalogo.Cifrado,
            _catalogo.Fichas,
            _catalogo.Relaciones);

        const int anchoCelda = 36;

        // Celda de ancho fijo: trunca con "…" si el valor no cabe (la tabla nunca desborda).
        static string Celda(string s, int ancho)
            => s.Length <= ancho ? s.PadRight(ancho) : s[..(ancho - 1)] + "…";

        // Tabla transpuesta: filas = aspectos cortos, columnas = protocolos.
        var a = filas.First(f => f.Protocolo == _presentacion.Seleccionado.Acronimo);
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
    /// protocolo previo (la comparación nunca cambia _presentacion.Seleccionado, solo oculta la ficha).
    /// El botón "✕ Volver a la ficha" solo es visible mientras la comparación está en
    /// pantalla.</summary>
    private void VolverALaFicha()
    {
        if (_presentacion.Seleccionado is not null)
            RenderFicha(_presentacion.Seleccionado); // vuelve a la ficha del protocolo que se comparaba
        else
        {
            DetailText.Text = Textos.T("estado.seleccione");
            DiagramPanel.IsVisible = true;
        }
        // RenderFicha ya oculta el botón; por si _presentacion.Seleccionado fuera null, se oculta aquí.
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

    /// <summary>
    /// Atajos de teclado de la ventana: Ctrl+«+» y Ctrl+«-» mueven el zoom del contenido y
    /// Ctrl+0 lo devuelve al 100 %. Se comprueba antes del resto de manejadores para que el
    /// atajo no escriba en un campo con el foco; sin Ctrl, la tecla sigue su curso normal.
    /// El zoom por rueda y por teclado comparten la misma ruta de aplicación.
    /// </summary>
    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (AtajoZoom(e.Key, e.KeyModifiers))
        {
            e.Handled = true;
            return;
        }
        base.OnKeyDown(e);
    }

    private bool AtajoZoom(Key tecla, KeyModifiers modificadores)
    {
        if ((modificadores & KeyModifiers.Control) == 0) return false;
        switch (tecla)
        {
            case Key.OemPlus or Key.Add:
                AplicarPasoZoom(PresentacionViewModel.ZoomPaso);
                return true;
            case Key.OemMinus or Key.Subtract:
                AplicarPasoZoom(-PresentacionViewModel.ZoomPaso);
                return true;
            case Key.D0 or Key.NumPad0:
                _presentacion.RestablecerZoom();
                AplicarZoomContenido();
                StatusText.Text = Textos.F("estado.zoom", _presentacion.ZoomTexto);
                return true;
            default:
                return false;
        }
    }

    /// <summary>Aplica una muesca de zoom y refleja el resultado en la barra de estado.</summary>
    private void AplicarPasoZoom(double delta)
    {
        _presentacion.AplicarZoom(delta);
        AplicarZoomContenido();
        StatusText.Text = Textos.F("estado.zoom", _presentacion.ZoomTexto);
    }

    /// <summary>Vuelca el catálogo de textos en los controles con nombre (chrome de la ventana).</summary>
    private void AplicarIdioma()
    {
        Title = Textos.T("app.titulo");
        SearchBox.PlaceholderText = Textos.T("buscar.placeholder");
        SearchButton.Content = Textos.T("buscar.boton");
        LblProtocolo.Text = Textos.T("etiqueta.protocolo");
        LblComparar.Text = Textos.T("etiqueta.comparar");
        CompareButton.Content = Textos.T("boton.comparar");
        VolverFichaButton.Content = Textos.T("boton.volver.ficha");
        LblFamilia.Text = Textos.T("etiqueta.familia");
        LblEstado.Text = Textos.T("etiqueta.estado");
        LblExportar.Text = Textos.T("etiqueta.exportar");
        ExportButton.Content = Textos.T("boton.exportar");
        AbrirCapturaButton.Content = Textos.T("boton.abrir.captura");
        MuestraButton.Content = Textos.T("boton.muestra");
        LanguageButton.Content = Textos.T("boton.idioma");
        NavFilter.PlaceholderText = Textos.T("nav.filtro");
        LblAcciones.Text = Textos.T("sidebar.acciones");
        LblTema.Text = Textos.T("sidebar.tema");
        LblLeyenda.Text = Textos.T("sidebar.leyenda");
        LblAcerca.Text = Textos.T("sidebar.acerca");
        DiagramTitle.Text = Textos.T("diagramas.titulo");
        CerrarCapturaButton.Content = Textos.T("boton.cerrar.captura");

        // Nombres accesibles: sin ellos un lector de pantalla anuncia botones vacíos (los
        // glifos de ventana) o campos sin propósito. Se aplican aquí para que sigan al
        // idioma activo igual que el resto del chrome.
        AutomationProperties.SetName(MinimizeButton, Textos.T("acc.ventana.minimizar"));
        AutomationProperties.SetName(MaximizeButton, Textos.T("acc.ventana.maximizar"));
        AutomationProperties.SetName(CloseButton, Textos.T("acc.ventana.cerrar"));
        ToolTip.SetTip(MinimizeButton, Textos.T("acc.ventana.minimizar"));
        ToolTip.SetTip(MaximizeButton, Textos.T("acc.ventana.maximizar"));
        ToolTip.SetTip(CloseButton, Textos.T("acc.ventana.cerrar"));

        AutomationProperties.SetName(SearchBox, Textos.T("acc.busqueda"));
        AutomationProperties.SetName(SearchButton, Textos.T("buscar.boton"));
        AutomationProperties.SetName(ProtocolSelector, Textos.T("acc.protocolo"));
        AutomationProperties.SetName(CompareTarget, Textos.T("acc.comparar.con"));
        AutomationProperties.SetName(CompareButton, Textos.T("boton.comparar"));
        AutomationProperties.SetHelpText(CompareButton, Textos.T("help.comparar"));
        AutomationProperties.SetName(VolverFichaButton, Textos.T("boton.volver.ficha"));
        AutomationProperties.SetName(FilterFamilia, Textos.T("acc.familia"));
        AutomationProperties.SetName(FilterEstado, Textos.T("acc.estado"));
        AutomationProperties.SetName(ExportFormat, Textos.T("acc.formato.exportacion"));
        AutomationProperties.SetName(ExportButton, Textos.T("boton.exportar"));
        AutomationProperties.SetHelpText(ExportButton, Textos.T("help.exportar"));
        AutomationProperties.SetName(AbrirCapturaButton, Textos.T("boton.abrir.captura"));
        AutomationProperties.SetHelpText(AbrirCapturaButton, Textos.T("help.abrir.captura"));
        AutomationProperties.SetName(MuestraButton, Textos.T("boton.muestra"));
        AutomationProperties.SetHelpText(MuestraButton, Textos.T("help.muestra"));
        AutomationProperties.SetName(LanguageButton, Textos.T("acc.idioma"));
        AutomationProperties.SetName(NavFilter, Textos.T("acc.filtro.familias"));
        AutomationProperties.SetName(ThemeButton, Textos.T("sidebar.tema"));
        AutomationProperties.SetName(LegendButton, Textos.T("sidebar.leyenda"));
        AutomationProperties.SetName(AboutButton, Textos.T("sidebar.acerca"));
        AutomationProperties.SetName(DetailText, Textos.T("acc.ficha"));
        AutomationProperties.SetName(DiagramTitle, Textos.T("acc.diagramas"));
        AutomationProperties.SetName(StatusText, Textos.T("acc.estado.barra"));
        AutomationProperties.SetName(ResumenCaptura, Textos.T("acc.captura.resumen"));
        AutomationProperties.SetName(ListaPaquetes, Textos.T("acc.captura.paquetes"));
        AutomationProperties.SetName(DetalleCaptura, Textos.T("acc.captura.detalle"));
        AutomationProperties.SetName(CerrarCapturaButton, Textos.T("boton.cerrar.captura"));
    }

    /// <summary>Alterna es/en, reaplica el chrome y repinta la MISMA vista en pantalla.</summary>
    private void AlternarIdioma()
    {
        Textos.Alternar();
        AplicarIdioma();
        // La captura abierta es una vista de datos: se deja tal cual y solo cambia el chrome.
        if (PanelCaptura.IsVisible) return;
        switch (_vista)
        {
            case Vista.Leyenda: MostrarLeyenda(); break;
            case Vista.Acerca: MostrarAcercaDe(); break;
            case Vista.Comparacion: CompararConReferencia(); break;
            default: RenderFicha(_presentacion.Seleccionado); break;
        }
    }

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
        _vista = Vista.Leyenda;
        // Visto textual: oculta los diagramas del protocolo previamente seleccionado.
        DiagramPanel.Children.Clear();
        _docsActuales.Clear();
        DiagramTitle.IsVisible = false;
        MinWidth = MinAnchoVentana; // sin diagramas, la ventana vuelve a su mínimo base
        VolverFichaButton.IsVisible = false; // la leyenda no es una comparación

        var sb = new StringBuilder();
        sb.AppendLine(Textos.T("leyenda.titulo"));
        sb.AppendLine();
        sb.AppendLine(Textos.T("leyenda.intro"));
        sb.AppendLine();

        foreach (var familia in Enum.GetValues<FamiliaProtocolo>()
                     .Cast<FamiliaProtocolo>()
                     .OrderBy(f => f.ToString(), StringComparer.OrdinalIgnoreCase))
        {
            var acronimo = familia.ToString();
            var descripcion = _catalogo.Familias.TryGetValue(acronimo, out var d) ? d : "[descripción en F3]";
            var ejemplos = _catalogo.Protocolos.Values
                .Where(p => p.Familia == familia)
                .OrderBy(p => p.Acronimo, StringComparer.OrdinalIgnoreCase)
                .Take(5)
                .Select(p => p.Acronimo);
            var ejemploTexto = ejemplos.Count() == 0 ? "" : Textos.F("leyenda.ejemplos", string.Join(", ", ejemplos));
            sb.AppendLine($"  {acronimo}: {descripcion}{ejemploTexto}");
        }

        sb.AppendLine();
        sb.AppendLine(Textos.F("leyenda.fuente", _catalogo.Protocolos.Count, _catalogo.Familias.Count));
        DetailText.Text = sb.ToString();

        if (IsLoaded)
            StatusText.Text = Textos.F("estado.leyenda", _presentacion.ZoomTexto);
    }

    /// <summary>Sección "Acerca de" con los datos de la aplicación, centrados y formateados.</summary>
    private void MostrarAcercaDe()
    {
        // Visto textual: oculta los diagramas del protocolo previamente seleccionado.
        CerrarCapturaSiAbierta(); // "Acerca de" es una vista de ficha: sale de la captura
        _vista = Vista.Acerca;
        DiagramPanel.Children.Clear();
        _docsActuales.Clear();
        DiagramTitle.IsVisible = false;
        MinWidth = MinAnchoVentana; // sin diagramas, la ventana vuelve a su mínimo base
        VolverFichaButton.IsVisible = false; // "Acerca de" no es una comparación

        var sb = new StringBuilder();
        sb.AppendLine();
        sb.AppendLine(Textos.T("acerca.titulo"));
        sb.AppendLine();
        sb.AppendLine(Textos.T("acerca.nombre"));
        sb.AppendLine();
        sb.AppendLine(Textos.F("acerca.version", Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "1.0.0"));
        sb.AppendLine();
        sb.AppendLine(Textos.T("acerca.autor"));
        sb.AppendLine();
        sb.AppendLine(Textos.T("acerca.email"));
        sb.AppendLine();
        sb.AppendLine();
        sb.AppendLine(Textos.T("acerca.derechos"));
        DetailText.Text = sb.ToString();
        DetailText.TextAlignment = TextAlignment.Center;

        if (IsLoaded)
            StatusText.Text = Textos.F("estado.acerca", _presentacion.ZoomTexto);
    }

    private static string Normalizar(string s)
        => CatalogoViewModel.Normalizar(s);

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
        var nombre = _catalogo.Normalizados.TryGetValue(Normalizar(v.Nombre), out var proto)
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