using Avalonia;
using Avalonia.Headless;
using Redes.Knowledge.App;

[assembly: AvaloniaTestApplication(typeof(Redes.Knowledge.App.Tests.PruebasAppBuilder))]

namespace Redes.Knowledge.App.Tests;

/// <summary>Arranque headless de la app real (sin ventana nativa ni GPU).</summary>
public static class PruebasAppBuilder
{
    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<App>()
            .UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = true });
}
