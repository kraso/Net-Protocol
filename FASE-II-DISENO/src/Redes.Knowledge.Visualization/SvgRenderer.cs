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
                    sb.AppendLine($"  <text x=\"{F(p.X)}\" y=\"{F(p.Y)}\" font-family=\"Consolas, monospace\" font-size=\"10\" " +
                                  $"fill=\"{p.Fill ?? "#0f172a"}\">{Esc(p.Label)}</text>");
                    break;
            }
        }
        sb.AppendLine("</svg>");
        return sb.ToString();
    }

    private static string F(double v) => v.ToString("0.##", CultureInfo.InvariantCulture);

    private static string Esc(string s)
        => s.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;");
}