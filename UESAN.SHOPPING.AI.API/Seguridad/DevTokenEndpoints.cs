using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace UESAN.SHOPPING.AI.API.Seguridad;

/// <summary>
/// H9 · SOLO para Development: emite un JWT de prueba sin flujo de login real.
/// En producción, los tokens los emite un Identity Provider (Auth0, Entra ID, IdentityServer, etc.).
/// </summary>
public static class DevTokenEndpoints
{
    public static IEndpointRouteBuilder MapDevTokenEndpoint(this IEndpointRouteBuilder app, IWebHostEnvironment env)
    {
        if (!env.IsDevelopment()) return app;

        app.MapPost("/dev/token", (int clienteId, bool supervisor, JwtOptions jwt) =>
        {
            List<Claim> claims = [new(ClaimTypes.NameIdentifier, clienteId.ToString())];
            if (supervisor) claims.Add(new Claim(ClaimsUesan.Rol, ClaimsUesan.RolSupervisor));

            var credenciales = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)), SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(jwt.Issuer, jwt.Audience, claims,
                expires: DateTime.UtcNow.AddHours(2), signingCredentials: credenciales);

            return new { token = new JwtSecurityTokenHandler().WriteToken(token) };
        }).WithTags("Dev · solo Development").ExcludeFromDescription();

        return app;
    }
}
