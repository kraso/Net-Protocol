using System.Text.Json;
using Redes.Knowledge.Domain;
using Redes.Knowledge.Infrastructure;
using Redes.Knowledge.Infrastructure.Quality;

namespace Redes.Knowledge.Tests;

/// <summary>D7-1: auditoría automática (A01…A07) y metadatos del dataset sobre datos reales.</summary>
public class QualityTests
{
    private static string Raiz()
    {
        var d = new DirectoryInfo(AppContext.BaseDirectory);
        while (d is not null && !File.Exists(Path.Combine(d.FullName, "PLANREDES.md"))) d = d.Parent;
        return d?.FullName ?? throw new InvalidOperationException("Raíz no encontrada.");
    }

    private static IReadOnlyList<Protocol> CargarProtocolos()
        => CatalogJson.CargarProtocolosF3(Path.Combine(Raiz(), "FASE-03-INVENTARIO", "F3-Protocolos.json"));

    private static IReadOnlyList<string> IdsDe(string carpeta, string archivo, string propiedad)
    {
        using var doc = JsonDocument.Parse(File.ReadAllText(Path.Combine(Raiz(), carpeta, archivo)));
        return doc.RootElement.GetProperty(propiedad).EnumerateArray()
            .Select(e => e.GetProperty("id").GetString() ?? "")
            .Where(s => s.Length > 0)
            .ToList();
    }

    private static IReadOnlyList<string> IdsF6()
    {
        using var doc = JsonDocument.Parse(File.ReadAllText(Path.Combine(Raiz(), "FASE-06-SEGURIDAD", "F6-Seguridad-Protocolos.json")));
        return doc.RootElement.GetProperty("protocolos").EnumerateArray()
            .Select(e => e.GetProperty("protocolo_id").GetString() ?? "").ToList();
    }

    private static IReadOnlyList<string> RefF7()
    {
        using var doc = JsonDocument.Parse(File.ReadAllText(Path.Combine(Raiz(), "FASE-07-DOMINIOS-ESPECIALES", "F7-Dominios.json")));
        return doc.RootElement.GetProperty("dominios").EnumerateArray()
            .SelectMany(d => d.GetProperty("protocolos").EnumerateArray()
                .Select(p => p.GetString() ?? "")).ToList();
    }

    [Fact]
    public void Auditoria_Sobre_Datos_Reales_OK()
    {
        var protocolos = CargarProtocolos();
        var f3 = protocolos.Select(p => p.Id.Value).ToList();
        var f5 = IdsDe("FASE-05-MENSAJERIA", "F5-Campos-PDU.json", "protocolos");
        var f6 = IdsF6();
        var f7 = RefF7();
        var golden = DatasetQuality.GoldenMasterDe(protocolos);

        var informe = DatasetQuality.Auditar("F3 v2 (26-08-2026)", protocolos, f3, f5, f6, f7, golden);

        Assert.True(informe.Ok, informe.ToString());
        Assert.Equal(7, informe.Chequeos.Count);
        Assert.All(informe.Chequeos, c => Assert.True(c.Ok, c.Detalle));
    }

    [Fact]
    public void GoldenMaster_Determinista()
    {
        var a = DatasetQuality.GoldenMasterDe(CargarProtocolos());
        var b = DatasetQuality.GoldenMasterDe(CargarProtocolos());
        Assert.Equal(a, b);
        Assert.False(string.IsNullOrWhiteSpace(a));
    }

    [Fact]
    public void Auditoria_Detecta_Duplicado_Urn()
    {
        var protocolos = CargarProtocolos().ToList();
        var tcp = protocolos.First(p => p.Acronimo == "TCP");
        protocolos.Add(tcp with { Nombre = "DUPLICADO INTENCIONAL" }); // misma URN

        var f3 = protocolos.Select(p => p.Id.Value).ToList();
        var informe = DatasetQuality.Auditar("tamper", protocolos, f3, Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>());
        var a01 = informe.Chequeos.First(c => c.Id == "A01");
        Assert.False(a01.Ok);
        Assert.Contains(tcp.Id.Value, a01.Detalle);
        Assert.False(informe.Ok);
    }

    [Fact]
    public void Metadatos_Dataset_Roundtrip()
    {
        var tmp = Path.Combine(Path.GetTempPath(), $"rk_dm_{Guid.NewGuid():N}.json");
        try
        {
            DatasetMetadataService.Escribir(tmp, "1.0.0", new DateTime(2026, 8, 26), "AABB", 113, 13141);
            var leido = DatasetMetadataService.Leer(tmp);
            Assert.NotNull(leido);
            Assert.Equal("1.0.0", leido!.Version);
            Assert.Equal(113, leido.Protocolos);
            Assert.Equal(13141, leido.Servicios);
        }
        finally
        {
            if (File.Exists(tmp)) File.Delete(tmp);
        }
    }
}