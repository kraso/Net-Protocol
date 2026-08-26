using Microsoft.Data.Sqlite;

namespace Redes.Knowledge.Infrastructure;

/// <summary>
/// Almacén SQLite con migraciones versionadas (ADR-002). Crea el esquema de
/// persistencia y el índice FTS5 para búsqueda textual.
/// </summary>
public sealed class SqliteKnowledgeStore
{
    public string ConnectionString { get; }

    public SqliteKnowledgeStore(string connectionString)
    {
        ConnectionString = connectionString;
        Migrate();
    }

    /// <summary>Migraciones versionadas (esquema mínimo D1).</summary>
    public void Migrate()
    {
        using var connection = Open();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            CREATE TABLE IF NOT EXISTS Protocols (
                urn        TEXT PRIMARY KEY,
                nombre     TEXT NOT NULL,
                acronimo   TEXT NOT NULL,
                familia    TEXT NOT NULL,
                estado     TEXT NOT NULL,
                capas      TEXT NULL,
                valid_from TEXT NULL,
                valid_to   TEXT NULL
            );
            CREATE VIRTUAL TABLE IF NOT EXISTS ProtocolsFts USING fts5(
                urn UNINDEXED, nombre, acronimo, familia
            );
            CREATE TABLE IF NOT EXISTS Fields (
                urn            TEXT PRIMARY KEY,
                protocolo_urn  TEXT NOT NULL,
                nombre         TEXT NOT NULL,
                offset_bits    INTEGER NULL,
                longitud_bits  INTEGER NULL,
                tipo           TEXT NOT NULL,
                semantica      TEXT NOT NULL,
                obligatorio    INTEGER NOT NULL,
                FOREIGN KEY (protocolo_urn) REFERENCES Protocols(urn)
            );
            CREATE TABLE IF NOT EXISTS Sources (
                urn               TEXT PRIMARY KEY,
                titulo            TEXT NOT NULL,
                url               TEXT NOT NULL,
                version           TEXT NOT NULL,
                organismo         TEXT NOT NULL,
                fecha_publicacion TEXT NULL,
                fecha_consulta    TEXT NULL,
                nivel             INTEGER NOT NULL,
                confianza         TEXT NOT NULL
            );
            CREATE TABLE IF NOT EXISTS Services (
                urn            TEXT PRIMARY KEY,
                nombre         TEXT NOT NULL,
                puerto         INTEGER NULL,
                transporte     TEXT NOT NULL,
                descripcion    TEXT NOT NULL,
                referencia     TEXT NOT NULL,
                fecha_registro TEXT NULL,
                fecha_consulta TEXT NOT NULL
            );
            """;
        cmd.ExecuteNonQuery();
    }

    internal SqliteConnection Open()
    {
        var connection = new SqliteConnection(ConnectionString);
        connection.Open();
        return connection;
    }
}