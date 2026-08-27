using System.Globalization;
using System.Text;

namespace Redes.Knowledge.Visualization;

/// <summary>Renderer SVG determinista (formato vectorial de intercambio, ADR-003).</summary>
public static class SvgRenderer
{
    public static string Render(DiagramDocument doc)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<svg xmlns=\"http://www.w3.org/2000/svg\" " +
                      $"width=\"{F(doc.Width)}\" height=\"{F(doc.Height)}\" " +
                      $"viewBox=\"0 0 {F(doc.Width)} {F(doc.Height)}\">");
        foreach (var p in doc.Items)
        {
            switch (p.Kind)
            {
                case PrimitiveKind.Rect:
                    sb.AppendLine($"  <rect x=\"{F(p.X)}\" y=\"{F(p.Y)}\" width=\"{F(p.W)}\" height=\"{F(p.H)}\" " +
                                  $"fill=\"{p.Fill ?? "#ffffff"}\" stroke=\"{p.Stroke ?? "#000000"}\" stroke-width=\"1.2\" />");
                    break;
                case PrimitiveKind.Line:
                    sb.AppendLine($"  <line x1=\"{F(p.X)}\" y1=\"{F(p.Y)}\" x2=\"{F(p.X + p.W)}\" y2=\"{F(p.Y + p.H)}\" " +
                                  $"stroke=\"{p.Stroke ?? "#334155"}\" stroke-width=\"1.4\" />");
                    break;
                case PrimitiveKind.Text:
                    var etiqueta = p.W > 0 ? AjustarTexto(p.Label, p.W) : p.Label;
                    // La app dibuja el texto con la esquina superior en (x,y) y 13px
                    // (tamaño de letra global). SVG coloca 'y' en la LÍNEA BASE: se suma
                    // el tamaño de fuente para que el bloque quede en la misma posición.
                    sb.AppendLine($"  <text x=\"{F(p.X)}\" y=\"{F(p.Y + 13)}\" font-family=\"Consolas, monospace\" font-size=\"13\" " +
                                  $"fill=\"{p.Fill ?? "#0f172a"}\">{Esc(etiqueta)}</text>");
                    break;
            }
        }
        sb.AppendLine("</svg>");
        return sb.ToString();
    }

    private static string F(double v) => v.ToString("0.##", CultureInfo.InvariantCulture);

    /// <summary>Truncación equivalente a la del renderer de pantalla para etiquetas con
    /// ancho máximo (W&gt;0, p. ej. casillas del wire format): JetBrains Mono ≈ 7,8 px/carácter a 13 px.</summary>
    private static string AjustarTexto(string s, double ancho)
    {
        const double pxPorCaracter = 7.8;
        if (s.Length * pxPorCaracter <= ancho) return s;
        var max = Math.Max(1, (int)(ancho / pxPorCaracter) - 1);
        return s[..Math.Min(max, s.Length)] + "…";
    }

    private static string Esc(string s)
        => s.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;");
}