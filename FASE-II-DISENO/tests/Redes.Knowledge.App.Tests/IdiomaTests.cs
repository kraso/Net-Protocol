using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Interactivity;
using Redes.Knowledge.App;
using Redes.Knowledge.Visualization;

namespace Redes.Knowledge.App.Tests;

/// <summary>
/// Regresión del hallazgo R3-001 de la revisión de i18n: cambiar de idioma mientras se ve
/// la leyenda, «Acerca de» o la comparación debe repintar ESA vista, no navegar a la ficha.
/// </summary>
public class IdiomaTests
{
    private static MainWindow NuevaVentana() => new();

    private static void Pulsar(MainWindow ventana, string nombre)
    {
        var boton = ventana.FindControl<Button>(nombre)!;
        boton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
    }

    private static string Detalle(MainWindow ventana) =>
        ventana.FindControl<SelectableTextBlock>("DetailText")!.Text ?? "";

    [AvaloniaFact]
    public void CambiarIdiomaEnLaLeyendaConservalaLeyenda()
    {
        var previo = Textos.Idioma;
        try
        {
            Textos.Fijar(Textos.Es);
            var ventana = NuevaVentana();
            Pulsar(ventana, "LegendButton");
            Assert.Contains("Leyenda de familias de protocolos", Detalle(ventana));

            Pulsar(ventana, "LanguageButton");

            Assert.Equal(Textos.En, Textos.Idioma);
            Assert.Contains("Protocol family legend", Detalle(ventana));
            Assert.DoesNotContain("Leyenda de familias de protocolos", Detalle(ventana));
        }
        finally
        {
            Textos.Fijar(previo);
        }
    }

    [AvaloniaFact]
    public void CambiarIdiomaEnAcercaDeConservaAcercaDe()
    {
        var previo = Textos.Idioma;
        try
        {
            Textos.Fijar(Textos.Es);
            var ventana = NuevaVentana();
            Pulsar(ventana, "AboutButton");
            Assert.Contains("=== Acerca de ===", Detalle(ventana));

            Pulsar(ventana, "LanguageButton");

            Assert.Contains("=== About ===", Detalle(ventana));
        }
        finally
        {
            Textos.Fijar(previo);
        }
    }

    [AvaloniaFact]
    public void CambiarIdiomaEnLaFichaConservaLaFicha()
    {
        var previo = Textos.Idioma;
        try
        {
            Textos.Fijar(Textos.Es);
            var ventana = NuevaVentana();
            var antes = Detalle(ventana);
            Assert.False(string.IsNullOrWhiteSpace(antes));

            Pulsar(ventana, "LanguageButton");

            Assert.Equal(Textos.En, Textos.Idioma);
            Assert.False(string.IsNullOrWhiteSpace(Detalle(ventana)));
        }
        finally
        {
            Textos.Fijar(previo);
        }
    }

    [AvaloniaFact]
    public void ElBotonDeIdiomaMuestraElIdiomaDestino()
    {
        var previo = Textos.Idioma;
        try
        {
            Textos.Fijar(Textos.Es);
            var ventana = NuevaVentana();
            var boton = ventana.FindControl<Button>("LanguageButton")!;
            Assert.Equal("EN", boton.Content);

            Pulsar(ventana, "LanguageButton");

            Assert.Equal("ES", boton.Content);
        }
        finally
        {
            Textos.Fijar(previo);
        }
    }
}
