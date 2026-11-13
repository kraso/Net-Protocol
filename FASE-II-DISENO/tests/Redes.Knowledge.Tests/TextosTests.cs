using Redes.Knowledge.Visualization;

namespace Redes.Knowledge.Tests;

/// <summary>
/// Catálogo de textos es/en: cobertura de claves, cambio de idioma, detección por
/// entorno y formato con argumentos. El idioma es estado estático del proceso, así que
/// cada prueba restaura español al terminar.
/// </summary>
public class TextosTests
{
    private static void ConIdioma(string idioma, Action cuerpo)
    {
        var previo = Textos.Idioma;
        try
        {
            Textos.Fijar(idioma);
            cuerpo();
        }
        finally
        {
            Textos.Fijar(previo);
        }
    }

    [Fact]
    public void Catalogo_tiene_claves_y_ambos_idiomas_no_vacios()
    {
        Assert.True(Textos.Claves.Count >= 40, $"claves={Textos.Claves.Count}");
        foreach (var clave in Textos.Claves)
        {
            ConIdioma(Textos.Es, () => Assert.False(string.IsNullOrWhiteSpace(Textos.T(clave)), $"es vacío: {clave}"));
            ConIdioma(Textos.En, () => Assert.False(string.IsNullOrWhiteSpace(Textos.T(clave)), $"en vacío: {clave}"));
        }
    }

    [Fact]
    public void Clave_desconocida_devuelve_la_propia_clave()
        => Assert.Equal("no.existe", Textos.T("no.existe"));

    [Fact]
    public void Fijar_en_devuelve_ingles_y_fijar_desconocido_vuelve_a_espanol()
    {
        ConIdioma(Textos.En, () => Assert.Equal("Search", Textos.T("buscar.boton")));
        ConIdioma("fr", () => Assert.Equal("Buscar", Textos.T("buscar.boton")));
        ConIdioma("EN", () => Assert.Equal("Search", Textos.T("buscar.boton")));
    }

    [Fact]
    public void Alternar_conmuta_entre_es_y_en()
    {
        ConIdioma(Textos.Es, () =>
        {
            Assert.Equal(Textos.En, Textos.Alternar());
            Assert.Equal("Search", Textos.T("buscar.boton"));
            Assert.Equal(Textos.Es, Textos.Alternar());
            Assert.Equal("Buscar", Textos.T("buscar.boton"));
        });
    }

    [Fact]
    public void Detectar_prioriza_la_variable_de_entorno()
    {
        var previo = Environment.GetEnvironmentVariable("NETPROTOCOL_IDIOMA");
        try
        {
            Environment.SetEnvironmentVariable("NETPROTOCOL_IDIOMA", "en");
            Assert.Equal(Textos.En, Textos.Detectar());
            Environment.SetEnvironmentVariable("NETPROTOCOL_IDIOMA", "es");
            Assert.Equal(Textos.Es, Textos.Detectar());
        }
        finally
        {
            Environment.SetEnvironmentVariable("NETPROTOCOL_IDIOMA", previo);
            Textos.Fijar(Textos.Es);
        }
    }

    [Fact]
    public void F_inserta_argumentos_en_ambos_idiomas()
    {
        ConIdioma(Textos.Es, () =>
        {
            var texto = Textos.F("estado.visible", 3, 10, 5, "100%");
            Assert.Contains("3 de 10", texto);
            Assert.Contains("100%", texto);
        });
        ConIdioma(Textos.En, () =>
        {
            var texto = Textos.F("estado.visible", 3, 10, 5, "100%");
            Assert.Contains("3 of 10", texto);
            Assert.Contains("100%", texto);
        });
    }

    [Fact]
    public void F_usa_cultura_invariante_para_los_numeros()
    {
        ConIdioma(Textos.Es, () =>
        {
            var texto = Textos.F("estado.export.ok", 2, "TCP", "PNG", "C:/tmp");
            Assert.Contains("2 diagrama(s)", texto);
            Assert.DoesNotContain("2,0", texto);
        });
    }

    [Fact]
    public void Claves_del_chrome_existen()
    {
        foreach (var clave in new[]
                 {
                     "app.titulo", "buscar.placeholder", "buscar.boton", "etiqueta.protocolo",
                     "etiqueta.comparar", "boton.comparar", "boton.volver.ficha", "etiqueta.familia",
                     "etiqueta.estado", "etiqueta.exportar", "boton.exportar", "boton.abrir.captura",
                     "boton.muestra", "boton.idioma", "nav.filtro", "sidebar.acciones", "sidebar.tema",
                     "sidebar.leyenda", "sidebar.acerca", "diagramas.titulo", "boton.cerrar.captura"
                 })
        {
            Assert.NotEqual(clave, Textos.T(clave));
        }
    }
}
