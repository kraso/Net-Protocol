using System.Buffers.Binary;
using System.Net;

namespace Redes.Knowledge.Infrastructure.Capturas;

public sealed record CampoDecodificado(string Nombre, int OffsetBits, int LongitudBits, bool EnLimites, string ValorHex);

public sealed record ResultadoValidacion(string Protocolo, int TotalCampos, int CamposEnLimites, bool Ok)
{
    public override string ToString()
        => $"{Protocolo}: {CamposEnLimites}/{TotalCampos} campos dentro de límites — {(Ok ? "OK" : "TRUNCADO/FUERA")}";
}

/// <summary>
/// Dissection por capas (D6-1, filosofía Wireshark sin embederlo) y validación de layouts
/// (D6-2): correspondencia paquete real ↔ campos documentados de F5-Campos-PDU.json.
/// </summary>
public static class PcapDissector
{
    public const int EthernetHeaderBytes = 14;
    public const ushort EtherTypeIpv4 = 0x0800;
    public const byte ProtocoloIpTcp = 6;
    public const byte ProtocoloIpUdp = 17;

    public sealed record DisseccionCompleta(
        bool EsEthernet,
        ushort? EtherType,
        bool EsIpv4,
        byte? ProtocoloIp,
        string? IpOrigen,
        string? IpDestino,
        bool EsTcp,
        ushort? PuertoOrigen,
        ushort? PuertoDestino,
        int CabecerasBytes);

    public static DisseccionCompleta Disectar(byte[] frame)
    {
        if (frame.Length < EthernetHeaderBytes) return new(false, null, false, null, null, null, false, null, null, 0);

        var etherType = BinaryPrimitives.ReadUInt16BigEndian(frame.AsSpan(12, 2));
        ushort? puertoOrigen = null, puertoDestino = null;
        byte? protoIp = null;
        string? ipOrigen = null, ipDestino = null;
        var esIpv4 = etherType == EtherTypeIpv4;
        var esTcp = false;
        var cabeceras = EthernetHeaderBytes;

        if (esIpv4 && frame.Length >= EthernetHeaderBytes + 20)
        {
            var ihl = (frame[14] & 0x0F) * 4;
            var ver = frame[14] >> 4;
            if (ver == 4 && ihl >= 20 && frame.Length >= EthernetHeaderBytes + ihl)
            {
                protoIp = frame[14 + 9];
                ipOrigen = new IPAddress(frame.AsSpan(14 + 12, 4)).ToString();
                ipDestino = new IPAddress(frame.AsSpan(14 + 16, 4)).ToString();
                cabeceras = EthernetHeaderBytes + ihl;
                esTcp = protoIp == ProtocoloIpTcp;
                if (esTcp && frame.Length >= EthernetHeaderBytes + ihl + 4)
                {
                    puertoOrigen = BinaryPrimitives.ReadUInt16BigEndian(frame.AsSpan(EthernetHeaderBytes + ihl, 2));
                    puertoDestino = BinaryPrimitives.ReadUInt16BigEndian(frame.AsSpan(EthernetHeaderBytes + ihl + 2, 2));
                    cabeceras = EthernetHeaderBytes + ihl + ((frame[EthernetHeaderBytes + ihl + 12] >> 4) * 4);
                }
            }
        }

        return new DisseccionCompleta(true, etherType, esIpv4, protoIp, ipOrigen, ipDestino, esTcp, puertoOrigen, puertoDestino, cabeceras);
    }

    /// <summary>
    /// Valida un bufer de cabecera contra los campos documentados (F5). offsetBaseBits permite
    /// validar solo la parte capturada (p. ej. Ethernet sin preámbulo → base = 64).
    /// </summary>
    public static IReadOnlyList<CampoDecodificado> Validar(byte[] bufer, IReadOnlyList<CampoDefinido> campos, int offsetBaseBits = 0)
    {
        var resultado = new List<CampoDecodificado>();
        foreach (var c in campos)
        {
            if (c.LongitudBits is not { } len) continue;
            var rel = c.OffsetBits - offsetBaseBits;
            if (rel < 0) { resultado.Add(new CampoDecodificado(c.Nombre, c.OffsetBits, len, false, "")); continue; }

            var byteOff = rel / 8;
            var bitIni = rel % 8;
            var bytesNecesarios = (bitIni + len + 7) / 8;
            var enLimites = byteOff + bytesNecesarios <= bufer.Length;
            var hex = enLimites
                ? Convert.ToHexString(bufer.AsSpan(byteOff, bytesNecesarios))
                : "";
            resultado.Add(new CampoDecodificado(c.Nombre, c.OffsetBits, len, enLimites, hex));
        }
        return resultado;
    }

    public static ResultadoValidacion Resumen(string protocolo, IReadOnlyList<CampoDecodificado> campos)
    {
        var conLongitud = campos.Count;
        var enLimites = campos.Count(c => c.EnLimites);
        return new ResultadoValidacion(protocolo, conLongitud, enLimites, enLimites == conLongitud);
    }

    /// <summary>Capa detectada en una trama (D6): acrónimo F5 + posición de su cabecera.</summary>
    public sealed record CapaDisectada(string AcronimoF5, int InicioBytes, int LongitudBytes, int BaseBits);

    /// <summary>
    /// Recorre la pila de la trama y devuelve las capas reconocidas con los 28 protocolos
    /// que tienen layout F5 (ETH, IPv4, IPv6, MPLS, STP, TCP, UDP, ICMP(v6), IGMP, VRRP, GRE,
    /// SCTP; y por puerto: DNS, DHCP, NTP, RTP, CoAP, Syslog, QUIC, VXLAN, GTP, TLS, BGP,
    /// MQTT, Telnet, HTTP/2). La selección transporte→aplicación por puertos es heurística
    /// (puertos bien conocidos) y documentada.
    /// </summary>
    public static IReadOnlyList<CapaDisectada> DisectarCapas(byte[] frame)
    {
        var capas = new List<CapaDisectada>();
        if (frame.Length < 14) return capas;
        capas.Add(new CapaDisectada("ETH", 0, frame.Length, 64)); // base 64: preámbulo/SFD físicos

        var etherType = BinaryPrimitives.ReadUInt16BigEndian(frame.AsSpan(12, 2));
        if (etherType >= 0x0600)
        {
            switch (etherType)
            {
                case 0x0800: CapasIpv4(frame, 14, capas); break;
                case 0x86DD: CapasIpv6(frame, 14, capas); break;
                case 0x8847: CapasMpls(frame, 14, capas); break;
            }
        }
        else if (frame.Length >= 17 && frame[14] == 0x42 && frame[15] == 0x42 && frame[16] == 0x03)
        {
            capas.Add(new CapaDisectada("STP", 17, Math.Min(35, frame.Length - 17), 0));
        }
        return CapasSinDuplicados(capas, frame);
    }

    private static void CapasIpv4(byte[] f, int inicio, List<CapaDisectada> capas)
    {
        if (inicio + 20 > f.Length) return;
        var ihl = (f[inicio] & 0x0F) * 4;
        if (ihl < 20 || inicio + ihl > f.Length) return;
        capas.Add(new CapaDisectada("IPv4", inicio, ihl, 0));
        var proto = f[inicio + 9];
        var capa = inicio + ihl;
        switch (proto)
        {
            case 6: CapasTcp(f, capa, capas); break;
            case 17: CapasUdp(f, capa, capas); break;
            case 1: capas.Add(new CapaDisectada("ICMP", capa, f.Length - capa, 0)); break;
            case 2: capas.Add(new CapaDisectada("IGMP", capa, f.Length - capa, 0)); break;
            case 112: capas.Add(new CapaDisectada("VRRP", capa, f.Length - capa, 0)); break;
            case 47: CapasGre(f, capa, capas); break;
            case 132: capas.Add(new CapaDisectada("SCTP", capa, f.Length - capa, 0)); break;
        }
    }

    private static void CapasIpv6(byte[] f, int inicio, List<CapaDisectada> capas)
    {
        if (inicio + 40 > f.Length) return;
        capas.Add(new CapaDisectada("IPv6", inicio, 40, 0));
        var siguiente = f[inicio + 6];
        var capa = inicio + 40;
        switch (siguiente)
        {
            case 6: CapasTcp(f, capa, capas); break;
            case 17: CapasUdp(f, capa, capas); break;
            case 58: capas.Add(new CapaDisectada("ICMPv6", capa, f.Length - capa, 0)); break;
        }
    }

    private static void CapasMpls(byte[] f, int inicio, List<CapaDisectada> capas)
    {
        if (inicio + 4 > f.Length) return;
        var sBit = (f[inicio + 2] & 0x01) == 1;
        capas.Add(new CapaDisectada("MPLS", inicio, 4, 0));
        if (!sBit || inicio + 8 > f.Length) return;
        // Tras la etiqueta MPLS se asume IPv4 (muestra determinista); si no, se acotaría.
        CapasIpv4(f, inicio + 4, capas);
    }

    private static void CapasGre(byte[] f, int inicio, List<CapaDisectada> capas)
    {
        if (inicio + 4 > f.Length) return;
        var proto = BinaryPrimitives.ReadUInt16BigEndian(f.AsSpan(inicio + 2, 2));
        capas.Add(new CapaDisectada("GRE", inicio, 4, 0));
        if (proto == 0x0800 && inicio + 4 + 20 <= f.Length) CapasIpv4(f, inicio + 4, capas);
    }

    private static void CapasTcp(byte[] f, int inicio, List<CapaDisectada> capas)
    {
        if (inicio + 20 > f.Length) return;
        var dataOffset = (f[inicio + 12] >> 4) * 4;
        if (dataOffset < 20 || inicio + dataOffset > f.Length) return;
        capas.Add(new CapaDisectada("TCP", inicio, dataOffset, 0));
        var payload = inicio + dataOffset;
        var puerto = BinaryPrimitives.ReadUInt16BigEndian(f.AsSpan(inicio + 2, 2));
        var puertoOrigen = BinaryPrimitives.ReadUInt16BigEndian(f.AsSpan(inicio, 2));
        // Los puertos bien conocidos se reconocen en cualquiera de los dos lados.
        if (puerto == 179 || puertoOrigen == 179) capas.Add(new CapaDisectada("BGP", payload, f.Length - payload, 0));
        else if (puerto == 1883 || puertoOrigen == 1883) capas.Add(new CapaDisectada("MQTT", payload, f.Length - payload, 0));
        else if (puerto == 23 || puertoOrigen == 23) capas.Add(new CapaDisectada("Telnet", payload, f.Length - payload, 0));
        else if (puerto == 53 || puertoOrigen == 53)
        {
            // DNS: solo se valida si hay mensaje real (>= 12 B); en TCP, posible prefijo de
            // longitud de 2 bytes delante del mensaje.
            if (payload + 12 <= f.Length && payload + 2 + 12 <= f.Length &&
                ((f[payload] << 8) | f[payload + 1]) == f.Length - (payload + 2))
                capas.Add(new CapaDisectada("DNS", payload + 2, f.Length - (payload + 2), 0));
            else if (payload + 12 <= f.Length)
                capas.Add(new CapaDisectada("DNS", payload, f.Length - payload, 0));
        }
        else if (puerto == 443 || puertoOrigen == 443)
        {
            if (payload < f.Length && f[payload] == 0x16)
                capas.Add(new CapaDisectada("TLS", payload, f.Length - payload, 0));
            else if (payload + 24 + 9 <= f.Length && f.AsSpan(payload, 24).SequenceEqual("PRI * HTTP/2.0\r\n\r\nSM\r\n\r\n"u8))
                capas.Add(new CapaDisectada("HTTP/2", payload + 24, 9, 0)); // marco tras el prefacio
        }
    }

    private static void CapasUdp(byte[] f, int inicio, List<CapaDisectada> capas)
    {
        if (inicio + 8 > f.Length) return;
        capas.Add(new CapaDisectada("UDP", inicio, 8, 0));
        var puerto = BinaryPrimitives.ReadUInt16BigEndian(f.AsSpan(inicio + 2, 2));
        var puertoOrigen = BinaryPrimitives.ReadUInt16BigEndian(f.AsSpan(inicio, 2));
        var payload = inicio + 8;
        // Los puertos bien conocidos se reconocen en cualquiera de los dos lados.
        if (puerto == 53 || puertoOrigen == 53) capas.Add(new CapaDisectada("DNS", payload, f.Length - payload, 0));
        else if (puerto is 67 or 68 || puertoOrigen is 67 or 68) capas.Add(new CapaDisectada("DHCP", payload, f.Length - payload, 0));
        else if (puerto == 123 || puertoOrigen == 123) capas.Add(new CapaDisectada("NTP", payload, f.Length - payload, 0));
        else if (puerto is 5004 or 5005 || puertoOrigen is 5004 or 5005) capas.Add(new CapaDisectada("RTP", payload, f.Length - payload, 0));
        else if (puerto == 5683 || puertoOrigen == 5683) capas.Add(new CapaDisectada("CoAP", payload, f.Length - payload, 0));
        else if (puerto == 514 || puertoOrigen == 514) capas.Add(new CapaDisectada("Syslog", payload, f.Length - payload, 0));
        else if (puerto == 520 || puertoOrigen == 520) capas.Add(new CapaDisectada("RIP", payload, f.Length - payload, 0));
        else if (puerto == 443 || puertoOrigen == 443) capas.Add(new CapaDisectada("QUIC", payload, f.Length - payload, 0));
        else if ((puerto == 4789 || puertoOrigen == 4789) && payload + 8 <= f.Length)
        {
            capas.Add(new CapaDisectada("VXLAN", payload, 8, 0));
            if (payload + 8 + 14 <= f.Length) CapasIpv4(f, payload + 8 + 14, capas); // trama interna
        }
        else if ((puerto == 2152 || puertoOrigen == 2152) && payload + 8 <= f.Length)
        {
            capas.Add(new CapaDisectada("GTP", payload, 8, 0));
            if (payload + 8 + 20 <= f.Length) CapasIpv4(f, payload + 8, capas);
        }
    }

    private static List<CapaDisectada> CapasSinDuplicados(List<CapaDisectada> capas, byte[] frame)
    {
        var vistos = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var limpio = new List<CapaDisectada>();
        foreach (var c in capas)
            if (vistos.Add(c.AcronimoF5))
                limpio.Add(c with { LongitudBytes = Math.Min(c.LongitudBytes, frame.Length - c.InicioBytes) });
        return limpio;
    }
}