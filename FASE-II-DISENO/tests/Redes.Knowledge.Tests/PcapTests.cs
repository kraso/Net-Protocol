using Redes.Knowledge.Infrastructure;
using Redes.Knowledge.Infrastructure.Capturas;

namespace Redes.Knowledge.Tests;

/// <summary>D6: muestra sintética determinista, round-trip PCAP, dissection y validación
/// de layouts F5 en bucle cerrado (L-004 / golden-master del disector).</summary>
public class PcapTests
{
    private static string RaizDelRepositorio()
    {
        var d = new DirectoryInfo(AppContext.BaseDirectory);
        while (d is not null && !File.Exists(Path.Combine(d.FullName, "PLANREDES.md")))
            d = d.Parent;
        return d?.FullName ?? throw new DirectoryNotFoundException("Raíz del repositorio no encontrada.");
    }

    private static string RutaF5() => Path.Combine(RaizDelRepositorio(), "FASE-05-MENSAJERIA", "F5-Campos-PDU.json");

    private static IReadOnlyList<CampoDefinido> CamposF5(string acronimo)
        => CatalogJson.CargarCamposF5(RutaF5(), acronimo)
            .Where(f => f.OffsetBits.HasValue) // FCS/PDU variable: sin offset fijo → no validable
            .Select(f => new CampoDefinido(f.OffsetBits!.Value, f.LongitudBits, f.Nombre))
            .ToList();

    [Fact]
    public void Muestra_Sintetica_Es_Determinista()
    {
        var bytes1 = PcapWriter.Escribir(PcapSintetico.Generar());
        var bytes2 = PcapWriter.Escribir(PcapSintetico.Generar());
        Assert.Equal(bytes1, bytes2);
    }

    [Fact]
    public void Muestra_RoundTrip_Con_El_Lector()
    {
        var captura = PcapSintetico.Generar();
        var tmp = Path.Combine(Path.GetTempPath(), $"rk_pcap_{Guid.NewGuid():N}.pcap");
        try
        {
            PcapWriter.EscribirAArchivo(tmp, captura);
            var leida = PcapCaptureReader.Abrir(tmp);
            Assert.Equal(1, leida.LinkType); // Ethernet
            Assert.Equal(4, leida.Paquetes.Count);
            foreach (var p in leida.Paquetes)
                Assert.Equal(p.CapturedLength, p.Data.Length);
        }
        finally
        {
            if (File.Exists(tmp)) File.Delete(tmp);
        }
    }

    [Fact]
    public void Muestra_Se_Disecta_Por_Capas()
    {
        var tcp = PcapDissector.Disectar(PcapSintetico.EthernetIpv4TcpSyn(0));
        Assert.True(tcp.EsEthernet && tcp.EsIpv4 && tcp.EsTcp);
        Assert.Equal((ushort)80, tcp.PuertoDestino);
        Assert.Equal("10.0.0.2", tcp.IpOrigen);
        Assert.Equal("10.0.0.1", tcp.IpDestino);

        var dns = PcapDissector.Disectar(PcapSintetico.EthernetIpv4UdpDns(1));
        Assert.True(dns.EsIpv4);
        Assert.Equal((byte)17, dns.ProtocoloIp); // UDP

        var v6 = PcapDissector.Disectar(PcapSintetico.EthernetIpv6Tcp(2));
        Assert.Equal((ushort)0x86DD, v6.EtherType); // IPv6

        var icmp = PcapDissector.Disectar(PcapSintetico.EthernetIpv4Icmp(3));
        Assert.Equal((byte)1, icmp.ProtocoloIp); // ICMP
    }

    [Fact]
    public void Muestra_Valida_Layouts_F5_En_Bucle_Cerrado()
    {
        // ETH: la trama capturada no incluye preámbulo/SFD (físicos); se validan con base 64
        // de modo que solo esos dos campos queden fuera de límites.
        var eth = PcapSintetico.EthernetIpv4TcpSyn(0);
        var camposEth = PcapDissector.Validar(eth, CamposF5("ETH"), offsetBaseBits: 64);
        Assert.All(camposEth.Where(c => c.Nombre is not ("Preamble" or "SFD")), c => Assert.True(c.EnLimites, c.Nombre));
        Assert.All(camposEth.Where(c => c.Nombre is "Preamble" or "SFD"), c => Assert.False(c.EnLimites, c.Nombre));

        // IPv4: todos los campos de la cabecera (20 B) dentro de límites.
        var ip = PcapDissector.Validar(eth.AsSpan(14, 20).ToArray(), CamposF5("IPv4"));
        Assert.True(PcapDissector.Resumen("IPv4", ip).Ok);

        // TCP: idem cabecera TCP de 20 B.
        var tcp = PcapDissector.Validar(eth[34..54], CamposF5("TCP"));
        Assert.True(PcapDissector.Resumen("TCP", tcp).Ok);

        // UDP + DNS (trama B): cabecera UDP y mensaje DNS completos.
        var tramaB = PcapSintetico.EthernetIpv4UdpDns(1);
        var udp = PcapDissector.Validar(tramaB.AsSpan(14 + 20, 8).ToArray(), CamposF5("UDP"));
        Assert.True(PcapDissector.Resumen("UDP", udp).Ok);
        var dns = PcapDissector.Validar(tramaB.AsSpan(14 + 20 + 8).ToArray(), CamposF5("DNS"));
        Assert.True(PcapDissector.Resumen("DNS", dns).Ok);

        // IPv6 (trama C) + ICMP (trama D).
        var tramaC = PcapSintetico.EthernetIpv6Tcp(2);
        var ipv6 = PcapDissector.Validar(tramaC.AsSpan(14, 40).ToArray(), CamposF5("IPv6"));
        Assert.True(PcapDissector.Resumen("IPv6", ipv6).Ok);
        var tramaD = PcapSintetico.EthernetIpv4Icmp(3);
        var icmp = PcapDissector.Validar(tramaD.AsSpan(14 + 20).ToArray(), CamposF5("ICMP"));
        Assert.True(PcapDissector.Resumen("ICMP", icmp).Ok);
    }
}