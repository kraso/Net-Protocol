using System.Text.Json;
using Redes.Knowledge.Domain;

namespace Redes.Knowledge.Infrastructure;

/// <summary>
/// Serialización e importación de los catálogos canónicos de la Fase I (D1-3):
/// lee F3-Protocolos.json y F5-Campos-PDU.json sin duplicar datos (regla nº 3 del repositorio).
/// </summary>
public static class CatalogJson
{
    public static IReadOnlyList<Protocol> CargarProtocolosF3(string jsonPath)
    {
        using var doc = JsonDocument.Parse(File.ReadAllText(jsonPath));
        var lista = new List<Protocol>();
        foreach (var el in doc.RootElement.GetProperty("protocolos").EnumerateArray())
        {
            lista.Add(new Protocol
            {
                Id = Urn.Parse(el.GetProperty("id").GetString()!),
                Nombre = el.GetProperty("nombre").GetString()!,
                Acronimo = el.GetProperty("acronimo").GetString()!,
                Familia = MapearFamilia(el.GetProperty("familia").GetString()),
                Estado = MapearEstado(el.GetProperty("estado").GetString()),
                Capas = el.GetProperty("capas").GetString()
            });
        }
        return lista;
    }

    /// <summary>Campos de PDU de un protocolo concreto desde F5-Campos-PDU.json.</summary>
    public static IReadOnlyList<Field> CargarCamposF5(string jsonPath, string acronimoObjetivo)
    {
        return CargarCatalogosF5(jsonPath)
            .FirstOrDefault(c => string.Equals(c.Acronimo, acronimoObjetivo, StringComparison.OrdinalIgnoreCase))
            ?.Campos ?? Array.Empty<Field>();
    }

    /// <summary>Todos los catálogos F5 (acrónimo + campos) para validaciones globales (L-004).</summary>
    public static IReadOnlyList<CatalogoF5> CargarCatalogosF5(string jsonPath)
    {
        using var doc = JsonDocument.Parse(File.ReadAllText(jsonPath));
        var lista = new List<CatalogoF5>();
        foreach (var p in doc.RootElement.GetProperty("protocolos").EnumerateArray())
        {
            var baseUrn = p.GetProperty("id").GetString()!;
            var acronimo = p.GetProperty("acronimo").GetString()!;
            var campos = new List<Field>();
            foreach (var c in p.GetProperty("campos").EnumerateArray())
            {
                int? offset = NuloInt(c, "offset_bits");
                int? longitud = NuloInt(c, "longitud_bits");
                campos.Add(new Field
                {
                    Id = Urn.Parse($"{baseUrn}:campo:{NormalizarId(c.GetProperty("nombre").GetString()!)}"),
                    Nombre = c.GetProperty("nombre").GetString()!,
                    OffsetBits = offset,
                    LongitudBits = longitud,
                    Tipo = c.GetProperty("tipo").GetString()!,
                    Semantica = c.GetProperty("semantica").GetString()!,
                    Obligatorio = c.GetProperty("obligatorio").GetBoolean()
                });
            }
            lista.Add(new CatalogoF5(acronimo, campos));
        }
        return lista;
    }

    /// <summary>Un catálogo F5: acrónimo y sus campos de PDU.</summary>
    public sealed record CatalogoF5(string Acronimo, IReadOnlyList<Field> Campos);

    /// <summary>Round-trip JSON canónico: Serialize → Deserialize → Serialize produce el mismo JSON.</summary>
    public static string RoundTripJson<T>(T valor)
    {
        var json1 = JsonSerializer.Serialize(valor);
        var regresado = JsonSerializer.Deserialize<T>(json1) ?? throw new InvalidOperationException("Deserialización nula.");
        return JsonSerializer.Serialize(regresado);
    }

    /// <summary>
    /// Datos descriptivos de F3 que la BD no persiste (nota y fuente por protocolo).
    /// Se leen del catálogo canónico en cada vista, sin duplicarlos (regla nº 3).
    /// </summary>
    /// <summary>Familias del catálogo F3 (acrónimo → descripción) para la leyenda de la app.</summary>
    public static IReadOnlyDictionary<string, string> CargarFamiliasF3(string jsonPath)
    {
        using var doc = JsonDocument.Parse(File.ReadAllText(jsonPath));
        var mapa = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var f in doc.RootElement.GetProperty("familias").EnumerateObject())
            mapa[f.Name] = f.Value.GetString() ?? "";
        return mapa;
    }

    public sealed record NotaFuente(string Nota, string Fuente);

    public static IReadOnlyDictionary<string, NotaFuente> CargarNotasFuenteF3(string jsonPath)
    {
        using var doc = JsonDocument.Parse(File.ReadAllText(jsonPath));
        var mapa = new Dictionary<string, NotaFuente>(StringComparer.OrdinalIgnoreCase);
        foreach (var el in doc.RootElement.GetProperty("protocolos").EnumerateArray())
        {
            var acronimo = el.GetProperty("acronimo").GetString()!;
            var nota = el.TryGetProperty("nota", out var n) ? n.GetString() ?? "" : "";
            var fuente = el.TryGetProperty("fuente", out var f) ? f.GetString() ?? "" : "";
            mapa[acronimo] = new NotaFuente(nota, fuente);
        }
        return mapa;
    }

    private static int? NuloInt(JsonElement el, string prop)
    {
        if (!el.TryGetProperty(prop, out var v) || v.ValueKind == JsonValueKind.Null) return null;
        return v.GetInt32();
    }

    private static FamiliaProtocolo MapearFamilia(string? s)
        => Enum.TryParse<FamiliaProtocolo>(s, out var f) ? f : FamiliaProtocolo.HIST;

    private static LifecycleState MapearEstado(string? s)
    {
        if (string.Equals(s, "histórico", StringComparison.OrdinalIgnoreCase)) return LifecycleState.Historico;
        return Enum.TryParse<LifecycleState>(s, true, out var e) ? e : LifecycleState.Desconocido;
    }

    private static string NormalizarId(string texto)
        => string.Concat(texto.Where(char.IsLetterOrDigit)).ToLowerInvariant();
}