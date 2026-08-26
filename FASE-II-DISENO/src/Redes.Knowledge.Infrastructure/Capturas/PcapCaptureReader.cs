using System.Buffers.Binary;

namespace Redes.Knowledge.Infrastructure.Capturas;

/// <summary>
/// Lector de capturas (D6-1): formato PCAP clásico (magic 0xA1B2C3D4 en ambos endianness)
/// y PCAPNG (bloques SHB/IDB/EPB con detección de endianness; bloques desconocidos se omiten).
/// Sin dependencias externas.
/// </summary>
public static class PcapCaptureReader
{
    private const uint MagicPcapLe = 0xA1B2C3D4;  // bytes d4 c3 b2 a1
    private const uint MagicPcapBe = 0xD4C3B2A1;  // bytes a1 b2 c3 d4
    private const uint NsBlockSection = 0x0A0D0D0A;
    private const uint NgBlockIdb = 0x00000001;
    private const uint NgBlockEpb = 0x00000006;

    public static PcapCapture Abrir(string ruta) => Leer(File.ReadAllBytes(ruta), ruta);

    private static PcapCapture Leer(byte[] b, string ruta)
    {
        if (b.Length < 24) throw new InvalidDataException("Captura demasiado corta para ser PCAP/PCAPNG.");

        if (EsPcapClasico(b)) return LeerClasico(b, ruta);
        if (EsPcapNg(b)) return LeerNg(b, ruta);
        throw new InvalidDataException("Cabecera PCAP/PCAPNG no reconocida.");
    }

    private static bool EsPcapClasico(byte[] b)
        => (U32(b, 0, true) == MagicPcapLe) || (U32(b, 0, false) == MagicPcapLe);

    private static bool EsPcapNg(byte[] b)
        => U32(b, 0, true) == NsBlockSection;

    private static PcapCapture LeerClasico(byte[] b, string ruta)
    {
        var le = U32(b, 0, true) == MagicPcapLe; // d4 c3 b2 a1 → little-endian
        var linkType = (int)U32(b, 20, le);
        var paquetes = new List<PcapPacket>();
        var pos = 24;
        while (pos + 16 <= b.Length)
        {
            var tsSec = I32(b, pos, le);
            var tsUsec = I32(b, pos + 4, le);
            var incl = (int)U32(b, pos + 8, le);
            var orig = (int)U32(b, pos + 12, le);
            if (incl < 0 || pos + 16 + incl > b.Length)
                throw new InvalidDataException("Registro PCAP truncado o longitud inválida.");
            var datos = new byte[incl];
            Array.Copy(b, pos + 16, datos, 0, incl);
            paquetes.Add(new PcapPacket(tsSec * 1_000_000L + tsUsec, incl, orig, datos));
            pos += 16 + incl;
        }
        return new PcapCapture(ruta, linkType, paquetes);
    }

    private static PcapCapture LeerNg(byte[] b, string ruta)
    {
        var le = U32(b, 8, true) == 0x1A2B3C4D; // BOM del SHB en el offset 8
        var linkType = 0;
        var paquetes = new List<PcapPacket>();
        var pos = 0;

        while (pos + 12 <= b.Length)
        {
            var tipo = U32(b, pos, le);
            var longitudTotal = (int)U32(b, pos + 4, le);
            if (longitudTotal < 12 || pos + longitudTotal > b.Length)
                throw new InvalidDataException("Bloque PCAPNG truncado o longitud inválida.");

            switch (tipo)
            {
                case NsBlockSection:
                    break; // SHB: ya se detectó; nada que extraer
                case NgBlockIdb:
                    if (longitudTotal >= 12) linkType = U16(b, pos + 8, le);
                    break;
                case NgBlockEpb:
                    if (longitudTotal < 32) break;
                    var capturado = (int)U32(b, pos + 20, le);
                    var original = (int)U32(b, pos + 24, le);
                    var inicioDatos = pos + 28;
                    if (capturado < 0 || inicioDatos + capturado > pos + longitudTotal - 4)
                        throw new InvalidDataException("EPB PCAPNG con datos fuera de límites.");
                    var datos = new byte[capturado];
                    Array.Copy(b, inicioDatos, datos, 0, capturado);
                    paquetes.Add(new PcapPacket(0, capturado, original, datos));
                    break;
                // Otros bloques (PB, SPB, NRB, opciones…) se omiten
            }
            pos += longitudTotal;
        }

        return new PcapCapture(ruta, linkType, paquetes);
    }

    private static ushort U16(byte[] b, int i, bool le)
        => le ? BinaryPrimitives.ReadUInt16LittleEndian(b.AsSpan(i, 2))
              : BinaryPrimitives.ReadUInt16BigEndian(b.AsSpan(i, 2));

    private static uint U32(byte[] b, int i, bool le)
        => le ? BinaryPrimitives.ReadUInt32LittleEndian(b.AsSpan(i, 4))
              : BinaryPrimitives.ReadUInt32BigEndian(b.AsSpan(i, 4));

    private static int I32(byte[] b, int i, bool le) => unchecked((int)U32(b, i, le));
}