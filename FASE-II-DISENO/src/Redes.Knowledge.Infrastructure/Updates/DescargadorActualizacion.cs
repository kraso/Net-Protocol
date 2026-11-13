namespace Redes.Knowledge.Infrastructure.Updates;

/// <summary>
/// Descarga de un instalador publicado a un directorio temporal. No instala ni ejecuta nada:
/// la decisión de abrir el archivo es del usuario (S2 de v1.1).
/// </summary>
public static class DescargadorActualizacion
{
    /// <summary>Carpeta temporal donde quedan los instaladores descargados.</summary>
    public static string CarpetaTemporal =>
        Path.Combine(Path.GetTempPath(), "NetProtocol", "actualizaciones");

    /// <summary>
    /// Cliente para descargar instaladores. El de los metadatos usa 5 s, que es correcto para
    /// un JSON y demasiado poco para un binario de ~100 MB; aquí el margen es amplio y el
    /// User-Agent sigue siendo obligatorio para GitHub.
    /// </summary>
    public static HttpClient CrearCliente(TimeSpan? timeout = null)
    {
        var http = new HttpClient { Timeout = timeout ?? TimeSpan.FromMinutes(10) };
        http.DefaultRequestHeaders.UserAgent.ParseAdd("NetProtocol-UpdateCheck");
        return http;
    }

    /// <summary>
    /// Descarga <paramref name="url"/> en la carpeta temporal conservando el nombre del
    /// asset y devuelve la ruta resultante. Lanza si la respuesta no es 2xx o si el nombre
    /// del fichero no es utilizable; el llamador decide cómo informarlo.
    /// </summary>
    public static async Task<string> DescargarAsync(
        HttpClient http,
        string url,
        string nombreArchivo,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(http);
        if (string.IsNullOrWhiteSpace(url)) throw new ArgumentException("URL de descarga vacía.", nameof(url));
        if (string.IsNullOrWhiteSpace(nombreArchivo)) throw new ArgumentException("Nombre de asset vacío.", nameof(nombreArchivo));

        // Solo el nombre: evita que un asset con separadores escriba fuera de la carpeta.
        var nombre = Path.GetFileName(nombreArchivo.Trim());
        if (string.IsNullOrWhiteSpace(nombre)) throw new ArgumentException("Nombre de asset inválido.", nameof(nombreArchivo));

        Directory.CreateDirectory(CarpetaTemporal);
        var destino = Path.Combine(CarpetaTemporal, nombre);

        using var respuesta = await http
            .GetAsync(url, HttpCompletionOption.ResponseHeadersRead, ct)
            .ConfigureAwait(false);
        respuesta.EnsureSuccessStatusCode();

        await using var origen = await respuesta.Content.ReadAsStreamAsync(ct).ConfigureAwait(false);
        await using var archivo = File.Create(destino);
        await origen.CopyToAsync(archivo, ct).ConfigureAwait(false);

        return destino;
    }
}
