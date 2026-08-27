using Redes.Knowledge.Infrastructure.Capturas;

namespace Redes.Knowledge.Tests;

/// <summary>L-004: validación cruzada contra corpus REAL de capturas (repositorio Wireshark).</summary>
public class CorpusL004Tests
{
    private static string RaizDelRepositorio()
    {
        var d = new DirectoryInfo(AppContext.BaseDirectory);
        while (d is not null && !File.Exists(Path.Combine(d.FullName, "PLANREDES.md")))
            d = d.Parent;
        return d?.FullName ?? throw new DirectoryNotFoundException("Raíz del repositorio no encontrada.");
    }

    private static string CarpetaCorpus() => Path.Combine(RaizDelRepositorio(), "FASE-08-VALIDACION", "corpus");
    private static string RutaF5() => Path.Combine(RaizDelRepositorio(), "FASE-05-MENSAJERIA", "F5-Campos-PDU.json");

    [Fact]
    public void Corpus_Real_Se_Parsea_Completo()
    {
        var ficheros = Directory.GetFiles(CarpetaCorpus(), "*.pc*").OrderBy(f => f).ToArray();
        Assert.True(ficheros.Length >= 5, $"El corpus debería tener capturas reales ({ficheros.Length})");
        foreach (var f in ficheros)
        {
            var captura = PcapCaptureReader.Abrir(f);
            Assert.True(captura.Paquetes.Count >= 1, $"{Path.GetFileName(f)} sin paquetes");
        }
    }

    [Fact]
    public void Corpus_Reconoce_Protocolos_Esperados()
    {
        var resultado = CorpusL004.Validar(CarpetaCorpus(), RutaF5());
        var nombres = resultado.PorProtocolo.Select(e => e.Protocolo).ToHashSet(StringComparer.OrdinalIgnoreCase);

        Assert.Contains("ETH", nombres);
        Assert.Contains("IPv4", nombres);
        Assert.True(nombres.Contains("TCP") || nombres.Contains("UDP"), "Debe reconocer TCP o UDP");
        // Protocolos aportados por capturas concretas del corpus:
        Assert.Contains("DNS", nombres);   // dns_port.pcap
        Assert.Contains("DHCP", nombres);  // dhcp.pcap
        Assert.Contains("NTP", nombres);   // ntp.pcap
        Assert.Contains("ICMP", nombres);  // icmp_ascii.pcapng
    }

    [Fact]
    public void Informe_L004_Se_Puede_Generar()
    {
        var salida = Path.Combine(Path.GetTempPath(), $"rk_l004_{Guid.NewGuid():N}.md");
        try
        {
            var resultado = CorpusL004.Validar(CarpetaCorpus(), RutaF5(), salida);
            Assert.True(File.Exists(salida));
            var texto = File.ReadAllText(salida);
            Assert.StartsWith("# L-004", texto);
            Assert.Contains("## Por protocolo", texto);
            Assert.True(resultado.PorProtocolo.Count > 0);
            Assert.True(resultado.SinLayoutEnCorpus.Count > 0, "Debe haber layouts F5 sin paquetes en el corpus (cobertura parcial honesta).");
        }
        finally
        {
            if (File.Exists(salida)) File.Delete(salida);
        }
    }
}