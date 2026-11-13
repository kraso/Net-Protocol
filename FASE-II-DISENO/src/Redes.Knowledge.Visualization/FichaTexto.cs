using System.Text;
using Redes.Knowledge.Domain;

namespace Redes.Knowledge.Visualization;

/// <summary>Datos ya resueltos que necesita la ficha textual de un protocolo. La ventana
/// resuelve catálogos y etiquetas; este tipo solo transporta el resultado.</summary>
public sealed record FichaTextoModelo
{
    public required Protocol Protocolo { get; init; }
    public FichaPrioritaria? Ficha { get; init; }
    public IReadOnlyList<Field> Campos { get; init; } = Array.Empty<Field>();
    /// <summary>Vecinos a 1 salto ya formateados ("ACR · Nombre (tipo)").</summary>
    public IReadOnlyList<string> Vecinos { get; init; } = Array.Empty<string>();
    public string? Pdu { get; init; }
    public string? Cifrado { get; init; }
    public string? Puertos { get; init; }
    public string? Nota { get; init; }
    public string? FuenteF3 { get; init; }
}

/// <summary>Formateador determinista de la ficha de 18 campos (P2, incremento I1).
/// Vive fuera de la ventana para poder probarlo sin Avalonia; la salida exacta está
/// congelada por las pruebas de caracterización de la app.</summary>
public static class FichaTexto
{
    public static string Render(FichaTextoModelo m)
    {
        var p = m.Protocolo;
        var ficha = m.Ficha;
        var vecinos = m.Vecinos;

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
            ficha?.Campo(4) ?? (vecinos.Count > 0 ? string.Join(", ", vecinos) : null));
        Linea(5, "Capas", p.Capas ?? ficha?.Campo(5));
        Linea(6, "Transp./dir", ficha?.Campo(6)
            ?? (m.Puertos is not null && m.Puertos != "—" ? $"{m.Puertos} [IANA]" : null));
        Linea(7, "PDU", m.Pdu);
        Linea(8, "Mensajes", ficha?.Campo(8));
        Linea(9, "Campos", ficha?.Campo(9));
        Linea(10, "Secuencia", ficha?.Campo(10));
        Linea(11, "Addressing", ficha?.Campo(11));
        Linea(12, "Routing", ficha?.Campo(12));
        Linea(13, "Seguridad", m.Cifrado is not null ? $"cifrado: {m.Cifrado} [F6]" : ficha?.Campo(13));
        Linea(14, "QoS/rendim.", ficha?.Campo(14));
        Linea(15, "Observabilidad", ficha?.Campo(15));
        Linea(16, "Interoperabilidad", ficha?.Campo(16));
        Linea(17, "Implementaciones", ficha?.Campo(17));
        Linea(18, "Fuentes", ficha?.Campo(18) ?? m.FuenteF3);

        if (!string.IsNullOrWhiteSpace(m.Nota)) sb.AppendLine($"      Nota/F3 .....: {m.Nota}");

        if (ficha is null && vecinos.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("--- Vecinos a 1 salto (grafo F4) ---");
            foreach (var v in vecinos) sb.AppendLine($"  • {v}");
        }

        if (m.Campos.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("--- Campos catalogados (F5) ---");
            foreach (var f in m.Campos)
                sb.AppendLine($"  {f.Nombre}: offset {f.OffsetBits?.ToString() ?? "var"} · {f.LongitudBits?.ToString() ?? "var"} bits · {f.Tipo}{(f.Obligatorio ? "" : " (opt.)")}");
        }

        return sb.ToString();
    }
}
