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

    /// <summary>Rasteriza el documento a tamaño 1:1 en píxeles (96 DPI) y devuelve PNG.</summary>
    public static byte[] Png(DiagramDocument doc)
    {
        var w = Math.Max(1, doc.Width);
        var h = Math.Max(1, doc.Height);

        var view = new DiagramView { Document = doc };
        view.Measure(new Size(w, h));
        view.Arrange(new Rect(0, 0, w, h));

        var rtb = new RenderTargetBitmap(new PixelSize(w, h), new Vector(96, 96));
        using (var ctx = rtb.CreateDrawingContext())
        {
            // CreateDrawingContext() devuelve directamente un DrawingContext válido
            // para renderizar el control (mismo código que pinta en pantalla).
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