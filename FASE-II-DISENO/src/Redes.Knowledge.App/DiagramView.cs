using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Redes.Knowledge.Visualization;

namespace Redes.Knowledge.App;

/// <summary>
/// Renderiza un DiagramDocument (primitivas Rect/Line/Text) con DrawingContext.
/// ADR-003: el modelo no conoce el renderer; este control es el renderer Avalonia.
/// Respeta el Fill/Stroke de cada primitiva y aplica una paleta por tipo de diagrama:
/// pila (capas), grafo (semilla/vecinos/aristas por tipo), wire format (campos).
/// Fondo claro fijo para que sea legible en tema claro y oscuro.
/// </summary>
public sealed class DiagramView : Control
{
    public static readonly StyledProperty<DiagramDocument?> DocumentProperty =
        AvaloniaProperty.Register<DiagramView, DiagramDocument?>(nameof(Document));

    public DiagramDocument? Document
    {
        get => GetValue(DocumentProperty);
        set => SetValue(DocumentProperty, value);
    }

    private static readonly IBrush FondoClaro = new SolidColorBrush(Color.Parse("#ffffff"));
    private static readonly IBrush TextoPorDefecto = new SolidColorBrush(Color.Parse("#0f172a"));
    private static readonly IBrush TrazoPorDefecto = new SolidColorBrush(Color.Parse("#334155"));

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
        AffectsRender<DiagramView>(DocumentProperty);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var d = Document;
        return d is null ? new Size(0, 0) : new Size(d.Width, d.Height);
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        var d = Document;
        if (d is null) return;

        context.FillRectangle(FondoClaro, new Rect(0, 0, Bounds.Width, Bounds.Height));

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
                    var colorTexto = p.Fill is null ? TextoPorDefecto : new SolidColorBrush(Color.Parse(p.Fill));
                    var typeface = new Typeface(FontFamily.Default, FontStyle.Normal, FontWeight.Normal);
                    var texto = p.Label;
                    // Si el layout indica un ancho máximo (W>0, p. ej. etiqueta dentro de una
                    // casilla del wire format), medir y truncar con "…" en caso de desborde.
                    if (p.W > 0)
                    {
                        var ftMedida = new FormattedText(texto,
                            System.Globalization.CultureInfo.CurrentCulture,
                            FlowDirection.LeftToRight, typeface, 13, colorTexto);
                        if (ftMedida.Width > p.W)
                        {
                            var i = texto.Length;
                            while (i > 0)
                            {
                                var candidato = texto[..i] + "…";
                                var ft2 = new FormattedText(candidato,
                                    System.Globalization.CultureInfo.CurrentCulture,
                                    FlowDirection.LeftToRight, typeface, 13, colorTexto);
                                if (ft2.Width <= p.W) { texto = candidato; break; }
                                i--;
                            }
                        }
                    }
                    var ft = new FormattedText(texto,
                        System.Globalization.CultureInfo.CurrentCulture,
                        FlowDirection.LeftToRight, typeface, 13, colorTexto);
                    context.DrawText(ft, new Point(p.X, p.Y));
                    break;
            }
        }
    }

    private static IBrush ColorRect(string tipo, Primitive p, int indice)
    {
        // Pila: paleta por capa (el layout usa el mismo Fill para todas).
        if (tipo == "pila")
            return new SolidColorBrush(Color.Parse(PaletaPila[indice % PaletaPila.Length]));

        // Grafo: la semilla trae su propio relleno (amarillo); los vecinos en azul claro.
        if (tipo == "grafo")
            return p.Fill is not null && !p.Fill.Equals("#eef2ff", StringComparison.OrdinalIgnoreCase)
                ? new SolidColorBrush(Color.Parse(p.Fill))
                : new SolidColorBrush(Color.Parse("#dbeafe"));

        // Wire format: campos en azul claro; resto según primitiva o por defecto.
        if (tipo == "wire-format") return new SolidColorBrush(Color.Parse("#dbeafe"));
        return p.Fill is null ? new SolidColorBrush(Color.Parse("#eef2ff")) : new SolidColorBrush(Color.Parse(p.Fill));
    }
}