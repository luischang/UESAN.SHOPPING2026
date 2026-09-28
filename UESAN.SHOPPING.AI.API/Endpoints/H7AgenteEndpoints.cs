using UESAN.SHOPPING.AI.API.Seguridad;
using UESAN.SHOPPING.AI.H7_Agente;

namespace UESAN.SHOPPING.AI.API.Endpoints;

public static class H7AgenteEndpoints
{
    public static RouteGroupBuilder MapH7Endpoints(this RouteGroupBuilder ia)
    {
        var g = ia.MapGroup("/h7").WithTags("H7 · Agente");

        g.MapPost("/agente", async (IniciarAgenteRequest req, HttpContext http, AgenteDevoluciones agente, CancellationToken ct) =>
        {
            if (UsuarioActual.ClienteId(http) is not int clienteId)
                return Results.Problem($"Falta la cabecera {UsuarioActual.Cabecera}.", statusCode: 401);
            return Results.Ok(await agente.IniciarAsync(clienteId, req.Mensaje, ct));
        });

        g.MapGet("/agente/{id:guid}", async (Guid id, RepositorioEjecuciones repo) =>
            await repo.ObtenerAsync(id) is { } e
                ? Results.Ok(new { e.Id, e.Estado, e.Pasos, e.TokensUsados, e.RespuestaFinal, e.MotivoFin, e.FechaInicio, e.FechaActualizacion })
                : Results.NotFound());

        g.MapGet("/agente/{id:guid}/pasos", async (Guid id, RepositorioEjecuciones repo) =>
            await repo.PasosAsync(id));

        // El agente pidió evidencia (SolicitarEvidencia): el cliente la envía y el agente continúa.
        g.MapPost("/agente/{id:guid}/evidencia", async (Guid id, EvidenciaRequest req, AgenteDevoluciones agente, CancellationToken ct) =>
            Results.Ok(await agente.ReanudarConEvidenciaAsync(id, req.Descripcion, ct)));

        // Bandeja de supervisor: ejecuciones esperando aprobación (reembolso > umbral)
        g.MapGet("/aprobaciones", async (RepositorioEjecuciones repo) =>
            (await repo.PendientesDeAprobacionAsync())
                .Select(e => new { e.Id, e.ClienteId, e.MensajeInicial, e.PendienteJson, e.FechaInicio }));

        g.MapPost("/aprobaciones/{id:guid}", async (Guid id, AprobacionRequest req, AgenteDevoluciones agente, CancellationToken ct) =>
            Results.Ok(await agente.AprobarAsync(id, req.Aprobador, ct)));

        return ia;
    }
}
