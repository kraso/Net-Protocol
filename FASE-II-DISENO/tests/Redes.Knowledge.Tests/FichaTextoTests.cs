using Redes.Knowledge.Domain;
using Redes.Knowledge.Visualization;

namespace Redes.Knowledge.Tests;

/// <summary>P2/I1: formateador de la ficha de 18 campos extraído a Visualization.
/// Cubre los caminos que la ventana ya no ejecuta (ficha ausente, vecinos, campos F5,
/// nota, cifrado, puertos y omisión de líneas sin dato).</summary>
public class FichaTextoTests
{
    private static Protocol Tcp() => new()
    {
        Nombre = "Transmission Control Protocol",
        Acronimo = "TCP",
        Familia = FamiliaProtocolo.TRAN,
        Estado = LifecycleState.Vigente,
        Capas = "4 · transporte"
    };

    private static FichaPrioritaria FichaCompleta() => new()
    {
        Acronimo = "TCP",
        Campos = new Dictionary<string, string>
        {
            ["2"] = "estandar (RFC 9293)",
            ["3"] = "transporte fiable orientado a conexión",
            ["4"] = "IP",
            ["6"] = "TCP",
            ["7"] = "segmento",
            ["8"] = "SYN, ACK, FIN, RST",
            ["9"] = "puerto origen, puerto destino, secuencia",
            ["10"] = "three-way handshake",
            ["11"] = "puertos de 16 bits",
            ["12"] = "no enruta",
            ["13"] = "TLS por encima",
            ["14"] = "control de flujo por ventana",
            ["15"] = "contadores y retransmisiones",
            ["16"] = "sockets BSD",
            ["17"] = "Linux, Windows, BSD",
            ["18"] = "RFC 9293"
        }
    };

    [Fact]
    public void FichaCompletaNumeraYalineaTodasLasLineas()
    {
        var texto = FichaTexto.Render(new FichaTextoModelo
        {
            Protocolo = Tcp(),
            Ficha = FichaCompleta(),
            Pdu = "segmento",
            Cifrado = "TLS",
            Puertos = "6, 80, 443",
            Nota = "nota de prueba",
            FuenteF3 = "RFC 9293"
        });

        var lineas = texto.Replace("\r\n", "\n").TrimEnd('\n').Split('\n');

        Assert.Equal("=== Transmission Control Protocol (TCP) ===", lineas[0]);
        Assert.Equal(" 1. Identidad             : Transmission Control Protocol · TCP · familia TRAN [F3]", lineas[1]);
        Assert.Equal(" 2. Estado                : estandar (RFC 9293)", lineas[2]);
        Assert.Equal(" 6. Transp./dir           : TCP", lineas[6]);
        Assert.Equal("13. Seguridad             : cifrado: TLS [F6]", lineas[13]);
        Assert.Equal("18. Fuentes               : RFC 9293", lineas[18]);
        Assert.Equal("      Nota/F3 .....: nota de prueba", lineas[19]);
        Assert.Equal(20, lineas.Length);
    }

    [Fact]
    public void SinFichaUsaCatalogosYListaVecinos()
    {
        var texto = FichaTexto.Render(new FichaTextoModelo
        {
            Protocolo = Tcp(),
            Vecinos = new[] { "IP · Internet Protocol (encapsula)", "UDP · User Datagram Protocol (corre sobre)" },
            Puertos = "6, 80",
            FuenteF3 = "RFC 9293"
        });

        Assert.Contains(" 2. Estado                : Vigente (catálogo F3, 2026-08-26)", texto);
        Assert.Contains(" 4. Encapsulación         : IP · Internet Protocol (encapsula), UDP · User Datagram Protocol (corre sobre)", texto);
        Assert.Contains(" 6. Transp./dir           : 6, 80 [IANA]", texto);
        Assert.Contains("--- Vecinos a 1 salto (grafo F4) ---", texto);
        Assert.Contains("  • UDP · User Datagram Protocol (corre sobre)", texto);
        Assert.Contains("18. Fuentes               : RFC 9293", texto);
        Assert.DoesNotContain("--- Campos catalogados (F5) ---", texto);
    }

    [Fact]
    public void PuertosVaciosNoImprimenLinea()
    {
        var texto = FichaTexto.Render(new FichaTextoModelo
        {
            Protocolo = Tcp(),
            Puertos = "—"
        });

        Assert.DoesNotContain("Transp./dir", texto);
        Assert.DoesNotContain("PDU", texto);
        Assert.DoesNotContain("Nota/F3", texto);
        Assert.DoesNotContain("Vecinos a 1 salto", texto);
    }

    [Fact]
    public void CamposF5ImprimenOffsetLongitudTipoYOpcionalidad()
    {
        var texto = FichaTexto.Render(new FichaTextoModelo
        {
            Protocolo = Tcp(),
            Campos = new[]
            {
                new Field { Nombre = "Puerto origen", OffsetBits = 0, LongitudBits = 16, Tipo = "uint16", Obligatorio = true },
                new Field { Nombre = "Opciones", Tipo = "variable" }
            }
        });

        Assert.Contains("--- Campos catalogados (F5) ---", texto);
        Assert.Contains("  Puerto origen: offset 0 · 16 bits · uint16", texto);
        Assert.Contains("  Opciones: offset var · var bits · variable (opt.)", texto);
    }

    [Fact]
    public void FichaPresenteVaciaLaListaDeVecinos()
    {
        var texto = FichaTexto.Render(new FichaTextoModelo
        {
            Protocolo = Tcp(),
            Ficha = FichaCompleta(),
            Vecinos = new[] { "IP · Internet Protocol (encapsula)" }
        });

        Assert.DoesNotContain("Vecinos a 1 salto", texto);
        Assert.Contains(" 4. Encapsulación         : IP", texto);
    }

    [Fact]
    public void EnInglesTraduceEtiquetasSinPerderElFormato()
    {
        var previo = Textos.Idioma;
        try
        {
            Textos.Fijar(Textos.En);

            var texto = FichaTexto.Render(new FichaTextoModelo
            {
                Protocolo = new Protocol
                {
                    Nombre = "Transmission Control Protocol",
                    Acronimo = "TCP",
                    Familia = FamiliaProtocolo.TRAN,
                    Estado = LifecycleState.Obsoleto,
                    Capas = "4"
                },
                Pdu = "segment"
            });

            Assert.Contains(" 1. Identity              : Transmission Control Protocol · TCP · family TRAN [F3]", texto);
            Assert.Contains(" 2. Status                : Obsolete (F3 catalog, 2026-08-26)", texto);
            Assert.Contains(" 5. Layers                : 4", texto);
            Assert.Contains(" 7. PDU                   : segment", texto);
            Assert.DoesNotContain("Identidad", texto);
            Assert.DoesNotContain("Estado ", texto);
        }
        finally
        {
            Textos.Fijar(previo);
        }
    }
}
