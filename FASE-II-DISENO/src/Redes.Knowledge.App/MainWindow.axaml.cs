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
    private enum Vista { Ficha, Leyenda, Acerca, Comparacion, SinResultados }

    private Vista _vista = Vista.Ficha;

    /// <summary>Término de la última búsqueda sin resultados, para repintar su aviso.</summary>
    private string _busquedaVacia = "";

    // Texto de los popups de información: se conserva la referencia para poder traducirlo
    // cuando cambia el idioma (los popups se construyen una sola vez).
    private TextBlock? _popupComparar;
    private TextBlock? _popupExportar;
    private TextBlock? _popupAbrirCaptura;
    private TextBlock? _popupMuestra;

    // Captura en pantalla: se guarda para poder repintar sus textos al cambiar de idioma.
    private PcapCapture? _captura;

    /// <summary>Última comprobación con versión nueva, para poder descargar su instalador.</summary>
    private ResultadoActualizacion? _actualizacion;

    // Popups de información de los botones de acción y su supervisión global (deadline,
    // geometría del puntero y gracia anti-reabrir). Ver <see cref="SupervisorPopups"/>.
    private readonly SupervisorPopups _popups;

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

        // Catálogo, base local y bootstrap de F3: todo el arranque vive en ArranqueCatalogo,
        // fuera de la ventana (ver ArranqueCatalogo.cs).
        var arranque = ArranqueCatalogo.Cargar();
        _repo = arranque.Repositorio;
        _servicios = arranque.Servicios;
        _busqueda = arranque.Busqueda;
        _catalogo = arranque.Catalogo;
        var importados = arranque.Importados;

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
        DownloadButton.Click += async (_, _) => await DescargarActualizacionAsync();
        // Tooltip de "Muestra de prueba": el contenido (frase + carpeta REAL de capturas) y
        // Popups de información de los botones de acción (Comparar, Exportar, Abrir captura,
        // Muestra de prueba), en sustitución de los tooltips nativos: estilo cristalino y
        // cierre determinista por deadline (ver ConfigurarPopupsBotones).
        _popups = new SupervisorPopups(this);
        _popups.Iniciar();
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
            (_, e) => _popups.RegistrarPuntero(e.GetPosition(this)),
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
            _actualizacion = resultado;
            UpdateText.Text = Textos.F("estado.actualizacion", resultado.VersionDisponible, resultado.UrlDescarga);
            UpdateText.IsVisible = true;
            // El botón solo aparece si hay instalador para este sistema (S2).
            DownloadButton.IsVisible = resultado.NombreAsset is not null;
        }
        catch
        {
            // La comprobación de actualizaciones nunca debe afectar al arranque ni a la UI.
        }
    }

    /// <summary>
    /// Descarga el instalador de la versión nueva y comprueba su SHA256. Nunca lo ejecuta:
    /// deja la ruta en el aviso y el usuario decide si lo abre (S2/T3 de v1.1).
    /// </summary>
    private async Task DescargarActualizacionAsync()
    {
        if (_actualizacion is not { } actualizacion) return;

        DownloadButton.IsEnabled = false;
        try
        {
            // Cliente propio: el de metadatos corta a los 5 s y el instalador pesa ~100 MB.
            using var http = DescargadorActualizacion.CrearCliente();
            UpdateText.Text = Textos.F("estado.descarga.progreso", actualizacion.NombreAsset ?? "");
            UpdateText.IsVisible = true;

            var resultado = await ActualizadorInstalador
                .DescargarYVerificarAsync(http, actualizacion)
                .ConfigureAwait(true);

            UpdateText.Text = MensajeDescarga(resultado);
            UpdateText.IsVisible = true;
        }
        catch (Exception ex)
        {
            // Ningún fallo de una actualización puede tumbar la ventana.
            UpdateText.Text = Textos.F("estado.descarga.error", ex.Message);
            UpdateText.IsVisible = true;
        }
        finally
        {
            DownloadButton.IsEnabled = true;
        }
    }

    /// <summary>Traduce el resultado de la descarga al aviso de la ventana.</summary>
    private static string MensajeDescarga(ResultadoDescarga resultado) => resultado.Estado switch
    {
        EstadoDescarga.Verificado => Textos.F("estado.descarga.ok", resultado.Ruta),
        EstadoDescarga.HashInvalido => Textos.T("estado.descarga.hash"),
        EstadoDescarga.SinSumas => Textos.T("estado.descarga.sin.sumas"),
        EstadoDescarga.SinAsset => Textos.T("estado.descarga.sin.asset"),
        _ => Textos.F("estado.descarga.error", resultado.Detalle ?? "")
    };

    /// <summary>Configura los popups de información de los 4 botones de acción (Comparar,
    /// Exportar, Abrir captura, Muestra de prueba), sustituyendo a los tooltips nativos.</summary>
    private void ConfigurarPopupsBotones()
    {
        _popupComparar = new TextBlock
        {
            Text = Textos.T("help.comparar"),
            TextWrapping = TextWrapping.Wrap,
            Foreground = SupervisorPopups.TextoPopup
        };
        _popups.Registrar(CompareButton, _popupComparar);

        _popupExportar = new TextBlock
        {
            Text = Textos.T("help.exportar"),
            TextWrapping = TextWrapping.Wrap,
            Foreground = SupervisorPopups.TextoPopup
        };
        _popups.Registrar(ExportButton, _popupExportar);

        _popupAbrirCaptura = new TextBlock
        {
            Text = Textos.T("help.abrir.captura"),
            TextWrapping = TextWrapping.Wrap,
            Foreground = SupervisorPopups.TextoPopup
        };
        _popups.Registrar(AbrirCapturaButton, _popupAbrirCaptura);

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
            Text = Textos.T("help.muestra"),
            TextWrapping = TextWrapping.Wrap,
            Foreground = SupervisorPopups.TextoPopup
        };
        _popupMuestra = intro;
        var enlace = new TextBlock
        {
            Text = ruta,
            TextWrapping = TextWrapping.Wrap,
            TextDecorations = TextDecorations.Underline,
            Foreground = SupervisorPopups.EnlacePopup,
            Cursor = new Cursor(StandardCursorType.Hand)
        };
        var contenidoMuestra = new StackPanel { Spacing = 2, Children = { intro, enlace } };
        _popups.Registrar(MuestraButton, contenidoMuestra, () => AbrirCapturaEnExplorador(ruta));
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
        // El valor del enum viaja en Tag y el texto visible en Content: traducir el nombre
        // no puede romper el filtro, que sigue leyendo el enum.
        FilterFamilia.Items.Clear();
        FilterFamilia.Items.Add(new ComboBoxItem { Content = Textos.T("filtro.todas.familias"), Tag = null });
        foreach (FamiliaProtocolo f in Enum.GetValues<FamiliaProtocolo>())
            FilterFamilia.Items.Add(new ComboBoxItem { Content = f.ToString(), Tag = f });
        FilterFamilia.SelectedIndex = 0;

        FilterEstado.Items.Clear();
        FilterEstado.Items.Add(new ComboBoxItem { Content = Textos.T("filtro.todos.estados"), Tag = null });
        foreach (LifecycleState e in Enum.GetValues<LifecycleState>())
            FilterEstado.Items.Add(new ComboBoxItem { Content = Textos.Estado(e), Tag = e });
        FilterEstado.SelectedIndex = 0;
        _cargando = false;
    }

    /// <summary>Reescribe las etiquetas de los desplegables conservando la selección.</summary>
    private void RefrescarFiltros()
    {
        if (FilterFamilia.ItemCount > 0)
        {
            ((ComboBoxItem)FilterFamilia.Items[0]!).Content = Textos.T("filtro.todas.familias");
            var i = 1;
            foreach (FamiliaProtocolo f in Enum.GetValues<FamiliaProtocolo>())
                if (i < FilterFamilia.ItemCount) ((ComboBoxItem)FilterFamilia.Items[i++]!).Content = f.ToString();
        }

        if (FilterEstado.ItemCount > 0)
        {
            ((ComboBoxItem)FilterEstado.Items[0]!).Content = Textos.T("filtro.todos.estados");
            var i = 1;
            foreach (LifecycleState e in Enum.GetValues<LifecycleState>())
                if (i < FilterEstado.ItemCount) ((ComboBoxItem)FilterEstado.Items[i++]!).Content = Textos.Estado(e);
        }
    }

    /// <summary>Rehace el contenido de los ítems de navegación sin tocar expanders ni selección.</summary>
    private void RefrescarNavegacion()
    {
        foreach (var expander in NavPanel.Children.OfType<Expander>())
            if (expander.Content is ListBox lista)
                foreach (var item in lista.Items.OfType<ListBoxItem>())
                    if (item.Tag is Protocol p) item.Content = ItemProtocolo(p);
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
                FilterFamilia.SelectedItem is ComboBoxItem { Tag: FamiliaProtocolo f } ? f : null;
            LifecycleState? estado = FilterEstado.SelectedIndex > 0 &&
                FilterEstado.SelectedItem is ComboBoxItem { Tag: LifecycleState e } ? e : null;
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
            AutomationProperties.SetName(lista, Textos.T("acc.lista.protocolos"));
            foreach (var p in g.OrderBy(p => p.Acronimo, StringComparer.OrdinalIgnoreCase))
            {
                var item = new ListBoxItem { Tag = p, Content = ItemProtocolo(p) };
                AutomationProperties.SetName(item, $"{p.Acronimo} · {p.Nombre}");
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
            _vista = Vista.SinResultados;
            _busquedaVacia = q;
            DetailText.Text = Textos.T("estado.sin.resultados");
            StatusText.Text = Textos.F("estado.busqueda.cero", q);
            return;
        }

        NavPanel.Children.Clear();
        var lista = new ListBox { Height = ListaAltura, Width = ListaAnchura };
        AutomationProperties.SetName(lista, Textos.T("acc.lista.protocolos"));
        foreach (var p in encontrados.OrderBy(p => p.Acronimo, StringComparer.OrdinalIgnoreCase))
        {
            var item = new ListBoxItem { Content = ItemProtocolo(p), Tag = p };
            AutomationProperties.SetName(item, $"{p.Acronimo} · {p.Nombre}");
            lista.Items.Add(item);
        }
        lista.SelectionChanged += (_, _) =>
        {
            if (lista.SelectedItem is ListBoxItem sel && sel.Tag is Protocol proto) RenderFicha(proto);
        };
        NavPanel.Children.Add(new Expander
        {
            Header = Textos.F("nav.resultados", encontrados.Count),
            Tag = encontrados.Count,
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
        _captura = captura;
        DetailText.IsVisible = false;
        DiagramTitle.IsVisible = false;
        DiagramPanel.IsVisible = false;
        PanelCaptura.IsVisible = true;
        MinWidth = MinAnchoVentana; // la vista de captura no tiene diagramas
        VolverFichaButton.IsVisible = false; // la captura no es una comparación

        PintarCaptura();
    }

    /// <summary>
    /// Vuelca en la vista de captura el resumen, la lista de paquetes y (vía selección) el
    /// detalle. Se usa al abrir y al cambiar de idioma, para que no quede texto en el idioma
    /// anterior; conserva el paquete seleccionado.
    /// </summary>
    private void PintarCaptura()
    {
        if (_captura is not { } captura) return;

        ResumenCaptura.Text = Textos.F(
            "cap.resumen",
            Path.GetFileName(_presentacion.RutaCaptura),
            captura.LinkType,
            captura.EsEthernet ? "Ethernet" : Textos.T("cap.otro"),
            _presentacion.TotalPaquetes);

        var seleccionado = ListaPaquetes.SelectedIndex;
        ListaPaquetes.Items.Clear();
        for (var i = 0; i < _presentacion.TotalPaquetes; i++)
        {
            var resumen = ResumenPaquete(i);
            var item = new ListBoxItem { Content = resumen, Tag = i };
            AutomationProperties.SetName(item, resumen);
            ListaPaquetes.Items.Add(item);
        }

        if (seleccionado >= 0 && seleccionado < ListaPaquetes.ItemCount)
            ListaPaquetes.SelectedIndex = seleccionado;
        else if (ListaPaquetes.ItemCount > 0)
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
        _captura = null;
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
            : d.ProtocoloIp is { } pp ? $"IP proto {pp}" : Textos.T("cap.otro");
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
        sb.AppendLine(Textos.F("cap.paquete", f.Length));
        if (d.EsEthernet)
        {
            sb.AppendLine($"Ethernet II: {Mac(f, 0)} → {Mac(f, 6)} · EtherType 0x{(d.EtherType ?? 0):X4}");
            if (d.EsIpv4) sb.AppendLine($"IPv4: {d.IpOrigen} → {d.IpDestino} · TTL {f[22]} · proto {d.ProtocoloIp}");
            if (d.EsTcp) sb.AppendLine($"TCP: {d.PuertoOrigen} → {d.PuertoDestino}");
            if (d.EtherType == 0x86DD) sb.AppendLine(Textos.T("cap.ipv6.f5"));
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
            return "   " + Textos.F("cap.sin.layout", acronimoF5);
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
        sb.AppendLine(Textos.F("iana.titulo", svc.Nombre));
        sb.AppendLine(Textos.F("iana.puertos", string.Join(", ", svc.Puertos.Select(p => $"{p.Puerto}/{p.Transporte}"))));
        if (proto is not null)
            sb.AppendLine(Textos.F("iana.vinculo", proto.Nombre, proto.Acronimo));
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

        // Tabla transpuesta: filas = aspectos cortos, columnas = protocolos.
        var a = filas.First(f => f.Protocolo == _presentacion.Seleccionado.Acronimo);
        var b = filas.First(f => f.Protocolo == referencia.Acronimo);
        var cortas = new[]
        {
            new ComparadorTexto.Fila(Textos.T("cmp.familia"), a.Familia, b.Familia),
            new ComparadorTexto.Fila(Textos.T("cmp.estado"), Textos.Estado(a.Estado), Textos.Estado(b.Estado)),
            new ComparadorTexto.Fila(Textos.T("cmp.capas"), a.Capas, b.Capas),
            new ComparadorTexto.Fila(Textos.T("cmp.pdu"), a.Pdu, b.Pdu),
            new ComparadorTexto.Fila(Textos.T("cmp.puertos"), a.Puertos, b.Puertos),
            new ComparadorTexto.Fila(Textos.T("cmp.cifrado"), a.Cifrado, b.Cifrado)
        };
        // Campos largos: fuera de la tabla, en líneas envueltas por protocolo.
        var largas = new[]
        {
            new ComparadorTexto.Fila(Textos.T("cmp.finalidad"),
                ComparadorTexto.Acortar(a.Finalidad), ComparadorTexto.Acortar(b.Finalidad)),
            new ComparadorTexto.Fila(Textos.T("cmp.encapsulacion"),
                ComparadorTexto.Acortar(a.Encapsulacion), ComparadorTexto.Acortar(b.Encapsulacion))
        };

        DetailText.Text = ComparadorTexto.Construir(a.Protocolo, b.Protocolo, cortas, largas);
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
        AplicarTextosChrome();
        AccesibilidadVentana.Aplicar(this);
        RefrescarFiltros();
        AplicarTextosPopups();
    }

    /// <summary>Texto visible de los controles con nombre: título, barra superior y sidebar.</summary>
    private void AplicarTextosChrome()
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
        DownloadButton.Content = Textos.T("boton.descargar");
        LanguageButton.Content = Textos.T("boton.idioma");
        NavFilter.PlaceholderText = Textos.T("nav.filtro");
        LblAcciones.Text = Textos.T("sidebar.acciones");
        LblTema.Text = Textos.T("sidebar.tema");
        LblLeyenda.Text = Textos.T("sidebar.leyenda");
        LblAcerca.Text = Textos.T("sidebar.acerca");
        DiagramTitle.Text = Textos.T("diagramas.titulo");
        CerrarCapturaButton.Content = Textos.T("boton.cerrar.captura");
    }

    /// <summary>Popups de información: se construyen una sola vez, así que su texto se refresca aquí.</summary>
    private void AplicarTextosPopups()
    {
        if (_popupComparar is not null) _popupComparar.Text = Textos.T("help.comparar");
        if (_popupExportar is not null) _popupExportar.Text = Textos.T("help.exportar");
        if (_popupAbrirCaptura is not null) _popupAbrirCaptura.Text = Textos.T("help.abrir.captura");
        if (_popupMuestra is not null) _popupMuestra.Text = Textos.T("help.muestra");
    }

    /// <summary>Alterna es/en, reaplica el chrome y repinta la MISMA vista en pantalla.</summary>
    private void AlternarIdioma()
    {
        Textos.Alternar();
        AplicarIdioma();

        // Ítems de la navegación: se rehacen en su sitio (conserva expanders y selección).
        RefrescarNavegacion();

        // La captura ya se puede traducir (resumen, lista y detalle): se repinta en su sitio
        // en vez de dejar el texto en el idioma anterior.
        if (PanelCaptura.IsVisible)
        {
            PintarCaptura();
            return;
        }

        // Cabeceras de resultados de búsqueda: se actualizan sin repetir la búsqueda.
        foreach (var expander in NavPanel.Children.OfType<Expander>())
            if (expander.Tag is int encontrados)
                expander.Header = Textos.F("nav.resultados", encontrados);

        switch (_vista)
        {
            case Vista.Leyenda: MostrarLeyenda(); break;
            case Vista.Acerca: MostrarAcercaDe(); break;
            case Vista.Comparacion: CompararConReferencia(); break;
            case Vista.SinResultados:
                // Se repinta el aviso, no la ficha anterior: la búsqueda sin resultados es
                // también una vista en pantalla.
                DetailText.Text = Textos.T("estado.sin.resultados");
                StatusText.Text = Textos.F("estado.busqueda.cero", _busquedaVacia);
                break;
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
            Text = $"{p.Familia} · {Textos.Estado(p.Estado)}",
            FontSize = 11,
            Opacity = 0.65
        });
        return panel;
    }

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
}