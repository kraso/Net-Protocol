using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Redes.Knowledge.App;

namespace Redes.Knowledge.App.Tests;

/// <summary>
/// Pruebas headless de la ventana principal (P1). Cubren los flujos que ya fallaron en
/// releases anteriores y que ningún test de dominio podía detectar: que la ventana se
/// construye, que la barra de estado informa del dataset y que el aviso de actualización
/// empieza oculto (no ocupa espacio hasta que hay versión nueva).
/// </summary>
public class MainWindowHeadlessTests
{
    [AvaloniaFact]
    public void VentanaSeConstruyeYExponeLosControlesClave()
    {
        var ventana = new MainWindow();

        Assert.NotNull(ventana.FindControl<SelectableTextBlock>("StatusText"));
        Assert.NotNull(ventana.FindControl<SelectableTextBlock>("UpdateText"));
        Assert.NotNull(ventana.FindControl<TextBox>("SearchBox"));
        Assert.NotNull(ventana.FindControl<Button>("CompareButton"));
        Assert.NotNull(ventana.FindControl<Button>("ExportButton"));
    }

    [AvaloniaFact]
    public void BarraDeEstadoInformaDelDatasetCargado()
    {
        var ventana = new MainWindow();
        var estado = ventana.FindControl<SelectableTextBlock>("StatusText");

        Assert.NotNull(estado);
        Assert.Contains("protocolos", estado!.Text);
        Assert.Contains("servicios IANA", estado.Text);
    }

    [AvaloniaFact]
    public void AvisoDeActualizacionEmpiezaOculto()
    {
        var ventana = new MainWindow();
        var aviso = ventana.FindControl<SelectableTextBlock>("UpdateText");

        Assert.NotNull(aviso);
        Assert.False(aviso!.IsVisible);
    }

    [AvaloniaFact]
    public void VentanaArrancaConTituloYDimensionMinima()
    {
        var ventana = new MainWindow();

        Assert.False(string.IsNullOrWhiteSpace(ventana.Title));
        Assert.True(ventana.MinWidth >= 900);
        Assert.True(ventana.MinHeight >= 500);
    }
}
