using Redes.Knowledge.Domain;

namespace Redes.Knowledge.Infrastructure;

public sealed record Vecino(string Nombre, RelacionTipo Tipo);

/// <summary>Grafo de relaciones (D5-1): vecinos a 1 salto desde la matriz de encapsulación de F4.</summary>
public static class GrafoRelaciones
{
    /// <summary>Nombre de la entidad contenido en una URN del grafo (último segmento).</summary>
    public static string EntidadDe(string urn) => urn.Split(':').Last();

    /// <summary>Vecinos a 1 salto de una entidad (coincidencia normalizada con F4).</summary>
    public static IReadOnlyList<Vecino> Vecinos1Salto(string entidad, IReadOnlyList<Relationship> relaciones)
    {
        var yo = Normalizar(entidad);
        var vistos = new HashSet<string>(StringComparer.Ordinal);
        var resultado = new List<Vecino>();

        foreach (var r in relaciones)
        {
            var origen = EntidadDe(r.Origen.Value);
            var destino = EntidadDe(r.Destino.Value);

            if (Normalizar(origen) == yo && vistos.Add(Normalizar(destino)))
                resultado.Add(new Vecino(destino, r.Tipo));
            else if (Normalizar(destino) == yo && vistos.Add(Normalizar(origen)))
                resultado.Add(new Vecino(origen, r.Tipo));
        }

        return resultado;
    }

    private static string Normalizar(string s)
        => string.Concat(s.Where(char.IsLetterOrDigit)).ToLowerInvariant();
}