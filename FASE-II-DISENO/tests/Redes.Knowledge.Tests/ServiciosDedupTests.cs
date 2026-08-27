using Redes.Knowledge.Infrastructure;
using Redes.Knowledge.Infrastructure.Iana;

namespace Redes.Knowledge.Tests;

/// <summary>D2-2: deduplicación fina IANA y entity-linking servicios ↔ catálogo F3.</summary>
public class ServiciosDedupTests
{
    private static IanaServiceEntry S(string nombre, int? puerto, string transporte)
        => new(nombre, puerto, transporte, "", "", "");

    [Fact]
    public void Sinonimos_Se_Agrupan_Bajo_El_Nombre_Canonico()
    {
        var dedup = ServiciosDedup.Agrupar(new[]
        {
            S("http", 80, "tcp"),
            S("www-http", 80, "tcp"),
            S("http-alt", 8080, "tcp"),
            S("HTTP", 80, "tcp") // variante de mayúsculas -> mismo grupo
        });

        Assert.Equal(3, dedup.Servicios.Count); // http, www-http, http-alt (claves normalizadas distintas)
        Assert.Equal(1, dedup.SinonimosAgrupados); // "HTTP" se pliega en "http"

        var http = dedup.Servicios.First(s => s.Nombre == "http");
        Assert.Single(http.Puertos);
        Assert.Contains((80, "TCP"), http.Puertos);

        // La unión de puertos por VÍNCULO (como hace la app): HTTP recoge http + http-alt.
        var puertosHttp = new SortedSet<(int, string)>();
        foreach (var s in dedup.Servicios)
            if (VinculoServicios.AcronimoDe(s.Nombre) == "HTTP")
                foreach (var p in s.Puertos) puertosHttp.Add(p);
        Assert.Contains((80, "TCP"), puertosHttp);
        Assert.Contains((8080, "TCP"), puertosHttp);
    }

    [Fact]
    public void Puertos_Duplicados_Dentro_Del_Grupo_Se_Descartan()
    {
        var dedup = ServiciosDedup.Agrupar(new[]
        {
            S("http", 80, "tcp"),
            S("http", 80, "tcp") // fila duplicada
        });
        Assert.Equal(1, dedup.PuertosDuplicados);
        Assert.Single(dedup.Servicios.Single().Puertos);
    }

    [Fact]
    public void Vinculos_Curados_Servicio_IANA_Protocolo_F3()
    {
        Assert.Equal("HTTP", VinculoServicios.AcronimoDe("www-http"));
        Assert.Equal("HTTP", VinculoServicios.AcronimoDe("http-alt"));
        Assert.Equal("DNS", VinculoServicios.AcronimoDe("domain"));
        Assert.Equal("DHCP", VinculoServicios.AcronimoDe("bootps"));
        Assert.Equal("SMTP", VinculoServicios.AcronimoDe("submission"));
        Assert.Null(VinculoServicios.AcronimoDe("nombre-inexistente"));
    }

    [Fact]
    public void Datos_IANA_Reales_Dedup_Y_Enlace()
    {
        var data = Path.Combine(RaizDelRepositorio(), "FASE-II-DISENO", "data");
        var csv = Directory.GetFiles(data, "iana-service-names-port-numbers-*.csv").FirstOrDefault();
        Assert.NotNull(csv);

        var importado = IanaServiceImporter.Importar(csv!);
        var dedup = ServiciosDedup.Agrupar(importado.Entradas);

        Assert.True(dedup.Servicios.Count > 1000, "El registro IANA debe deduplicarse a miles de servicios.");
        Assert.True(importado.Entradas.Count >= dedup.Servicios.Count);

        // HTTP: el grupo "http" trae 80/tcp; el vínculo curado une http-alt (8080).
        var http = dedup.Servicios.First(s => s.Nombre == "http");
        Assert.Contains((80, "TCP"), http.Puertos);
        Assert.Equal("HTTP", VinculoServicios.AcronimoDe("www"));
        Assert.Equal("HTTP", VinculoServicios.AcronimoDe("http-alt"));

        // DNS: el servicio "domain" (53) se enlaza a DNS.
        var domain = dedup.Servicios.First(s => s.Nombre == "domain");
        Assert.Contains((53, "TCP"), domain.Puertos);
        Assert.Equal("DNS", VinculoServicios.AcronimoDe("domain"));
    }

    private static string RaizDelRepositorio()
    {
        var d = new DirectoryInfo(AppContext.BaseDirectory);
        while (d is not null && !File.Exists(Path.Combine(d.FullName, "PLANREDES.md")))
            d = d.Parent;
        return d?.FullName ?? throw new DirectoryNotFoundException("Raíz del repositorio no encontrada.");
    }
}