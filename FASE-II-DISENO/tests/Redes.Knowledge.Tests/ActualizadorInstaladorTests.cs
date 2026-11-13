using System.Net;
using System.Security.Cryptography;
using System.Text;
using Redes.Knowledge.Domain;
using Redes.Knowledge.Infrastructure.Updates;

namespace Redes.Knowledge.Tests;

/// <summary>
/// v1.1/S2-T3: descarga del instalador con verificación SHA256. Los cinco caminos que fija
/// el criterio de aceptación: hash correcto, hash manipulado, SHA256SUMS sin el asset,
/// release sin asset para el sistema y red caída.
/// </summary>
public class ActualizadorInstaladorTests
{
    private static readonly byte[] Payload = { 10, 20, 30, 40, 50 };

    private static string Sha(byte[] datos) => Convert.ToHexString(SHA256.HashData(datos)).ToLowerInvariant();

    /// <summary>Sirve una URL por ruta: /SHA256SUMS y /instalador.</summary>
    private sealed class HandlerMapa : HttpMessageHandler
    {
        private readonly Dictionary<string, (HttpStatusCode Codigo, byte[] Cuerpo)> _rutas = new(StringComparer.Ordinal);

        public HandlerMapa Con(string ruta, string cuerpo, HttpStatusCode codigo = HttpStatusCode.OK)
        {
            _rutas[ruta] = (codigo, Encoding.UTF8.GetBytes(cuerpo));
            return this;
        }

        public HandlerMapa ConBytes(string ruta, byte[] cuerpo, HttpStatusCode codigo = HttpStatusCode.OK)
        {
            _rutas[ruta] = (codigo, cuerpo);
            return this;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            var ruta = request.RequestUri!.AbsolutePath;
            return Task.FromResult(_rutas.TryGetValue(ruta, out var r)
                ? new HttpResponseMessage(r.Codigo) { Content = new ByteArrayContent(r.Cuerpo) }
                : new HttpResponseMessage(HttpStatusCode.NotFound));
        }
    }

    private static ResultadoActualizacion Disponible(string nombre = "NetProtocol-1.0.15-win-x64.exe") =>
        ResultadoActualizacion.Disponible("1.0.15", "https://x/releases/tag/v1.0.15", nombre,
            $"https://dl/releases/download/v1.0.15/{nombre}");

    [Fact]
    public async Task HashCorrecto_VerificaYDejaElArchivo()
    {
        using var http = new HttpClient(new HandlerMapa()
            .Con("/releases/download/v1.0.15/SHA256SUMS", $"{Sha(Payload)}  NetProtocol-1.0.15-win-x64.exe\n")
            .ConBytes("/releases/download/v1.0.15/NetProtocol-1.0.15-win-x64.exe", Payload));

        var r = await ActualizadorInstalador.DescargarYVerificarAsync(http, Disponible());

        try
        {
            Assert.Equal(EstadoDescarga.Verificado, r.Estado);
            Assert.True(r.Verificado);
            Assert.True(File.Exists(r.Ruta));
            Assert.Equal(Payload, await File.ReadAllBytesAsync(r.Ruta!));
        }
        finally
        {
            if (r.Ruta is not null && File.Exists(r.Ruta)) File.Delete(r.Ruta);
        }
    }

    [Fact]
    public async Task HashManipulado_RechazaYBorraElArchivo()
    {
        using var http = new HttpClient(new HandlerMapa()
            .Con("/releases/download/v1.0.15/SHA256SUMS", $"{new string('a', 64)}  NetProtocol-1.0.15-win-x64.exe\n")
            .ConBytes("/releases/download/v1.0.15/NetProtocol-1.0.15-win-x64.exe", Payload));

        var r = await ActualizadorInstalador.DescargarYVerificarAsync(http, Disponible());

        Assert.Equal(EstadoDescarga.HashInvalido, r.Estado);
        Assert.Null(r.Ruta);
        Assert.False(File.Exists(Path.Combine(DescargadorActualizacion.CarpetaTemporal, "NetProtocol-1.0.15-win-x64.exe")));
    }

    [Fact]
    public async Task SumasSinElAsset_NoDescargaNada()
    {
        using var http = new HttpClient(new HandlerMapa()
            .Con("/releases/download/v1.0.15/SHA256SUMS", $"{Sha(Payload)}  otro.exe\n"));

        var r = await ActualizadorInstalador.DescargarYVerificarAsync(http, Disponible());

        Assert.Equal(EstadoDescarga.SinSumas, r.Estado);
    }

    [Fact]
    public async Task ReleaseSinAssetParaElSistema_DevuelveSinAsset()
    {
        using var http = new HttpClient(new HandlerMapa());
        var sinAsset = ResultadoActualizacion.Disponible("1.0.15", "https://x/releases/tag/v1.0.15");

        var r = await ActualizadorInstalador.DescargarYVerificarAsync(http, sinAsset);

        Assert.Equal(EstadoDescarga.SinAsset, r.Estado);
    }

    [Fact]
    public async Task RedCaida_DevuelveErrorSinLanzar()
    {
        using var http = new HttpClient(new HandlerMapa());
        var r = await ActualizadorInstalador.DescargarYVerificarAsync(http, Disponible());

        Assert.Equal(EstadoDescarga.Error, r.Estado);
        Assert.False(string.IsNullOrWhiteSpace(r.Detalle));
    }

    [Fact]
    public void LeerSumas_AceptaMarcadorBinarioEIgnoraBasura()
    {
        var contenido = string.Join("\n", new[]
        {
            $"{Sha(Payload)}  NetProtocol-1.0.15-win-x64.exe",
            $"{Sha(Payload)} *NetProtocol-1.0.15-linux-x86_64.AppImage",
            "linea sin formato",
            "",
            $"{new string('f', 64)}",
        });

        var sumas = ActualizadorInstalador.LeerSumas(contenido);

        Assert.Equal(2, sumas.Count);
        Assert.Equal(Sha(Payload), sumas["NetProtocol-1.0.15-win-x64.exe"]);
        Assert.Equal(Sha(Payload), sumas["NetProtocol-1.0.15-linux-x86_64.AppImage"]);
    }

    [Fact]
    public void UrlHermana_ApuntaAlMismoDirectorio()
    {
        Assert.Equal(
            "https://dl/releases/download/v1.0.15/SHA256SUMS",
            ActualizadorInstalador.UrlHermana("https://dl/releases/download/v1.0.15/NetProtocol.exe", "SHA256SUMS"));

        Assert.Equal("SHA256SUMS", ActualizadorInstalador.UrlHermana("sinbarras", "SHA256SUMS"));
    }
}
