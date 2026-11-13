using System.Text;

namespace Redes.Knowledge.Visualization;

/// <summary>
/// Formateador determinista de la tabla del comparador de dos protocolos (P2). Vive fuera de
/// la ventana para poder probarlo sin Avalonia; recibe las filas ya resueltas por
/// <c>ProtocoloComparador</c> y no conoce el catálogo. La salida en español está congelada
/// por las pruebas de caracterización de la app.
/// </summary>
public static class ComparadorTexto
{
    /// <summary>Ancho fijo de cada columna de protocolo; los valores largos se truncan con «…».</summary>
    public const int AnchoCelda = 36;

    /// <summary>Ancho de la columna de etiquetas (aspectos).</summary>
    public const int AnchoEtiqueta = 16;

    /// <summary>Una fila del comparador: etiqueta del aspecto y su valor en cada protocolo.</summary>
    public readonly record struct Fila(string Etiqueta, string? A, string? B);

    /// <summary>
    /// Construye la tabla transpuesta: filas = aspectos cortos con dos columnas, y debajo los
    /// campos largos en líneas envueltas por protocolo (no caben en una celda).
    /// </summary>
    public static string Construir(
        string protocoloA,
        string protocoloB,
        IReadOnlyList<Fila> cortas,
        IReadOnlyList<Fila> largas)
    {
        var sb = new StringBuilder();
        sb.AppendLine(Textos.F("cmp.titulo", protocoloA, protocoloB));
        sb.AppendLine();
        sb.AppendLine($"{Textos.T("cmp.aspecto"),-AnchoEtiqueta}| {Celda(protocoloA)} | {Celda(protocoloB)}");
        sb.AppendLine(new string('-', 17 + 3 + AnchoCelda + 3 + AnchoCelda));

        foreach (var fila in cortas)
        {
            var a = fila.A ?? "—";
            var b = fila.B ?? "—";
            sb.Append($"{fila.Etiqueta,-AnchoEtiqueta}| {Celda(a)} | {Celda(b)}");
            sb.AppendLine(a == b && a != "—" ? Textos.T("cmp.igual") : "");
        }

        sb.AppendLine();
        foreach (var fila in largas)
        {
            sb.AppendLine($"{fila.Etiqueta} — {protocoloA}: {fila.A ?? "—"}");
            sb.AppendLine($"{fila.Etiqueta} — {protocoloB}: {fila.B ?? "—"}");
            sb.AppendLine();
        }

        sb.AppendLine(Textos.T("cmp.fuentes"));
        return sb.ToString();
    }

    /// <summary>Celda de ancho fijo: trunca con «…» si el valor no cabe (la tabla nunca desborda).</summary>
    public static string Celda(string valor, int ancho = AnchoCelda)
        => valor.Length <= ancho ? valor.PadRight(ancho) : valor[..(ancho - 1)] + "…";

    /// <summary>Acorta un valor largo para las líneas de comparación (sin desbordar la pantalla).</summary>
    public static string? Acortar(string? s) =>
        string.IsNullOrWhiteSpace(s) || s == "—" ? null
        : s.Length <= 180 ? s
        : s[..177] + "…";
}
