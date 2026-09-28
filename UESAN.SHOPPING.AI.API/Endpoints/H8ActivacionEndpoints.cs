using UESAN.SHOPPING.AI.H8_Activacion;

namespace UESAN.SHOPPING.AI.API.Endpoints;

public static class H8ActivacionEndpoints
{
    public static RouteGroupBuilder MapH8Endpoints(this RouteGroupBuilder ia)
    {
        var g = ia.MapGroup("/h8").WithTags("H8 · Activación (cola + worker)");

        // Simula, por ejemplo, un correo entrante o un formulario web: no hay un humano
        // frente al endpoint de H7, solo un evento que el worker recogerá en segundo plano.
        g.MapPost("/eventos", async (EncolarEventoRequest req, RepositorioEventos eventos) =>
            new { id = await eventos.EncolarAsync("SOLICITUD_CLIENTE", req.ClienteId, req.Mensaje) });

        g.MapGet("/eventos", async (RepositorioEventos eventos) => await eventos.RecientesAsync());

        return ia;
    }
}
