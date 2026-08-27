using System.Buffers.Binary;

namespace Redes.Knowledge.Infrastructure.Capturas;

/// <summary>
/// Muestras sintéticas DETERMINISTAS (D6-1/D6-2): genera tramas Ethernet representativas
/// construidas desde los formatos estándar que documenta F5-Campos-PDU.json (ETH, IPv4, IPv6,
/// TCP, UDP, DNS, ICMP), para poder validar el dissector y los layouts sin red ni privilegios.
/// Mismo input → mismos bytes (regla del proyecto).
/// </summary>
public static class PcapSintetico
{
    // Marca de tiempo fija (determinista): 2023-11-14T22:13:20Z y un segundo más por trama.
    private const long InicioUs = 1_700_000_000_000_000L;

    private static readonly byte[] MacDestino = { 0x00, 0x11, 0x22, 0x33, 0x44, 0x55 };
    private static readonly byte[] MacOrigen = { 0x66, 0x77, 0x88, 0x99, 0xAA, 0xBB };

    public static PcapCapture Generar()
    {
        var paquetes = new List<PcapPacket>
        {
            Paquete(0, EthernetIpv4TcpSyn(0)),
            Paquete(1, EthernetIpv4UdpDns(1)),
            Paquete(2, EthernetIpv6Tcp(2)),
            Paquete(3, EthernetIpv4Icmp(3))
        };
        return new PcapCapture("muestra-sintetica", 1 /* LINKTYPE_ETHERNET */, paquetes);
    }

    private static PcapPacket Paquete(int sec, byte[] frame)
        => new(InicioUs + sec * 1_000_000L, frame.Length, frame.Length, frame);

    // ── Trama A: Ethernet / IPv4 / TCP (SYN a un servidor web) ────────────────────────
    public static byte[] EthernetIpv4TcpSyn(int sec)
    {
        var tcp = new byte[20];
        BinaryPrimitives.WriteUInt16BigEndian(tcp.AsSpan(0, 2), 49152);   // puerto origen
        BinaryPrimitives.WriteUInt16BigEndian(tcp.AsSpan(2, 2), 80);      // puerto destino
        BinaryPrimitives.WriteUInt32BigEndian(tcp.AsSpan(4, 4), 0x01020304); // seq
        BinaryPrimitives.WriteUInt32BigEndian(tcp.AsSpan(8, 4), 0);       // ack
        tcp[12] = 0x50;   // data offset 5 (cabecera 20 B)
        tcp[13] = 0x02;   // flags: SYN
        BinaryPrimitives.WriteUInt16BigEndian(tcp.AsSpan(14, 2), 0x2000); // ventana
        tcp[16] = 0x00; tcp[17] = 0x00; // checksum (no se valida en F5)
        BinaryPrimitives.WriteUInt16BigEndian(tcp.AsSpan(18, 2), 0);      // urg

        var ip = Ipv4(6 /* TCP */, 20, 0x0001, new byte[] { 10, 0, 0, 2 }, new byte[] { 10, 0, 0, 1 });
        return Ethernet(0x0800, Concatenar(ip, tcp));
    }

    // ── Trama B: Ethernet / IPv4 / UDP / DNS (consulta desde 10.0.0.2) ───────────────
    public static byte[] EthernetIpv4UdpDns(int sec)
    {
        var dns = DnsConsulta("example.com");
        var udp = new byte[8 + dns.Length];
        BinaryPrimitives.WriteUInt16BigEndian(udp.AsSpan(0, 2), 5353);
        BinaryPrimitives.WriteUInt16BigEndian(udp.AsSpan(2, 2), 53);
        BinaryPrimitives.WriteUInt16BigEndian(udp.AsSpan(4, 2), (ushort)udp.Length);
        udp[6] = 0x00; udp[7] = 0x00; // checksum UDP (0)
        dns.CopyTo(udp, 8);

        var ip = Ipv4(17 /* UDP */, (ushort)udp.Length, 0x0002,
            new byte[] { 10, 0, 0, 2 }, new byte[] { 8, 8, 8, 8 });
        return Ethernet(0x0800, Concatenar(ip, udp));
    }

    // ── Trama C: Ethernet / IPv6 / TCP (PSH+ACK a un servicio 443) ───────────────────
    public static byte[] EthernetIpv6Tcp(int sec)
    {
        var tcp = new byte[20];
        BinaryPrimitives.WriteUInt16BigEndian(tcp.AsSpan(0, 2), 49153);
        BinaryPrimitives.WriteUInt16BigEndian(tcp.AsSpan(2, 2), 443);
        BinaryPrimitives.WriteUInt32BigEndian(tcp.AsSpan(4, 4), 0x11223344);
        BinaryPrimitives.WriteUInt32BigEndian(tcp.AsSpan(8, 4), 0x55667788);
        tcp[12] = 0x50;
        tcp[13] = 0x18;   // flags: PSH|ACK
        BinaryPrimitives.WriteUInt16BigEndian(tcp.AsSpan(14, 2), 0x2000);
        tcp[16] = 0x00; tcp[17] = 0x00;
        BinaryPrimitives.WriteUInt16BigEndian(tcp.AsSpan(18, 2), 0);

        var ipv6 = Ipv6(6 /* TCP */, 20,
            [0x20, 0x01, 0x0d, 0xb8, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2],
            [0x20, 0x01, 0x0d, 0xb8, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1]);
        return Ethernet(0x86DD, Concatenar(ipv6, tcp));
    }

    // ── Trama D: Ethernet / IPv4 / ICMP (echo request) ────────────────────────────────
    public static byte[] EthernetIpv4Icmp(int sec)
    {
        var payload = "HolaNetProtocol!"u8.ToArray();
        var icmp = new byte[8 + payload.Length];
        icmp[0] = 8;  // Echo request
        icmp[1] = 0;
        BinaryPrimitives.WriteUInt16BigEndian(icmp.AsSpan(2, 2), 0); // checksum: se calcula
        BinaryPrimitives.WriteUInt16BigEndian(icmp.AsSpan(4, 2), 1); // id
        BinaryPrimitives.WriteUInt16BigEndian(icmp.AsSpan(6, 2), 1); // seq
        payload.CopyTo(icmp, 8);
        BinaryPrimitives.WriteUInt16BigEndian(icmp.AsSpan(2, 2), Checksum(icmp, 0, icmp.Length));

        var ip = Ipv4(1 /* ICMP */, (ushort)icmp.Length, 0x0004,
            new byte[] { 10, 0, 0, 2 }, new byte[] { 10, 0, 0, 1 });
        return Ethernet(0x0800, Concatenar(ip, icmp));
    }

    // ── Helpers ───────────────────────────────────────────────────────────────────────
    private static byte[] Ethernet(ushort etherType, byte[] payload)
    {
        var frame = new byte[14 + payload.Length];
        MacDestino.CopyTo(frame, 0);
        MacOrigen.CopyTo(frame, 6);
        BinaryPrimitives.WriteUInt16BigEndian(frame.AsSpan(12, 2), etherType);
        payload.CopyTo(frame, 14);
        return frame;
    }

    private static byte[] Ipv4(byte protocolo, ushort longitudPayload, ushort id,
        byte[] origen, byte[] destino)
    {
        var ip = new byte[20];
        ip[0] = 0x45;                 // versión 4, IHL 5
        ip[1] = 0;                    // TOS
        BinaryPrimitives.WriteUInt16BigEndian(ip.AsSpan(2, 2), (ushort)(20 + longitudPayload));
        BinaryPrimitives.WriteUInt16BigEndian(ip.AsSpan(4, 2), id);
        BinaryPrimitives.WriteUInt16BigEndian(ip.AsSpan(6, 2), 0x4000); // DF
        ip[8] = 64;                   // TTL
        ip[9] = protocolo;
        ip[10] = 0; ip[11] = 0;       // checksum (se calcula)
        origen.CopyTo(ip, 12);
        destino.CopyTo(ip, 16);
        BinaryPrimitives.WriteUInt16BigEndian(ip.AsSpan(10, 2), Checksum(ip, 0, 20));
        return ip;
    }

    private static byte[] Ipv6(byte nextHeader, int longitudPayload, byte[] origen, byte[] destino)
    {
        var ip = new byte[40];
        ip[0] = 0x60; ip[1] = 0x00; ip[2] = 0x00; ip[3] = 0x00; // versión 6 + traffic/flow 0
        BinaryPrimitives.WriteUInt16BigEndian(ip.AsSpan(4, 2), (ushort)longitudPayload);
        ip[6] = nextHeader;
        ip[7] = 64;                   // hop limit
        origen.CopyTo(ip, 8);
        destino.CopyTo(ip, 24);
        return ip;
    }

    internal static byte[] DnsConsulta(string nombre)
    {
        // Cabecera DNS (12 B): id 0x1234, flags 0x0100 (query, RD), 1 pregunta.
        var preguntaNombre = new List<byte>();
        foreach (var etiqueta in nombre.Split('.'))
        {
            preguntaNombre.Add((byte)etiqueta.Length);
            foreach (var c in etiqueta) preguntaNombre.Add((byte)c);
        }
        preguntaNombre.Add(0);

        var msg = new byte[12 + preguntaNombre.Count + 4];
        BinaryPrimitives.WriteUInt16BigEndian(msg.AsSpan(0, 2), 0x1234);
        BinaryPrimitives.WriteUInt16BigEndian(msg.AsSpan(2, 2), 0x0100);
        BinaryPrimitives.WriteUInt16BigEndian(msg.AsSpan(4, 2), 1); // QDCOUNT
        preguntaNombre.CopyTo(msg, 12);
        BinaryPrimitives.WriteUInt16BigEndian(msg.AsSpan(12 + preguntaNombre.Count, 2), 1); // A
        BinaryPrimitives.WriteUInt16BigEndian(msg.AsSpan(14 + preguntaNombre.Count, 2), 1); // IN
        return msg;
    }

    private static byte[] Concatenar(params byte[][] partes)
    {
        var total = partes.Sum(p => p.Length);
        var r = new byte[total];
        var pos = 0;
        foreach (var p in partes) { p.CopyTo(r, pos); pos += p.Length; }
        return r;
    }

    private static ushort Checksum(byte[] b, int inicio, int longitud)
    {
        long suma = 0;
        var i = inicio;
        var fin = inicio + longitud;
        while (i + 1 < fin)
        {
            suma += (b[i] << 8) | b[i + 1];
            i += 2;
        }
        if (i < fin) suma += b[i] << 8;
        while ((suma >> 16) != 0) suma = (suma & 0xFFFF) + (suma >> 16);
        return (ushort)~suma;
    }
}