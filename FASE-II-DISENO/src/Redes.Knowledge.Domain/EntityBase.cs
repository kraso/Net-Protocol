namespace Redes.Knowledge.Domain;

/// <summary>
/// Base de todas las entidades: URN estable + versionado temporal (valid_from / valid_to),
/// conforme al diseño de datos del plan (sección 6.1 y ADR-002).
/// </summary>
public abstract record EntityBase
{
    public Urn Id { get; init; }

    /// <summary>Inicio de vigencia (fecha absoluta) o null si sin restricción.</summary>
    public DateTime? ValidFrom { get; init; }

    /// <summary>Fin de vigencia (fecha absoluta) o null si sin restricción.</summary>
    public DateTime? ValidTo { get; init; }

    public bool EsValidoEn(DateTime momento)
        => (ValidFrom is null || ValidFrom.Value <= momento)
           && (ValidTo is null || ValidTo.Value >= momento);
}