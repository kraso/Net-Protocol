using Microsoft.Data.Sqlite;
using Redes.Knowledge.Domain;
using Redes.Knowledge.Infrastructure;

namespace Redes.Knowledge.Tests;

/// <summary>Pruebas de integración sobre SQLite en archivo temporal (D1-2).</summary>
public class RepositoryTests : IDisposable
{
    private readonly string _archivo = Path.Combine(Path.GetTempPath(), $"rk_{Guid.NewGuid():N}.db");
    private readonly SqliteKnowledgeStore _store;

    public RepositoryTests()
    {
        // Pooling=False: cierra el archivo al final de cada operación (evita bloqueo en Dispose).
        _store = new SqliteKnowledgeStore($"Data Source={_archivo};Pooling=False");
    }

    [Fact]
    public void Guardar_Y_Obtener_PorUrn()
    {
        var repo = new SqliteProtocolRepository(_store);

        var p = Protocolo("TCP");
        repo.Save(p);

        var deVuelta = repo.GetByUrn(p.Id);
        Assert.NotNull(deVuelta);
        Assert.Equal("TCP", deVuelta!.Acronimo);
        Assert.Equal(FamiliaProtocolo.TRAN, deVuelta.Familia);
    }

    [Fact]
    public void Guardar_Duplicado_Actualiza()
    {
        var repo = new SqliteProtocolRepository(_store);
        var p = Protocolo("TCP");
        repo.Save(p);
        var p2 = p with { Nombre = "Transmission Control Protocol (actualizado)" };
        repo.Save(p2);

        Assert.Single(repo.GetAll());
        Assert.Equal(p2.Nombre, repo.GetByUrn(p.Id)!.Nombre);
    }

    [Fact]
    public void Buscar_PorFamilia()
    {
        var repo = new SqliteProtocolRepository(_store);
        repo.Save(Protocolo("TCP"));
        repo.Save(Protocolo("UDP"));

        Assert.Equal(2, repo.GetByFamilia(FamiliaProtocolo.TRAN).Count);
    }

    [Fact]
    public void Eliminar()
    {
        var repo = new SqliteProtocolRepository(_store);
        var p = Protocolo("TCP");
        repo.Save(p);

        Assert.True(repo.Delete(p.Id));
        Assert.Null(repo.GetByUrn(p.Id));
    }

    [Fact]
    public void Busqueda_Fts_Encuentra_Tcp()
    {
        var repo = new SqliteProtocolRepository(_store);
        repo.Save(Protocolo("TCP"));

        var hits = new SqliteSearchEngine(_store).Search("tcp");
        Assert.Contains(hits, h => h.Acronimo == "TCP");
    }

    [Fact]
    public void Busqueda_Fts_PorNombre()
    {
        var repo = new SqliteProtocolRepository(_store);
        repo.Save(Protocolo("TCP"));

        var hits = new SqliteSearchEngine(_store).Search("Transmission");
        Assert.Contains(hits, h => h.Acronimo == "TCP");
    }

    private static Protocol Protocolo(string acronimo) => new()
    {
        Id = Urn.Protocol("TRAN", acronimo),
        Nombre = acronimo switch
        {
            "TCP" => "Transmission Control Protocol",
            "UDP" => "User Datagram Protocol",
            _ => $"Protocolo {acronimo}"
        },
        Acronimo = acronimo,
        Familia = FamiliaProtocolo.TRAN,
        Estado = LifecycleState.Vigente
    };

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        if (File.Exists(_archivo)) File.Delete(_archivo);
    }
}