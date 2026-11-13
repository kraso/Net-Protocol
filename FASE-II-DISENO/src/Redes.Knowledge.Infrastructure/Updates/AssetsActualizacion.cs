namespace Redes.Knowledge.Infrastructure.Updates;

/// <summary>
/// Elección del instalador que corresponde al sistema en ejecución. La release publica un
/// asset por plataforma y arquitectura; el actualizador tiene que pedir el suyo y no otro.
/// <para>
/// En Linux se elige el <c>AppImage</c>: no exige permisos de administrador y funciona en
/// cualquier distribución, que es justo lo que necesita una actualización desde la app.
/// </para>
/// </summary>
public static class AssetsActualizacion
{
    /// <summary>
    /// Devuelve el nombre del asset para el RID dado, o <c>null</c> si no hay ninguno que
    /// corresponda (plataforma no soportada o release sin ese instalador).
    /// </summary>
    public static string? Seleccionar(string? rid, IEnumerable<string>? nombres)
    {
        if (string.IsNullOrWhiteSpace(rid) || nombres is null) return null;

        var sufijo = SufijoSegunRid(rid.Trim().ToLowerInvariant());
        if (sufijo is null) return null;

        return nombres.FirstOrDefault(n =>
            !string.IsNullOrWhiteSpace(n) && n.EndsWith(sufijo, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>Sufijo del nombre de fichero para cada RID soportado por la matriz de build.</summary>
    private static string? SufijoSegunRid(string rid) => rid switch
    {
        "win-x64" => "-win-x64.exe",
        "linux-x64" => "-linux-x86_64.AppImage",
        "linux-arm64" => "-linux-aarch64.AppImage",
        "osx-x64" => "-macos-x64.dmg",
        "osx-arm64" => "-macos-arm64.dmg",
        _ => null
    };
}
