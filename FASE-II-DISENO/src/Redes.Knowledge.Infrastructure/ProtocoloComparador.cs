using Redes.Knowledge.Domain;
using Redes.Knowledge.Infrastructure.Iana;

namespace Redes.Knowledge.Infrastructure;

public sealed record FilaComparacion(
    string Protocolo,
    string Nombre,
    string Familia,
    string Estado,
    string Pdu,
    string Puertos,
    string Cifrado,
    string Capas = "—",
    string Finalidad = "—",
    string Encapsulacion = "—");

/// <summary>
/// Comparador de protocolos (D5-2): tabla comparativa regenerable con datos reales
/// (capa/familia y estado del dominio; capas por protocolo; finalidad y encapsulación
/// desde las fichas F4 o el grafo de relaciones; PDU de F5; puertos del registro IANA;
/// cifrado de F6). Es una función pura: los datos se inyectan (app/tests).
/// </summary>
public static class ProtocoloComparador
{
    public static IReadOnlyList<FilaComparacion> Comparar(
        IReadOnlyList<Protocol> protocolos,
        Func<string, int, IReadOnlyList<IanaServiceEntry>> puertosPorNombre,
        IReadOnlyDictionary<string, string>? pduPorAcronimo = null,
        IReadOnlyDictionary<string, string>? cifradoPorAcronimo = null,
        IReadOnlyDictionary<string, FichaPrioritaria>? fichas = null,
        IReadOnlyList<Relationship>? relaciones = null)
    {
        pduPorAcronimo ??= new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        cifradoPorAcronimo ??= new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        fichas ??= new Dictionary<string, FichaPrioritaria>(StringComparer.OrdinalIgnoreCase);

        return protocolos
            .OrderBy(p => p.Acronimo, StringComparer.OrdinalIgnoreCase)
            .Select(p =>
            {
                var servicios = puertosPorNombre(p.Acronimo.ToLowerInvariant(), 20);
                var puertos = servicios.Count == 0
                    ? "—"
                    : string.Join(", ", servicios
                        .Where(s => s.Port.HasValue)
                        .Select(s => $"{s.Port}/{s.Transport}")
                        .Distinct()
                        .Take(4));

                fichas.TryGetValue(p.Acronimo, out var ficha);
                var vecinos = relaciones is null
                    ? Array.Empty<Vecino>()
                    : GrafoRelaciones.Vecinos1Salto(p.Acronimo, relaciones);
                var encapsulacion = ficha?.Campo(4)
                    ?? (vecinos.Count > 0
                        ? string.Join(", ", vecinos.Select(v => v.Nombre))
                        : "—");

                return new FilaComparacion(
                    p.Acronimo,
                    p.Nombre,
                    p.Familia.ToString(),
                    p.Estado.ToString(),
                    pduPorAcronimo.TryGetValue(p.Acronimo, out var pdu) ? pdu : "—",
                    puertos,
                    cifradoPorAcronimo.TryGetValue(p.Acronimo, out var cf) ? cf : "—",
                    ficha?.Campo(5) ?? p.Capas ?? "—",
                    ficha?.Campo(3) ?? "—",
                    encapsulacion);
            })
            .ToList();
    }
}