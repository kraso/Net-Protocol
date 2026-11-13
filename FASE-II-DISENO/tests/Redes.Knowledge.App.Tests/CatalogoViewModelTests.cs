using Redes.Knowledge.App.ViewModels;
using Redes.Knowledge.Domain;
using Redes.Knowledge.Infrastructure.Iana;

namespace Redes.Knowledge.App.Tests;

/// <summary>
/// Pruebas del estado del catálogo extraído del code-behind (P2, incremento de ViewModels).
/// Son pruebas puras: no arrancan Avalonia ni tocan la base de datos.
/// </summary>
public class CatalogoViewModelTests
{
    private static string Raiz()
    {
        var d = new DirectoryInfo(AppContext.BaseDirectory);
        while (d is not null && !File.Exists(Path.Combine(d.FullName, "PLANREDES.md")))
            d = d.Parent;
        return d?.FullName ?? throw new InvalidOperationException("No se encontró la raíz del repositorio.");
    }

    private static CatalogoViewModel Cargar(IEnumerable<IanaServiceEntry>? servicios = null)
        => CatalogoViewModel.Cargar(
            Raiz(),
            ProtocolosDePrueba(),
            servicios ?? Array.Empty<IanaServiceEntry>());

    /// <summary>Catálogo mínimo con los acrónimos que usan las pruebas.</summary>
    private static IEnumerable<Protocol> ProtocolosDePrueba()
    {
        yield return new Protocol
        {
            Id = new Urn("urn:redes:protocolo:tcp"),
            Acronimo = "TCP",
            Nombre = "Transmission Control Protocol",
            Familia = FamiliaProtocolo.TRAN,
            Estado = LifecycleState.Vigente,
            Capas = "Transporte"
        };
        yield return new Protocol
        {
            Id = new Urn("urn:redes:protocolo:udp"),
            Acronimo = "UDP",
            Nombre = "User Datagram Protocol",
            Familia = FamiliaProtocolo.TRAN,
            Estado = LifecycleState.Vigente,
            Capas = "Transporte"
        };
        yield return new Protocol
        {
            Id = new Urn("urn:redes:protocolo:http"),
            Acronimo = "HTTP",
            Nombre = "Hypertext Transfer Protocol",
            Familia = FamiliaProtocolo.APP,
            Estado = LifecycleState.Vigente,
            Capas = "Aplicación"
        };
    }

    [Fact]
    public void Cargar_indexa_por_acronimo_y_por_nombre()
    {
        var vm = Cargar();

        Assert.Equal(3, vm.Total);
        Assert.Equal("TCP", vm.Resolver("tcp")?.Acronimo);
        Assert.Equal("TCP", vm.Resolver("Transmission Control Protocol")?.Acronimo);
        Assert.Equal("HTTP", vm.Resolver("hypertext-transfer-protocol")?.Acronimo);
        Assert.Null(vm.Resolver("inexistente"));
    }

    [Fact]
    public void Cargar_lee_los_catalogos_de_la_fase_i()
    {
        var vm = Cargar();

        Assert.NotEmpty(vm.Relaciones);
        Assert.NotEmpty(vm.Fichas);
        Assert.NotEmpty(vm.Familias);
        Assert.NotEmpty(vm.Notas);
        Assert.NotNull(vm.CamposF5De("TCP"));
        Assert.NotNull(vm.PduDe("TCP"));
    }

    [Fact]
    public void Activos_filtra_por_familia_y_estado()
    {
        var vm = Cargar();

        Assert.Equal(3, vm.Activos(null, null).Count());
        Assert.All(vm.Activos(FamiliaProtocolo.TRAN, null), p => Assert.Equal(FamiliaProtocolo.TRAN, p.Familia));
        Assert.Equal(2, vm.Activos(FamiliaProtocolo.TRAN, null).Count());
        Assert.Empty(vm.Activos(FamiliaProtocolo.APP, LifecycleState.Obsoleto));
    }

    [Fact]
    public void PuertosDe_une_coincidencia_exacta_y_vinculo_curado()
    {
        var vm = Cargar(new[]
        {
            new IanaServiceEntry("http", 80, "tcp", "", "", ""),
            new IanaServiceEntry("www-http", 80, "tcp", "", "", ""),
            new IanaServiceEntry("http-alt", 8080, "tcp", "", "", ""),
            new IanaServiceEntry("dominio-ajeno", 9999, "udp", "", "", "")
        });

        var puertos = vm.PuertosDe("HTTP");

        Assert.Contains("80/TCP", puertos);
        Assert.Contains("8080/TCP", puertos);
        Assert.DoesNotContain("9999", puertos);
        Assert.Equal("—", vm.PuertosDe("SIN-PUERTOS"));
    }

    [Fact]
    public void VecinosDe_usa_el_grafo_f4()
    {
        var vm = Cargar();

        var vecinos = vm.VecinosDe("TCP");

        Assert.NotEmpty(vecinos);
        Assert.Contains(vecinos, v => v.Nombre.Contains("IP", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Normalizar_quita_signos_y_mayusculas()
    {
        Assert.Equal("tcpip", CatalogoViewModel.Normalizar("TCP/IP"));
        Assert.Equal("http2", CatalogoViewModel.Normalizar("HTTP/2"));
        Assert.Equal("", CatalogoViewModel.Normalizar("---"));
    }

    [Fact]
    public void PorUrn_resuelve_el_identificador_de_catalogo()
    {
        var vm = Cargar();

        Assert.Equal("UDP", vm.PorUrn("urn:redes:protocolo:udp")?.Acronimo);
        Assert.Null(vm.PorUrn("urn:redes:protocolo:no-existe"));
    }
}
