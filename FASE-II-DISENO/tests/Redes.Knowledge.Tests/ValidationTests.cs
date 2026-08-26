using Redes.Knowledge.Domain;

namespace Redes.Knowledge.Tests;

public class ValidationTests
{
    [Fact]
    public void Protocolo_Valido_Pasa()
    {
        var p = new Protocol
        {
            Id = Urn.Protocol("TRAN", "TCP"),
            Nombre = "Transmission Control Protocol",
            Acronimo = "TCP",
            Familia = FamiliaProtocolo.TRAN,
            Estado = LifecycleState.Vigente
        };
        var r = ProtocolValidator.Validate(p);
        Assert.True(r.IsValid, string.Join("; ", r.Errors));
    }

    [Fact]
    public void Protocolo_SinNombre_NoPasa()
    {
        var p = new Protocol { Id = Urn.Protocol("APP", "X"), Nombre = " ", Acronimo = "X", Familia = FamiliaProtocolo.APP, Estado = LifecycleState.Vigente };
        Assert.False(ProtocolValidator.Validate(p).IsValid);
    }

    [Fact]
    public void Protocolo_Vigencia_Invertida_NoPasa()
    {
        var p = new Protocol
        {
            Id = Urn.Protocol("HIST", "X25"),
            Nombre = "X.25",
            Acronimo = "X.25",
            Familia = FamiliaProtocolo.HIST,
            Estado = LifecycleState.Historico,
            ValidFrom = new DateTime(2026, 8, 26),
            ValidTo = new DateTime(2000, 1, 1)
        };
        Assert.False(ProtocolValidator.Validate(p).IsValid);
        Assert.Contains(ProtocolValidator.Validate(p).Errors, e => e.Contains("valid_from"));
    }

    [Fact]
    public void Fuente_SinFechaConsulta_NoPasa()
    {
        var s = new Source
        {
            Id = Urn.Parse("urn:source:test:r1"),
            Titulo = "IANA Registry",
            Url = "https://www.iana.org/",
            Version = "2026-08",
            Organismo = "IANA",
            Nivel = NivelAutoridad.PrimariaNormativa,
            Confianza = Confianza.Alto
        };
        Assert.False(SourceValidator.Validate(s).IsValid);
    }

    [Fact]
    public void Vigencia_Temporal_SeEvalua()
    {
        var p = new Protocol
        {
            Id = Urn.Protocol("APP", "HTTP3"),
            Nombre = "HTTP/3",
            Acronimo = "HTTP/3",
            Familia = FamiliaProtocolo.APP,
            Estado = LifecycleState.Vigente,
            ValidFrom = new DateTime(2022, 6, 6)
        };
        Assert.True(p.EsValidoEn(new DateTime(2026, 8, 26)));
        Assert.False(p.EsValidoEn(new DateTime(2021, 1, 1)));
    }
}