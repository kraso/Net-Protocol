using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless.XUnit;
using Redes.Knowledge.App;
using Redes.Knowledge.Visualization;

namespace Redes.Knowledge.App.Tests;

/// <summary>
/// P2: reglas puras del supervisor de popups y su registro en la ventana. El cierre por
/// geometría y los re-enters espurios de X11 no se pueden reproducir en headless: eso queda
/// para la comprobación manual en Linux, como ya ocurría antes de extraerlo.
/// </summary>
public class PopupsTests
{
    private static readonly DateTime Instante = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void LaDeadlineCierraSoloPasadosLosCincoSegundos()
    {
        Assert.False(SupervisorPopups.DebeCerrarse(Instante, Instante.AddSeconds(5)));
        Assert.True(SupervisorPopups.DebeCerrarse(Instante, Instante.AddSeconds(6)));
    }

    [Fact]
    public void LaGraciaImpideReabrirHastaQueExpira()
    {
        var noReabrirHasta = Instante.AddMilliseconds(1200);

        Assert.True(SupervisorPopups.EnGracia(noReabrirHasta, Instante.AddMilliseconds(1199)));
        Assert.False(SupervisorPopups.EnGracia(noReabrirHasta, noReabrirHasta));
    }

    [AvaloniaFact]
    public void LaVentanaRegistraLosCuatroPopupsDeAccion()
    {
        var previo = Textos.Idioma;
        try
        {
            Textos.Fijar(Textos.Es);
            var ventana = new MainWindow();

            var raiz = ventana.Content as Panel;
            Assert.NotNull(raiz);
            Assert.Equal(4, raiz!.Children.OfType<Popup>().Count());
        }
        finally
        {
            Textos.Fijar(previo);
        }
    }
}
