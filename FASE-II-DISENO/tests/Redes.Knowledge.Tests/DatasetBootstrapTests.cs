using Microsoft.Data.Sqlite;
using Redes.Knowledge.Domain;
using Redes.Knowledge.Infrastructure;

namespace Redes.Knowledge.Tests;

/// <summary>D3: bootstrap del dataset y escenarios de búsqueda/filtros sobre el catálogo real.</summary>
public class DatasetBootstrapTests : IDisposable
{
    private readonly string _archivo = Path.Combine(Path.GetTempPath(), $"rk_boot_{Guid.NewGuid():N}.db");

    private static string RaizDelRepositorio()
    {
        var d = new DirectoryInfo(AppContext.BaseDirectory);
        while (d is not null && !File.Exists(Path.Combine(d.FullName, "PLANREDES.md")))
            d = d.Parent;
        return d?.FullName ?? throw new InvalidOperationException("No se encontró la raíz del repositorio.");
    }

    private static string F3Json => Path.Combine(RaizDelRepositorio(), "FASE-03-INVENTARIO", "F3-Protocolos.json");

    [Fact]
    public void Bootstrap_Carga_113_Idempotente()
    {
        var store = new SqliteKnowledgeStore($"Data Source={_archivo};Pooling=False");
        var repo = new SqliteProtocolRepository(store);

        var primera = DatasetBootstrap.EnsureProtocolos(store, F3Json);
        Assert.Equal(113, primera);
        Assert.Equal(113, repo.GetAll().Count);

        var segunda = DatasetBootstrap.EnsureProtocolos(store, F3Json);
        Assert.Equal(0, segunda); // idempotente: ya cargado
        Assert.Equal(113, repo.GetAll().Count);
    }

    [Fact]
    public void Busqueda_Fts_Sobre_Catalogo_Real()
    {
        var store = new SqliteKnowledgeStore($"Data Source={_archivo};Pooling=False");
        DatasetBootstrap.EnsureProtocolos(store, F3Json);
        var busqueda = new SqliteSearchEngine(store);

        Assert.Contains(busqueda.Search("tcp"), h => h.Acronimo == "TCP");
        Assert.Contains(busqueda.Search("Transmission"), h => h.Acronimo == "TCP");
        Assert.Contains(busqueda.Search("border"), h => h.Acronimo == "BGP");
    }

    [Fact]
    public void Filtro_porFamilia_Y_Estado()
    {
        var store = new SqliteKnowledgeStore($"Data Source={_archivo};Pooling=False");
        DatasetBootstrap.EnsureProtocolos(store, F3Json);
        var repo = new SqliteProtocolRepository(store);

        var trans = repo.GetByFamilia(FamiliaProtocolo.TRAN);
        Assert.Contains(trans, p => p.Acronimo == "TCP");
        Assert.Contains(trans, p => p.Acronimo == "QUIC");

        var historicos = repo.GetAll().Where(p => p.Estado == LifecycleState.Historico).ToList();
        Assert.Contains(historicos, p => p.Acronimo == "X.25");
        Assert.Contains(historicos, p => p.Acronimo == "Token Ring");
        Assert.True(historicos.Count >= 10);
    }

    [Fact]
    public void Ignora_Estado_NoMapeado_SinRomper_Import()
    {
        var store = new SqliteKnowledgeStore($"Data Source={_archivo};Pooling=False");
        DatasetBootstrap.EnsureProtocolos(store, F3Json);
        // military_public no está en el enum → se mapea a Desconocido sin romper el import
        var repo = new SqliteProtocolRepository(store);
        var link16 = repo.GetByUrn(Urn.Parse("PR-099"));
        Assert.NotNull(link16);
        Assert.Equal(LifecycleState.Desconocido, link16!.Estado);
    }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        if (File.Exists(_archivo)) File.Delete(_archivo);
    }
}