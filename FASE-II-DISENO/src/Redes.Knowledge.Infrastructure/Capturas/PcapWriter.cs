using System.Buffers.Binary;

namespace Redes.Knowledge.Infrastructure.Capturas;

/// <summary>
/// Escritor de capturas en formato PCAP clásico (little-endian, linktype Ethernet).
/// Complemento de <see cref="PcapCaptureReader"/>: permite volcar muestras a archivo (D6-1).
/// </summary>
public static class PcapWriter
{
    private const uint MagicPcapLe = 0xA1B2C3D4;

    public static byte[] Escribir(PcapCapture captura)
    {
        using var ms = new MemoryStream();
        void U16(ushort v) { ms.WriteByte((byte)v); ms.WriteByte((byte)(v >> 8)); }
        void U32(uint v)
        {
            ms.WriteByte((byte)v); ms.WriteByte((byte)(v >> 8));
            ms.WriteByte((byte)(v >> 16)); ms.WriteByte((byte)(v >> 24));
        }

        U32(MagicPcapLe);
        U16(2); U16(4);        // versión 2.4
        U32(0); U32(0);        // thiszone, sigfigs
        U32(65535);            // snaplen
        U32((uint)captura.LinkType);

        foreach (var p in captura.Paquetes)
        {
            uint tsSec = (uint)(p.TimestampUs / 1_000_000);
            uint tsResto = (uint)(p.TimestampUs % 1_000_000);
            U32(tsSec); U32(tsResto);
            U32((uint)p.CapturedLength);
            U32((uint)p.OriginalLength);
            ms.Write(p.Data, 0, p.Data.Length);
        }
        return ms.ToArray();
    }

    public static void EscribirAArchivo(string ruta, PcapCapture captura)
    {
        var directorio = Path.GetDirectoryName(ruta);
        if (!string.IsNullOrEmpty(directorio)) Directory.CreateDirectory(directorio);
        File.WriteAllBytes(ruta, Escribir(captura));
    }
}