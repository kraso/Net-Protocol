using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Interactivity;
using Redes.Knowledge.App;
using Redes.Knowledge.Visualization;

namespace Redes.Knowledge.App.Tests;

/// <summary>
/// Accesibilidad de la ventana (I11): nombre que cada control expone a un lector de
/// pantalla, ayuda larga de los botones de acción y atajos de teclado del zoom. Todo se
/// comprueba sobre la ventana real construida en headless, sin abrirla: abrirla lanzaría
/// la comprobación de actualizaciones contra la red.
/// </summary>
public class AccesibilidadTests
{
    /// <summary>
    /// Fija el idioma por la variable de entorno que tiene prioridad, para que la ventana
    /// no dependa del LANG del equipo donde corre la prueba.
    /// </summary>
    private sealed class IdiomaFijo : IDisposable
    {
        private readonly string? _previo;
        private readonly string _idiomaPrevio;

        public IdiomaFijo(string idioma)
        {
            _previo = Environment.GetEnvironmentVariable("NETPROTOCOL_IDIOMA");
            _idiomaPrevio = Textos.Idioma;
            Environment.SetEnvironmentVariable("NETPROTOCOL_IDIOMA", idioma);
            Textos.Fijar(idioma);
        }

        public void Dispose()
        {
            Environment.SetEnvironmentVariable("NETPROTOCOL_IDIOMA", _previo);
            Textos.Fijar(_idiomaPrevio);
        }
    }

    private static readonly string[] ControlesConNombre =
    {
        "SearchBox", "SearchButton", "ProtocolSelector", "CompareTarget", "CompareButton",
        "VolverFichaButton", "FilterFamilia", "FilterEstado", "ExportFormat", "ExportButton",
        "AbrirCapturaButton", "MuestraButton", "LanguageButton", "NavFilter", "ThemeButton",
        "LegendButton", "AboutButton", "MinimizeButton", "MaximizeButton", "CloseButton",
        "CerrarCapturaButton", "ListaPaquetes", "DetailText", "StatusText", "ResumenCaptura",
        "DetalleCaptura", "DiagramTitle"
    };

    private static void PulsarTecla(MainWindow ventana, Key tecla, KeyModifiers modificadores)
        => ventana.RaiseEvent(new KeyEventArgs
        {
            RoutedEvent = InputElement.KeyDownEvent,
            Key = tecla,
            KeyModifiers = modificadores
        });

    [AvaloniaFact]
    public void TodoControlInteractivoExponeNombreAccesible()
    {
        using var _ = new IdiomaFijo(Textos.Es);
        var ventana = new MainWindow();

        foreach (var nombre in ControlesConNombre)
        {
            var control = ventana.FindControl<Control>(nombre);
            Assert.True(control is not null, $"no se encontró el control {nombre}");
            var accesible = AutomationProperties.GetName(control!);
            Assert.False(string.IsNullOrWhiteSpace(accesible), $"{nombre} no tiene nombre accesible");
        }
    }

    [AvaloniaFact]
    public void LosBotonesDeVentanaTienenNombreYTooltipLocalizados()
    {
        using var _ = new IdiomaFijo(Textos.Es);
        var ventana = new MainWindow();

        Assert.Equal("Minimizar", AutomationProperties.GetName(ventana.FindControl<Button>("MinimizeButton")!));
        Assert.Equal("Maximizar", AutomationProperties.GetName(ventana.FindControl<Button>("MaximizeButton")!));
        Assert.Equal("Cerrar", AutomationProperties.GetName(ventana.FindControl<Button>("CloseButton")!));
        Assert.Equal("Minimizar", ToolTip.GetTip(ventana.FindControl<Button>("MinimizeButton")!));

        ventana.FindControl<Button>("LanguageButton")!.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

        Assert.Equal("Minimize", AutomationProperties.GetName(ventana.FindControl<Button>("MinimizeButton")!));
        Assert.Equal("Maximize", AutomationProperties.GetName(ventana.FindControl<Button>("MaximizeButton")!));
        Assert.Equal("Close", AutomationProperties.GetName(ventana.FindControl<Button>("CloseButton")!));
        Assert.Equal("Minimize", ToolTip.GetTip(ventana.FindControl<Button>("MinimizeButton")!));
    }

    [AvaloniaFact]
    public void LosBotonesDeAccionExponenAyudaLarga()
    {
        using var _ = new IdiomaFijo(Textos.Es);
        var ventana = new MainWindow();

        foreach (var nombre in new[] { "CompareButton", "ExportButton", "AbrirCapturaButton", "MuestraButton" })
        {
            var ayuda = AutomationProperties.GetHelpText(ventana.FindControl<Button>(nombre)!);
            Assert.False(string.IsNullOrWhiteSpace(ayuda), $"{nombre} no expone ayuda");
            Assert.True(ayuda!.Length > 30, $"{nombre} expone una ayuda demasiado corta: {ayuda}");
        }
    }

    [AvaloniaFact]
    public void CtrlMasAumentaElZoomYCtrlCeroLoDevuelveAlCien()
    {
        using var _ = new IdiomaFijo(Textos.Es);
        var ventana = new MainWindow();
        var estado = ventana.FindControl<SelectableTextBlock>("StatusText")!;

        PulsarTecla(ventana, Key.OemPlus, KeyModifiers.Control);
        Assert.Contains("110%", estado.Text);

        PulsarTecla(ventana, Key.OemPlus, KeyModifiers.Control);
        Assert.Contains("120%", estado.Text);

        PulsarTecla(ventana, Key.D0, KeyModifiers.Control);
        Assert.Contains("100%", estado.Text);
    }

    [AvaloniaFact]
    public void CtrlMenosReduceElZoom()
    {
        using var _ = new IdiomaFijo(Textos.Es);
        var ventana = new MainWindow();
        var estado = ventana.FindControl<SelectableTextBlock>("StatusText")!;

        PulsarTecla(ventana, Key.OemMinus, KeyModifiers.Control);

        Assert.Contains("90%", estado.Text);
    }

    [AvaloniaFact]
    public void SinControlElAtajoNoSeConsumeNiCambiaElZoom()
    {
        using var _ = new IdiomaFijo(Textos.Es);
        var ventana = new MainWindow();
        var estado = ventana.FindControl<SelectableTextBlock>("StatusText")!;
        var antes = estado.Text;

        var evento = new KeyEventArgs
        {
            RoutedEvent = InputElement.KeyDownEvent,
            Key = Key.OemPlus,
            KeyModifiers = KeyModifiers.None
        };
        ventana.RaiseEvent(evento);

        Assert.False(evento.Handled);
        Assert.Equal(antes, estado.Text);
    }
}
