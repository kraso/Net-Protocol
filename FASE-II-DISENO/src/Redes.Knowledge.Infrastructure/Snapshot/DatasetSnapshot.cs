namespace Redes.Knowledge.Infrastructure.Snapshot;

public sealed record SnapshotFile(string Ruta, long Bytes, string Sha256);

public sealed record SnapshotManifest(
    string Version,
    string Fecha,
    string Procedencia,
    string HashAgregado,
    IReadOnlyList<SnapshotFile> Archivos,
    long BytesTotales);

public sealed record SnapshotDiff(
    IReadOnlyList<string> Anadidos,
    IReadOnlyList<string> Eliminados,
    IReadOnlyList<string> Cambiados);