using System.Globalization;
using Microsoft.Data.Sqlite;
using Redes.Knowledge.Infrastructure.Iana;

namespace Redes.Knowledge.Infrastructure;

/// <summary>Persistencia de los servicios del registro IANA (D2-1).</summary>
public sealed class SqliteServiceRepository
{
    private readonly SqliteKnowledgeStore _store;

    public SqliteServiceRepository(SqliteKnowledgeStore store) => _store = store;

    /// <summary>Inserción en lote dentro de una única transacción (reemplaza el contenido previo).</summary>
    public int ReemplazarEInsertar(IReadOnlyList<IanaServiceEntry> entradas, DateTime fechaConsulta)
    {
        using var connection = _store.Open();
        using var tx = connection.BeginTransaction();

        using (var del = connection.CreateCommand())
        {
            del.Transaction = tx;
            del.CommandText = "DELETE FROM Services;";
            del.ExecuteNonQuery();
        }

        using var ins = connection.CreateCommand();
        ins.Transaction = tx;
        ins.CommandText = """
            INSERT INTO Services (urn, nombre, puerto, transporte, descripcion, referencia, fecha_registro, fecha_consulta)
            VALUES ($urn, $nombre, $puerto, $transporte, $descripcion, $referencia, $fechaRegistro, $fechaConsulta);
            """;
        var u = ins.Parameters.Add("$urn", SqliteType.Text);
        var n = ins.Parameters.Add("$nombre", SqliteType.Text);
        var p = ins.Parameters.Add("$puerto", SqliteType.Integer);
        var t = ins.Parameters.Add("$transporte", SqliteType.Text);
        var d = ins.Parameters.Add("$descripcion", SqliteType.Text);
        var r = ins.Parameters.Add("$referencia", SqliteType.Text);
        var fr = ins.Parameters.Add("$fechaRegistro", SqliteType.Text);
        var fc = ins.Parameters.Add("$fechaConsulta", SqliteType.Text);

        foreach (var e in entradas)
        {
            u.Value = UrnServicio(e, fechaConsulta);
            n.Value = e.ServiceName;
            p.Value = (object?)e.Port ?? DBNull.Value;
            t.Value = e.Transport;
            d.Value = e.Description;
            r.Value = e.Reference;
            fr.Value = string.IsNullOrWhiteSpace(e.RegistrationDate) ? DBNull.Value : e.RegistrationDate;
            fc.Value = fechaConsulta.ToString("yyyy-MM-dd");
            ins.ExecuteNonQuery();
        }

        tx.Commit();
        return entradas.Count;
    }

    public int Contar()
    {
        using var connection = _store.Open();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM Services;";
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    public IanaServiceEntry? Buscar(string nombre, string transporte, int puerto)
    {
        using var connection = _store.Open();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT nombre, puerto, transporte, descripcion, referencia, fecha_registro FROM Services WHERE nombre = $n AND transporte = $t AND puerto = $p LIMIT 1;";
        cmd.Parameters.AddWithValue("$n", nombre.ToLowerInvariant());
        cmd.Parameters.AddWithValue("$t", transporte.ToUpperInvariant());
        cmd.Parameters.AddWithValue("$p", puerto);
        using var reader = cmd.ExecuteReader();
        return reader.Read()
            ? new IanaServiceEntry(reader.GetString(0), reader.IsDBNull(1) ? null : reader.GetInt32(1), reader.GetString(2), reader.GetString(3), reader.GetString(4), reader.IsDBNull(5) ? "" : reader.GetString(5))
            : null;
    }

    public IReadOnlyList<IanaServiceEntry> PorPuerto(int puerto, int limite = 200)
    {
        using var connection = _store.Open();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT nombre, puerto, transporte, descripcion, referencia, fecha_registro FROM Services WHERE puerto = $p ORDER BY nombre LIMIT $lim;";
        cmd.Parameters.AddWithValue("$p", puerto);
        cmd.Parameters.AddWithValue("$lim", limite);
        using var reader = cmd.ExecuteReader();
        var lista = new List<IanaServiceEntry>();
        while (reader.Read())
            lista.Add(new IanaServiceEntry(reader.GetString(0), reader.IsDBNull(1) ? null : reader.GetInt32(1), reader.GetString(2), reader.GetString(3), reader.GetString(4), reader.IsDBNull(5) ? "" : reader.GetString(5)));
        return lista;
    }

    public IReadOnlyList<IanaServiceEntry> PorNombre(string nombre, int limite = 50)
    {
        using var connection = _store.Open();
        using var cmd = connection.CreateCommand();
        // Coincidencia de prefijo: "netbios" -> netbios-ns/dgm/ssn; "ssh" -> ssh; "http" -> http, http-alt…
        cmd.CommandText = "SELECT nombre, puerto, transporte, descripcion, referencia, fecha_registro FROM Services WHERE nombre = $n OR nombre LIKE $p ORDER BY puerto LIMIT $lim;";
        cmd.Parameters.AddWithValue("$n", nombre.ToLowerInvariant());
        cmd.Parameters.AddWithValue("$p", nombre.ToLowerInvariant() + "%");
        cmd.Parameters.AddWithValue("$lim", limite);
        using var reader = cmd.ExecuteReader();
        var lista = new List<IanaServiceEntry>();
        while (reader.Read())
            lista.Add(new IanaServiceEntry(reader.GetString(0), reader.IsDBNull(1) ? null : reader.GetInt32(1), reader.GetString(2), reader.GetString(3), reader.GetString(4), reader.IsDBNull(5) ? "" : reader.GetString(5)));
        return lista;
    }

    private static string UrnServicio(IanaServiceEntry e, DateTime fechaConsulta)
        => $"urn:iana:{e.ServiceName}:{(e.Transport ?? "").ToLowerInvariant()}:{(e.Port?.ToString(CultureInfo.InvariantCulture) ?? "sinpuerto")}:{fechaConsulta:yyyyMMdd}";
}