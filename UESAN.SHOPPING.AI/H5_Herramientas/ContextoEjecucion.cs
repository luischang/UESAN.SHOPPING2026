namespace UESAN.SHOPPING.AI.H5_Herramientas;

/// <summary>Registro de cada herramienta invocada (trazabilidad visible en la respuesta).</summary>
public sealed record InvocacionHerramienta(string Herramienta, string Parametros, bool Exitosa, string Resultado);

/// <summary>
/// H5 · Contexto con el que se ejecutan las herramientas: QUIÉN es el usuario real,
/// DESDE DÓNDE se invoca y QUÉ herramientas están permitidas. El modelo no puede cambiarlo.
/// </summary>
public sealed class ContextoEjecucion
{
    public required int ClienteId { get; init; }
    public required string Origen { get; init; }
    public required IReadOnlySet<string> HerramientasPermitidas { get; init; }
    public List<InvocacionHerramienta> Invocaciones { get; } = [];
}

/// <summary>Error de negocio que se devuelve al modelo como resultado (no como excepción del sistema).</summary>
public sealed class ReglaNegocioException(string mensaje) : Exception(mensaje);

/// <summary>Allowlists: qué herramientas puede usar cada tipo de sistema.</summary>
public static class CatalogoHerramientas
{
    public static readonly IReadOnlySet<string> Asistente = new HashSet<string>
    {
        "ConsultarProducto", "ConsultarPedido", "BuscarPolitica", "RegistrarDevolucion"
    };
}
