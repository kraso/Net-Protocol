using Redes.Knowledge.Domain;
using Redes.Knowledge.Infrastructure;

namespace Redes.Knowledge.Tests;

/// <summary>
/// L-005: asignación formal de <see cref="Source"/> de nivel 1 a las fichas del catálogo,
/// con métrica de cobertura por registro y objetivo ≥ 90 %.
/// </summary>
public class FuentesFichasTests
{
    private static string RaizDelRepositorio()
    {
        var d = new DirectoryInfo(AppContext.BaseDirectory);
        while (d is not null && !File.Exists(Path.Combine(d.FullName, "PLANREDES.md")))
            d = d.Parent;
        return d?.FullName ?? throw new DirectoryNotFoundException("Raíz del repositorio no encontrada.");
    }

    private static string RutaF8() =>
        Path.Combine(RaizDelRepositorio(), "FASE-08-VALIDACION", "F8-Fuentes-Fichas.json");

    private static (IReadOnlyDictionary<string, IReadOnlyList<FuenteFicha>> PorAcronimo, CoberturaFuentes Cobertura)
        Cargar() => FuentesFichas.Cargar(RutaF8());

    [Fact]
    public void Catalogo_Existe_Y_Se_Carga()
    {
        Assert.True(File.Exists(RutaF8()), "Falta F8-Fuentes-Fichas.json (ejecutar derivar-fuentes.py).");
        var (porAcronimo, cobertura) = Cargar();
        Assert.NotEmpty(porAcronimo);
        Assert.True(cobertura.FuentesDistintas > 0);
    }

    [Fact]
    public void Cobertura_Nivel1_Cumple_Objetivo_Del_90()
    {
        var (_, c) = Cargar();
        Assert.Equal(90.0, c.ObjetivoPct);
        Assert.True(c.CoberturaNivel1Pct >= c.ObjetivoPct,
            $"Cobertura {c.CoberturaNivel1Pct} % por debajo del objetivo {c.ObjetivoPct} %");
        Assert.True(c.ObjetivoCumplido);
    }

    [Fact]
    public void Metricas_Coinciden_Con_Las_Asignaciones()
    {
        var (porAcronimo, c) = Cargar();
        Assert.Equal(porAcronimo.Count, c.FichasTotales);
        Assert.Equal(porAcronimo.Count(kv => kv.Value.Count > 0), c.FichasConFuente);
        Assert.Equal(
            porAcronimo.Count(kv => kv.Value.Any(f => f.Nivel == NivelAutoridad.PrimariaNormativa)),
            c.FichasConNivel1);
    }

    [Fact]
    public void Cada_Fuente_Tiene_Identidad_Completa()
    {
        var (porAcronimo, _) = Cargar();
        foreach (var f in porAcronimo.Values.SelectMany(v => v).DistinctBy(f => f.Id))
        {
            Assert.False(string.IsNullOrWhiteSpace(f.Id));
            Assert.False(string.IsNullOrWhiteSpace(f.Titulo));
            Assert.False(string.IsNullOrWhiteSpace(f.Organismo));
            Assert.False(string.IsNullOrWhiteSpace(f.Version));
            Assert.StartsWith("http", f.Url);
            Assert.NotNull(f.FechaConsulta);
            Assert.True(Enum.IsDefined(f.Nivel));
        }
    }

    [Fact]
    public void Fuentes_Pasan_El_Validador_De_Dominio()
    {
        var (porAcronimo, _) = Cargar();
        foreach (var f in porAcronimo.Values.SelectMany(v => v).DistinctBy(f => f.Id))
        {
            var r = SourceValidator.Validate(f.ADominio());
            Assert.True(r.IsValid, $"{f.Id}: {string.Join("; ", r.Errors)}");
        }
    }

    [Fact]
    public void Protocolos_Conocidos_Tienen_Su_RFC_Nivel1()
    {
        var (porAcronimo, _) = Cargar();

        var tcp = porAcronimo["TCP"];
        Assert.Contains(tcp, f => f.Nivel == NivelAutoridad.PrimariaNormativa && f.Version.Contains("9293"));

        var dns = porAcronimo["DNS"];
        Assert.Contains(dns, f => f.Version.Contains("1035"));

        var quic = porAcronimo["QUIC"];
        Assert.Contains(quic, f => f.Version.Contains("9000"));
    }

    [Fact]
    public void Fichas_Sin_Nivel1_Se_Declaran_Explicitamente()
    {
        var (porAcronimo, _) = Cargar();
        var sinNivel1 = porAcronimo
            .Where(kv => kv.Value.All(f => f.Nivel != NivelAutoridad.PrimariaNormativa))
            .Select(kv => kv.Key)
            .OrderBy(k => k, StringComparer.Ordinal)
            .ToArray();

        // Documentación de fabricante o proyecto (nivel 2): no hay norma primaria que citar.
        Assert.Equal(
            new[] { "AppleTalk", "IPX/SPX", "NetBEUI", "NetFlow", "SMB", "gRPC" }
                .OrderBy(k => k, StringComparer.Ordinal),
            sinNivel1);
    }

    [Fact]
    public void Sin_Fuentes_Sin_Clasificar()
    {
        var (porAcronimo, _) = Cargar();
        var huerfanas = porAcronimo.Values.SelectMany(v => v)
            .Where(f => f.Organismo is "" or "Sin clasificar")
            .Select(f => f.Id)
            .Distinct()
            .ToArray();
        Assert.Empty(huerfanas);
    }
}
