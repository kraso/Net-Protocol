using System.Net;
using Redes.Knowledge.Infrastructure.Updates;

namespace Redes.Knowledge.Tests;

/// <summary>
/// v1.1/S2: elección del instalador por RID y descarga a temporal. Nada de esto instala ni
/// ejecuta: la verificación del hash y la decisión de abrir el archivo son del paso S2/T3.
/// </summary>
public class DescargadorActualizacionTests
{
    private static readonly string[] Assets =
    {
        "NetProtocol-1.0.15-win-x64.exe",
        "NetProtocol-1.0.15-linux-amd64.deb",
        "NetProtocol-1.0.15-linux-x86_64.AppImage",
        "NetProtocol-1.0.15-linux-x86_64.AppImage.asc",
        "NetProtocol-1.0.15-linux-arm64.deb",
        "NetProtocol-1.0.15-linux-aarch64.AppImage",
        "NetProtocol-1.0.15-macos-x64.dmg",
        "NetProtocol-1.0.15-macos-arm64.dmg",
        "SHA256SUMS"
    };

    [Theory]
    [InlineData("win-x64", "NetProtocol-1.0.15-win-x64.exe")]
    [InlineData("linux-x64", "NetProtocol-1.0.15-linux-x86_64.AppImage")]
    [InlineData("linux-arm64", "NetProtocol-1.0.15-linux-aarch64.AppImage")]
    [InlineData("osx-x64", "NetProtocol-1.0.15-macos-x64.dmg")]
    [InlineData("osx-arm64", "NetProtocol-1.0.15-macos-arm64.dmg")]
    public void EligeElInstaladorDelRid(string rid, string esperado)
        => Assert.Equal(esperado, AssetsActualizacion.Seleccionar(rid, Assets));

    [Fact]
    public void UnaFirmaNoSeConfundeConElInstalador()
    {
        var elegido = AssetsActualizacion.Seleccionar("linux-x64", Assets);

        Assert.NotNull(elegido);
        Assert.DoesNotContain(".asc", elegido);
    }

    [Fact]
    public void RidNoSoportadoOListaVaciaDevuelveNull()
    {
        Assert.Null(AssetsActualizacion.Seleccionar("freebsd-x64", Assets));
        Assert.Null(AssetsActualizacion.Seleccionar("win-x64", Array.Empty<string>()));
        Assert.Null(AssetsActualizacion.Seleccionar("win-x64", null));
        Assert.Null(AssetsActualizacion.Seleccionar(null, Assets));
    }

    private sealed class HandlerBytes : HttpMessageHandler
    {
        private readonly HttpStatusCode _codigo;
        private readonly byte[] _datos;

        public HandlerBytes(HttpStatusCode codigo, byte[] datos)
        {
            _codigo = codigo;
            _datos = datos;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
            => Task.FromResult(new HttpResponseMessage(_codigo) { Content = new ByteArrayContent(_datos) });
    }

    [Fact]
    public async Task DescargarEscribeElArchivoYDevuelveLaRuta()
    {
        var datos = new byte[] { 1, 2, 3, 4, 5 };
        using var http = new HttpClient(new HandlerBytes(HttpStatusCode.OK, datos));
        var nombre = $"NetProtocol-prueba-{Guid.NewGuid():N}.bin";

        var ruta = await DescargadorActualizacion.DescargarAsync(http, "https://x/asset", nombre);

        try
        {
            Assert.True(File.Exists(ruta));
            Assert.Equal(datos, await File.ReadAllBytesAsync(ruta));
            Assert.Equal(DescargadorActualizacion.CarpetaTemporal, Path.GetDirectoryName(ruta));
        }
        finally
        {
            File.Delete(ruta);
        }
    }

    [Fact]
    public async Task UnNombreConSeparadoresSeReduceAlNombreBase()
    {
        using var http = new HttpClient(new HandlerBytes(HttpStatusCode.OK, new byte[] { 9 }));
        var nombre = $"../../evil-{Guid.NewGuid():N}.bin";

        var ruta = await DescargadorActualizacion.DescargarAsync(http, "https://x/asset", nombre);

        try
        {
            Assert.Equal(DescargadorActualizacion.CarpetaTemporal, Path.GetDirectoryName(ruta));
            Assert.DoesNotContain("..", Path.GetFileName(ruta));
        }
        finally
        {
            File.Delete(ruta);
        }
    }

    [Fact]
    public async Task UnaRespuestaDeErrorLanzaYNoDejaArchivo()
    {
        using var http = new HttpClient(new HandlerBytes(HttpStatusCode.NotFound, Array.Empty<byte>()));
        var nombre = $"NetProtocol-error-{Guid.NewGuid():N}.bin";

        await Assert.ThrowsAsync<HttpRequestException>(
            () => DescargadorActualizacion.DescargarAsync(http, "https://x/asset", nombre));

        Assert.False(File.Exists(Path.Combine(DescargadorActualizacion.CarpetaTemporal, nombre)));
    }
}
