using UESAN.SHOPPING.AI.H2_DatosTransaccionales;

namespace UESAN.SHOPPING.AI.API.Endpoints;

public static class H2DatosEndpoints
{
    public static RouteGroupBuilder MapH2Endpoints(this RouteGroupBuilder ia)
    {
        var g = ia.MapGroup("/h2").WithTags("H2 · Datos transaccionales como contexto");

        g.MapPost("/mis-pedidos", async (PreguntaClienteRequest req, AsistentePedidosService servicio, CancellationToken ct) =>
        {
            try
            {
                return Results.Ok(await servicio.ResponderAsync(req.ClienteId, req.Pregunta, ct));
            }
            catch (KeyNotFoundException ex)
            {
                return Results.NotFound(new { error = ex.Message });
            }
        });

        return ia;
    }
}
