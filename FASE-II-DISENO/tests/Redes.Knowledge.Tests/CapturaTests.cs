using Redes.Knowledge.Infrastructure;
using Redes.Knowledge.Infrastructure.Capturas;

namespace Redes.Knowledge.Tests;

/// <summary>
/// D6: lector PCAP/PCAPNG, dissection por capas y validación de layouts contra F5 (L-004).
/// Las capturas de prueba se construyen programáticamente a partir de la matemática de campos.
/// </summary>
public class CapturaTests
{
    // ------------------------------------------------------------------
    // Fixtures: paquete Ethernet + IPv4 + TCP (62 bytes) y archivos PCAP/PCAPNG
    // ------------------------------------------------------------------

    private static byte[] PaqueteTcpEjemplo()
    {
        var b = new byte[62];
        // Ethernet: DA (6) + SA (6) + EtherType 0x0800
        "00112233445566778899aabb0800".Chunks(2).ToList().ForEach(h => { }); // (no-op de claridad)
        b[0] = 0x00; b[1] = 0x11; b[2] = 0x22; b[3] = 0x33; b[4] = 0x44; b[5] = 0x55;
        b[6] = 0x66; b[7] = 0x77; b[8] = 0x88; b[9] = 0x99; b[10] = 0xAA; b[11] = 0xBB;
        b[12] = 0x08; b[13] = 0x00;
        // IPv4: 45 00 | len 48 | id 0001 | 40 00 | ttl 40 | proto 06 | chk 0000 | src 192.0.2.1 | dst 203.0.113.2
        b[14] = 0x45; b[15] = 0x00; b[16] = 0x00; b[17] = 0x30;
        b[18] = 0x00; b[19] = 0x01; b[20] = 0x40; b[21] = 0x00;
        b[22] = 0x40; b[23] = 0x06; b[24] = 0x00; b[25] = 0x00;
        b[26] = 192; b[27] = 0; b[28] = 2; b[29] = 1;
        b[30] = 203; b[31] = 0; b[32] = 113; b[33] = 2;
        // TCP 20B: sport 49152 (0xC000) | dport 80 (0x0050) | seq 1 | ack 0 | 50 12 (SYN|ACK) | win 0x2000 | chk 0 | urg 0
        b[34] = 0xC0; b[35] = 0x00; b[36] = 0x00; b[37] = 0x50;
        b[38] = 0x00; b[39] = 0x00; b[40] = 0x00; b[41] = 0x01;
        b[42] = 0x00; b[43] = 0x00; b[44] = 0x00; b[45] = 0x00;
        b[46] = 0x50; b[47] = 0x12; b[48] = 0x20; b[49] = 0x00;
        b[50] = 0x00; b[51] = 0x00; b[52] = 0x00; b[53] = 0x00;
        // payload 8B
        for (var i = 0; i < 8; i++) b[54 + i] = (byte)('A' + i);
        return b;
    }

    private static byte[] PcapClasico(byte[] paquete)
    {
        using var ms = new MemoryStream();
        void W(byte[] v) => ms.Write(v, 0, v.Length);
        // global header (LE): magic d4c3b2a1 | ver 2.4 | thiszone 0 | sigfigs 0 | snaplen 0xffff | linktype 1
        W(new byte[] { 0xd4, 0xc3, 0xb2, 0xa1, 0x02, 0x00, 0x04, 0x00, 0, 0, 0, 0, 0, 0, 0, 0, 0xff, 0xff, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00 });
        // record: ts_sec 0, ts_usec 0, incl, orig
        W(BitConverter.GetBytes(0)); W(BitConverter.GetBytes(0));
        W(BitConverter.GetBytes(paquete.Length)); W(BitConverter.GetBytes(paquete.Length));
        W(paquete);
        return ms.ToArray();
    }

    private static byte[] PcapNg(byte[] paquete)
    {
        using var ms = new MemoryStream();
        void W(byte[] v) => ms.Write(v, 0, v.Length);
        void Bloque(uint tipo, int longitud, Action cuerpo)
        {
            W(BitConverter.GetBytes(tipo)); W(BitConverter.GetBytes((uint)longitud));
            cuerpo();
            W(BitConverter.GetBytes((uint)longitud));
        }

        // SHB (28 B): BOM 1A2B3C4D en orden little-endian (bytes 4D 3C 2B 1A), versión 1.0, sección -1
        Bloque(0x0A0D0D0A, 28, () => W(new byte[] { 0x4d, 0x3c, 0x2b, 0x1a, 0x01, 0x00, 0x00, 0x00, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff }));
        // IDB (20 B): linktype 1 (Ethernet)
        Bloque(0x00000001, 20, () => W(new byte[] { 0x01, 0x00, 0x00, 0x00, 0xff, 0xff, 0x00, 0x00 }));
        // EPB: iface 0 | ts 0 | caplen | origlen | datos
        var epbLen = 32 + paquete.Length + ((4 - (paquete.Length % 4)) % 4);
        Bloque(0x00000006, epbLen, () =>
        {
            W(BitConverter.GetBytes(0u)); // iface
            W(BitConverter.GetBytes(0u)); W(BitConverter.GetBytes(0u)); // ts hi/lo
            W(BitConverter.GetBytes((uint)paquete.Length)); W(BitConverter.GetBytes((uint)paquete.Length));
            W(paquete);
            var pad = (4 - (paquete.Length % 4)) % 4;
            for (var i = 0; i < pad; i++) W(new byte[] { 0 });
        });
        return ms.ToArray();
    }

    private static IReadOnlyList<CampoDefinido> CamposDeF5(string acronimo)
    {
        var raiz = Raiz();
        var ruta = Path.Combine(raiz, "FASE-05-MENSAJERIA", "F5-Campos-PDU.json");
        return CatalogJson.CargarCamposF5(ruta, acronimo)
            .Select(f => new CampoDefinido(f.OffsetBits ?? 0, f.LongitudBits, f.Nombre))
            .ToList();
    }

    private static string Raiz()
    {
        var d = new DirectoryInfo(AppContext.BaseDirectory);
        while (d is not null && !File.Exists(Path.Combine(d.FullName, "PLANREDES.md"))) d = d.Parent;
        return d?.FullName ?? throw new InvalidOperationException("Raíz no encontrada.");
    }

    // ------------------------------------------------------------------
    // Pruebas
    // ------------------------------------------------------------------

    [Fact]
    public void Pcap_Clasico_Se_Lee()
    {
        var tmp = Path.GetTempFileName();
        try
        {
            File.WriteAllBytes(tmp, PcapClasico(PaqueteTcpEjemplo()));
            var captura = PcapCaptureReader.Abrir(tmp);
            Assert.True(captura.EsEthernet);
            Assert.Single(captura.Paquetes);
            Assert.Equal(62, captura.Paquetes[0].OriginalLength);
        }
        finally { File.Delete(tmp); }
    }

    [Fact]
    public void PcapNg_Se_Lee()
    {
        var tmp = Path.GetTempFileName();
        try
        {
            File.WriteAllBytes(tmp, PcapNg(PaqueteTcpEjemplo()));
            var captura = PcapCaptureReader.Abrir(tmp);
            Assert.True(captura.EsEthernet);
            Assert.Single(captura.Paquetes);
            Assert.Equal(62, captura.Paquetes[0].CapturedLength);
        }
        finally { File.Delete(tmp); }
    }

    [Fact]
    public void Cabecera_Invalida_Lanza()
    {
        var tmp = Path.GetTempFileName();
        try
        {
            File.WriteAllBytes(tmp, new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24 });
            Assert.Throws<InvalidDataException>(() => PcapCaptureReader.Abrir(tmp));
        }
        finally { File.Delete(tmp); }
    }

    [Fact]
    public void Disseccion_TCP_Completa()
    {
        var d = PcapDissector.Disectar(PaqueteTcpEjemplo());
        Assert.True(d.EsEthernet);
        Assert.Equal(PcapDissector.EtherTypeIpv4, d.EtherType);
        Assert.True(d.EsIpv4);
        Assert.Equal(PcapDissector.ProtocoloIpTcp, d.ProtocoloIp);
        Assert.Equal("192.0.2.1", d.IpOrigen);
        Assert.Equal("203.0.113.2", d.IpDestino);
        Assert.True(d.EsTcp);
        Assert.Equal(49152, (int)d.PuertoOrigen!.Value);
        Assert.Equal(80, (int)d.PuertoDestino!.Value);
    }

    [Fact]
    public void Layout_TCP_Se_Valida_Con_F5()
    {
        var tcp = new byte[20];
        Array.Copy(PaqueteTcpEjemplo(), 34, tcp, 0, 20); // cabecera TCP

        var campos = PcapDissector.Validar(tcp, CamposDeF5("TCP"));
        var resumen = PcapDissector.Resumen("TCP", campos);
        Assert.True(resumen.Ok, resumen.ToString());

        var sport = campos.First(c => c.Nombre == "Source Port");
        var dport = campos.First(c => c.Nombre == "Destination Port");
        var seq = campos.First(c => c.Nombre == "Sequence Number");
        var window = campos.First(c => c.Nombre == "Window");
        Assert.Equal("C000", sport.ValorHex);
        Assert.Equal("0050", dport.ValorHex);
        Assert.Equal("00000001", seq.ValorHex);
        Assert.Equal("2000", window.ValorHex);
    }

    [Fact]
    public void Layout_Ethernet_Base_Preamble_Semantica()
    {
        // F5 define Ethernet con preámbulo (offset 0); la captura no lo incluye → base 64 (DA).
        var frame = PaqueteTcpEjemplo();
        var campos = PcapDissector.Validar(frame, CamposDeF5("ETH"), offsetBaseBits: 64);

        var preambulo = campos.First(c => c.Nombre == "Preamble");
        var da = campos.First(c => c.Nombre == "Destination MAC");
        var et = campos.First(c => c.Nombre == "EtherType / Length");

        Assert.False(preambulo.EnLimites);            // preámbulo no capturado (fuera)
        Assert.True(da.EnLimites);
        Assert.Equal("001122334455", da.ValorHex);
        Assert.Equal("0800", et.ValorHex);            // EtherType IPv4
    }

    [Fact]
    public void Layout_Ipv4_Se_Valida_Con_F5()
    {
        var ip = new byte[20];
        Array.Copy(PaqueteTcpEjemplo(), 14, ip, 0, 20);

        var campos = PcapDissector.Validar(ip, CamposDeF5("IPv4"));
        var resumen = PcapDissector.Resumen("IPv4", campos);
        Assert.True(resumen.Ok, resumen.ToString());

        Assert.Equal("06", campos.First(c => c.Nombre == "Protocol").ValorHex);
        Assert.Equal("C0000201", campos.First(c => c.Nombre == "Source Address").ValorHex);
    }
}

internal static class StringChunks
{
    public static IEnumerable<string> Chunks(this string s, int n)
        => System.Linq.Enumerable.Range(0, (s.Length + n - 1) / n).Select(i => s.Substring(i * n, Math.Min(n, s.Length - i * n)));
}