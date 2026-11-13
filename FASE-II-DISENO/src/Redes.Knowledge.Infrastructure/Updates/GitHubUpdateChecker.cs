using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Redes.Knowledge.Domain;

namespace Redes.Knowledge.Infrastructure.Updates;

/// <summary>
/// Comprobación de actualizaciones contra la API pública de releases de GitHub.
/// Diseño local-first: la consulta tiene timeout corto, cualquier fallo se traduce en
/// <see cref="EstadoActualizacion.NoComprobado"/> y nunca lanza hacia la interfaz.
/// </summary>
public sealed class GitHubUpdateChecker : IUpdateChecker
{
    public const string RepositorioPorDefecto = "kraso/Net-Protocol";

    private static readonly TimeSpan TimeoutPorDefecto = TimeSpan.FromSeconds(5);

    private readonly HttpClient _http;
    private readonly string _repositorio;
    private readonly TimeSpan _timeout;

    public GitHubUpdateChecker(HttpClient http, string repositorio = RepositorioPorDefecto, TimeSpan? timeout = null)
    {
        _http = http ?? throw new ArgumentNullException(nameof(http));
        _repositorio = string.IsNullOrWhiteSpace(repositorio) ? RepositorioPorDefecto : repositorio;
        _timeout = timeout ?? TimeoutPorDefecto;
    }

    /// <summary>Cliente por defecto: User-Agent obligatorio en la API de GitHub.</summary>
    public static HttpClient CrearCliente()
    {
        var http = new HttpClient { Timeout = TimeoutPorDefecto };
        http.DefaultRequestHeaders.UserAgent.ParseAdd("NetProtocol-UpdateCheck");
        http.DefaultRequestHeaders.Accept.ParseAdd("application/vnd.github+json");
        return http;
    }

    public async Task<ResultadoActualizacion> CheckAsync(System.Version versionActual, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(versionActual);

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(_timeout);

        try
        {
            var url = $"https://api.github.com/repos/{_repositorio}/releases/latest";
            using var respuesta = await _http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cts.Token)
                .ConfigureAwait(false);

            if (respuesta.StatusCode == HttpStatusCode.NotFound)
                return ResultadoActualizacion.NoComprobado("el repositorio no publica releases");

            if (!respuesta.IsSuccessStatusCode)
                return ResultadoActualizacion.NoComprobado($"respuesta HTTP {(int)respuesta.StatusCode}");

            var release = await respuesta.Content
                .ReadFromJsonAsync<ReleaseGitHub>(cancellationToken: cts.Token)
                .ConfigureAwait(false);

            if (release is null || string.IsNullOrWhiteSpace(release.TagName))
                return ResultadoActualizacion.NoComprobado("respuesta sin tag de versión");

            if (!TryParsearVersion(release.TagName, out var publicada))
                return ResultadoActualizacion.NoComprobado($"tag no interpretable: {release.TagName}");

            var texto = publicada.ToString(3);
            return publicada > versionActual
                ? ResultadoActualizacion.Disponible(texto, release.HtmlUrl)
                : ResultadoActualizacion.AlDia(texto);
        }
        catch (OperationCanceledException)
        {
            return ResultadoActualizacion.NoComprobado("tiempo de espera agotado");
        }
        catch (HttpRequestException ex)
        {
            return ResultadoActualizacion.NoComprobado($"sin conexión: {ex.Message}");
        }
        catch (JsonException)
        {
            return ResultadoActualizacion.NoComprobado("respuesta JSON inválida");
        }
    }

    /// <summary>Acepta "v1.0.14", "1.0.14" y sufijos tipo "v1.0.14-beta.1".</summary>
    public static bool TryParsearVersion(string tag, out System.Version version)
    {
        version = new System.Version(0, 0, 0);
        if (string.IsNullOrWhiteSpace(tag)) return false;

        var limpio = tag.Trim();
        if (limpio.StartsWith('v') || limpio.StartsWith('V')) limpio = limpio[1..];

        var corte = limpio.IndexOfAny(new[] { '-', '+' });
        if (corte >= 0) limpio = limpio[..corte];

        if (!System.Version.TryParse(limpio, out var parseada) || parseada.Major < 0) return false;
        version = parseada;
        return true;
    }

    private sealed record ReleaseGitHub(
        [property: JsonPropertyName("tag_name")] string? TagName,
        [property: JsonPropertyName("html_url")] string? HtmlUrl);
}
