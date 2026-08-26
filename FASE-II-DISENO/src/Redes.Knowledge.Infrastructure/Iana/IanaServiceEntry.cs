namespace Redes.Knowledge.Infrastructure.Iana;

/// <summary>Entrada normalizada del registro IANA de nombres de servicio y puertos (D2-1).</summary>
public sealed record IanaServiceEntry(
    string ServiceName,
    int? Port,
    string Transport,
    string Description,
    string Reference,
    string RegistrationDate);

/// <summary>Resultado del proceso de importación (estadísticas reales del registro).</summary>
public sealed record IanaImportResult(
    int TotalFilas,
    int SinNombre,
    int SinPuerto,
    int Importados,
    DateTime FechaConsulta,
    IReadOnlyList<IanaServiceEntry> Entradas);