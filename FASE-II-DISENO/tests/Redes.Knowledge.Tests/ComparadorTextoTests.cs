using Redes.Knowledge.Visualization;

namespace Redes.Knowledge.Tests;

/// <summary>
/// P2: formateador de la tabla del comparador extraído a Visualization. Cubre el ancho de
/// línea, la marca de igualdad, el truncado con «…» y el efecto del idioma. La salida
/// completa en español está congelada por las pruebas de caracterización de la app.
/// </summary>
public class ComparadorTextoTests
{
    private static readonly ComparadorTexto.Fila[] Cortas =
    {
        new("Familia", "TRAN", "TRAN"),
        new("Estado", "Vigente", "Obsoleto"),
        new("Capas", "4", "—")
    };

    private static readonly ComparadorTexto.Fila[] Largas =
    {
        new("Finalidad", "transporte fiable", "no orientado a conexión")
    };

    [Fact]
    public void CabeceraYFilasMantienenElAnchoFijo()
    {
        var texto = ComparadorTexto.Construir("TCP", "UDP", Cortas, Largas);
        var lineas = texto.Replace("\r\n", "\n").TrimEnd('\n').Split('\n');

        Assert.Equal("=== Comparador: TCP vs UDP ===", lineas[0]);
        Assert.Equal("", lineas[1]);
        Assert.StartsWith("Aspecto", lineas[2]);
        Assert.Equal(17 + 3 + ComparadorTexto.AnchoCelda + 3 + ComparadorTexto.AnchoCelda, lineas[3].Length);
        Assert.Equal("TRAN".PadRight(ComparadorTexto.AnchoCelda), lineas[4].Substring(18, ComparadorTexto.AnchoCelda));
    }

    [Fact]
    public void MarcaIgualdadSoloCuandoAmbosValoresCoincidenYNoSonVacios()
    {
        var texto = ComparadorTexto.Construir("TCP", "UDP", Cortas, Largas);
        var lineas = texto.Replace("\r\n", "\n").Split('\n');

        Assert.EndsWith("(= igual)", lineas[4]);   // Familia: TRAN vs TRAN
        Assert.False(lineas[5].EndsWith("(= igual)")); // Estado: Vigente vs Obsoleto
        Assert.False(lineas[6].EndsWith("(= igual)")); // Capas: 4 vs —
    }

    [Fact]
    public void LosCamposLargosSalenPorProtocoloYNoEnLaTabla()
    {
        var texto = ComparadorTexto.Construir("TCP", "UDP", Cortas, Largas);

        Assert.Contains("Finalidad — TCP: transporte fiable", texto);
        Assert.Contains("Finalidad — UDP: no orientado a conexión", texto);
        Assert.Contains("Fuentes: F3", texto);
    }

    [Fact]
    public void CeldaTruncaConPuntosSuspensivos()
    {
        var larga = new string('x', ComparadorTexto.AnchoCelda + 10);

        var celda = ComparadorTexto.Celda(larga);

        Assert.Equal(ComparadorTexto.AnchoCelda, celda.Length);
        Assert.EndsWith("…", celda);
    }

    [Fact]
    public void AcortarDejaPasarLoCortoYRecortaLoLargo()
    {
        Assert.Null(ComparadorTexto.Acortar("  "));
        Assert.Null(ComparadorTexto.Acortar("—"));
        Assert.Equal("corto", ComparadorTexto.Acortar("corto"));

        var larga = ComparadorTexto.Acortar(new string('y', 300));
        Assert.NotNull(larga);
        Assert.Equal(178, larga!.Length);
        Assert.EndsWith("…", larga);
    }

    [Fact]
    public void EnInglesTraduceLaCabeceraYLasFuentes()
    {
        var previo = Textos.Idioma;
        try
        {
            Textos.Fijar(Textos.En);

            var texto = ComparadorTexto.Construir("TCP", "UDP", Cortas, Largas);

            Assert.Contains("=== Comparison: TCP vs UDP ===", texto);
            Assert.Contains("Sources: F3", texto);
            Assert.DoesNotContain("Comparador:", texto);
        }
        finally
        {
            Textos.Fijar(previo);
        }
    }
}
