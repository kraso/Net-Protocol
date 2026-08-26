using System.Text.Json;

namespace Redes.Knowledge.Infrastructure.Quality;

/// <summary>Metadatos del dataset distribuido (versión independiente del ejecutable; D7-2).</summary>
public sealed record DatasetMetadata(
    string Version,
    string Fecha,
    string HashGolden,
    int Protocolos,
    int Servicios);

public static class DatasetMetadataService
{
    private static readonly JsonSerializerOptions Json = new() { WriteIndented = true };

    public static DatasetMetadata? Leer(string path)
        => File.Exists(path) ? JsonSerializer.Deserialize<DatasetMetadata>(File.ReadAllText(path)) : null;

    public static DatasetMetadata Escribir(string path, string version, DateTime fecha, string hashGolden, int protocolos, int servicios)
    {
        var meta = new DatasetMetadata(version, fecha.ToString("yyyy-MM-dd"), hashGolden, protocolos, servicios);
        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
        File.WriteAllText(path, JsonSerializer.Serialize(meta, Json));
        return meta;
    }
}