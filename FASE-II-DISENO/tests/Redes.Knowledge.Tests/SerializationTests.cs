using Redes.Knowledge.Domain;
using Redes.Knowledge.Infrastructure;

namespace Redes.Knowledge.Tests;

/// <summary>D1-3: importación de los catálogos canónicos de la Fase I sin duplicar datos + round-trip.</summary>
public class SerializationTests
{
    private static string RaizDelRepositorio()
    {
        var d = new DirectoryInfo(AppContext.BaseDirectory);
        while (d is not null && !File.Exists(Path.Combine(d.FullName, "PLANREDES.md")))
            d = d.Parent;
        return d?.FullName ?? throw new InvalidOperationException("No se encontró la raíz del repositorio.");
    }

    [Fact]
    public void Importar_F3_Protocolos_113()
    {
        var ruta = Path.Combine(RaizDelRepositorio(), "FASE-03-INVENTARIO", "F3-Protocolos.json");
        var protocolos = CatalogJson.CargarProtocolosF3(ruta);
        Assert.Equal(113, protocolos.Count);
        Assert.Contains(protocolos, p => p.Acronimo == "TCP" && p.Familia == FamiliaProtocolo.TRAN);
        Assert.Contains(protocolos, p => p.Acronimo == "X.25" && p.Estado == LifecycleState.Historico);
    }

    [Fact]
    public void Importar_F5_Campos_TCP_11()
    {
        var ruta = Path.Combine(RaizDelRepositorio(), "FASE-05-MENSAJERIA", "F5-Campos-PDU.json");
        var campos = CatalogJson.CargarCamposF5(ruta, "TCP");
        Assert.Equal(11, campos.Count);
        var destino = Assert.Single(campos, f => f.Nombre == "Destination Port");
        Assert.Equal(16, destino.OffsetBits);
        Assert.Equal(16, destino.LongitudBits);
    }

    [Fact]
    public void RoundTrip_Json_Canonico()
    {
        var p = new Protocol
        {
            Id = Urn.Protocol("TRAN", "TCP"),
            Nombre = "Transmission Control Protocol",
            Acronimo = "TCP",
            Familia = FamiliaProtocolo.TRAN,
            Estado = LifecycleState.Vigente
        };
        var json1 = CatalogJson.RoundTripJson(p);
        var json2 = CatalogJson.RoundTripJson(p);
        Assert.Equal(json1, json2);
    }

    [Fact]
    public void RoundTrip_Yaml_Conserva_Valores()
    {
        var p = new Protocol
        {
            Id = Urn.Protocol("APP", "HTTP3"),
            Nombre = "HTTP/3",
            Acronimo = "HTTP/3",
            Familia = FamiliaProtocolo.APP,
            Estado = LifecycleState.Vigente
        };

        var yaml = SchemaYaml.Serialize(p);
        Assert.False(string.IsNullOrWhiteSpace(yaml));

        var regresado = SchemaYaml.Deserialize<Protocol>(yaml);
        Assert.Equal(p.Nombre, regresado.Nombre);
        Assert.Equal(p.Acronimo, regresado.Acronimo);
        Assert.Equal(p.Estado, regresado.Estado);
        Assert.Equal(p.Id.Value, regresado.Id.Value);
    }
}