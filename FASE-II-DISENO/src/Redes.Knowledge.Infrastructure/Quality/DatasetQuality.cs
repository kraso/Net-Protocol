using System.Security.Cryptography;
using System.Text;
using Redes.Knowledge.Domain;

namespace Redes.Knowledge.Infrastructure.Quality;

public sealed record ChequeoCalidad(string Id, string Nombre, bool Ok, string Detalle);

public sealed record InformeCalidad(string Dataset, bool Ok, IReadOnlyList<ChequeoCalidad> Chequeos, DateTime Fecha)
{
    public override string ToString()
        => $"Dataset '{Dataset}': {(Ok ? "OK" : "FALLOS")} — {Chequeos.Count(c => c.Ok)}/{Chequeos.Count} chequeos correctos ({Fecha:u})";
}

/// <summary>
/// Controles automáticos de datos (plan §9.3 / D7-1): URNs únicas, duplicados por (familia, acrónimo),
/// fichas válidas, integridad referencial F5/F6/F7 → F3 y golden-master determinista.
/// </summary>
public static class DatasetQuality
{
    public static InformeCalidad Auditar(
        string dataset,
        IReadOnlyList<Protocol> protocolos,
        IReadOnlyList<string> f3Ids,
        IReadOnlyList<string> f5ProtocolosIds,
        IReadOnlyList<string> f6ProtocoloIds,
        IReadOnlyList<string> f7Referencias,
        string? goldenEsperado = null)
    {
        var idSet = new HashSet<string>(f3Ids, StringComparer.Ordinal);
        var chequeos = new List<ChequeoCalidad>();

        // A01 — claves estables únicas
        var dups = protocolos.GroupBy(p => p.Id.Value).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
        chequeos.Add(new ChequeoCalidad("A01", "URNs únicas", dups.Count == 0,
            dups.Count == 0 ? "OK" : $"Duplicadas: {string.Join(", ", dups)}"));

        // A02 — duplicados (familia, acrónimo)
        var dupAcr = protocolos.GroupBy(p => (p.Familia, p.Acronimo)).Where(g => g.Count() > 1)
            .Select(g => $"{g.Key.Acronimo}/{g.Key.Familia}").ToList();
        chequeos.Add(new ChequeoCalidad("A02", "Sin duplicados (familia, acrónimo)", dupAcr.Count == 0,
            dupAcr.Count == 0 ? "OK" : $"Duplicados: {string.Join(", ", dupAcr)}"));

        // A03 — fichas válidas según el dominio
        var invalidos = protocolos.Where(p => !ProtocolValidator.Validate(p).IsValid).Select(p => p.Acronimo).ToList();
        chequeos.Add(new ChequeoCalidad("A03", "Fichas válidas (esquema F4)", invalidos.Count == 0,
            invalidos.Count == 0 ? $"OK ({protocolos.Count} fichas)" : $"Inválidas: {string.Join(", ", invalidos)}"));

        // A04..A06 — integridad referencial
        var f5Faltan = f5ProtocolosIds.Where(id => !idSet.Contains(id)).ToList();
        chequeos.Add(new ChequeoCalidad("A04", "Referencias F5 → F3", f5Faltan.Count == 0,
            f5Faltan.Count == 0 ? "OK" : $"Faltan: {string.Join(", ", f5Faltan)}"));

        var f6Faltan = f6ProtocoloIds.Where(id => !idSet.Contains(id)).ToList();
        chequeos.Add(new ChequeoCalidad("A05", "Referencias F6 → F3", f6Faltan.Count == 0,
            f6Faltan.Count == 0 ? "OK" : $"Faltan: {string.Join(", ", f6Faltan)}"));

        var f7Faltan = f7Referencias.Where(id => !idSet.Contains(id)).Distinct().ToList();
        chequeos.Add(new ChequeoCalidad("A06", "Referencias F7 → F3", f7Faltan.Count == 0,
            f7Faltan.Count == 0 ? "OK" : $"Faltan: {string.Join(", ", f7Faltan)}"));

        // A07 — golden master (regresión del dataset)
        if (goldenEsperado is not null)
        {
            var calculado = GoldenMasterDe(protocolos);
            chequeos.Add(new ChequeoCalidad("A07", "Golden master (hash determinista)", calculado == goldenEsperado,
                calculado == goldenEsperado ? "OK" : $"Esperado {goldenEsperado} ≠ calculado {calculado}"));
        }

        var ok = chequeos.All(c => c.Ok);
        return new InformeCalidad(dataset, ok, chequeos, DateTime.UtcNow);
    }

    /// <summary>Hash canónico y determinista del dataset (orden por URN, representación estable).</summary>
    public static string GoldenMasterDe(IReadOnlyList<Protocol> protocolos)
    {
        var sb = new StringBuilder();
        foreach (var p in protocolos.OrderBy(p => p.Id.Value, StringComparer.Ordinal))
            sb.AppendLine($"{p.Id.Value}|{p.Nombre}|{p.Acronimo}|{p.Familia}|{p.Estado}");
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(sb.ToString())));
    }
}