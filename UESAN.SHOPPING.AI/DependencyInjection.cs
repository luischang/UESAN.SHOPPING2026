using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using UESAN.SHOPPING.AI.Configuracion;
using UESAN.SHOPPING.AI.Datos;
using UESAN.SHOPPING.AI.H1_Chat;
using UESAN.SHOPPING.AI.H2_DatosTransaccionales;
using UESAN.SHOPPING.AI.H3_Embeddings;
using UESAN.SHOPPING.AI.H4_Rag;
using UESAN.SHOPPING.AI.H5_Herramientas;
using UESAN.SHOPPING.AI.H6_Workflow;

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

        // H2 · datos transaccionales como contexto
        services.AddScoped<AsistentePedidosService>();

        // H3 · embeddings y búsqueda vectorial
        services.AddSingleton<IEmbeddingGenerator<string, Embedding<float>>>(sp =>
            ProveedoresIa.CrearEmbeddings(sp.GetRequiredService<IOptions<AiOptions>>().Value));
        services.AddScoped<RepositorioConocimiento>();
        services.AddScoped<IngestaService>();
        services.AddScoped<BuscadorSemantico>();

        // H4 · RAG
        services.AddScoped<RagService>();

        // H5 · herramientas con puerta de validación y auditoría
        services.AddScoped<RepositorioAuditoria>();
        services.AddScoped<RepositorioDevoluciones>();
        services.AddScoped<PuertaHerramientas>();
        services.AddScoped<HerramientasFactory>();
        services.AddScoped<AsistenteHerramientasService>();

        // H6 · workflow con IA (camino fijo en código)
        services.AddScoped<DevolucionWorkflow>();

        return services;
    }
}
