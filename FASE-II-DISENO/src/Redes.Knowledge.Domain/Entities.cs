namespace Redes.Knowledge.Domain;

// Las 17 entidades núcleo del modelo de dominio (plan, sección 6.1; F2I-Diseno-de-Software.md §C MOD-02).

public sealed record Protocol : EntityBase
{
    public string Nombre { get; init; } = "";
    public string Acronimo { get; init; } = "";
    public string[] Aliases { get; init; } = Array.Empty<string>();
    public FamiliaProtocolo Familia { get; init; }
    public LifecycleState Estado { get; init; }
    public string? Capas { get; init; }
}

public sealed record Standard : EntityBase
{
    public string Titulo { get; init; } = "";
    public string Organismo { get; init; } = "";
    public string Version { get; init; } = "";
    public DateTime? FechaPublicacion { get; init; }
}

public sealed record Version : EntityBase
{
    public Urn ProtocoloId { get; init; }
    public string Numero { get; init; } = "";
}

public sealed record MessageType : EntityBase
{
    public string Nombre { get; init; } = "";
    public string Proposito { get; init; } = "";
}

public sealed record Field : EntityBase
{
    public string Nombre { get; init; } = "";
    public int? OffsetBits { get; init; }
    public int? LongitudBits { get; init; }
    public string Tipo { get; init; } = "";
    public string Semantica { get; init; } = "";
    public bool Obligatorio { get; init; }
}

public sealed record PDU : EntityBase
{
    public string Nombre { get; init; } = "";
    public string UnidadDatos { get; init; } = "";
    public string Endianness { get; init; } = "network order";
}

public sealed record Layer : EntityBase
{
    public string Nombre { get; init; } = "";
    public string? Osi { get; init; }
    public string? TcpIp { get; init; }
}

public sealed record Plane : EntityBase
{
    public string Nombre { get; init; } = "";
}

public sealed record Device : EntityBase
{
    public string Nombre { get; init; } = "";
    public string Clase { get; init; } = "";
    public string? Capas { get; init; }
    public string[] Planos { get; init; } = Array.Empty<string>();
    public string? Pdu { get; init; }
}

public sealed record NetworkType : EntityBase
{
    public string Nombre { get; init; } = "";
    public string Ambito { get; init; } = "";
}

public sealed record AddressingScheme : EntityBase
{
    public string Nombre { get; init; } = "";
    public string AmbitoDirecciones { get; init; } = "";
}

public sealed record Source : EntityBase
{
    public string Titulo { get; init; } = "";
    public string Url { get; init; } = "";
    public string Version { get; init; } = "";
    public string Organismo { get; init; } = "";
    public DateTime? FechaPublicacion { get; init; }
    public DateTime? FechaConsulta { get; init; }
    public NivelAutoridad Nivel { get; init; }
    public Confianza Confianza { get; init; }
}

public sealed record Implementation : EntityBase
{
    public string Nombre { get; init; } = "";
    public string Tipo { get; init; } = "";
    public bool Soporta { get; init; }
    public bool ImplementaCompleto { get; init; }
}

public sealed record Capture : EntityBase
{
    public string Ruta { get; init; } = "";
    public string Formato { get; init; } = "";
}

public sealed record Diagram : EntityBase
{
    public string Nombre { get; init; } = "";
    public string Plantilla { get; init; } = "";
    public string OrigenDatos { get; init; } = "";
}

public sealed record SecurityMechanism : EntityBase
{
    public string ProtocoloUrn { get; init; } = "";
    public string Mecanismo { get; init; } = "";
    public string Descripcion { get; init; } = "";
    public string Amenazas { get; init; } = "";
    public string Recomendaciones { get; init; } = "";
}

public sealed record Relationship : EntityBase
{
    public Urn Origen { get; init; }
    public Urn Destino { get; init; }
    public RelacionTipo Tipo { get; init; }
}

/// <summary>Ficha prioritaria F4 (18 campos textuales por protocolo; deriva de F4-Fichas-Prioritarias.md).</summary>
public sealed record FichaPrioritaria
{
    public string Id { get; init; } = "";
    public string Acronimo { get; init; } = "";
    public string Nombre { get; init; } = "";
    /// <summary>Campo número (1..18) → valor textual.</summary>
    public IReadOnlyDictionary<string, string> Campos { get; init; }
        = new Dictionary<string, string>(StringComparer.Ordinal);

    public string? Campo(int numero)
        => Campos.TryGetValue(numero.ToString(), out var v) && !string.IsNullOrWhiteSpace(v) ? v : null;
}