using System.Text.Json;
using Dapper;
using UESAN.SHOPPING.AI.Datos;

namespace UESAN.SHOPPING.AI.H3_Embeddings;

public sealed class FragmentoIndexado
{
    public int Id { get; set; }
    public string Fuente { get; set; } = "";
    public string Seccion { get; set; } = "";
    public string Contenido { get; set; } = "";
    public string Embedding { get; set; } = "[]";
    public int Dimensiones { get; set; }

    private float[]? _vector;
    public float[] Vector => _vector ??= JsonSerializer.Deserialize<float[]>(Embedding) ?? [];
}

/// <summary>H3 · Guarda y lee fragmentos con su vector en la tabla ai.Fragmento.</summary>
public sealed class RepositorioConocimiento(ConexionSql conexion)
{
    public async Task ReemplazarFuenteAsync(string fuente, string modelo, IEnumerable<(FragmentoTexto Fragmento, float[] Vector)> items)
    {
        await using var cn = conexion.Crear();
        await cn.OpenAsync();
        await using var tx = await cn.BeginTransactionAsync();

        await cn.ExecuteAsync("DELETE FROM ai.Fragmento WHERE Fuente = @fuente AND Modelo = @modelo",
            new { fuente, modelo }, tx);

        foreach (var (fragmento, vector) in items)
        {
            await cn.ExecuteAsync("""
                INSERT ai.Fragmento (Fuente, Seccion, Contenido, Embedding, Dimensiones, Modelo)
                VALUES (@Fuente, @Seccion, @Contenido, @Embedding, @Dimensiones, @Modelo)
                """,
                new
                {
                    fragmento.Fuente,
                    fragmento.Seccion,
                    fragmento.Contenido,
                    Embedding = JsonSerializer.Serialize(vector),
                    Dimensiones = vector.Length,
                    Modelo = modelo
                }, tx);
        }

        await tx.CommitAsync();
    }

    public async Task<IReadOnlyList<FragmentoIndexado>> ListarAsync(string modelo)
    {
        await using var cn = conexion.Crear();
        return (await cn.QueryAsync<FragmentoIndexado>(
            "SELECT Id, Fuente, Seccion, Contenido, Embedding, Dimensiones FROM ai.Fragmento WHERE Modelo = @modelo",
            new { modelo })).AsList();
    }

    public async Task<IReadOnlyList<object>> ResumenAsync()
    {
        await using var cn = conexion.Crear();
        return (await cn.QueryAsync("""
            SELECT Modelo, Fuente, COUNT(*) AS Fragmentos, MAX(Dimensiones) AS Dimensiones
            FROM ai.Fragmento GROUP BY Modelo, Fuente ORDER BY Modelo, Fuente
            """)).Cast<object>().ToList();
    }
}
