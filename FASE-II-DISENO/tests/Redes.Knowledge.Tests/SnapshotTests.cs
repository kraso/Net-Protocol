using Redes.Knowledge.Infrastructure.Snapshot;

namespace Redes.Knowledge.Tests;

/// <summary>D2-3: snapshots versionados con hash, diff y rollback con verificación de integridad.</summary>
public class SnapshotTests : IDisposable
{
    private readonly string _src = Path.Combine(Path.GetTempPath(), $"rk_src_{Guid.NewGuid():N}");
    private readonly string _snap1 = Path.Combine(Path.GetTempPath(), $"rk_snap1_{Guid.NewGuid():N}");
    private readonly string _snap2 = Path.Combine(Path.GetTempPath(), $"rk_snap2_{Guid.NewGuid():N}");
    private readonly string _target = Path.Combine(Path.GetTempPath(), $"rk_target_{Guid.NewGuid():N}");

    public SnapshotTests()
    {
        Directory.CreateDirectory(_src);
        Directory.CreateDirectory(Path.Combine(_src, "sub"));
        File.WriteAllText(Path.Combine(_src, "a.txt"), "contenido-a-v1");
        File.WriteAllText(Path.Combine(_src, "b.json"), "{\"x\":1}");
        File.WriteAllText(Path.Combine(_src, "sub", "c.csv"), "h1,h2\n1,2\n");
    }

    [Fact]
    public void Crea_Manifiesto_Y_Contenido()
    {
        var m = DatasetSnapshotService.Crear(_src, _snap1, "prueba");
        Assert.Equal(3, m.Archivos.Count);
        Assert.True(m.BytesTotales > 0);
        Assert.False(string.IsNullOrWhiteSpace(m.HashAgregado));
        Assert.True(File.Exists(Path.Combine(_snap1, "files", "a.txt")));
        Assert.True(File.Exists(Path.Combine(_snap1, "files", "sub", "c.csv")));
        Assert.True(File.Exists(Path.Combine(_snap1, "snapshot.json")));
    }

    [Fact]
    public void Hash_Agregado_Determinista()
    {
        var m1 = DatasetSnapshotService.Crear(_src, _snap1, "procedencia-1");
        var m2 = DatasetSnapshotService.Crear(_src, _snap2, "procedencia-2");
        // La procedencia/fecha NO forman parte del hash: mismo contenido → mismo hash.
        Assert.Equal(m1.HashAgregado, m2.HashAgregado);
        Assert.Equal(m1.Archivos.Count, m2.Archivos.Count);
    }

    [Fact]
    public void Diff_Detecta_Cambios()
    {
        var m1 = DatasetSnapshotService.Crear(_src, _snap1, "p");
        File.WriteAllText(Path.Combine(_src, "a.txt"), "contenido-a-v2-CAMBIADO");
        File.WriteAllText(Path.Combine(_src, "d.md"), "nuevo");
        var m2 = DatasetSnapshotService.Crear(_src, _snap2, "p");

        var diff = DatasetSnapshotService.Diff(m1, m2);
        Assert.Contains("a.txt", diff.Cambiados);
        Assert.Contains("d.md", diff.Anadidos);
        Assert.DoesNotContain("b.json", diff.Cambiados);
    }

    [Fact]
    public void Restaurar_Verifica_Integridad_Y_Rollback()
    {
        var m1 = DatasetSnapshotService.Crear(_src, _snap1, "p");
        // Se modifica el origen (simula una regresión)
        File.WriteAllText(Path.Combine(_src, "a.txt"), "regresión");

        DatasetSnapshotService.Restaurar(m1, _snap1, _target);
        Assert.Equal("contenido-a-v1", File.ReadAllText(Path.Combine(_target, "a.txt")));
        Assert.Equal("{\"x\":1}", File.ReadAllText(Path.Combine(_target, "b.json")));
        Assert.Equal("h1,h2\n1,2\n", File.ReadAllText(Path.Combine(_target, "sub", "c.csv")));
    }

    [Fact]
    public void Snapshot_Manipulado_Se_Rechaza()
    {
        var m1 = DatasetSnapshotService.Crear(_src, _snap1, "p");
        File.WriteAllText(Path.Combine(_snap1, "files", "a.txt"), "corrupto");
        Assert.Throws<InvalidDataException>(() => DatasetSnapshotService.Restaurar(m1, _snap1, _target));
    }

    public void Dispose()
    {
        foreach (var d in new[] { _src, _snap1, _snap2, _target })
        {
            if (Directory.Exists(d)) Directory.Delete(d, recursive: true);
        }
    }
}