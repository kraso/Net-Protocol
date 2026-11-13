namespace Redes.Knowledge.Domain;

/// <summary>Puerto de persistencia (patrón repositorio).</summary>
public interface IProtocolRepository
{
    Urn Save(Protocol protocol);
    Protocol? GetByUrn(Urn urn);
    IReadOnlyList<Protocol> GetAll();
    IReadOnlyList<Protocol> GetByFamilia(FamiliaProtocolo familia);
    bool Delete(Urn urn);
}

public sealed record SearchHit(string Urn, string Nombre, string Acronimo, string? Familia);

/// <summary>Puerto de búsqueda (FTS5).</summary>
public interface ISearchEngine
{
    IReadOnlyList<SearchHit> Search(string query, int limite = 50);
}

/// <summary>Estado de una comprobación de actualizaciones.</summary>
public enum EstadoActualizacion
{
    /// <summary>No se pudo consultar (sin red, timeout, respuesta inválida).</summary>
    NoComprobado,
    /// <summary>La versión instalada es la última publicada.</summary>
    AlDia,
    /// <summary>Existe una versión publicada más reciente.</summary>
    Disponible
}

/// <summary>Resultado de comprobar si hay una versión más reciente publicada.</summary>
public sealed record ResultadoActualizacion(
    EstadoActualizacion Estado,
    string? VersionDisponible = null,
    string? UrlDescarga = null,
    string? Detalle = null)
{
    public bool HayActualizacion => Estado == EstadoActualizacion.Disponible;

    public static ResultadoActualizacion NoComprobado(string detalle) =>
        new(EstadoActualizacion.NoComprobado, Detalle: detalle);

    public static ResultadoActualizacion AlDia(string version) =>
        new(EstadoActualizacion.AlDia, VersionDisponible: version);

    public static ResultadoActualizacion Disponible(string version, string? url) =>
        new(EstadoActualizacion.Disponible, version, url);
}

/// <summary>
/// Puerto de comprobación de actualizaciones. La aplicación es local-first: la
/// comprobación es opcional, nunca bloquea el arranque y su fallo no es un error.
/// </summary>
public interface IUpdateChecker
{
    Task<ResultadoActualizacion> CheckAsync(System.Version versionActual, CancellationToken ct = default);
}