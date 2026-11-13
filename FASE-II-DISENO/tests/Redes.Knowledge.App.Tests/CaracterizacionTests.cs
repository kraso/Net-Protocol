using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Interactivity;
using Redes.Knowledge.App;

namespace Redes.Knowledge.App.Tests;

/// <summary>
/// Pruebas de caracterización (P2, incremento I0): congelan la salida EXACTA que produce hoy
/// la ventana antes de extraer los formateadores a <c>Redes.Knowledge.Visualization</c>. Si un
/// refactor cambia un espacio de padding o el orden de una línea, estas pruebas fallan; las
/// doradas viven en <c>Golden/</c> y se comparan con los saltos de línea normalizados a <c>\n</c>
/// para que el resultado sea idéntico en Windows y en Linux.
/// </summary>
public class CaracterizacionTests
{
    private static string Dorada(string nombre)
    {
        var ruta = Path.Combine(AppContext.BaseDirectory, "Golden", nombre);
        Assert.True(File.Exists(ruta), $"Falta la dorada {ruta}");
        return Normalizar(File.ReadAllText(ruta));
    }

    private static string Normalizar(string texto) =>
        texto.Replace("\r\n", "\n").Replace('\r', '\n').TrimEnd('\n');

    private static MainWindow NuevaVentana() => new();

    private static object Protocolo(MainWindow ventana, string acronimo)
    {
        var selector = ventana.FindControl<ComboBox>("ProtocolSelector")!;
        return selector.Items.Cast<object>().First(o =>
            string.Equals(o.GetType().GetProperty("Acronimo")?.GetValue(o)?.ToString(),
                          acronimo, StringComparison.OrdinalIgnoreCase));
    }

    private static void Seleccionar(MainWindow ventana, string acronimo) =>
        ventana.FindControl<ComboBox>("ProtocolSelector")!.SelectedItem = Protocolo(ventana, acronimo);

    [AvaloniaFact]
    public void FichaTcpCoincideConLaDorada()
    {
        var ventana = NuevaVentana();
        Seleccionar(ventana, "TCP");

        var detalle = ventana.FindControl<SelectableTextBlock>("DetailText")!;

        Assert.Equal(Dorada("ficha-tcp.txt"), Normalizar(detalle.Text ?? ""));
    }

    [AvaloniaFact]
    public void FichaDnsCoincideConLaDorada()
    {
        var ventana = NuevaVentana();
        Seleccionar(ventana, "DNS");

        var detalle = ventana.FindControl<SelectableTextBlock>("DetailText")!;

        Assert.Equal(Dorada("ficha-dns.txt"), Normalizar(detalle.Text ?? ""));
    }

    [AvaloniaFact]
    public void NavegacionAgrupaPorFamiliaConSuRecuento()
    {
        var ventana = NuevaVentana();
        var panel = ventana.FindControl<StackPanel>("NavPanel")!;

        var cabeceras = panel.Children.OfType<Expander>()
            .Select(e => e.Header?.ToString() ?? "")
            .ToArray();

        Assert.Equal(Dorada("nav.txt").Split('\n'), cabeceras);
    }

    [AvaloniaFact]
    public void FiltroDeFamiliaExactaNoArrastraOtrasFamilias()
    {
        var ventana = NuevaVentana();
        var filtro = ventana.FindControl<TextBox>("NavFilter")!;
        filtro.Text = "ROUT";
        filtro.RaiseEvent(new TextChangedEventArgs(TextBox.TextChangedEvent));

        var panel = ventana.FindControl<StackPanel>("NavPanel")!;
        var cabeceras = panel.Children.OfType<Expander>()
            .Select(e => e.Header?.ToString() ?? "")
            .ToArray();

        Assert.Equal(new[] { "ROUT (12)" }, cabeceras);
    }

    [AvaloniaFact]
    public void ComparadorTcpUdpCoincideConLaDorada()
    {
        var ventana = NuevaVentana();
        Seleccionar(ventana, "TCP");
        ventana.FindControl<ComboBox>("CompareTarget")!.SelectedItem = Protocolo(ventana, "UDP");
        ventana.FindControl<Button>("CompareButton")!
            .RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

        var detalle = ventana.FindControl<SelectableTextBlock>("DetailText")!;

        Assert.Equal(Dorada("comparador-tcp-udp.txt"), Normalizar(detalle.Text ?? ""));
    }

    [AvaloniaFact]
    public void MuestraDePruebaListaTodosLosPaquetes()
    {
        var ventana = NuevaVentana();
        ventana.FindControl<Button>("MuestraButton")!
            .RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

        var resumen = ventana.FindControl<SelectableTextBlock>("ResumenCaptura")!;
        var lista = ventana.FindControl<ListBox>("ListaPaquetes")!;

        Assert.Contains("linktype 1 (Ethernet)", resumen.Text);
        Assert.Contains("25 paquetes", resumen.Text);
        Assert.Equal(25, lista.ItemCount);
        Assert.Equal("#1 · 10.0.0.2:49152 → 10.0.0.1:80 · TCP · 54 B",
                     (lista.Items[0] as ListBoxItem)?.Content?.ToString());
    }

    [AvaloniaFact]
    public void DetalleDelPrimerPaqueteCoincideConLaDorada()
    {
        var ventana = NuevaVentana();
        ventana.FindControl<Button>("MuestraButton")!
            .RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

        var lista = ventana.FindControl<ListBox>("ListaPaquetes")!;
        lista.SelectedIndex = 0;
        var detalle = ventana.FindControl<SelectableTextBlock>("DetalleCaptura")!;

        Assert.Equal(Dorada("captura-detalle0.txt"), Normalizar(detalle.Text ?? ""));
    }
}
