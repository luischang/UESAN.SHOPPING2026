using Dapper;
using UESAN.SHOPPING.AI.Datos;

namespace UESAN.SHOPPING.AI.H8_Activacion;

public sealed class EventoAgenteDto
{
    public long Id { get; set; }
    public string Tipo { get; set; } = "";
    public int? ClienteId { get; set; }
    public string Payload { get; set; } = "";
}

/// <summary>H8 · Cola de eventos: la puerta de entrada para activar al agente sin un clic humano.</summary>
public sealed class RepositorioEventos(ConexionSql conexion)
{
    public async Task<long> EncolarAsync(string tipo, int? clienteId, string payload)
    {
        await using var cn = conexion.Crear();
        return await cn.ExecuteScalarAsync<long>("""
            INSERT ai.EventoAgente (Tipo, ClienteId, Payload) OUTPUT INSERTED.Id
            VALUES (@tipo, @clienteId, @payload)
            """, new { tipo, clienteId, payload });
    }

    /// <summary>Toma el evento pendiente más antiguo y lo marca en proceso (evita duplicar trabajo entre workers).</summary>
    public async Task<EventoAgenteDto?> TomarSiguientePendienteAsync()
    {
        await using var cn = conexion.Crear();
        return await cn.QuerySingleOrDefaultAsync<EventoAgenteDto>("""
            WITH Siguiente AS (
                SELECT TOP (1) Id, Tipo, ClienteId, Payload
                FROM ai.EventoAgente WITH (READPAST, UPDLOCK, ROWLOCK)
                WHERE Estado = 'PENDIENTE'
                ORDER BY FechaCreacion
            )
            UPDATE ai.EventoAgente SET Estado = 'PROCESADO', FechaProceso = SYSDATETIME()
            OUTPUT INSERTED.Id, INSERTED.Tipo, INSERTED.ClienteId, INSERTED.Payload
            FROM ai.EventoAgente e JOIN Siguiente s ON s.Id = e.Id
            """);
    }

    public async Task MarcarResultadoAsync(long id, Guid? ejecucionId, string? error)
    {
        await using var cn = conexion.Crear();
        await cn.ExecuteAsync("""
            UPDATE ai.EventoAgente SET Estado = @estado, EjecucionId = @ejecucionId, Error = @error
            WHERE Id = @id
            """, new { id, estado = error is null ? "PROCESADO" : "ERROR", ejecucionId, error });
    }

    public async Task<IReadOnlyList<object>> RecientesAsync(int max = 50)
    {
        await using var cn = conexion.Crear();
        return (await cn.QueryAsync(
            "SELECT TOP (@max) * FROM ai.EventoAgente ORDER BY Id DESC", new { max })).Cast<object>().ToList();
    }
}
