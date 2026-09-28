using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UESAN.SHOPPING.AI.Configuracion;
using UESAN.SHOPPING.AI.Datos;

namespace UESAN.SHOPPING.AI;

/// <summary>
/// Registro de servicios de IA. Cada hito agrega sus servicios aquí.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddShoppingAi(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AiOptions>(configuration.GetSection(AiOptions.Seccion));

        // H0 · datos transaccionales
        services.AddSingleton<ConexionSql>();
        services.AddScoped<TiendaRepositorio>();

        return services;
    }
}
