using Microsoft.Data.Sqlite;
using Redes.Knowledge.Infrastructure;
using Redes.Knowledge.Infrastructure.Iana;

namespace Redes.Knowledge.Tests;

/// <summary>D2-1: importador del registro real de IANA (fixture versionado) + persistencia en lote.</summary>
public class IanaImporterTests : IDisposable
{
    private readonly string _archivo = Path.Combine(Path.GetTempPath(), $"rk_iana_{Guid.NewGuid():N}.db");

    private static string RaizDelRepositorio()
    {
        var d = new DirectoryInfo(AppContext.BaseDirectory);
        while (d is not null && !File.Exists(Path.Combine(d.FullName, "PLANREDES.md")))
            d = d.Parent;
        return d?.FullName ?? throw new InvalidOperationException("No se encontró la raíz del repositorio.");
    }

    private static string FixtureIana => Path.Combine(
        RaizDelRepositorio(), "FASE-II-DISENO", "data", "iana-service-names-port-numbers-2026-08-26.csv");

    private static readonly DateTime FechaConsulta = new(2026, 8, 26);

    [Fact]
    public void Importa_Registro_Real_Completo()
    {
        var r = IanaServiceImporter.Importar(FixtureIana, FechaConsulta);
        Assert.True(r.TotalFilas >= 15000, $"TotalFilas={r.TotalFilas}");
        Assert.True(r.Importados > 12000, $"Importados={r.Importados}");
        Assert.True(r.SinNombre > 1000, $"SinNombre={r.SinNombre}");
        Assert.Equal(FechaConsulta, r.FechaConsulta);
    }

    [Fact]
    public void Servicios_Conocidos_Presentes()
    {
        var r = IanaServiceImporter.Importar(FixtureIana, FechaConsulta);
        Assert.Contains(r.Entradas, e => e.ServiceName == "ssh" && e.Port == 22 && e.Transport == "TCP");
        Assert.Contains(r.Entradas, e => e.ServiceName == "domain" && e.Port == 53 && e.Transport == "UDP");
        Assert.Contains(r.Entradas, e => e.ServiceName == "https" && e.Port == 443 && e.Transport == "TCP");
    }

    [Fact]
    public void Deduplicacion_PorClave()
    {
        var r = IanaServiceImporter.Importar(FixtureIana, FechaConsulta);
        var unicos = r.Entradas.Select(e => (e.ServiceName, e.Port, e.Transport)).Distinct().Count();
        Assert.Equal(r.Importados, unicos);
    }

    [Fact]
    public void Cabecera_Invalida_Lanza()
    {
        var tmp = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tmp, "foo,bar\n1,2\n");
            Assert.Throws<InvalidDataException>(() => IanaServiceImporter.Importar(tmp, FechaConsulta));
        }
        finally
        {
            File.Delete(tmp);
        }
    }

    [Fact]
    public void Persistencia_EnLote_Y_Consulta()
    {
        var store = new SqliteKnowledgeStore($"Data Source={_archivo};Pooling=False");
        var repo = new SqliteServiceRepository(store);
        var r = IanaServiceImporter.Importar(FixtureIana, FechaConsulta);

        var insertados = repo.ReemplazarEInsertar(r.Entradas, FechaConsulta);
        Assert.Equal(r.Importados, insertados);
        Assert.Equal(r.Importados, repo.Contar());

        var ssh = repo.Buscar("ssh", "TCP", 22);
        Assert.NotNull(ssh);
        Assert.Equal("ssh", ssh!.ServiceName);

        var en443 = repo.PorPuerto(443);
        Assert.Contains(en443, e => e.ServiceName == "https");
    }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        if (File.Exists(_archivo)) File.Delete(_archivo);
    }
}