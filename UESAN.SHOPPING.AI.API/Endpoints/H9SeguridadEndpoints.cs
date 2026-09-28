using UESAN.SHOPPING.AI.H9_Seguridad;

namespace UESAN.SHOPPING.AI.API.Endpoints;

/// <summary>H9 · Endpoint didáctico: ver qué detecta la guardia de contenido, sin llamar al modelo.</summary>
public static class H9SeguridadEndpoints
{
    public static RouteGroupBuilder MapH9Endpoints(this RouteGroupBuilder ia)
    {
        var g = ia.MapGroup("/h9").WithTags("H9 · Seguridad y gobierno");

        g.MapPost("/guardia", (TextoRequest req) => GuardiaContenido.Analizar(req.Texto));

        return ia;
    }
}
