using System.Text;

namespace SpikeDiagramas;

/// <summary>Definición mínima de un campo de PDU (origen: F5-Campos-PDU.json).</summary>
public sealed record FieldDef(string Nombre, int OffsetBits, int? LongitudBits, string Tipo);

/// <summary>
/// Spike D0-3 — renderer de diagramas determinista.
/// Genera un layout SVG "bit/byte" (estilo RFC) a partir de datos estructurados
/// (offset/longitud en bits). Sin timestamps ni orden aleatorio: mismo input → mismo SVG.
/// </summary>
public static class DeterministicSvg
{
    private const int RowBits = 32;
    private const int CellPx = 20;   // píxeles por bit
    private const int RowH = 34;
    private const int LabelH = 18;

    public static string Render(string titulo, IReadOnlyList<FieldDef> campos)
    {
        var fijos = campos.Where(c => c.LongitudBits.HasValue).OrderBy(c => c.OffsetBits).ToList();
        var maxEnd = fijos.Count == 0 ? 0 : fijos.Max(c => c.OffsetBits + c.LongitudBits!.Value);
        var filas = (maxEnd + RowBits - 1) / RowBits;
        var widthPx = RowBits * CellPx + 48;
        var heightPx = 26 + LabelH + filas * RowH + 16;

        var sb = new StringBuilder();
        sb.AppendLine($"<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"{widthPx}\" height=\"{heightPx}\" viewBox=\"0 0 {widthPx} {heightPx}\">");
        sb.AppendLine($"  <text x=\"10\" y=\"18\" font-family=\"Consolas, monospace\" font-size=\"14\">{Escape(titulo)}</text>");

        for (var r = 0; r < filas; r++)
        {
            var y = 26 + LabelH + r * RowH;
            sb.AppendLine($"  <text x=\"6\" y=\"{y + RowH / 2 + 4}\" font-family=\"Consolas, monospace\" font-size=\"10\">{r * RowBits:D2}</text>");
            foreach (var c in fijos)
            {
                var s = c.OffsetBits - r * RowBits;
                if (s >= RowBits || s + c.LongitudBits!.Value <= 0) continue;
                var start = Math.Max(s, 0);
                var len = Math.Min(c.LongitudBits.Value, RowBits - start);
                var x = 44 + start * CellPx;
                sb.AppendLine($"  <rect x=\"{x}\" y=\"{y}\" width=\"{len * CellPx}\" height=\"{RowH - 4}\" fill=\"#eef2ff\" stroke=\"#334155\" stroke-width=\"1\" />");
                sb.AppendLine($"  <text x=\"{x + 3}\" y=\"{y + RowH / 2 + 4}\" font-family=\"Consolas, monospace\" font-size=\"9\">{Escape($"{c.Nombre} ({c.OffsetBits}-{c.OffsetBits + c.LongitudBits.Value})")}</text>");
            }
        }
        sb.AppendLine("</svg>");
        return sb.ToString();
    }

    private static string Escape(string s)
        => s.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;");
}