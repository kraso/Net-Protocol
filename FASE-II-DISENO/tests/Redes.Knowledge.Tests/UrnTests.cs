using Redes.Knowledge.Domain;

namespace Redes.Knowledge.Tests;

public class UrnTests
{
    [Fact]
    public void UrnProtocola_Normaliza_Acronimo()
    {
        var urn = Urn.Protocol("TRAN", "TCP");
        Assert.Equal("urn:proto:tran:tcp", urn.Value);
    }

    [Fact]
    public void Urn_Rechaza_Vacia()
    {
        Assert.Throws<ArgumentException>(() => new Urn("   "));
    }

    [Fact]
    public void Urn_Parse_RoundTrip()
    {
        var original = "urn:proto:ietf:rfc9114";
        Assert.Equal(original, Urn.Parse(original).Value);
    }
}