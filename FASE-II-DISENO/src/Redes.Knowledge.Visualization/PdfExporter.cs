using System.Globalization;
using System.Text;

namespace Redes.Knowledge.Visualization;

/// <summary>
/// Exportador PDF vectorial mínimo (sin dependencias externas): genera un PDF 1.4 válido
/// y determinista a partir de las primitivas del documento (rectángulos, líneas y texto).
/// Formato de intercambio adicional al SVG (D4-3). La rasterización PNG se integra en la app (D5).
/// </summary>
public static class PdfExporter
{
    public static byte[] Export(DiagramDocument doc)
    {
        var contenido = new StringBuilder();
        foreach (var p in doc.Items)
        {
            switch (p.Kind)
            {
                case PrimitiveKind.Rect:
                    var yB = doc.Height - p.Y - p.H;
                    if (p.Fill is not null)
                    {
                        contenido.Append($"{C(p.Fill)} rg\n");
                        contenido.Append($"{F(p.X)} {F(yB)} {F(p.W)} {F(p.H)} re\nf\n");
                    }
                    if (p.Stroke is not null)
                    {
                        contenido.Append($"{C(p.Stroke)} RG\n");
                        contenido.Append($"{F(p.X)} {F(yB)} {F(p.W)} {F(p.H)} re\nS\n");
                    }
                    break;
                case PrimitiveKind.Line:
                    if (p.Stroke is null) break;
                    contenido.Append($"{C(p.Stroke)} RG\n");
                    contenido.Append($"{F(p.X)} {F(doc.Height - p.Y)} m\n");
                    contenido.Append($"{F(p.X + p.W)} {F(doc.Height - p.Y - p.H)} l\nS\n");
                    break;
                case PrimitiveKind.Text:
                    contenido.Append($"0 0 0 rg\nBT /F1 9 Tf {F(p.X)} {F(doc.Height - p.Y - 4)} Td ({San(p.Label)}) Tj ET\n");
                    break;
            }
        }

        var contenidoBytes = Encoding.ASCII.GetBytes(contenido.ToString());
        var objs = new List<string>
        {
            "<< /Type /Catalog /Pages 2 0 R >>",
            "<< /Type /Pages /Kids [3 0 R] /Count 1 >>",
            $"<< /Type /Page /Parent 2 0 R /MediaBox [0 0 {F(doc.Width)} {F(doc.Height)}] " +
            "/Resources << /Font << /F1 5 0 R >> >> /Contents 4 0 R >>",
            $"<< /Length {contenidoBytes.Length} >>\nstream\n{contenido}endstream",
            "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>"
        };

        using var pdf = new MemoryStream();
        void W(string s)
        {
            var b = Encoding.ASCII.GetBytes(s);
            pdf.Write(b, 0, b.Length);
        }

        W("%PDF-1.4\n");
        var offsets = new long[objs.Count];
        for (var i = 0; i < objs.Count; i++)
        {
            offsets[i] = pdf.Length;
            W($"{i + 1} 0 obj\n{objs[i]}\nendobj\n");
        }

        var xref = pdf.Length;
        W($"xref\n0 {objs.Count + 1}\n0000000000 65535 f \n");
        foreach (var o in offsets) W($"{o,10:D10} 00000 n \n");
        W($"trailer\n<< /Size {objs.Count + 1} /Root 1 0 R >>\nstartxref\n{xref}\n%%EOF\n");
        return pdf.ToArray();
    }

    private static string F(double v) => v.ToString("0.##", CultureInfo.InvariantCulture);

    private static string C(string hex)
    {
        var r = int.Parse(hex.Substring(1, 2), NumberStyles.HexNumber) / 255.0;
        var g = int.Parse(hex.Substring(3, 2), NumberStyles.HexNumber) / 255.0;
        var b = int.Parse(hex.Substring(5, 2), NumberStyles.HexNumber) / 255.0;
        return $"{r:0.###} {g:0.###} {b:0.###}";
    }

    private static string San(string label)
    {
        var sb = new StringBuilder();
        foreach (var c in label)
        {
            if (c < 32) continue;
            if (c == '\\' || c == '(' || c == ')') { sb.Append('\\').Append(c); continue; }
            sb.Append(c < 128 ? c : '?');
        }
        return sb.ToString();
    }
}