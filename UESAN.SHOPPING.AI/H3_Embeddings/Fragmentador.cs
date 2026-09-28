using System.Text;

namespace UESAN.SHOPPING.AI.H3_Embeddings;

public sealed record FragmentoTexto(string Fuente, string Seccion, string Contenido);

/// <summary>
/// H3 · Divide un documento Markdown en fragmentos (chunking).
/// Regla simple: un fragmento por sección "## ", y si la sección es muy larga,
/// se corta por párrafos. Cada fragmento lleva el título del documento y de la
/// sección para no perder contexto cuando se recupere solo.
/// </summary>
public static class Fragmentador
{
    public static List<FragmentoTexto> Fragmentar(string fuente, string markdown, int tamanoMaximo)
    {
        var lineas = markdown.Replace("\r\n", "\n").Split('\n');
        var titulo = lineas.FirstOrDefault(l => l.StartsWith("# "))?[2..].Trim() ?? fuente;

        var fragmentos = new List<FragmentoTexto>();
        var seccion = "General";
        var cuerpo = new StringBuilder();

        void Cerrar()
        {
            var texto = cuerpo.ToString().Trim();
            if (texto.Length > 0) fragmentos.AddRange(Cortar(fuente, titulo, seccion, texto, tamanoMaximo));
            cuerpo.Clear();
        }

        foreach (var linea in lineas)
        {
            if (linea.StartsWith("# ")) continue;
            if (linea.StartsWith("## "))
            {
                Cerrar();
                seccion = linea[3..].Trim();
                continue;
            }
            cuerpo.AppendLine(linea);
        }
        Cerrar();
        return fragmentos;
    }

    private static IEnumerable<FragmentoTexto> Cortar(string fuente, string titulo, string seccion, string texto, int max)
    {
        var encabezado = $"{titulo} — {seccion}\n";
        if (texto.Length <= max)
        {
            yield return new FragmentoTexto(fuente, seccion, encabezado + texto);
            yield break;
        }

        var actual = new StringBuilder();
        foreach (var parrafo in texto.Split("\n\n", StringSplitOptions.RemoveEmptyEntries))
        {
            if (actual.Length > 0 && actual.Length + parrafo.Length > max)
            {
                yield return new FragmentoTexto(fuente, seccion, encabezado + actual.ToString().Trim());
                actual.Clear();
            }
            actual.AppendLine(parrafo).AppendLine();
        }
        if (actual.Length > 0) yield return new FragmentoTexto(fuente, seccion, encabezado + actual.ToString().Trim());
    }
}
