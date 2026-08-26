using Avalonia;

namespace SpikeUi;

internal static class Program
{
    // Punto de entrada: construcción de la app Avalonia sin XAML (spike de validación D0-2).
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);

    public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<App>()
        .UsePlatformDetect()
        .LogToTrace();
}