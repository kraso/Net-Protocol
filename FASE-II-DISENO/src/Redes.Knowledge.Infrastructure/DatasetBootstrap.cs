using Redes.Knowledge.Domain;

namespace Redes.Knowledge.Infrastructure;

/// <summary>
/// Bootstrap del dataset local (D3): si el almacén está vacío, importa el catálogo
/// canónico de la Fase I (F3-Protocolos.json) y lo indexa. Idempotente.
/// </summary>
public static class DatasetBootstrap
{
    public static int EnsureProtocolos(SqliteKnowledgeStore store, string f3JsonPath)
    {
        var repo = new SqliteProtocolRepository(store);
        if (repo.GetAll().Count > 0) return 0;

        var protocolos = CatalogJson.CargarProtocolosF3(f3JsonPath);
        foreach (var p in protocolos) repo.Save(p);
        return protocolos.Count;
    }
}