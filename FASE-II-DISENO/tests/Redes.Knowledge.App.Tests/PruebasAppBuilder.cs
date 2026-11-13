using Avalonia;
using Avalonia.Headless;
using Redes.Knowledge.App;

[assembly: AvaloniaTestApplication(typeof(Redes.Knowledge.App.Tests.PruebasAppBuilder))]

namespace Redes.Knowledge.App.Tests;

/// <summary>Arranque headless de la app real (sin ventana nativa ni GPU).</summary>
public static class PruebasAppBuilder
{
    /// <summary>
    /// Fija español antes de que se construya ninguna ventana. La app resuelve el idioma en
    /// el constructor (NETPROTOCOL_IDIOMA y, si falta, LANG/LC_ALL), así que sin esta fijación
    /// las doradas y las pruebas de idioma dependerían del entorno del equipo que las ejecuta.
    /// </summary>
    static PruebasAppBuilder()
        => Environment.SetEnvironmentVariable("NETPROTOCOL_IDIOMA", Redes.Knowledge.Visualization.Textos.Es);

    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<App>()
            .UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = true });
}
