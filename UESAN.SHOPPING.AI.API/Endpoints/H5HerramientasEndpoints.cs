using UESAN.SHOPPING.AI.API.Seguridad;
using UESAN.SHOPPING.AI.H5_Herramientas;

namespace UESAN.SHOPPING.AI.API.Endpoints;

public static class H5HerramientasEndpoints
{
    public static RouteGroupBuilder MapH5Endpoints(this RouteGroupBuilder ia)
    {
        var g = ia.MapGroup("/h5").WithTags("H5 · Herramientas (tool calling)");

        g.MapPost("/asistente", async (PreguntaRequest req, HttpContext http,
            AsistenteHerramientasService asistente, CancellationToken ct) =>
        {
            if (UsuarioActual.ClienteId(http) is not int clienteId)
                return Results.Problem($"Falta la cabecera {UsuarioActual.Cabecera}.", statusCode: 401);

            return Results.Ok(await asistente.ResponderAsync(clienteId, req.Pregunta, ct));
        });

        // Trazabilidad: qué herramientas se ejecutaron, con qué parámetros y si se permitieron
        g.MapGet("/auditoria", async (RepositorioAuditoria auditoria) => await auditoria.UltimasAsync());

        g.MapGet("/devoluciones", async (int? clienteId, RepositorioDevoluciones repo) => await repo.ListarAsync(clienteId));

        return ia;
    }
}
