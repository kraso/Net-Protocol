using Redes.Knowledge.App;

namespace Redes.Knowledge.App.Tests;

/// <summary>
/// P2: arranque del catálogo extraído de la ventana. Comprueba que resuelve la raíz de datos,
/// que carga F3–F6 con sus índices y que repetirlo no duplica nada (bootstrap idempotente).
/// </summary>
public class ArranqueCatalogoTests
{
    [Fact]
    public void LaRaizDeDatosContieneElInventarioF3()
    {
        var raiz = ArranqueCatalogo.RaizDatos();

        Assert.True(File.Exists(Path.Combine(raiz, "FASE-03-INVENTARIO", "F3-Protocolos.json")),
            $"no se encontró F3 bajo {raiz}");
    }

    [Fact]
    public void CargaElCatalogoConTcpRelacionesYFamilias()
    {
        var arranque = ArranqueCatalogo.Cargar();

        Assert.True(arranque.Catalogo.Protocolos.Count >= 100,
            $"solo {arranque.Catalogo.Protocolos.Count} protocolos");
        Assert.Contains("TCP", arranque.Catalogo.Protocolos.Values.Select(p => p.Acronimo));
        Assert.NotEmpty(arranque.Catalogo.Relaciones);
        Assert.True(arranque.Catalogo.Familias.Count > 0);
        // S3: el arranque importa el registro IANA del CSV versionado si la tabla está vacía.
        Assert.True(arranque.Servicios.Contar() > 0);
    }

    [Fact]
    public void CargarEsIdempotenteYNoReimportaProtocolos()
    {
        var primero = ArranqueCatalogo.Cargar();
        var segundo = ArranqueCatalogo.Cargar();

        Assert.Equal(primero.Catalogo.Protocolos.Count, segundo.Catalogo.Protocolos.Count);
        Assert.Equal(primero.Servicios.Contar(), segundo.Servicios.Contar());
        Assert.Equal(0, segundo.Importados); // la segunda pasada no inserta nada
    }
}
