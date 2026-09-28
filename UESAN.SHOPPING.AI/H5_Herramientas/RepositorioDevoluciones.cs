using Dapper;
using UESAN.SHOPPING.AI.Datos;

namespace UESAN.SHOPPING.AI.H5_Herramientas;

public sealed class SolicitudDevolucionDto
{
    public int Id { get; set; }
    public int PedidoId { get; set; }
    public int ProductoId { get; set; }
    public int ClienteId { get; set; }
    public string Motivo { get; set; } = "";
    public string Estado { get; set; } = "";
    public decimal? MontoReembolso { get; set; }
    public string Origen { get; set; } = "";
    public DateTime FechaRegistro { get; set; }
}

public sealed class RepositorioDevoluciones(ConexionSql conexion)
{
    public async Task<bool> EsRetornableAsync(int productoId)
    {
        await using var cn = conexion.Crear();
        return await cn.ExecuteScalarAsync<bool?>(
            "SELECT Retornable FROM dbo.Producto WHERE Id = @productoId", new { productoId }) ?? false;
    }

    /// <summary>Idempotente: si ya existe una solicitud para el mismo pedido y producto, la devuelve.</summary>
    public async Task<(SolicitudDevolucionDto Solicitud, bool YaExistia)> RegistrarAsync(
        int pedidoId, int productoId, int clienteId, string motivo, string origen)
    {
        await using var cn = conexion.Crear();
        var existente = await cn.QuerySingleOrDefaultAsync<SolicitudDevolucionDto>(
            "SELECT * FROM ai.SolicitudDevolucion WHERE PedidoId = @pedidoId AND ProductoId = @productoId",
            new { pedidoId, productoId });
        if (existente is not null) return (existente, true);

        var id = await cn.ExecuteScalarAsync<int>("""
            INSERT ai.SolicitudDevolucion (PedidoId, ProductoId, ClienteId, Motivo, Estado, Origen)
            OUTPUT INSERTED.Id
            VALUES (@pedidoId, @productoId, @clienteId, @motivo, 'REGISTRADA', @origen)
            """, new { pedidoId, productoId, clienteId, motivo = motivo.Length > 500 ? motivo[..500] : motivo, origen });

        return ((await ObtenerAsync(id))!, false);
    }

    public async Task<SolicitudDevolucionDto?> ObtenerAsync(int id)
    {
        await using var cn = conexion.Crear();
        return await cn.QuerySingleOrDefaultAsync<SolicitudDevolucionDto>(
            "SELECT * FROM ai.SolicitudDevolucion WHERE Id = @id", new { id });
    }

    /// <summary>H7 · Emite el reembolso solo si la solicitud sigue REGISTRADA.</summary>
    public async Task<bool> EmitirReembolsoAsync(int id, decimal monto)
    {
        await using var cn = conexion.Crear();
        return await cn.ExecuteAsync("""
            UPDATE ai.SolicitudDevolucion
            SET Estado = 'REEMBOLSO_EMITIDO', MontoReembolso = @monto
            WHERE Id = @id AND Estado = 'REGISTRADA'
            """, new { id, monto }) == 1;
    }

    public async Task<IReadOnlyList<SolicitudDevolucionDto>> ListarAsync(int? clienteId)
    {
        await using var cn = conexion.Crear();
        return (await cn.QueryAsync<SolicitudDevolucionDto>(
            "SELECT * FROM ai.SolicitudDevolucion WHERE @clienteId IS NULL OR ClienteId = @clienteId ORDER BY Id DESC",
            new { clienteId })).AsList();
    }
}
