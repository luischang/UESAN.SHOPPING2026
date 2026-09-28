using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using UESAN.SHOPPING.AI.Configuracion;
using UESAN.SHOPPING.AI.Datos;
using UESAN.SHOPPING.AI.H1_Chat;

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

        // H1 · cliente del modelo (local u nube según AI:Proveedor) + registro de llamadas
        services.AddChatClient(sp => ProveedoresIa.CrearChat(sp.GetRequiredService<IOptions<AiOptions>>().Value))
                .UseLogging();
        services.AddScoped<ChatBasicoService>();

        return services;
    }
}
