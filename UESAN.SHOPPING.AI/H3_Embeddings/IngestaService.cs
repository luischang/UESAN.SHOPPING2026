using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using UESAN.SHOPPING.AI.Configuracion;
using UESAN.SHOPPING.AI.H1_Chat;

namespace UESAN.SHOPPING.AI.H3_Embeddings;

public sealed record ResultadoIngesta(string Modelo, int Documentos, int Fragmentos, IReadOnlyList<string> Archivos);

/// <summary>
/// H3 · Indexación: documentos → fragmentos → embeddings → almacén vectorial.
/// Se ejecuta ANTES de las consultas y cada vez que cambian los documentos.
/// </summary>
public sealed class IngestaService(
    IEmbeddingGenerator<string, Embedding<float>> generador,
    RepositorioConocimiento repositorio,
    IOptions<AiOptions> opciones)
{
    public async Task<ResultadoIngesta> IndexarCarpetaAsync(CancellationToken ct)
    {
        var o = opciones.Value;
        var modelo = ProveedoresIa.ModeloEmbeddingsActivo(o);
        var carpeta = Path.IsPathRooted(o.Conocimiento.Carpeta)
            ? o.Conocimiento.Carpeta
            : Path.Combine(AppContext.BaseDirectory, o.Conocimiento.Carpeta);

        if (!Directory.Exists(carpeta))
            throw new DirectoryNotFoundException($"No existe la carpeta de conocimiento: {carpeta}");

        var archivos = Directory.GetFiles(carpeta, "*.md").OrderBy(a => a).ToList();
        var total = 0;

        foreach (var archivo in archivos)
        {
            var fuente = Path.GetFileName(archivo);
            var fragmentos = Fragmentador.Fragmentar(fuente, await File.ReadAllTextAsync(archivo, ct),
                o.Conocimiento.TamanoMaximoFragmento);
            if (fragmentos.Count == 0) continue;

            // Un solo llamado al modelo de embeddings por documento (lote)
            var vectores = await generador.GenerateAsync(fragmentos.Select(f => f.Contenido), cancellationToken: ct);

            await repositorio.ReemplazarFuenteAsync(fuente, modelo,
                fragmentos.Zip(vectores, (f, v) => (f, v.Vector.ToArray())));
            total += fragmentos.Count;
        }

        return new ResultadoIngesta(modelo, archivos.Count, total, archivos.Select(a => Path.GetFileName(a) ?? a).ToList());
    }
}
