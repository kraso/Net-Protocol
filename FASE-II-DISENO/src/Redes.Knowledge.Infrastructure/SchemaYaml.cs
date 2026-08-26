using Redes.Knowledge.Domain;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace Redes.Knowledge.Infrastructure;

/// <summary>
/// Soporte YAML de los catálogos/fixtures (D1-3): conversor de URN a escalar.
/// YamlDotNet no deserializa el record struct Urn por defecto (valor por constructor).
/// </summary>
public sealed class UrnYamlConverter : IYamlTypeConverter
{
    public bool Accepts(Type type) => type == typeof(Urn);

    public object? ReadYaml(IParser parser, Type type, ObjectDeserializer rootDeserializer)
    {
        var scalar = parser.Consume<Scalar>();
        return Urn.Parse(scalar.Value);
    }

    public void WriteYaml(IEmitter emitter, object? value, Type type, ObjectSerializer valueSerializer)
    {
        emitter.Emit(new Scalar(((Urn)value!).Value));
    }
}

public static class SchemaYaml
{
    public static string Serialize<T>(T value)
        => new SerializerBuilder()
            .WithTypeConverter(new UrnYamlConverter())
            .Build()
            .Serialize(value);

    public static T Deserialize<T>(string yaml)
        => new DeserializerBuilder()
            .WithTypeConverter(new UrnYamlConverter())
            .Build()
            .Deserialize<T>(yaml);
}