using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Redes.Knowledge.Visualization;

namespace Redes.Knowledge.App;

/// <summary>
/// Exportación de diagramas a archivo (D4-3): SVG (vectorial, <see cref="SvgRenderer"/>),
/// PDF (vectorial mínimo, <see cref="PdfExporter"/>) y PNG (rasterización offscreen del
/// MISMO renderer <see cref="DiagramView"/> que pinta la app, para que PNG sea idéntico
/// a lo que se ve en pantalla). Determinista: mismo documento → mismos bytes por formato.
/// </summary>
public static class DiagramExporter
{
    public static string Svg(DiagramDocument doc) => SvgRenderer.Render(doc);

    public static byte[] Pdf(DiagramDocument doc) => PdfExporter.Export(doc);

    /// <summary>Rasteriza el documento a factor ×1/×2/… (p. ej. 2× para PNG nítidos en
    /// impresión). La transformación se aplica al contexto de dibujo, de modo que texto y
    /// geometría se escalan de verdad (no un simple estiramiento de píxeles).</summary>
    public static byte[] Png(DiagramDocument doc, double factor = 1.0)
    {
        var w = Math.Max(1, doc.Width);
        var h = Math.Max(1, doc.Height);
        var escala = factor <= 0 ? 1.0 : factor;
        var pxW = Math.Max(1, (int)Math.Ceiling(w * escala));
        var pxH = Math.Max(1, (int)Math.Ceiling(h * escala));

        var view = new DiagramView { Document = doc };
        view.Measure(new Size(w, h));
        view.Arrange(new Rect(0, 0, w, h));

        var rtb = new RenderTargetBitmap(new PixelSize(pxW, pxH), new Vector(96 * escala, 96 * escala));
        using var ctx = rtb.CreateDrawingContext();
        if (escala != 1.0)
        {
            // La transformación se aplica al contexto: geometría y texto se escalan de verdad.
            using (ctx.PushTransform(Matrix.CreateScale(escala, escala)))
                view.Render(ctx);
        }
        else
        {
            view.Render(ctx);
        }

        using var ms = new MemoryStream();
        rtb.Save(ms, PngBitmapEncoderOptions.Default);
        return ms.ToArray();
    }

    /// <summary>Comprueba la firma PNG (89 50 4E 47) — autocomprobación antes de guardar.</summary>
    public static bool EsPngValido(byte[] bytes)
        => bytes.Length > 8 &&
           bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47;
}