using UESAN.SHOPPING.AI.Datos;

namespace UESAN.SHOPPING.AI.API.Endpoints;

/// <summary>
/// H0 · Endpoints de la tienda SIN IA. Sirven para comprobar que la base de datos
/// del laboratorio está arriba y para ver los datos que luego usará la IA.
/// </summary>
public static class TiendaEndpoints
{
    public static IEndpointRouteBuilder MapTiendaEndpoints(this IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/api/tienda").WithTags("H0 · Tienda (datos transaccionales)");

        g.MapGet("/productos", async (string? q, TiendaRepositorio repo) =>
            string.IsNullOrWhiteSpace(q) ? await repo.ListarProductosAsync() : await repo.BuscarProductosAsync(q, 20));

        g.MapGet("/pedidos/{id:int}", async (int id, TiendaRepositorio repo) =>
            await repo.ObtenerPedidoAsync(id) is { } pedido ? Results.Ok(pedido) : Results.NotFound());

        g.MapGet("/clientes/{id:int}/pedidos", async (int id, TiendaRepositorio repo) =>
            Results.Ok(await repo.PedidosDeClienteAsync(id)));

        return app;
    }
}
