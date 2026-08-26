using System.Text.Json;
using Redes.Knowledge.Domain;

namespace Redes.Knowledge.Infrastructure;

/// <summary>
/// Lectores de los catálogos canónicos de la Fase I para exploración avanzada (D5):
/// relaciones (F4), dispositivos (F2), redes (F2), PDU (F5) y seguridad (F6).
/// Nunca duplican datos: consumen los JSON de la Fase I.
/// </summary>
public static class CatalogoExploracion
{
    // Aliases entidad-F4 → identidad canónica del catálogo F3 (la matriz F4 nombra
    // la entidad por su nombre, F3 la cataloga por acrónimo; el resto coincide por normalización).
    private static readonly Dictionary<string, string> AliasEntidadF4 =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["Ethernet (802.3)"] = "ETH",
            ["Ethernet"] = "ETH",
            ["IPsec (ESP)"] = "IPsec",
            ["IPsec"] = "IPsec"
        };

    public static IReadOnlyList<Relationship> CargarRelacionesF4(string path)
    {
        using var doc = JsonDocument.Parse(File.ReadAllText(path));
        var lista = new List<Relationship>();
        foreach (var e in doc.RootElement.GetProperty("relaciones").EnumerateArray())
        {
            var origen = AliasEntidadF4.TryGetValue(e.GetProperty("origen").GetString()!, out var o)
                ? o : e.GetProperty("origen").GetString()!;
            var destino = AliasEntidadF4.TryGetValue(e.GetProperty("destino").GetString()!, out var d)
                ? d : e.GetProperty("destino").GetString()!;
            var tipo = (e.GetProperty("tipo").GetString() ?? "corre_sobre") switch
            {
                "encapsula" => RelacionTipo.Encapsula,
                "corre_sobre" => RelacionTipo.CorreSobre,
                "depende_de" => RelacionTipo.DependeDe,
                "es_version_de" => RelacionTipo.EsVersionDe,
                "sustituye_a" => RelacionTipo.SustituyeA,
                _ => RelacionTipo.Documenta
            };
            lista.Add(new Relationship
            {
                Id = Urn.Parse($"urn:rel:{Normalizar(origen)}:{Normalizar(destino)}"),
                Origen = Urn.Parse($"urn:entidad:{Normalizar(origen)}"),
                Destino = Urn.Parse($"urn:entidad:{Normalizar(destino)}"),
                Tipo = tipo
            });
        }
        return lista;
    }

    /// <summary>Fichas prioritarias F4 (18 campos textuales) desde el JSON derivado del MD.</summary>
    public static IReadOnlyDictionary<string, FichaPrioritaria> CargarFichasF4(string path)
    {
        using var doc = JsonDocument.Parse(File.ReadAllText(path));
        var mapa = new Dictionary<string, FichaPrioritaria>(StringComparer.OrdinalIgnoreCase);
        foreach (var e in doc.RootElement.GetProperty("fichas").EnumerateArray())
        {
            var campos = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var c in e.GetProperty("campos").EnumerateObject())
            {
                if (c.Value.ValueKind == JsonValueKind.Object &&
                    c.Value.TryGetProperty("Valor", out var val))
                    campos[c.Name] = val.GetString() ?? "";
            }
            mapa[e.GetProperty("acronimo").GetString()!] = new FichaPrioritaria
            {
                Id = e.GetProperty("id").GetString()!,
                Acronimo = e.GetProperty("acronimo").GetString()!,
                Nombre = e.GetProperty("nombre").GetString()!,
                Campos = campos
            };
        }
        return mapa;
    }

    public static IReadOnlyList<Device> CargarDispositivosF2(string path)
    {
        using var doc = JsonDocument.Parse(File.ReadAllText(path));
        var lista = new List<Device>();
        foreach (var e in doc.RootElement.GetProperty("dispositivos").EnumerateArray())
        {
            var planos = e.TryGetProperty("planos", out var p) && p.ValueKind == JsonValueKind.Array
                ? p.EnumerateArray().Select(x => x.GetString() ?? "").ToArray()
                : Array.Empty<string>();
            lista.Add(new Device
            {
                Id = Urn.Parse(e.GetProperty("id").GetString()!),
                Nombre = e.GetProperty("clase").GetString()!,
                Clase = e.GetProperty("clase").GetString()!,
                Capas = e.TryGetProperty("capas", out var c) ? c.GetString() : null,
                Planos = planos,
                Pdu = e.TryGetProperty("pdu", out var d) ? d.GetString() : null
            });
        }
        return lista;
    }

    public static IReadOnlyList<NetworkType> CargarRedesF2(string path)
    {
        using var doc = JsonDocument.Parse(File.ReadAllText(path));
        var lista = new List<NetworkType>();
        foreach (var e in doc.RootElement.GetProperty("redes").EnumerateArray())
        {
            lista.Add(new NetworkType
            {
                Id = Urn.Parse(e.GetProperty("id").GetString()!),
                Nombre = e.GetProperty("tipo").GetString()!,
                Ambito = e.GetProperty("ambito").GetString()!
            });
        }
        return lista;
    }

    /// <summary>Unidad de datos (PDU) de un protocolo catalogado en F5 (p. ej. TCP → "segmento").</summary>
    public static string? ObtenerPduF5(string path, string acronimo)
    {
        using var doc = JsonDocument.Parse(File.ReadAllText(path));
        foreach (var p in doc.RootElement.GetProperty("protocolos").EnumerateArray())
        {
            if (string.Equals(p.GetProperty("acronimo").GetString(), acronimo, StringComparison.OrdinalIgnoreCase)
                && p.TryGetProperty("pdu", out var pdu))
                return pdu.GetString();
        }
        return null;
    }

    /// <summary>Mapa acrónimo → valor de una propiedad del registro de seguridad de F6 (p. ej. "cifrado").</summary>
    public static IReadOnlyDictionary<string, string> CargarSeguridadF6(string path, string propiedad)
    {
        using var doc = JsonDocument.Parse(File.ReadAllText(path));
        var mapa = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var e in doc.RootElement.GetProperty("protocolos").EnumerateArray())
        {
            var acronimo = e.TryGetProperty("acronimo", out var a) ? a.GetString() : null;
            if (acronimo is null || !e.TryGetProperty(propiedad, out var v)) continue;
            mapa[acronimo!] = v.GetString() ?? "";
        }
        return mapa;
    }

    private static string Normalizar(string s)
        => string.Concat(s.Where(char.IsLetterOrDigit)).ToLowerInvariant();
}