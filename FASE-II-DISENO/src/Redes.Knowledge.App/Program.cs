using Avalonia;
using Redes.Knowledge.Infrastructure;
using Redes.Knowledge.Infrastructure.Capturas;

namespace Redes.Knowledge.App;

internal static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        // Modo auditoría L-004 (sin interfaz): cruza un corpus PCAP/PCAPNG real con los
        // layouts F5 y escribe el informe. Uso: NetProtocol.exe --l004 <carpeta> [rutaSalida]
        if (args.Length >= 2 && args[0] == "--l004")
        {
            try
            {
                var f5 = LocalizarF5() ?? throw new FileNotFoundException("F5-Campos-PDU.json no localizado.");
                var salida = args.Length >= 3 ? args[2] : Path.Combine(args[1], "L004-informe.md");
                var resultado = CorpusL004.Validar(args[1], f5, salida);
                // WinExe: la consola no se ve; se escribe un resumen junto al informe.
                File.WriteAllText(salida + ".resumen.txt",
                    $"L-004: {resultado.Ficheros.Count} captura(s) · {resultado.PorProtocolo.Count} protocolos F5 con paquetes · " +
                    $"{resultado.SinLayoutEnCorpus.Count} sin paquetes en el corpus · informe: {Path.GetFullPath(salida)}");
                return;
            }
            catch (Exception ex)
            {
                var salida = args.Length >= 3 ? args[2] : Path.Combine(args[1], "L004-informe.md");
                File.WriteAllText(salida + ".error.txt", ex.ToString());
                return;
            }
        }

        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<App>()
        .UsePlatformDetect()
        .LogToTrace();

    /// <summary>Localiza F5-Campos-PDU.json junto al ejecutable (modo instalado) o en el repo.</summary>
    private static string? LocalizarF5()
    {
        var bundled = Path.Combine(AppContext.BaseDirectory, "datos", "FASE-05-MENSAJERIA", "F5-Campos-PDU.json");
        if (File.Exists(bundled)) return bundled;
        var d = new DirectoryInfo(AppContext.BaseDirectory);
        while (d is not null)
        {
            var candidato = Path.Combine(d.FullName, "FASE-05-MENSAJERIA", "F5-Campos-PDU.json");
            if (File.Exists(candidato)) return candidato;
            d = d.Parent;
        }
        return null;
    }
}