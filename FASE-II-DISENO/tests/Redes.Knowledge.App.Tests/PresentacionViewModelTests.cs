using Redes.Knowledge.App.ViewModels;
using Redes.Knowledge.Domain;
using Redes.Knowledge.Infrastructure.Capturas;

namespace Redes.Knowledge.App.Tests;

/// <summary>
/// Pruebas del estado de presentación extraído del code-behind (P2, incremento de
/// ViewModels de presentación). Son pruebas puras: no arrancan Avalonia.
/// </summary>
public class PresentacionViewModelTests
{
    private static Protocol Protocolo(string acronimo) => new()
    {
        Id = Urn.Parse($"urn:redes:protocol:{acronimo.ToLowerInvariant()}"),
        Acronimo = acronimo,
        Nombre = acronimo,
        Familia = FamiliaProtocolo.TRAN,
        Capas = "4",
        Estado = LifecycleState.Vigente
    };

    private static PcapPacket Paquete(int i) =>
        new(TimestampUs: i, CapturedLength: 1, OriginalLength: 1, Data: new byte[] { (byte)i });

    // --- Zoom -----------------------------------------------------------------

    [Fact]
    public void Zoom_arranca_en_100_por_ciento()
    {
        var vm = new PresentacionViewModel();

        Assert.Equal(1.0, vm.Zoom);
        Assert.Equal("100%", vm.ZoomTexto);
    }

    [Fact]
    public void AplicarZoom_suma_el_paso_y_formatea_el_porcentaje()
    {
        var vm = new PresentacionViewModel();

        Assert.Equal(1.1, vm.AplicarZoom(PresentacionViewModel.ZoomPaso), 10);
        Assert.Equal("110%", vm.ZoomTexto);

        Assert.Equal(1.0, vm.AplicarZoom(-PresentacionViewModel.ZoomPaso), 10);
        Assert.Equal("100%", vm.ZoomTexto);
    }

    [Fact]
    public void AplicarZoom_no_baja_del_minimo_ni_sube_del_maximo()
    {
        var vm = new PresentacionViewModel();

        for (var i = 0; i < 50; i++) vm.AplicarZoom(-PresentacionViewModel.ZoomPaso);
        Assert.Equal(PresentacionViewModel.ZoomMin, vm.Zoom, 10);
        Assert.Equal("70%", vm.ZoomTexto);

        for (var i = 0; i < 100; i++) vm.AplicarZoom(PresentacionViewModel.ZoomPaso);
        Assert.Equal(PresentacionViewModel.ZoomMax, vm.Zoom, 10);
        Assert.Equal("300%", vm.ZoomTexto);
    }

    [Fact]
    public void FijarBases_usa_la_reserva_si_la_fuente_no_es_valida()
    {
        var vm = new PresentacionViewModel();

        // Antes de abrir la ventana hay valores provisionales utilizables (13/14).
        Assert.True(vm.BasesFijadas);
        Assert.Equal(PresentacionViewModel.FuenteReserva, vm.FuenteBase);
        Assert.Equal(PresentacionViewModel.FuenteReserva + 1, vm.TituloBase);

        vm.FijarBases(double.NaN, double.NaN);

        Assert.Equal(PresentacionViewModel.FuenteReserva, vm.FuenteBase);
        Assert.Equal(PresentacionViewModel.FuenteReserva, vm.TituloBase);
    }

    [Fact]
    public void FijarBases_nunca_deja_el_titulo_por_debajo_del_contenido()
    {
        var vm = new PresentacionViewModel();

        vm.FijarBases(16, 10);

        Assert.Equal(16, vm.FuenteBase);
        Assert.Equal(16, vm.TituloBase);
    }

    [Fact]
    public void Fuentes_efectivas_escalan_con_el_zoom()
    {
        var vm = new PresentacionViewModel();
        vm.FijarBases(13, 14);
        vm.AplicarZoom(1.0); // 200 %

        Assert.Equal(26, vm.FuenteContenido, 10);
        Assert.Equal(28, vm.FuenteTitulo, 10);
    }

    // --- Selección ------------------------------------------------------------

    [Fact]
    public void Seleccionar_guarda_el_protocolo()
    {
        var vm = new PresentacionViewModel();
        var tcp = Protocolo("TCP");

        vm.Seleccionar(tcp);

        Assert.Same(tcp, vm.Seleccionado);
    }

    [Fact]
    public void Seleccionar_null_limpia_la_seleccion()
    {
        var vm = new PresentacionViewModel();
        vm.Seleccionar(Protocolo("TCP"));

        vm.Seleccionar(null);

        Assert.Null(vm.Seleccionado);
    }

    [Fact]
    public void Seleccionar_cierra_la_captura_abierta()
    {
        var vm = new PresentacionViewModel();
        vm.AbrirCaptura(new[] { Paquete(0) }, "muestra.pcap");

        vm.Seleccionar(Protocolo("TCP"));

        Assert.False(vm.CapturaAbierta);
        Assert.Empty(vm.Paquetes);
        Assert.Equal("", vm.RutaCaptura);
        Assert.Equal(-1, vm.IndicePaquete);
    }

    // --- Captura --------------------------------------------------------------

    [Fact]
    public void AbrirCaptura_deja_seleccionado_el_primer_paquete()
    {
        var vm = new PresentacionViewModel();

        vm.AbrirCaptura(new[] { Paquete(0), Paquete(1) }, "muestra.pcap");

        Assert.True(vm.CapturaAbierta);
        Assert.Equal(2, vm.TotalPaquetes);
        Assert.Equal("muestra.pcap", vm.RutaCaptura);
        Assert.Equal(0, vm.IndicePaquete);
        Assert.Equal(0, vm.PaqueteActual!.TimestampUs);
    }

    [Fact]
    public void AbrirCaptura_sin_paquetes_no_deja_indice_fuera_de_rango()
    {
        var vm = new PresentacionViewModel();

        vm.AbrirCaptura(Array.Empty<PcapPacket>(), "vacia.pcap");

        Assert.True(vm.CapturaAbierta);
        Assert.Equal(0, vm.TotalPaquetes);
        Assert.Equal(-1, vm.IndicePaquete);
        Assert.Null(vm.PaqueteActual);
    }

    [Fact]
    public void CerrarCaptura_devuelve_si_habia_una_abierta()
    {
        var vm = new PresentacionViewModel();
        Assert.False(vm.CerrarCaptura());

        vm.AbrirCaptura(new[] { Paquete(0) }, "muestra.pcap");
        Assert.True(vm.CerrarCaptura());

        Assert.False(vm.CapturaAbierta);
        Assert.Equal(-1, vm.IndicePaquete);
        Assert.False(vm.CerrarCaptura());
    }

    [Fact]
    public void SeleccionarPaquete_ignora_indices_fuera_de_rango()
    {
        var vm = new PresentacionViewModel();
        vm.AbrirCaptura(new[] { Paquete(0), Paquete(1) }, "muestra.pcap");

        Assert.False(vm.SeleccionarPaquete(-1));
        Assert.False(vm.SeleccionarPaquete(2));
        Assert.Equal(0, vm.IndicePaquete);

        Assert.True(vm.SeleccionarPaquete(1));
        Assert.Equal(1, vm.PaqueteActual!.TimestampUs);
    }

    [Fact]
    public void SeleccionarPaquete_sin_captura_siempre_falla()
    {
        var vm = new PresentacionViewModel();

        Assert.False(vm.SeleccionarPaquete(0));
        Assert.Equal(-1, vm.IndicePaquete);
    }
}
