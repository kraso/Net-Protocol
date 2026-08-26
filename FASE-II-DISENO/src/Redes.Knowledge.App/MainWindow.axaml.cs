using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Styling;
using Redes.Knowledge.Domain;
using Redes.Knowledge.Infrastructure;
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
    private const double ZoomMin = 0.7, ZoomMax = 2.5;
    private const double ListaAltura = 240;
    private const double ListaAnchura = 306; // idéntica para todos los grupos (340 - márgenes - scrollbar)

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

        var raiz = RaizDelRepositorio();
        var dirDb = Path.Combine(raiz, "FASE-II-DISENO", "run");
        Directory.CreateDirectory(dirDb);
        var store = new SqliteKnowledgeStore($"Data Source={Path.Combine(dirDb, "knowledge.db")};Pooling=False");

        _repo = new SqliteProtocolRepository(store);
        _servicios = new SqliteServiceRepository(store);
        _busqueda = new SqliteSearchEngine(store);

        var importados = DatasetBootstrap.EnsureProtocolos(store,
            Path.Combine(raiz, "FASE-03-INVENTARIO", "F3-Protocolos.json"));
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
        LegendButton.Click += (_, _) => MostrarLeyenda();
        FilterFamilia.SelectionChanged += (_, _) => { if (!_cargando) ReconstruirNavegacion(); };
        FilterEstado.SelectionChanged += (_, _) => { if (!_cargando) ReconstruirNavegacion(); };
        NavFilter.TextChanged += (_, _) => { if (!_cargando) ReconstruirNavegacion(); };

        // Zoom global del tamaño de letra: Ctrl + rueda del ratón en toda la interfaz.
        AddHandler(InputElement.PointerWheelChangedEvent,
            (_, e) =>
            {
                if ((e.KeyModifiers & KeyModifiers.Control) == 0) return;
                _zoom = Math.Clamp(_zoom + (e.Delta.Y > 0 ? 0.1 : -0.1), ZoomMin, ZoomMax);
                if (Content is Control root)
                {
                    // Avalonia no expone LayoutTransform: se aplica escala de render con origen arriba-izquierda.
                    root.RenderTransform = new ScaleTransform(_zoom, _zoom);
                    root.RenderTransformOrigin = new RelativePoint(0, 0, RelativeUnit.Relative);
                }
                StatusText.Text = $"Zoom: {_zoom * 100:0}% — Ctrl+Scroll ajusta el tamaño de letra en toda la interfaz";
                e.Handled = true;
            },
            RoutingStrategies.Bubble,
            handledEventsToo: true);
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
        // Muestra "ACR · Nombre" en el desplegable (ancho suficiente para los 113).
        // IMPORTANTE: el data template se invoca con null al reciclar contenedores
        // del popup al abrir; sin la guarda, p.Acronimo lanza NullReferenceException.
        CompareTarget.ItemTemplate = new FuncDataTemplate<Protocol>((p, _) =>
            p is null ? null : new TextBlock
            {
                Text = $"{p.Acronimo} · {p.Nombre}",
                TextWrapping = TextWrapping.Wrap
            });
        CompareTarget.SelectedItem = _protocolos.Values.FirstOrDefault(p => p.Acronimo == "TCP");
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
            grupos = grupos.Where(g =>
                g.Key.ToString().ToLowerInvariant().Contains(texto) ||
                g.Any(p => p.Acronimo.ToLowerInvariant().Contains(texto) ||
                           p.Nombre.ToLowerInvariant().Contains(texto)));
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
        if (p is null) { DetailText.Text = "Seleccione un protocolo."; return; }

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
        DiagramPanel.Children.Clear();
        var docs = new List<(string Titulo, DiagramDocument Doc)>();

        // 1) Pila de encapsulación: cadena de "corre sobre" desde el medio hacia el protocolo.
        var pila = PilaDeEncapsulacion(p.Acronimo);
        if (pila is not null)
            docs.Add(("Pila de encapsulación (F4)", pila));

        // 2) Grafo de vecinos a 1 salto (sin texto en las aristas; color por tipo + leyenda).
        if (vecinos.Count > 0)
        {
            var nodos = new List<(string Nodo, string Etiqueta)>
            {
                (p.Acronimo, p.Acronimo)
            };
            foreach (var v in vecinos)
                nodos.Add((Normalizar(v.Nombre), EtiquetaNodo(v)));
            var aristas = new List<(string A, string B, string Etiqueta)>();
            foreach (var v in vecinos)
                aristas.Add((p.Acronimo, Normalizar(v.Nombre), TipoRelacion(v.Tipo)));
            var doc = Layouts.Grafo(
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
            docs.Add(("Grafo de vecinos a 1 salto (F4)", doc));
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
                    $"Cabecera {p.Acronimo} — campos F5", wire)));
        }

        if (docs.Count == 0)
        {
            DiagramTitle.IsVisible = false;
            return;
        }

        DiagramTitle.IsVisible = true;
        foreach (var (titulo, doc) in docs)
        {
            var panel = new StackPanel { Spacing = 4 };
            // Título hereda el color del tema (sin color fijo) y es seleccionable.
            panel.Children.Add(new SelectableTextBlock
            {
                Text = titulo,
                FontWeight = Avalonia.Media.FontWeight.SemiBold
            });
            panel.Children.Add(new DiagramView { Document = doc });
            DiagramPanel.Children.Add(panel);
        }
    }

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

    private string PuertosDe(string acronimo)
    {
        var servicios = _servicios.PorNombre(acronimo.ToLowerInvariant(), 5);
        return servicios.Count == 0
            ? "—"
            : string.Join(", ", servicios.Where(s => s.Port.HasValue).Select(s => $"{s.Port}/{s.Transport}"));
    }

    private void CompararConReferencia()
    {
        if (_seleccionado is null) return;
        // Referencia elegida por el usuario (ComboBox "Comparar con:"); TCP por defecto,
        // pero cualquier protocolo del catálogo es válido.
        var referencia = CompareTarget.SelectedItem as Protocol
            ?? _protocolos.Values.FirstOrDefault(p => p.Acronimo == "TCP");
        if (referencia is null) return;

        // La comparación es textual: oculta los diagramas individuales del protocolo.
        DiagramPanel.Children.Clear();
        DiagramTitle.IsVisible = false;

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

    private void TitleBar_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        // Arrastre de la ventana desde la barra de título personalizada (SystemDecorations=None).
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            BeginMoveDrag(e);
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
    }

    /// <summary>Leyenda de las 13 familias: acrónimo → descripción escueta + ejemplos del catálogo.</summary>
    private void MostrarLeyenda()
    {
        // Visto textual: oculta los diagramas del protocolo previamente seleccionado.
        DiagramPanel.Children.Clear();
        DiagramTitle.IsVisible = false;

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

    private static string RaizDelRepositorio()
    {
        var d = new DirectoryInfo(AppContext.BaseDirectory);
        while (d is not null && !File.Exists(Path.Combine(d.FullName, "PLANREDES.md")))
            d = d.Parent;
        return d?.FullName ?? throw new InvalidOperationException("No se encontró la raíz del repositorio.");
    }
}