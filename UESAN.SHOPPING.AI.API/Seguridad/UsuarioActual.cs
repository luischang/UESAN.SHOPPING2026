using System.Security.Claims;

namespace UESAN.SHOPPING.AI.API.Seguridad;

/// <summary>
/// H9 · ¿Quién es el usuario real? Las herramientas autorizan con este valor, nunca con lo que diga el modelo.
///
/// - En Development, sin token, se acepta la cabecera X-Cliente-Id (comodidad de laboratorio).
/// - Con JWT válido, el id del cliente es el claim "sub" (o NameIdentifier); el rol Supervisor
///   habilita /h7/aprobaciones.
/// </summary>
public static class UsuarioActual
{
    public const string Cabecera = "X-Cliente-Id";

    public static int? ClienteId(HttpContext http)
    {
        var claim = http.User.FindFirst(ClaimTypes.NameIdentifier) ?? http.User.FindFirst("sub");
        if (claim is not null && int.TryParse(claim.Value, out var idClaim)) return idClaim;

        if (http.RequestServices.GetRequiredService<IHostEnvironment>().IsDevelopment() &&
            int.TryParse(http.Request.Headers[Cabecera], out var idCabecera))
            return idCabecera;

        return null;
    }

    public static bool EsSupervisor(HttpContext http) =>
        http.User.HasClaim(c => c.Type is "role" && c.Value == "Supervisor");
}
