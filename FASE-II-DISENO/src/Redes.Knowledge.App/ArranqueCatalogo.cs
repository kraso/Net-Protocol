using Redes.Knowledge.App.ViewModels;
using Redes.Knowledge.Infrastructure;

namespace Redes.Knowledge.App;

/// <summary>Estado de arranque ya cargado: repositorios, motor de búsqueda y catálogo F3–F6.</summary>
public sealed record EstadoArranque(
    SqliteProtocolRepository Repositorio,
    SqliteServiceRepository Servicios,
    SqliteSearchEngine Busqueda,
    CatalogoViewModel Catalogo,
    int Importados);

/// <summary>
/// Arranque del catálogo: resuelve la raíz de datos, abre la base SQLite en un directorio
/// escribible, crea los repositorios, asegura el bootstrap de F3 y carga los catálogos F3–F6
/// con sus índices. Vive fuera de la ventana para poder ejercitarlo sin construir la UI.
/// </summary>
public static class ArranqueCatalogo
{
    /// <summary>
    /// Raíz de datos: en distribución, la carpeta <c>datos</c> junto al ejecutable; en
    /// desarrollo se sube desde el directorio del exe hasta la raíz del repositorio.
    /// </summary>
    public static string RaizDatos()
    {
        var bundled = Path.Combine(AppContext.BaseDirectory, "datos");
        if (Directory.Exists(Path.Combine(bundled, "FASE-03-INVENTARIO")))
            return bundled;
        var d = new DirectoryInfo(AppContext.BaseDirectory);
        while (d is not null && !File.Exists(Path.Combine(d.FullName, "PLANREDES.md")))
            d = d.Parent;
        return d?.FullName
            ?? throw new InvalidOperationException("No se encontró la raíz del repositorio.");
    }

    /// <summary>Abre la base, asegura F3 y carga el catálogo completo. Idempotente.</summary>
    public static EstadoArranque Cargar()
    {
        var raiz = RaizDatos();

        // Base de datos local en un directorio con permiso de escritura (%LOCALAPPDATA%):
        // junto al ejecutable (Program Files) NO se puede escribir como usuario normal.
        var localApp = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var dirDb = string.IsNullOrEmpty(localApp)
            ? Path.Combine(raiz, "FASE-II-DISENO", "run")
            : Path.Combine(localApp, "NetProtocol");
        Directory.CreateDirectory(dirDb);
        var store = new SqliteKnowledgeStore($"Data Source={Path.Combine(dirDb, "knowledge.db")};Pooling=False");

        var repositorio = new SqliteProtocolRepository(store);
        var servicios = new SqliteServiceRepository(store);
        var busqueda = new SqliteSearchEngine(store);

        var importados = DatasetBootstrap.EnsureProtocolos(store,
            Path.Combine(raiz, "FASE-03-INVENTARIO", "F3-Protocolos.json"));

        // Catálogos F3–F6 e índices de consulta: estado y lógica pura en el ViewModel, sin
        // dependencia de Avalonia (ver ViewModels/CatalogoViewModel.cs).
        var catalogo = CatalogoViewModel.Cargar(raiz, repositorio.GetAll(), servicios.Todos());

        return new EstadoArranque(repositorio, servicios, busqueda, catalogo, importados);
    }
}
