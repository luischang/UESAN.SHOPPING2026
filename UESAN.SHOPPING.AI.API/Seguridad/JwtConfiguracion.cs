using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace UESAN.SHOPPING.AI.API.Seguridad;

public sealed class JwtOptions
{
    public const string Seccion = "Jwt";
    public string Issuer { get; set; } = "uesan-shopping-ai";
    public string Audience { get; set; } = "uesan-shopping-ai-clientes";
    /// <summary>Nunca en el appsettings versionado: usar user-secrets o variable de entorno Jwt__Key.</summary>
    public string Key { get; set; } = "";
}

/// <summary>H9 · Claims que usa la aplicación. "sub" = id del cliente autenticado.</summary>
public static class ClaimsUesan
{
    public const string Rol = "role";
    public const string RolSupervisor = "Supervisor";
}

public static class JwtConfiguracion
{
    public static IServiceCollection AddJwtUesan(this IServiceCollection services, IConfiguration configuration)
    {
        var jwt = configuration.GetSection(JwtOptions.Seccion).Get<JwtOptions>() ?? new JwtOptions();
        if (string.IsNullOrWhiteSpace(jwt.Key))
            throw new InvalidOperationException(
                "Configura Jwt:Key con 'dotnet user-secrets' o la variable de entorno Jwt__Key (mínimo 32 caracteres).");

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(o =>
            {
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = jwt.Issuer,
                    ValidAudience = jwt.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ClockSkew = TimeSpan.FromSeconds(30)
                };
            });

        services.AddAuthorizationBuilder()
            .AddPolicy("Supervisor", p => p.RequireClaim(ClaimsUesan.Rol, ClaimsUesan.RolSupervisor));

        return services;
    }
}
