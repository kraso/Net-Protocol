using System.Buffers.Binary;
using System.Text;

namespace Redes.Knowledge.Infrastructure.Capturas;

/// <summary>
/// Muestras sintéticas DETERMINISTAS (D6-1/D6-2): genera tramas Ethernet representativas
/// para los 28 protocolos con layout F5-Campos-PDU.json (ETH, IPv4, IPv6, MPLS, STP, TCP,
/// UDP, ICMP, ICMPv6, IGMP, VRRP, GRE, SCTP, DNS, DHCP, NTP, RTP, CoAP, Syslog, VXLAN, GTP,
/// QUIC, TLS, BGP, MQTT, Telnet, HTTP/2). Sin red ni privilegios; mismo input → mismos bytes.
/// </summary>
public static class PcapSintetico
{
    // Marca de tiempo fija (determinista): 2023-11-14T22:13:20Z y un segundo más por trama.
    private const long InicioUs = 1_700_000_000_000_000L;

    private static readonly byte[] MacDestino = { 0x00, 0x11, 0x22, 0x33, 0x44, 0x55 };
    private static readonly byte[] MacOrigen = { 0x66, 0x77, 0x88, 0x99, 0xAA, 0xBB };
    private static readonly byte[] IpA = { 10, 0, 0, 2 };
    private static readonly byte[] IpB = { 10, 0, 0, 1 };
    private static readonly byte[] Ip6A = { 0x20, 0x01, 0x0d, 0xb8, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2 };
    private static readonly byte[] Ip6B = { 0x20, 0x01, 0x0d, 0xb8, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 };

    /// <summary>Muestra base: las 4 tramas clásicas (compatibilidad con tests iniciales).</summary>
    public static PcapCapture Generar() => new("muestra-sintetica", 1, new List<PcapPacket>
    {
        Paquete(0, EthernetIpv4TcpSyn(0)),
        Paquete(1, EthernetIpv4UdpDns(1)),
        Paquete(2, EthernetIpv6Tcp(2)),
        Paquete(3, EthernetIpv4Icmp(3))
    });

    /// <summary>Muestra completa: una trama por cada protocolo con layout F5 (24 tramas).</summary>
    public static PcapCapture GenerarTodas()
    {
        var builders = new Func<int, byte[]>[]
        {
            EthernetIpv4TcpSyn,
            EthernetIpv4UdpDns,
            EthernetIpv6Tcp,
            EthernetIpv4Icmp,
            _ => EthernetIpv4TcpTls(),
            _ => EthernetIpv4TcpHttp2(),
            _ => EthernetIpv4TcpBgp(),
            _ => EthernetIpv4TcpMqtt(),
            _ => EthernetIpv4TcpTelnet(),
            _ => EthernetIpv4UdpDhcp(),
            _ => EthernetIpv4UdpNtp(),
            _ => EthernetIpv4UdpRtp(),
            _ => EthernetIpv4UdpCoap(),
            _ => EthernetIpv4UdpSyslog(),
            _ => EthernetIpv4UdpRip(),
            _ => EthernetIpv4UdpVxlan(),
            _ => EthernetIpv4UdpGtp(),
            _ => EthernetIpv4UdpQuic(),
            _ => EthernetIpv4Igmp(),
            _ => EthernetIpv4Vrrp(),
            _ => EthernetIpv4Gre(),
            _ => EthernetIpv4Sctp(),
            _ => EthernetIpv6Icmpv6(),
            _ => EthernetMpls(),
            _ => EthernetStp()
        };
        var paquetes = builders.Select((b, i) => Paquete(i, b(i))).ToList();
        return new PcapCapture("muestra-sintetica-completa", 1, paquetes);
    }

    private static PcapPacket Paquete(int sec, byte[] frame)
        => new(InicioUs + sec * 1_000_000L, frame.Length, frame.Length, frame);

    // ── Trama A: Ethernet / IPv4 / TCP (SYN) ──────────────────────────────────────────
    public static byte[] EthernetIpv4TcpSyn(int _)
        => Ethernet(0x0800, Concatenar(Ipv4(6, 20, 0x0001, IpA, IpB),
            Tcp(49152, 80, 0x02, 0x01020304, 0, Array.Empty<byte>())));

    // ── Trama B: Ethernet / IPv4 / UDP / DNS ──────────────────────────────────────────
    public static byte[] EthernetIpv4UdpDns(int _)
    {
        var dns = DnsConsulta("example.com");
        return Ethernet(0x0800, Concatenar(Ipv4(17, (ushort)(8 + dns.Length), 0x0002, IpA, IpB), Udp(5353, 53, dns)));
    }

    // ── Trama C: Ethernet / IPv6 / TCP ────────────────────────────────────────────────
    public static byte[] EthernetIpv6Tcp(int _)
        => Ethernet(0x86DD, Concatenar(Ipv6(6, 20, Ip6A, Ip6B),
            Tcp(49153, 443, 0x18, 0x11223344, 0x55667788, Array.Empty<byte>())));

    // ── Trama D: Ethernet / IPv4 / ICMP echo request ──────────────────────────────────
    public static byte[] EthernetIpv4Icmp(int _)
    {
        const string dato = "HolaNetProtocol!";
        var icmp = new byte[8 + dato.Length];
        icmp[0] = 8; icmp[1] = 0;
        BinaryPrimitives.WriteUInt16BigEndian(icmp.AsSpan(4, 2), 1);
        BinaryPrimitives.WriteUInt16BigEndian(icmp.AsSpan(6, 2), 1);
        Encoding.ASCII.GetBytes(dato).CopyTo(icmp, 8);
        BinaryPrimitives.WriteUInt16BigEndian(icmp.AsSpan(2, 2), Checksum(icmp, 0, icmp.Length));
        return Ethernet(0x0800, Concatenar(Ipv4(1, (ushort)icmp.Length, 0x0004, IpA, IpB), icmp));
    }

    // ── Aplicaciones sobre TCP ────────────────────────────────────────────────────────
    public static byte[] EthernetIpv4TcpTls()
    {
        var hello = TlsClientHello();
        var record = new byte[5 + hello.Length];
        record[0] = 0x16;                                  // handshake
        record[1] = 0x03; record[2] = 0x03;                // TLS 1.2
        BinaryPrimitives.WriteUInt16BigEndian(record.AsSpan(3, 2), (ushort)hello.Length);
        hello.CopyTo(record, 5);
        return Ethernet(0x0800, ConcatIpTcp(6, record, 0x0005, 49154, 443, 0x18, 0x02020202, 0x04040404));
    }

    public static byte[] EthernetIpv4TcpHttp2()
    {
        var prefacio = "PRI * HTTP/2.0\r\n\r\nSM\r\n\r\n"u8.ToArray();
        var marco = new byte[9]; // SETTINGS vacío
        marco[3] = 0x04;
        return Ethernet(0x0800, ConcatIpTcp(6, Concatenar(prefacio, marco), 0x0006, 49155, 443, 0x18, 0x05050505, 0x06060606));
    }

    public static byte[] EthernetIpv4TcpBgp()
    {
        var abrir = new byte[29];
        for (var i = 0; i < 16; i++) abrir[i] = 0xFF;      // marker
        BinaryPrimitives.WriteUInt16BigEndian(abrir.AsSpan(16, 2), 29);
        abrir[18] = 1;                                     // OPEN
        abrir[19] = 4;
        BinaryPrimitives.WriteUInt16BigEndian(abrir.AsSpan(20, 2), 3000);
        BinaryPrimitives.WriteUInt16BigEndian(abrir.AsSpan(22, 2), 180);
        IpA.CopyTo(abrir, 24);
        abrir[28] = 0;
        return Ethernet(0x0800, ConcatIpTcp(6, abrir, 0x0007, 179, 49160, 0x18, 0x07070707, 0x08080808));
    }

    public static byte[] EthernetIpv4TcpMqtt()
        => Ethernet(0x0800, ConcatIpTcp(6, new byte[] { 0x10, 0x00 }, 0x0008, 49161, 1883, 0x18, 0x09090909, 0x0A0A0A0A));

    public static byte[] EthernetIpv4TcpTelnet()
    {
        var telnet = Concatenar(new byte[] { 0xFF, 0xFD, 0x03 }, "Hola"u8.ToArray());
        return Ethernet(0x0800, ConcatIpTcp(6, telnet, 0x0009, 49162, 23, 0x18, 0x0B0B0B0B, 0x0C0C0C0C));
    }

    // ── Aplicaciones sobre UDP ────────────────────────────────────────────────────────
    public static byte[] EthernetIpv4UdpDhcp()
    {
        var bootp = new byte[44 + 12]; // 44 B fijos + opciones
        bootp[0] = 0x01; bootp[1] = 0x01; bootp[2] = 0x06;
        BinaryPrimitives.WriteUInt32BigEndian(bootp.AsSpan(4, 4), 0x01020304);
        bootp[10] = 0x80; bootp[11] = 0x00;
        MacOrigen.CopyTo(bootp, 28);
        new byte[] { 0x63, 0x82, 0x53, 0x63, 0x35, 0x01, 0x01, 0x37, 0x02, 0x01, 0x03, 0xFF }.CopyTo(bootp, 44);
        return Ethernet(0x0800, ConcatIpUdp(17, bootp, 0x000A, 68, 67, IpA, IpB));
    }

    public static byte[] EthernetIpv4UdpNtp()
    {
        var ntp = new byte[48];
        ntp[0] = 0x23; ntp[1] = 0; ntp[2] = 4; ntp[3] = 0xFA;
        // transmit timestamp fijo (determinista)
        foreach (var (v, i) in new (byte, int)[] { (0xE3, 40), (0x78, 41), (0x8E, 42), (0xE4, 43), (0x20, 44), (0x00, 45), (0x00, 46), (0x00, 47) }) ntp[i] = v;
        return Ethernet(0x0800, ConcatIpUdp(17, ntp, 0x000B, 49163, 123, IpA, IpB));
    }

    public static byte[] EthernetIpv4UdpRtp()
    {
        var rtp = new byte[12 + 4];
        rtp[0] = 0x80; rtp[1] = 0x60;
        BinaryPrimitives.WriteUInt16BigEndian(rtp.AsSpan(2, 2), 1);
        BinaryPrimitives.WriteUInt32BigEndian(rtp.AsSpan(4, 4), 1);
        BinaryPrimitives.WriteUInt32BigEndian(rtp.AsSpan(8, 4), 0xCAFEBABE);
        "hola"u8.ToArray().CopyTo(rtp, 12);
        return Ethernet(0x0800, ConcatIpUdp(17, rtp, 0x000C, 5004, 5004, IpA, IpB));
    }

    public static byte[] EthernetIpv4UdpCoap()
        => Ethernet(0x0800, ConcatIpUdp(17, new byte[] { 0x40, 0x01, 0x00, 0x01, (byte)'a' }, 0x000D, 49164, 5683, IpA, IpB));

    public static byte[] EthernetIpv4UdpSyslog()
        => Ethernet(0x0800, ConcatIpUdp(17, Concatenar(new byte[] { 0x0E }, "Net Protocol syslog"u8.ToArray()), 0x000E, 49165, 514, IpA, IpB));

    public static byte[] EthernetIpv4UdpRip()
    {
        var rip = new byte[24];
        rip[0] = 1;            // Command: request
        rip[1] = 2;            // RIPv2
        // 2 B reservados + entrada: AFI 2, tag 0, IP 0.0.0.0, máscara 0, next-hop 0, métrica 1
        BinaryPrimitives.WriteUInt16BigEndian(rip.AsSpan(4, 2), 2);
        rip[22] = 0; rip[23] = 1; // métrica
        return Ethernet(0x0800, ConcatIpUdp(17, rip, 0x000F, 49169, 520, IpA, IpB));
    }

    public static byte[] EthernetIpv4UdpVxlan()
    {
        var vxlan = new byte[8 + 14 + 20 + 20];
        vxlan[0] = 0x08;
        vxlan[4] = 0x00; vxlan[5] = 0x00; vxlan[6] = 0x01; // VNI 1
        Concatenar(Ethernet(0x0800, Concatenar(Ipv4(6, 20, 0x000F, IpB, IpA),
            Tcp(80, 49170, 0x10, 0x0D0D0D0D, 0x0E0E0E0E, Array.Empty<byte>())))).CopyTo(vxlan, 8);
        return Ethernet(0x0800, ConcatIpUdp(17, vxlan, 0x0010, 49166, 4789, IpA, IpB));
    }

    public static byte[] EthernetIpv4UdpGtp()
    {
        var gtp = new byte[8 + 20 + 20];
        gtp[0] = 0x30; gtp[1] = 0xFF;
        BinaryPrimitives.WriteUInt16BigEndian(gtp.AsSpan(2, 2), 40);
        BinaryPrimitives.WriteUInt32BigEndian(gtp.AsSpan(4, 4), 1);
        Concatenar(Ipv4(6, 20, 0x0011, IpB, IpA), Tcp(8080, 49171, 0x10, 0x10101010, 0x11111111, Array.Empty<byte>())).CopyTo(gtp, 8);
        return Ethernet(0x0800, ConcatIpUdp(17, gtp, 0x0012, 49167, 2152, IpA, IpB));
    }

    public static byte[] EthernetIpv4UdpQuic()
    {
        var quic = new byte[10];
        quic[0] = 0xC0;
        BinaryPrimitives.WriteUInt32BigEndian(quic.AsSpan(1, 4), 1);
        quic[5] = 4;
        foreach (var (v, i) in new (byte, int)[] { (0xDE, 6), (0xAD, 7), (0xBE, 8), (0xEF, 9) }) quic[i] = v;
        return Ethernet(0x0800, ConcatIpUdp(17, quic, 0x0013, 49168, 443, IpA, IpB));
    }

    // ── Protocolos IP directos ────────────────────────────────────────────────────────
    public static byte[] EthernetIpv4Igmp()
    {
        var igmp = new byte[8];
        igmp[0] = 0x16;
        igmp[4] = 224;
        BinaryPrimitives.WriteUInt16BigEndian(igmp.AsSpan(2, 2), Checksum(igmp, 0, 8));
        return Ethernet(0x0800, Concatenar(Ipv4(2, 8, 0x0014, IpA, IpB), igmp));
    }

    public static byte[] EthernetIpv4Vrrp()
    {
        var vrrp = new byte[12];
        vrrp[0] = 0x21; vrrp[1] = 1; vrrp[2] = 100; vrrp[3] = 1;
        vrrp[8] = 192; vrrp[9] = 168; vrrp[10] = 1; vrrp[11] = 1;
        BinaryPrimitives.WriteUInt16BigEndian(vrrp.AsSpan(4, 2), Checksum(vrrp, 0, vrrp.Length));
        return Ethernet(0x0800, Concatenar(Ipv4(112, 12, 0x0015, IpA, IpB), vrrp));
    }

    public static byte[] EthernetIpv4Gre()
    {
        var gre = new byte[4 + 20 + 20];
        gre[2] = 0x08; gre[3] = 0x00;
        Concatenar(Ipv4(6, 20, 0x0016, IpB, IpA), Tcp(80, 49172, 0x10, 0x12121212, 0x13131313, Array.Empty<byte>())).CopyTo(gre, 4);
        return Ethernet(0x0800, Concatenar(Ipv4(47, (ushort)gre.Length, 0x0017, IpA, IpB), gre));
    }

    public static byte[] EthernetIpv4Sctp()
    {
        var sctp = new byte[32];
        BinaryPrimitives.WriteUInt16BigEndian(sctp.AsSpan(0, 2), 50000);
        BinaryPrimitives.WriteUInt16BigEndian(sctp.AsSpan(2, 2), 50001);
        BinaryPrimitives.WriteUInt32BigEndian(sctp.AsSpan(4, 4), 1);
        sctp[12] = 0x00;
        BinaryPrimitives.WriteUInt16BigEndian(sctp.AsSpan(14, 2), 16);
        BinaryPrimitives.WriteUInt32BigEndian(sctp.AsSpan(16, 4), 1);
        return Ethernet(0x0800, Concatenar(Ipv4(132, 32, 0x0018, IpA, IpB), sctp));
    }

    public static byte[] EthernetIpv6Icmpv6()
    {
        var ns = new byte[24];
        ns[0] = 0x87; // NS (135)
        Ip6B.CopyTo(ns, 8);
        BinaryPrimitives.WriteUInt16BigEndian(ns.AsSpan(2, 2), ChecksumIpv6Pseudo(Ip6A, Ip6B, 58, ns));
        return Ethernet(0x86DD, Concatenar(Ipv6(58, 24, Ip6A, Ip6B), ns));
    }

    // ── Capa de enlace ────────────────────────────────────────────────────────────────
    public static byte[] EthernetMpls()
    {
        var label = new byte[4];
        BinaryPrimitives.WriteUInt32BigEndian(label.AsSpan(0, 4), (100u << 12) | (1u << 8) | 64u);
        var interno = Concatenar(Ipv4(6, 20, 0x0019, IpB, IpA), Tcp(8080, 49173, 0x10, 0x14141414, 0x15151515, Array.Empty<byte>()));
        return Ethernet(0x8847, Concatenar(label, interno));
    }

    public static byte[] EthernetStp()
    {
        // BPDU de configuración (IEEE 802.1D) sobre LLC en 802.3.
        byte[] bpdu =
        {
            0x00, 0x00, 0x00, 0x00, 0x00,
            0x80, 0x01, 0x22, 0x33, 0x44, 0x55, 0x66, 0x77,
            0x00, 0x00, 0x00, 0x00,
            0x80, 0x01, 0x88, 0x99, 0xAA, 0xBB, 0xCC, 0xDD,
            0x80, 0x01, 0x00, 0x00, 0x00, 0x14, 0x00, 0x02, 0x00, 0x0F
        };
        var cuerpo = Concatenar(new byte[] { 0x42, 0x42, 0x03 }, bpdu);
        var frame = new byte[14 + cuerpo.Length];
        new byte[] { 0x01, 0x80, 0xC2, 0x00, 0x00, 0x00 }.CopyTo(frame, 0);
        MacOrigen.CopyTo(frame, 6);
        BinaryPrimitives.WriteUInt16BigEndian(frame.AsSpan(12, 2), (ushort)cuerpo.Length);
        cuerpo.CopyTo(frame, 14);
        return frame;
    }

    // ── Toolkit de tramas ─────────────────────────────────────────────────────────────
    private static byte[] Ethernet(ushort etherType, byte[] payload)
    {
        var frame = new byte[14 + payload.Length];
        MacDestino.CopyTo(frame, 0);
        MacOrigen.CopyTo(frame, 6);
        BinaryPrimitives.WriteUInt16BigEndian(frame.AsSpan(12, 2), etherType);
        payload.CopyTo(frame, 14);
        return frame;
    }

    private static byte[] Ipv4(byte protocolo, ushort longitudPayload, ushort id, byte[] origen, byte[] destino)
    {
        var ip = new byte[20];
        ip[0] = 0x45; ip[1] = 0;
        BinaryPrimitives.WriteUInt16BigEndian(ip.AsSpan(2, 2), (ushort)(20 + longitudPayload));
        BinaryPrimitives.WriteUInt16BigEndian(ip.AsSpan(4, 2), id);
        BinaryPrimitives.WriteUInt16BigEndian(ip.AsSpan(6, 2), 0x4000);
        ip[8] = 64; ip[9] = protocolo;
        origen.CopyTo(ip, 12);
        destino.CopyTo(ip, 16);
        BinaryPrimitives.WriteUInt16BigEndian(ip.AsSpan(10, 2), Checksum(ip, 0, 20));
        return ip;
    }

    private static byte[] Ipv6(byte nextHeader, int longitudPayload, byte[] origen, byte[] destino)
    {
        var ip = new byte[40];
        ip[0] = 0x60;
        BinaryPrimitives.WriteUInt16BigEndian(ip.AsSpan(4, 2), (ushort)longitudPayload);
        ip[6] = nextHeader;
        ip[7] = 64;
        origen.CopyTo(ip, 8);
        destino.CopyTo(ip, 24);
        return ip;
    }

    private static byte[] Tcp(ushort sport, ushort dport, byte flags, uint seq, uint ack, byte[] payload)
    {
        var tcp = new byte[20 + payload.Length];
        BinaryPrimitives.WriteUInt16BigEndian(tcp.AsSpan(0, 2), sport);
        BinaryPrimitives.WriteUInt16BigEndian(tcp.AsSpan(2, 2), dport);
        BinaryPrimitives.WriteUInt32BigEndian(tcp.AsSpan(4, 4), seq);
        BinaryPrimitives.WriteUInt32BigEndian(tcp.AsSpan(8, 4), ack);
        tcp[12] = 0x50;
        tcp[13] = flags;
        BinaryPrimitives.WriteUInt16BigEndian(tcp.AsSpan(14, 2), 0x2000);
        payload.CopyTo(tcp, 20);
        return tcp;
    }

    private static byte[] Udp(ushort sport, ushort dport, byte[] payload)
    {
        var udp = new byte[8 + payload.Length];
        BinaryPrimitives.WriteUInt16BigEndian(udp.AsSpan(0, 2), sport);
        BinaryPrimitives.WriteUInt16BigEndian(udp.AsSpan(2, 2), dport);
        BinaryPrimitives.WriteUInt16BigEndian(udp.AsSpan(4, 2), (ushort)udp.Length);
        payload.CopyTo(udp, 8);
        return udp;
    }

    /// <summary>IPv4 + TCP con el payload de aplicación (mismo origen/destino que el resto).</summary>
    private static byte[] ConcatIpTcp(byte protoIp, byte[] tcpPayload, ushort id, ushort sport, ushort dport,
        byte flags, uint seq, uint ack)
        => Concatenar(Ipv4(protoIp, (ushort)(20 + tcpPayload.Length), id, IpA, IpB),
            Tcp(sport, dport, flags, seq, ack, tcpPayload));

    private static byte[] ConcatIpUdp(byte protoIp, byte[] udpPayload, ushort id, ushort sport, ushort dport,
        byte[] origen, byte[] destino)
        => Concatenar(Ipv4(protoIp, (ushort)(8 + udpPayload.Length), id, origen, destino),
            Udp(sport, dport, udpPayload));

    internal static byte[] DnsConsulta(string nombre)
    {
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
        BinaryPrimitives.WriteUInt16BigEndian(msg.AsSpan(4, 2), 1);
        preguntaNombre.CopyTo(msg, 12);
        BinaryPrimitives.WriteUInt16BigEndian(msg.AsSpan(12 + preguntaNombre.Count, 2), 1);
        BinaryPrimitives.WriteUInt16BigEndian(msg.AsSpan(14 + preguntaNombre.Count, 2), 1);
        return msg;
    }

    /// <summary>ClientHello mínimo determinista: versión (2) + random (32) + session 0 + cipher 0 + comp 0 + ext 0.</summary>
    private static byte[] TlsClientHello()
    {
        var hola = new byte[2 + 32 + 1 + 2 + 1 + 2];
        hola[0] = 0x03; hola[1] = 0x03;
        for (var i = 0; i < 32; i++) hola[2 + i] = (byte)(i * 7);
        var hello = new byte[4 + hola.Length];
        hello[0] = 0x01; // ClientHello
        hello[1] = (byte)(hola.Length >> 16);
        hello[2] = (byte)(hola.Length >> 8);
        hello[3] = (byte)hola.Length;
        hola.CopyTo(hello, 4);
        return hello;
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

    private static ushort ChecksumIpv6Pseudo(byte[] src16, byte[] dst16, int siguiente, byte[] mensaje)
    {
        var pseudo = new byte[40 + mensaje.Length];
        src16.CopyTo(pseudo, 0);
        dst16.CopyTo(pseudo, 16);
        BinaryPrimitives.WriteUInt32BigEndian(pseudo.AsSpan(32, 4), (uint)mensaje.Length);
        pseudo[39] = (byte)siguiente;
        mensaje.CopyTo(pseudo, 40);
        return Checksum(pseudo, 0, pseudo.Length);
    }
}