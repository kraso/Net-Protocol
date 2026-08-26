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
}