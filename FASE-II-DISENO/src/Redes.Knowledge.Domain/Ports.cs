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