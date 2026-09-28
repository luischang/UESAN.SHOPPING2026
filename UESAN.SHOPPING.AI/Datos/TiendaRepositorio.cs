using Dapper;

namespace UESAN.SHOPPING.AI.Datos;

/// <summary>
/// Acceso a los DATOS TRANSACCIONALES de la tienda (productos, clientes, pedidos).
/// Consultas exactas con SQL: esto NO es RAG.
/// </summary>
public sealed class TiendaRepositorio(ConexionSql conexion)
{
    private const string SelectProducto = """
        SELECT p.Id, p.Nombre, p.Descripcion, p.Precio, p.Stock, c.Nombre AS Categoria
        FROM dbo.Producto p
        JOIN dbo.Categoria c ON c.Id = p.CategoriaId
        """;

    public async Task<IReadOnlyList<ProductoDto>> ListarProductosAsync(int max = 50)
    {
        await using var cn = conexion.Crear();
        var sql = $"{SelectProducto} WHERE p.Activo = 1 ORDER BY c.Nombre, p.Nombre OFFSET 0 ROWS FETCH NEXT @max ROWS ONLY";
        return (await cn.QueryAsync<ProductoDto>(sql, new { max })).AsList();
    }

    public async Task<IReadOnlyList<ProductoDto>> BuscarProductosAsync(string texto, int max = 5)
    {
        await using var cn = conexion.Crear();
        var sql = $"""
            {SelectProducto}
            WHERE p.Activo = 1
              AND (p.Nombre LIKE @patron OR p.Descripcion LIKE @patron OR c.Nombre LIKE @patron)
            ORDER BY p.Nombre
            OFFSET 0 ROWS FETCH NEXT @max ROWS ONLY
            """;
        return (await cn.QueryAsync<ProductoDto>(sql, new { max, patron = $"%{texto.Trim()}%" })).AsList();
    }

    public async Task<ProductoDto?> ObtenerProductoAsync(int id)
    {
        await using var cn = conexion.Crear();
        return await cn.QuerySingleOrDefaultAsync<ProductoDto>($"{SelectProducto} WHERE p.Id = @id", new { id });
    }

    public async Task<ClienteDto?> ObtenerClienteAsync(int id)
    {
        await using var cn = conexion.Crear();
        return await cn.QuerySingleOrDefaultAsync<ClienteDto>(
            "SELECT Id, Nombres FROM dbo.Cliente WHERE Id = @id", new { id });
    }

    public async Task<PedidoDto?> ObtenerPedidoAsync(int pedidoId)
    {
        const string sql = """
            SELECT p.Id, p.ClienteId, cl.Nombres AS Cliente, p.Fecha, p.Estado, p.FechaEntrega, p.Total
            FROM dbo.Pedido p
            JOIN dbo.Cliente cl ON cl.Id = p.ClienteId
            WHERE p.Id = @pedidoId;

            SELECT d.ProductoId, pr.Nombre AS Producto, d.Cantidad, d.PrecioUnitario
            FROM dbo.PedidoDetalle d
            JOIN dbo.Producto pr ON pr.Id = d.ProductoId
            WHERE d.PedidoId = @pedidoId;
            """;

        await using var cn = conexion.Crear();
        using var lector = await cn.QueryMultipleAsync(sql, new { pedidoId });
        var pedido = await lector.ReadSingleOrDefaultAsync<PedidoDto>();
        if (pedido is null) return null;
        pedido.Detalle = (await lector.ReadAsync<PedidoDetalleDto>()).AsList();
        return pedido;
    }

    public async Task<IReadOnlyList<PedidoDto>> PedidosDeClienteAsync(int clienteId)
    {
        await using var cn = conexion.Crear();
        var ids = (await cn.QueryAsync<int>(
            "SELECT Id FROM dbo.Pedido WHERE ClienteId = @clienteId ORDER BY Fecha DESC", new { clienteId })).AsList();

        var pedidos = new List<PedidoDto>();
        foreach (var id in ids)
        {
            if (await ObtenerPedidoAsync(id) is { } pedido) pedidos.Add(pedido);
        }
        return pedidos;
    }
}
