using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace UESAN.SHOPPING.AI.Comun;

/// <summary>Opciones JSON compartidas (camelCase y tildes legibles para el modelo).</summary>
public static class JsonOpciones
{
    public static readonly JsonSerializerOptions Web = new(JsonSerializerDefaults.Web)
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public static string Serializar(object? valor) => JsonSerializer.Serialize(valor, Web);
}
