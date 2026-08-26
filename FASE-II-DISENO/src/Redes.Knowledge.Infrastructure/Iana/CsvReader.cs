using System.Text;

namespace Redes.Knowledge.Infrastructure.Iana;

/// <summary>Parser mínimo de CSV con soporte de comillas (regla del registro IANA).</summary>
public static class CsvReader
{
    public static IReadOnlyList<string> ParseLine(string line)
    {
        var resultado = new List<string>();
        var sb = new StringBuilder();
        var enComillas = false;

        for (var i = 0; i < line.Length; i++)
        {
            var c = line[i];
            if (enComillas)
            {
                if (c == '"')
                {
                    if (i + 1 < line.Length && line[i + 1] == '"')
                    {
                        sb.Append('"');
                        i++;
                    }
                    else
                    {
                        enComillas = false;
                    }
                }
                else
                {
                    sb.Append(c);
                }
            }
            else
            {
                if (c == '"') enComillas = true;
                else if (c == ',') { resultado.Add(sb.ToString()); sb.Clear(); }
                else sb.Append(c);
            }
        }

        resultado.Add(sb.ToString());
        return resultado;
    }
}