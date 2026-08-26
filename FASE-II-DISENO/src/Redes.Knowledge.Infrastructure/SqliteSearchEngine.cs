using Microsoft.Data.Sqlite;
using Redes.Knowledge.Domain;

namespace Redes.Knowledge.Infrastructure;

/// <summary>Búsqueda textual sobre el índice FTS5 (D1-2 / MOD-04).</summary>
public sealed class SqliteSearchEngine : ISearchEngine
{
    private readonly SqliteKnowledgeStore _store;

    public SqliteSearchEngine(SqliteKnowledgeStore store) => _store = store;

    public IReadOnlyList<SearchHit> Search(string query, int limite = 50)
    {
        if (string.IsNullOrWhiteSpace(query)) return Array.Empty<SearchHit>();

        using var connection = _store.Open();
        using var cmd = connection.CreateCommand();
        // Escapado básico: cada término se encierra entre comillas (frase/término) para FTS5.
        var patron = "\"" + query.Trim().Replace("\"", "\"\"") + "\"";
        cmd.CommandText = "SELECT urn, nombre, acronimo, familia FROM ProtocolsFts WHERE ProtocolsFts MATCH $q ORDER BY rank LIMIT $lim;";
        cmd.Parameters.AddWithValue("$q", patron);
        cmd.Parameters.AddWithValue("$lim", limite);
        using var reader = cmd.ExecuteReader();
        var resultados = new List<SearchHit>();
        while (reader.Read())
        {
            resultados.Add(new SearchHit(
                reader.GetString(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.GetString(3)));
        }
        return resultados;
    }
}