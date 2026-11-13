using System.Text.Json;
using Redes.Knowledge.Domain;

namespace Redes.Knowledge.Infrastructure;

/// <summary>Fuente normalizada asignada a una ficha (L-005).</summary>
public sealed record FuenteFicha
{
    public string Id { get; init; } = "";
    public string Titulo { get; init; } = "";
    public string Url { get; init; } = "";
    public string Version { get; init; } = "";
    public string Organismo { get; init; } = "";
    public NivelAutoridad Nivel { get; init; }
    public DateTime? FechaConsulta { get; init; }

    /// <summary>Proyección al agregado de dominio, para validarlo con <see cref="SourceValidator"/>.</summary>
    public Source ADominio() => new()
    {
        Id = new Urn(Id),
        Titulo = Titulo,
        Url = Url,
        Version = Version,
        Organismo = Organismo,
        FechaConsulta = FechaConsulta,
        Nivel = Nivel,
        Confianza = Confianza.Alto
    };
}

/// <summary>Cobertura de fuentes por registro y global (métrica de L-005).</summary>
public sealed record CoberturaFuentes
{
    public int FichasTotales { get; init; }
    public int FichasConFuente { get; init; }
    public int FichasConNivel1 { get; init; }
    public double CoberturaNivel1Pct { get; init; }
    public double ObjetivoPct { get; init; }
    public int FuentesDistintas { get; init; }

    public bool ObjetivoCumplido => CoberturaNivel1Pct >= ObjetivoPct;
}

/// <summary>
/// Catálogo de fuentes derivado del campo 18 de las fichas F4 (laguna L-005).
/// Lo genera <c>FASE-08-VALIDACION/derivar-fuentes.py</c>; aquí solo se lee.
/// </summary>
public static class FuentesFichas
{
    public static (IReadOnlyDictionary<string, IReadOnlyList<FuenteFicha>> PorAcronimo, CoberturaFuentes Cobertura)
        Cargar(string jsonPath)
    {
        using var doc = JsonDocument.Parse(File.ReadAllText(jsonPath));
        var raiz = doc.RootElement;

        var porId = new Dictionary<string, FuenteFicha>(StringComparer.OrdinalIgnoreCase);
        foreach (var f in raiz.GetProperty("fuentes").EnumerateArray())
        {
            var id = f.GetProperty("id").GetString() ?? "";
            porId[id] = new FuenteFicha
            {
                Id = id,
                Titulo = f.GetProperty("titulo").GetString() ?? "",
                Url = f.GetProperty("url").GetString() ?? "",
                Version = f.GetProperty("version").GetString() ?? "",
                Organismo = f.GetProperty("organismo").GetString() ?? "",
                Nivel = (NivelAutoridad)f.GetProperty("nivel_autoridad").GetInt32(),
                FechaConsulta = LeerFecha(f, "fecha_consulta")
            };
        }

        var porAcronimo = new Dictionary<string, IReadOnlyList<FuenteFicha>>(StringComparer.OrdinalIgnoreCase);
        foreach (var a in raiz.GetProperty("asignaciones").EnumerateArray())
        {
            var acronimo = a.GetProperty("acronimo").GetString() ?? "";
            var lista = new List<FuenteFicha>();
            foreach (var idRef in a.GetProperty("fuentes").EnumerateArray())
            {
                var id = idRef.GetString() ?? "";
                if (porId.TryGetValue(id, out var fuente)) lista.Add(fuente);
            }
            porAcronimo[acronimo] = lista;
        }

        var m = raiz.GetProperty("metricas");
        var cobertura = new CoberturaFuentes
        {
            FichasTotales = m.GetProperty("fichas_totales").GetInt32(),
            FichasConFuente = m.GetProperty("fichas_con_fuente").GetInt32(),
            FichasConNivel1 = m.GetProperty("fichas_con_nivel1").GetInt32(),
            CoberturaNivel1Pct = m.GetProperty("cobertura_nivel1_pct").GetDouble(),
            ObjetivoPct = raiz.GetProperty("objetivo_cobertura").GetDouble(),
            FuentesDistintas = m.GetProperty("fuentes_distintas").GetInt32()
        };

        return (porAcronimo, cobertura);
    }

    private static DateTime? LeerFecha(JsonElement e, string nombre)
        => e.TryGetProperty(nombre, out var v) &&
           v.ValueKind == JsonValueKind.String &&
           DateTime.TryParse(v.GetString(), out var d)
            ? d
            : null;
}
