namespace Redes.Knowledge.Infrastructure.Capturas;

/// <summary>Paquete decodificado de una captura.</summary>
public sealed record PcapPacket(long TimestampUs, int CapturedLength, int OriginalLength, byte[] Data);

/// <summary>Captura abierta: cabecera global + paquetes (formato PCAP clásico o PCAPNG).</summary>
public sealed record PcapCapture(string NombreArchivo, int LinkType, IReadOnlyList<PcapPacket> Paquetes)
{
    public bool EsEthernet => LinkType == 1; // LINKTYPE_ETHERNET
}

/// <summary>Definición de campo usada por el validador de layouts (proyección de F5).</summary>
public sealed record CampoDefinido(int OffsetBits, int? LongitudBits, string Nombre);