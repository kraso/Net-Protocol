using Microsoft.Data.Sqlite;
using Redes.Knowledge.Domain;

namespace Redes.Knowledge.Infrastructure;

/// <summary>Repositorio de Protocol sobre SQLite (D1-2): CRUD + sincronización del índice FTS5.</summary>
public sealed class SqliteProtocolRepository : IProtocolRepository
{
    private readonly SqliteKnowledgeStore _store;

    public SqliteProtocolRepository(SqliteKnowledgeStore store) => _store = store;

    public Urn Save(Protocol protocol)
    {
        using var connection = _store.Open();
        using var tx = connection.BeginTransaction();

        // Microsoft.Data.Sqlite no permite que un lote de sentencias omite parámetros declarados:
        // se ejecutan sentencias separadas dentro de la misma transacción.
        using (var delFts = connection.CreateCommand())
        {
            delFts.Transaction = tx;
            delFts.CommandText = "DELETE FROM ProtocolsFts WHERE urn = $urn;";
            delFts.Parameters.AddWithValue("$urn", protocol.Id.Value);
            delFts.ExecuteNonQuery();
        }

        using (var del = connection.CreateCommand())
        {
            del.Transaction = tx;
            del.CommandText = "DELETE FROM Protocols WHERE urn = $urn;";
            del.Parameters.AddWithValue("$urn", protocol.Id.Value);
            del.ExecuteNonQuery();
        }

        using (var ins = connection.CreateCommand())
        {
            ins.Transaction = tx;
            ins.CommandText = """
                INSERT INTO Protocols (urn, nombre, acronimo, familia, estado, capas, valid_from, valid_to)
                VALUES ($urn, $nombre, $acronimo, $familia, $estado, $capas, $from, $to);
                """;
            ins.Parameters.AddWithValue("$urn", protocol.Id.Value);
            ins.Parameters.AddWithValue("$nombre", protocol.Nombre);
            ins.Parameters.AddWithValue("$acronimo", protocol.Acronimo);
            ins.Parameters.AddWithValue("$familia", protocol.Familia.ToString());
            ins.Parameters.AddWithValue("$estado", protocol.Estado.ToString());
            ins.Parameters.AddWithValue("$capas", (object?)protocol.Capas ?? DBNull.Value);
            ins.Parameters.AddWithValue("$from", (object?)Fecha(protocol.ValidFrom) ?? DBNull.Value);
            ins.Parameters.AddWithValue("$to", (object?)Fecha(protocol.ValidTo) ?? DBNull.Value);
            ins.ExecuteNonQuery();
        }

        using (var fts = connection.CreateCommand())
        {
            fts.Transaction = tx;
            fts.CommandText = "INSERT INTO ProtocolsFts (urn, nombre, acronimo, familia) VALUES ($urn, $nombre, $acronimo, $familia);";
            fts.Parameters.AddWithValue("$urn", protocol.Id.Value);
            fts.Parameters.AddWithValue("$nombre", protocol.Nombre);
            fts.Parameters.AddWithValue("$acronimo", protocol.Acronimo);
            fts.Parameters.AddWithValue("$familia", protocol.Familia.ToString());
            fts.ExecuteNonQuery();
        }

        tx.Commit();
        return protocol.Id;
    }

    public Protocol? GetByUrn(Urn urn)
    {
        using var connection = _store.Open();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT urn, nombre, acronimo, familia, estado, capas, valid_from, valid_to FROM Protocols WHERE urn = $urn;";
        cmd.Parameters.AddWithValue("$urn", urn.Value);
        using var reader = cmd.ExecuteReader();
        return reader.Read() ? Leer(reader) : null;
    }

    public IReadOnlyList<Protocol> GetAll()
    {
        using var connection = _store.Open();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT urn, nombre, acronimo, familia, estado, capas, valid_from, valid_to FROM Protocols ORDER BY acronimo;";
        using var reader = cmd.ExecuteReader();
        var lista = new List<Protocol>();
        while (reader.Read()) lista.Add(Leer(reader));
        return lista;
    }

    public IReadOnlyList<Protocol> GetByFamilia(FamiliaProtocolo familia)
    {
        using var connection = _store.Open();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT urn, nombre, acronimo, familia, estado, capas, valid_from, valid_to FROM Protocols WHERE familia = $familia ORDER BY acronimo;";
        cmd.Parameters.AddWithValue("$familia", familia.ToString());
        using var reader = cmd.ExecuteReader();
        var lista = new List<Protocol>();
        while (reader.Read()) lista.Add(Leer(reader));
        return lista;
    }

    public bool Delete(Urn urn)
    {
        using var connection = _store.Open();
        using (var cmdFts = connection.CreateCommand())
        {
            cmdFts.CommandText = "DELETE FROM ProtocolsFts WHERE urn = $urn;";
            cmdFts.Parameters.AddWithValue("$urn", urn.Value);
            cmdFts.ExecuteNonQuery();
        }
        using (var cmd = connection.CreateCommand())
        {
            cmd.CommandText = "DELETE FROM Protocols WHERE urn = $urn;";
            cmd.Parameters.AddWithValue("$urn", urn.Value);
            return cmd.ExecuteNonQuery() > 0;
        }
    }

    private static Protocol Leer(SqliteDataReader reader) => new()
    {
        Id = Urn.Parse(reader.GetString(0)),
        Nombre = reader.GetString(1),
        Acronimo = reader.GetString(2),
        Familia = Enum.TryParse<FamiliaProtocolo>(reader.GetString(3), out var f) ? f : FamiliaProtocolo.HIST,
        Estado = Enum.TryParse<LifecycleState>(reader.GetString(4), out var e) ? e : LifecycleState.Desconocido,
        Capas = reader.IsDBNull(5) ? null : reader.GetString(5),
        ValidFrom = FechaNull(reader, 6),
        ValidTo = FechaNull(reader, 7)
    };

    private static object? Fecha(DateTime? valor) => valor is { } v ? v.ToString("yyyy-MM-dd") : null;

    private static DateTime? FechaNull(SqliteDataReader reader, int indice)
        => reader.IsDBNull(indice) ? null : DateTime.Parse(reader.GetString(indice));
}