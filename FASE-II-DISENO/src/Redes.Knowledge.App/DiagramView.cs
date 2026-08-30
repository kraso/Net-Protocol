using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Styling;
using Redes.Knowledge.Visualization;

namespace Redes.Knowledge.App;

/// <summary>
/// Renderiza un DiagramDocument (primitivas Rect/Line/Text) con DrawingContext.
/// ADR-003: el modelo no conoce el renderer; este control es el renderer Avalonia.
/// Respeta el Fill/Stroke de cada primitiva y aplica una paleta por tipo de diagrama:
/// pila (capas), grafo (semilla/vecinos/aristas por tipo), wire format (campos).
/// Paleta POR TEMA: claro (fondo blanco, pastel) y oscuro (fondo pizarra, cajas slate);
/// el texto "por defecto" del layout (#0f172a) se reinterpreta con el color del tema.
/// </summary>
public sealed class DiagramView : Control
{
    // La misma tipografía global de la app (embebida, portátil). El nombre de familia
    // real del TTF es "JetBrainsMono NFM" (Nerd Font Mono).
    private static readonly FontFamily Fuente =
        FontFamily.Parse("avares://NetProtocol/Assets/Fonts/JetBrainsMonoNerdFontMono-SemiBold.ttf#JetBrainsMono NFM");

    private static readonly Typeface TypefaceUi =
        new(Fuente, FontStyle.Normal, FontWeight.Normal);
    public static readonly StyledProperty<DiagramDocument?> DocumentProperty =
        AvaloniaProperty.Register<DiagramView, DiagramDocument?>(nameof(Document));

    public DiagramDocument? Document
    {
        get => GetValue(DocumentProperty);
        set => SetValue(DocumentProperty, value);
    }

    // Zoom del diagrama como UNIDAD (diseño UX adoptado: "reflow de texto + factor
    // propio en los diagramas"). La escala se aplica al contexto de dibujo (PushTransform,
    // igual que el PNG 2× del exporter), de modo que geometría y texto escalan de verdad,
    // y MeasureOverride devuelve el tamaño escalado para que el ScrollViewer horizontal
    // que envuelve al diagrama sepa cuándo mostrar su barra. El hit-testing del grafo
    // compensa el factor (coordenadas de documento fieles a cualquier zoom).
    public static readonly StyledProperty<double> FactorZoomProperty =
        AvaloniaProperty.Register<DiagramView, double>(nameof(FactorZoom), 1.0);

    public double FactorZoom
    {
        get => GetValue(FactorZoomProperty);
        set => SetValue(FactorZoomProperty, value);
    }

    // Nodos del grafo (D5-1): rectángulos en coordenadas de documento para hit-testing
    // de navegación. Solo los diagramas de tipo grafo los aportan.
    public static readonly StyledProperty<IReadOnlyList<NodoGrafo>?> NodosProperty =
        AvaloniaProperty.Register<DiagramView, IReadOnlyList<NodoGrafo>?>(nameof(Nodos));

    public IReadOnlyList<NodoGrafo>? Nodos
    {
        get => GetValue(NodosProperty);
        set => SetValue(NodosProperty, value);
    }

    /// <summary>Se dispara al pulsar (botón izquierdo) sobre un nodo del grafo;
    /// el argumento es la clave (Nodo) del rectángulo pulsado.</summary>
    public event Action<string>? NodoPulsado;

    private static readonly Cursor CursorMano = new(StandardCursorType.Hand);
    private static readonly Cursor CursorFlecha = new(StandardCursorType.Arrow);

    private double FactorReal => Math.Max(0.1, FactorZoom);

    private NodoGrafo? NodoEn(Point p)
    {
        var nodos = Nodos;
        if (nodos is null) return null;
        // El punto viene en coordenadas del control (ya con el zoom aplicado); los
        // rectángulos de nodo viven en coordenadas de documento → se compensa el factor.
        var f = FactorReal;
        var px = p.X / f;
        var py = p.Y / f;
        foreach (var n in nodos)
        {
            if (px >= n.X && px <= n.X + n.W &&
                py >= n.Y && py <= n.Y + n.H)
                return n;
        }
        return null;
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        // Las coordenadas de GetPosition(this) ya compensan el RenderTransform del zoom:
        // el hit-testing contra los rectángulos de documento es fiel a cualquier zoom.
        Cursor = NodoEn(e.GetPosition(this)) is null ? CursorFlecha : CursorMano;
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) return;
        if (NodoEn(e.GetPosition(this)) is { } nodo)
            NodoPulsado?.Invoke(nodo.Clave);
    }

    // Paleta POR TEMA (estudio F2I-UX-Colorido-y-Estetica): los diagramas ya no son
    // "islas blancas" en modo oscuro. Determinismo: sin Application.Current (tests)
    // Oscuro=false → paleta clara de siempre; el golden-master mide layout/estructura,
    // no los colores del tema.
    private static bool Oscuro =>
        Application.Current?.RequestedThemeVariant == ThemeVariant.Dark;

    private static readonly IBrush FondoClaro = new SolidColorBrush(Color.Parse("#ffffff"));
    private static readonly IBrush FondoOscuro = new SolidColorBrush(Color.Parse("#0F172A"));
    private static readonly IBrush TextoClaro = new SolidColorBrush(Color.Parse("#0f172a"));
    private static readonly IBrush TextoOscuro = new SolidColorBrush(Color.Parse("#E2E8F0"));
    private static readonly IBrush TrazoClaro = new SolidColorBrush(Color.Parse("#334155"));
    private static readonly IBrush TrazoOscuro = new SolidColorBrush(Color.Parse("#475569"));

    private static IBrush Fondo => Oscuro ? FondoOscuro : FondoClaro;
    private static IBrush TextoPorDefecto => Oscuro ? TextoOscuro : TextoClaro;
    private static IBrush TrazoPorDefecto => Oscuro ? TrazoOscuro : TrazoClaro;

    // Paleta de capas para el diagrama de pila (top -> bottom).
    private static readonly string[] PaletaPila =
    {
        "#fde68a", "#fdba74", "#a7f3d0", "#7dd3fc", "#93c5fd",
        "#c4b5fd", "#f0abfc", "#f9a8d4", "#fca5a5", "#d9f99d"
    };

    // Colores de arista según el tipo de relación (etiqueta en español).
    private static readonly Dictionary<string, string> ColorArista =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["corre sobre"] = "#16a34a",
            ["encapsula"] = "#2563eb",
            ["depende de"] = "#ea580c",
            ["es versión de"] = "#7c3aed",
            ["sustituye a"] = "#dc2626",
            ["implementa"] = "#0d9488",
            ["documenta"] = "#64748b"
        };

    /// <summary>Color (hex) de una etiqueta de relación, para leyendas; null si no es un tipo conocido.</summary>
    public static string? ColorDeTipo(string etiqueta)
        => ColorArista.TryGetValue(etiqueta, out var hex) ? hex : null;

    static DiagramView()
    {
        AffectsRender<DiagramView>(DocumentProperty, FactorZoomProperty);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var d = Document;
        if (d is null) return new Size(0, 0);
        // El layout mide el tamaño ESCALADO: el ScrollViewer que envuelve al diagrama
        // sabe el ancho real y muestra su barra horizontal cuando desborda el panel.
        var f = FactorReal;
        return new Size(d.Width * f, d.Height * f);
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        var d = Document;
        if (d is null) return;

        context.FillRectangle(Fondo, new Rect(0, 0, Bounds.Width, Bounds.Height));

        // Zoom del diagrama como unidad: la escala se aplica al CONTEXTO de dibujo
        // (igual que el PNG 2× del exporter), de modo que geometría y texto escalan de
        // verdad y el texto del diagrama crece con el zoom (13 px base → 13·zoom visual).
        // Dentro del transform TODO va en coordenadas de documento (los 13 px / trazos
        // base): el push transform los escala después una sola vez. Con factor 1.0 la
        // matriz es la identidad: dibuja exactamente como antes.
        var f = FactorReal;
        using (context.PushTransform(Matrix.CreateScale(f, f)))
        {
            var rectIndex = 0;
            foreach (var p in d.Items)
            {
                switch (p.Kind)
                {
                    case PrimitiveKind.Rect:
                        var fill = ColorRect(d.Tipo, p, rectIndex++);
                        var stroke = p.Stroke is null ? TrazoPorDefecto : new SolidColorBrush(Color.Parse(p.Stroke));
                        context.DrawRectangle(fill, new Pen(stroke, 1.2),
                            new Rect(p.X, p.Y, p.W, p.H));
                        break;

                    case PrimitiveKind.Line:
                        var trazo = p.Stroke is null ? TrazoPorDefecto : new SolidColorBrush(Color.Parse(p.Stroke));
                        if (d.Tipo == "grafo" && !string.IsNullOrWhiteSpace(p.Label) &&
                            ColorArista.TryGetValue(p.Label, out var hex))
                            trazo = new SolidColorBrush(Color.Parse(hex));
                        context.DrawLine(new Pen(trazo, 1.4),
                            new Point(p.X, p.Y), new Point(p.X + p.W, p.Y + p.H));
                        break;

                    case PrimitiveKind.Text:
                        // El layout usa #0f172a como color de texto "por defecto"; en modo
                        // oscuro se reinterpreta con el texto del tema (los colores
                        // explícitos —leyendas, acentos— se respetan tal cual).
                        var esTextoPorDefecto = p.Fill is null
                            || p.Fill.Equals("#0f172a", StringComparison.OrdinalIgnoreCase);
                        var colorTexto = esTextoPorDefecto
                            ? TextoPorDefecto
                            : new SolidColorBrush(Color.Parse(p.Fill!));
                        var texto = p.Label;
                        // Si el layout indica un ancho máximo (W>0, p. ej. etiqueta dentro de una
                        // casilla del wire format), medir y truncar con "…" en caso de desborde.
                        if (p.W > 0)
                        {
                            var ftMedida = new FormattedText(texto,
                                System.Globalization.CultureInfo.CurrentCulture,
                                FlowDirection.LeftToRight, TypefaceUi, 13, colorTexto);
                            if (ftMedida.Width > p.W)
                            {
                                var i = texto.Length;
                                while (i > 0)
                                {
                                    var candidato = texto[..i] + "…";
                                    var ft2 = new FormattedText(candidato,
                                        System.Globalization.CultureInfo.CurrentCulture,
                                        FlowDirection.LeftToRight, TypefaceUi, 13, colorTexto);
                                    if (ft2.Width <= p.W) { texto = candidato; break; }
                                    i--;
                                }
                            }
                        }
                        var ft = new FormattedText(texto,
                            System.Globalization.CultureInfo.CurrentCulture,
                            FlowDirection.LeftToRight, TypefaceUi, 13, colorTexto);
                        context.DrawText(ft, new Point(p.X, p.Y));
                        break;
                }
            }
        }
    }

    private static IBrush ColorRect(string tipo, Primitive p, int indice)
    {
        // Pila: paleta por capa (el layout usa el mismo Fill para todas); los rellenos
        // pastel se conservan como "chips" de color sobre el lienzo del tema.
        if (tipo == "pila")
            return new SolidColorBrush(Color.Parse(PaletaPila[indice % PaletaPila.Length]));

        // Grafo: la semilla trae su propio relleno (amarillo); los vecinos en azul claro.
        if (tipo == "grafo")
            return p.Fill is not null && !p.Fill.Equals("#eef2ff", StringComparison.OrdinalIgnoreCase)
                ? new SolidColorBrush(Color.Parse(p.Fill))
                : RellenoAzulClaro();

        // Wire format: campos en azul claro; resto según primitiva o por defecto.
        if (tipo == "wire-format") return RellenoAzulClaro();
        return p.Fill is null ? RellenoNeutro() : new SolidColorBrush(Color.Parse(p.Fill));
    }

    // Rellenos de caja que en claro eran pastel y en oscuro pasan a pizarra (sin "isla blanca").
    private static IBrush RellenoAzulClaro() =>
        new SolidColorBrush(Color.Parse(Oscuro ? "#1E3A5F" : "#dbeafe"));
    private static IBrush RellenoNeutro() =>
        new SolidColorBrush(Color.Parse(Oscuro ? "#1E293B" : "#eef2ff"));
}