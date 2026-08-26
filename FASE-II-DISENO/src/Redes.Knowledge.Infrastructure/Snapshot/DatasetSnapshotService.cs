using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Redes.Knowledge.Infrastructure.Snapshot;

/// <summary>
/// Snapshots versionados del dataset (D2-3 / ADR-004): manifiesto inmutable {fecha, procedencia,
/// hash agregado, archivos con hash}, copia de contenidos bajo files/, diff y rollback con verificación.
/// El hash agregado NO incluye la fecha → dos directorios idénticos producen el mismo manifiesto (determinismo).
/// </summary>
public static class DatasetSnapshotService
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public static SnapshotManifest Crear(string sourceDir, string snapshotDir, string procedencia)
    {
        var rutas = Directory.EnumerateFiles(sourceDir, "*", SearchOption.AllDirectories)
            .Where(p => !Path.GetFileName(p).EndsWith(".snapshot.json", StringComparison.OrdinalIgnoreCase))
            .OrderBy(p => Path.GetRelativePath(sourceDir, p), StringComparer.Ordinal)
            .ToList();

        var archivos = new List<SnapshotFile>();
        foreach (var r in rutas)
        {
            var bytes = File.ReadAllBytes(r);
            archivos.Add(new SnapshotFile(
                Path.GetRelativePath(sourceDir, r),
                bytes.Length,
                Convert.ToHexString(SHA256.HashData(bytes))));
        }

        var builder = new StringBuilder();
        foreach (var a in archivos) builder.Append(a.Ruta).Append('|').Append(a.Sha256).Append('|');
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(builder.ToString())));

        var manifiesto = new SnapshotManifest(
            "1",
            DateTime.UtcNow.ToString("s"),
            procedencia,
            hash,
            archivos,
            archivos.Sum(a => a.Bytes));

        Directory.CreateDirectory(snapshotDir);
        foreach (var a in archivos)
        {
            var origen = Path.Combine(sourceDir, a.Ruta);
            var destino = Path.Combine(snapshotDir, "files", a.Ruta);
            Directory.CreateDirectory(Path.GetDirectoryName(destino)!);
            File.Copy(origen, destino, overwrite: true);
        }

        File.WriteAllText(Path.Combine(snapshotDir, "snapshot.json"), JsonSerializer.Serialize(manifiesto, JsonOptions));
        return manifiesto;
    }

    public static SnapshotManifest Leer(string snapshotJsonPath)
        => JsonSerializer.Deserialize<SnapshotManifest>(File.ReadAllText(snapshotJsonPath))
           ?? throw new InvalidDataException("Manifiesto de snapshot inválido.");

    /// <summary>Restaura los contenidos del snapshot al directorio destino, verificando los hash.</summary>
    public static void Restaurar(SnapshotManifest manifiesto, string snapshotDir, string targetDir)
    {
        foreach (var a in manifiesto.Archivos)
        {
            var origen = Path.Combine(snapshotDir, "files", a.Ruta);
            if (!File.Exists(origen)) throw new FileNotFoundException($"Falta contenido del snapshot: {a.Ruta}", origen);

            var bytes = File.ReadAllBytes(origen);
            var hash = Convert.ToHexString(SHA256.HashData(bytes));
            if (!string.Equals(hash, a.Sha256, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException($"Integridad del snapshot fallida para {a.Ruta}: {hash} != {a.Sha256}");

            var destino = Path.Combine(targetDir, a.Ruta);
            Directory.CreateDirectory(Path.GetDirectoryName(destino)!);
            File.WriteAllBytes(destino, bytes);
        }
    }

    public static SnapshotDiff Diff(SnapshotManifest previo, SnapshotManifest actual)
    {
        var prev = previo.Archivos.ToDictionary(a => a.Ruta, a => a.Sha256, StringComparer.Ordinal);
        var cur = actual.Archivos.ToDictionary(a => a.Ruta, a => a.Sha256, StringComparer.Ordinal);

        var anadidos = cur.Keys.Where(k => !prev.ContainsKey(k)).OrderBy(k => k).ToList();
        var eliminados = prev.Keys.Where(k => !cur.ContainsKey(k)).OrderBy(k => k).ToList();
        var cambiados = cur.Keys
            .Where(k => prev.TryGetValue(k, out var h) && h != cur[k])
            .OrderBy(k => k)
            .ToList();

        return new SnapshotDiff(anadidos, eliminados, cambiados);
    }
}