using System.Text.RegularExpressions;

namespace UESAN.SHOPPING.AI.H9_Seguridad;

public sealed record ResultadoGuardia(bool Sospechoso, IReadOnlyList<string> Señales);

/// <summary>
/// H9 · Defensa básica contra PROMPT INJECTION (OWASP LLM01): un mensaje de cliente,
/// un documento indexado o el resultado de una herramienta pueden contener texto que
/// intente cambiar las instrucciones del sistema. No hay una defensa perfecta; esto
/// reduce el riesgo con dos capas: envolver el dato no confiable y detectar patrones.
/// </summary>
public static partial class GuardiaContenido
{
    [GeneratedRegex(
        "ignora (las )?instrucciones|olvida (las )?instrucciones|eres ahora|nuevo sistema|" +
        "system prompt|actúa como|no eres un asistente|revela tu prompt|ignore (the )?instructions",
        RegexOptions.IgnoreCase)]
    private static partial Regex PatronesSospechosos();

    public static ResultadoGuardia Analizar(string texto)
    {
        var señales = PatronesSospechosos().Matches(texto)
            .Select(m => m.Value).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        return new ResultadoGuardia(señales.Count > 0, señales);
    }

    /// <summary>
    /// Envuelve datos NO CONFIABLES (mensaje del cliente, fragmentos de documentos, resultados
    /// de herramientas) con una etiqueta explícita: el prompt del sistema indica que lo que
    /// esté dentro de &lt;dato_no_confiable&gt; es información a analizar, nunca una instrucción.
    /// </summary>
    public static string Envolver(string texto) => $"<dato_no_confiable>\n{texto}\n</dato_no_confiable>";
}
