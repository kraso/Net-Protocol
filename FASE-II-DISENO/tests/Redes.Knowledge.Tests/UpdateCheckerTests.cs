using System.Net;
using System.Text;
using Redes.Knowledge.Domain;
using Redes.Knowledge.Infrastructure.Updates;
using Version = System.Version;

namespace Redes.Knowledge.Tests;

/// <summary>
/// Comprobación de actualizaciones (P1): la app es local-first, así que el contrato que
/// se prueba aquí es que NUNCA lanza y que distingue al día / disponible / no comprobado.
/// </summary>
public class UpdateCheckerTests
{
    private sealed class HandlerEstatico : HttpMessageHandler
    {
        private readonly HttpStatusCode _codigo;
        private readonly string _cuerpo;
        private readonly Exception? _excepcion;

        public HandlerEstatico(HttpStatusCode codigo, string cuerpo, Exception? excepcion = null)
        {
            _codigo = codigo;
            _cuerpo = cuerpo;
            _excepcion = excepcion;
        }

        public string? UrlSolicitada { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            UrlSolicitada = request.RequestUri?.ToString();
            if (_excepcion is not null) throw _excepcion;
            return Task.FromResult(new HttpResponseMessage(_codigo)
            {
                Content = new StringContent(_cuerpo, Encoding.UTF8, "application/json")
            });
        }
    }

    private static GitHubUpdateChecker Con(HttpStatusCode codigo, string cuerpo, Exception? ex = null) =>
        new(new HttpClient(new HandlerEstatico(codigo, cuerpo, ex)));

    private const string ReleaseValida =
        """{"tag_name":"v1.0.15","html_url":"https://github.com/kraso/Net-Protocol/releases/tag/v1.0.15"}""";

    private const string ReleaseConAssets =
        """{"tag_name":"v1.0.15","html_url":"https://github.com/kraso/Net-Protocol/releases/tag/v1.0.15","assets":[{"name":"NetProtocol-1.0.15-win-x64.exe","browser_download_url":"https://dl/win.exe"},{"name":"NetProtocol-1.0.15-linux-x86_64.AppImage","browser_download_url":"https://dl/linux.AppImage"},{"name":"NetProtocol-1.0.15-linux-aarch64.AppImage","browser_download_url":"https://dl/linux-arm.AppImage"},{"name":"NetProtocol-1.0.15-macos-x64.dmg","browser_download_url":"https://dl/mac-x64.dmg"},{"name":"NetProtocol-1.0.15-macos-arm64.dmg","browser_download_url":"https://dl/mac-arm.dmg"}]}""";

    [Fact]
    public async Task VersionDisponibleConAssets_EligeElDelSistemaEnEjecucion()
    {
        var rid = System.Runtime.InteropServices.RuntimeInformation.RuntimeIdentifier;
        var nombres = new[]
        {
            "NetProtocol-1.0.15-win-x64.exe",
            "NetProtocol-1.0.15-linux-x86_64.AppImage",
            "NetProtocol-1.0.15-linux-aarch64.AppImage",
            "NetProtocol-1.0.15-macos-x64.dmg",
            "NetProtocol-1.0.15-macos-arm64.dmg"
        };

        var r = await Con(HttpStatusCode.OK, ReleaseConAssets).CheckAsync(new Version(1, 0, 14));

        Assert.Equal(AssetsActualizacion.Seleccionar(rid, nombres), r.NombreAsset);
        if (r.NombreAsset is not null)
            Assert.StartsWith("https://dl/", r.UrlAsset);
    }

    [Fact]
    public async Task VersionPublicadaMayor_DevuelveDisponibleConUrl()
    {
        var r = await Con(HttpStatusCode.OK, ReleaseValida).CheckAsync(new Version(1, 0, 14));

        Assert.Equal(EstadoActualizacion.Disponible, r.Estado);
        Assert.True(r.HayActualizacion);
        Assert.Equal("1.0.15", r.VersionDisponible);
        Assert.Contains("v1.0.15", r.UrlDescarga);
    }

    [Fact]
    public async Task MismaVersion_DevuelveAlDia()
    {
        var r = await Con(HttpStatusCode.OK, ReleaseValida).CheckAsync(new Version(1, 0, 15));

        Assert.Equal(EstadoActualizacion.AlDia, r.Estado);
        Assert.False(r.HayActualizacion);
    }

    [Fact]
    public async Task VersionInstaladaMayor_DevuelveAlDia()
    {
        var r = await Con(HttpStatusCode.OK, ReleaseValida).CheckAsync(new Version(2, 0, 0));

        Assert.Equal(EstadoActualizacion.AlDia, r.Estado);
    }

    [Fact]
    public async Task SinRed_NoLanzaYDevuelveNoComprobado()
    {
        var r = await Con(HttpStatusCode.OK, "", new HttpRequestException("host inalcanzable"))
            .CheckAsync(new Version(1, 0, 14));

        Assert.Equal(EstadoActualizacion.NoComprobado, r.Estado);
        Assert.Contains("sin conexión", r.Detalle);
    }

    [Fact]
    public async Task RepositorioSinReleases_DevuelveNoComprobado()
    {
        var r = await Con(HttpStatusCode.NotFound, "{}").CheckAsync(new Version(1, 0, 14));

        Assert.Equal(EstadoActualizacion.NoComprobado, r.Estado);
        Assert.Contains("releases", r.Detalle);
    }

    [Fact]
    public async Task ErrorHttp_DevuelveNoComprobadoConCodigo()
    {
        var r = await Con(HttpStatusCode.Forbidden, "{}").CheckAsync(new Version(1, 0, 14));

        Assert.Equal(EstadoActualizacion.NoComprobado, r.Estado);
        Assert.Contains("403", r.Detalle);
    }

    [Fact]
    public async Task JsonInvalido_DevuelveNoComprobado()
    {
        var r = await Con(HttpStatusCode.OK, "no es json").CheckAsync(new Version(1, 0, 14));

        Assert.Equal(EstadoActualizacion.NoComprobado, r.Estado);
    }

    [Fact]
    public async Task TagSinVersion_DevuelveNoComprobado()
    {
        var r = await Con(HttpStatusCode.OK, """{"tag_name":"nightly"}""")
            .CheckAsync(new Version(1, 0, 14));

        Assert.Equal(EstadoActualizacion.NoComprobado, r.Estado);
        Assert.Contains("nightly", r.Detalle);
    }

    [Fact]
    public async Task ConsultaLaUrlDelRepositorioIndicado()
    {
        var handler = new HandlerEstatico(HttpStatusCode.OK, ReleaseValida);
        var checker = new GitHubUpdateChecker(new HttpClient(handler), "kraso/Net-Protocol");

        await checker.CheckAsync(new Version(1, 0, 14));

        Assert.Equal("https://api.github.com/repos/kraso/Net-Protocol/releases/latest", handler.UrlSolicitada);
    }

    [Theory]
    [InlineData("v1.0.14", "1.0.14")]
    [InlineData("1.0.14", "1.0.14")]
    [InlineData("V2.3.4", "2.3.4")]
    [InlineData("v1.0.14-beta.1", "1.0.14")]
    [InlineData("v1.0.14+build.7", "1.0.14")]
    public void TryParsearVersion_AceptaFormatosDeTag(string tag, string esperado)
    {
        Assert.True(GitHubUpdateChecker.TryParsearVersion(tag, out var v));
        Assert.Equal(Version.Parse(esperado), v);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("nightly")]
    [InlineData("v")]
    public void TryParsearVersion_RechazaTagsNoVersionados(string tag)
    {
        Assert.False(GitHubUpdateChecker.TryParsearVersion(tag, out _));
    }
}
