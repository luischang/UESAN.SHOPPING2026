using UESAN.SHOPPING.AI.API.Seguridad;
using UESAN.SHOPPING.AI.H10_Multiagente;

namespace UESAN.SHOPPING.AI.API.Endpoints;

public static class H10MultiagenteEndpoints
{
    public static RouteGroupBuilder MapH10Endpoints(this RouteGroupBuilder ia)
    {
        var g = ia.MapGroup("/h10").WithTags("H10 · Multiagente (orquestador-trabajadores)");

        g.MapPost("/orquestar", async (ConsultaOrquestadaRequest req, HttpContext http,
            OrquestadorPostventaService orquestador, CancellationToken ct) =>
        {
            if (UsuarioActual.ClienteId(http) is not int clienteId)
                return Results.Problem($"Falta la cabecera {UsuarioActual.Cabecera} o un token válido.", statusCode: 401);
            return Results.Ok(await orquestador.AtenderAsync(clienteId, req.Pregunta, ct));
        });

        return ia;
    }
}
