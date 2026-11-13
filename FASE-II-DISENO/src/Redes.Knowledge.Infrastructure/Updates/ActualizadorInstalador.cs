using System.Security.Cryptography;
using Redes.Knowledge.Domain;

namespace Redes.Knowledge.Infrastructure.Updates;

/// <summary>Resultado de descargar un instalador y comprobar su hash.</summary>
public enum EstadoDescarga
{
    /// <summary>Descargado y con el SHA256 que publica la release.</summary>
    Verificado,
    /// <summary>La release no publica instalador para este sistema.</summary>
    SinAsset,
    /// <summary>No hay SHA256SUMS o no incluye este instalador.</summary>
    SinSumas,
    /// <summary>El hash no coincide: el archivo se descarta.</summary>
    HashInvalido,
    /// <summary>Sin red, timeout o error de escritura.</summary>
    Error
}

public sealed record ResultadoDescarga(EstadoDescarga Estado, string? Ruta = null, string? Detalle = null)
{
    public bool Verificado => Estado == EstadoDescarga.Verificado;
}

/// <summary>
/// Descarga el instalador de la versión nueva y comprueba su SHA256 contra el fichero
/// <c>SHA256SUMS</c> que publica la release. No ejecuta ni instala nada: eso queda en manos
/// del usuario (S2 de v1.1).
/// <para>
/// Sin firma de código, el hash garantiza integridad del transporte, no autoría: si alguien
/// controla la release, controla también el hash. Se documenta en la interfaz y en SECURITY.
/// </para>
/// </summary>
public static class ActualizadorInstalador
{
    public const string NombreSumas = "SHA256SUMS";

    /// <summary>Descarga y verifica. Nunca lanza por fallo de red: devuelve el estado.</summary>
    public static async Task<ResultadoDescarga> DescargarYVerificarAsync(
        HttpClient http,
        ResultadoActualizacion actualizacion,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(http);
        ArgumentNullException.ThrowIfNull(actualizacion);

        if (actualizacion.NombreAsset is not { Length: > 0 } nombre ||
            string.IsNullOrWhiteSpace(actualizacion.UrlAsset))
            return new(EstadoDescarga.SinAsset, Detalle: "la release no publica instalador para este sistema");

        try
        {
            var sumas = LeerSumas(
                await http.GetStringAsync(UrlHermana(actualizacion.UrlAsset, NombreSumas), ct).ConfigureAwait(false));

            if (!sumas.TryGetValue(nombre, out var esperado))
                return new(EstadoDescarga.SinSumas, Detalle: $"{NombreSumas} no incluye {nombre}");

            var ruta = await DescargadorActualizacion
                .DescargarAsync(http, actualizacion.UrlAsset, nombre, ct)
                .ConfigureAwait(false);

            string obtenido;
            await using (var archivo = File.OpenRead(ruta))
                obtenido = Convert.ToHexString(await SHA256.HashDataAsync(archivo, ct).ConfigureAwait(false));

            if (!string.Equals(obtenido, esperado, StringComparison.OrdinalIgnoreCase))
            {
                File.Delete(ruta); // no dejamos en disco algo que no coincide con lo publicado
                return new(EstadoDescarga.HashInvalido, Detalle: $"esperado {esperado}, obtenido {obtenido}");
            }

            return new(EstadoDescarga.Verificado, Ruta: ruta);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or IOException or UnauthorizedAccessException)
        {
            return new(EstadoDescarga.Error, Detalle: ex.Message);
        }
    }

    /// <summary>URL del fichero hermano al asset (mismo directorio de la release).</summary>
    public static string UrlHermana(string urlAsset, string nombre)
    {
        var corte = urlAsset.LastIndexOf('/');
        return corte < 0 ? nombre : string.Concat(urlAsset.AsSpan(0, corte + 1), nombre);
    }

    /// <summary>
    /// Lee un <c>SHA256SUMS</c> en formato <c>&lt;64 hex&gt;  &lt;nombre&gt;</c> (admite el
    /// marcador <c>*</c> del modo binario). Las líneas que no encajan se ignoran.
    /// </summary>
    public static IReadOnlyDictionary<string, string> LeerSumas(string? contenido)
    {
        var mapa = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var cruda in (contenido ?? "").Replace("\r\n", "\n").Split('\n'))
        {
            var linea = cruda.Trim();
            if (linea.Length == 0) continue;

            var corte = linea.IndexOfAny(new[] { ' ', '\t' });
            if (corte <= 0) continue;

            var hash = linea[..corte];
            if (hash.Length != 64 || !hash.All(Uri.IsHexDigit)) continue;

            var nombre = linea[(corte + 1)..].TrimStart('*', ' ', '\t').Trim();
            if (nombre.Length == 0) continue;

            mapa[nombre] = hash;
        }
        return mapa;
    }
}
