using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Interactivity;
using Redes.Knowledge.App;
using Redes.Knowledge.Domain;
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

    [AvaloniaFact]
    public void CambiarIdiomaTraduceElComparador()
    {
        var previo = Textos.Idioma;
        try
        {
            Textos.Fijar(Textos.Es);
            var ventana = NuevaVentana();

            Pulsar(ventana, "LanguageButton");
            Pulsar(ventana, "CompareButton");

            var texto = Detalle(ventana);
            Assert.Contains("=== Comparison:", texto);
            Assert.Contains("Ports (IANA)", texto);
            Assert.Contains("Sources: F3", texto);
            Assert.DoesNotContain("Comparador:", texto);
        }
        finally
        {
            Textos.Fijar(previo);
        }
    }

    [AvaloniaFact]
    public void CambiarIdiomaTraduceLaCabeceraDeResultados()
    {
        var previo = Textos.Idioma;
        try
        {
            Textos.Fijar(Textos.Es);
            var ventana = NuevaVentana();
            ventana.FindControl<TextBox>("SearchBox")!.Text = "TCP";

            Pulsar(ventana, "SearchButton");

            var panel = ventana.FindControl<StackPanel>("NavPanel")!;
            var cabecera = panel.Children.OfType<Expander>().Single().Header as string;
            Assert.StartsWith("Resultados (", cabecera);

            Pulsar(ventana, "LanguageButton");

            var traducida = panel.Children.OfType<Expander>().Single().Header as string;
            Assert.StartsWith("Results (", traducida);
        }
        finally
        {
            Textos.Fijar(previo);
        }
    }

    [AvaloniaFact]
    public void ElFiltroPorEstadoSigueFuncionandoConEtiquetasTraducidas()
    {
        var previo = Textos.Idioma;
        try
        {
            Textos.Fijar(Textos.Es);
            var ventana = NuevaVentana();
            var panel = ventana.FindControl<StackPanel>("NavPanel")!;
            var estado = ventana.FindControl<ComboBox>("FilterEstado")!;

            var todos = Recolectar(panel);
            Assert.True(todos.Count > 0);

            estado.SelectedIndex = 1; // primer valor del enum: Vigente / Current
            var vigentes = Recolectar(panel);

            Assert.Equal(todos.Count(p => p.Estado == LifecycleState.Vigente), vigentes.Count);
            Assert.All(vigentes, p => Assert.Equal(LifecycleState.Vigente, p.Estado));

            // Al traducir cambia la etiqueta visible, pero el filtro sigue leyendo el enum
            // que viaja en Tag: el conjunto filtrado no se mueve.
            Pulsar(ventana, "LanguageButton");

            Assert.Equal("Current", (estado.SelectedItem as ComboBoxItem)?.Content as string);
            Assert.Equal(vigentes.Count, Recolectar(panel).Count);
        }
        finally
        {
            Textos.Fijar(previo);
        }
    }

    private static List<Protocol> Recolectar(StackPanel panel) =>
        panel.Children.OfType<Expander>()
            .SelectMany(e => ((ListBox)e.Content!).Items.OfType<ListBoxItem>())
            .Select(i => i.Tag).OfType<Protocol>()
            .ToList();
}
