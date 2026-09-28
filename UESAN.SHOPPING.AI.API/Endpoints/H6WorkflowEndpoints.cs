using UESAN.SHOPPING.AI.API.Seguridad;
using UESAN.SHOPPING.AI.H6_Workflow;

namespace UESAN.SHOPPING.AI.API.Endpoints;

public static class H6WorkflowEndpoints
{
    public static RouteGroupBuilder MapH6Endpoints(this RouteGroupBuilder ia)
    {
        var g = ia.MapGroup("/h6").WithTags("H6 · Workflow con IA");

        g.MapPost("/devoluciones", async (SolicitudDevolucionRequest req, HttpContext http,
            DevolucionWorkflow workflow, CancellationToken ct) =>
        {
            if (UsuarioActual.ClienteId(http) is not int clienteId)
                return Results.Problem($"Falta la cabecera {UsuarioActual.Cabecera}.", statusCode: 401);

            return Results.Ok(await workflow.EjecutarAsync(clienteId, req.PedidoId, req.Mensaje, ct));
        });

        return ia;
    }
}
