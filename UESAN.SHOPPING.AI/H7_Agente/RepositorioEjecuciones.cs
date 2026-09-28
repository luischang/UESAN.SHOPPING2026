using Dapper;
using UESAN.SHOPPING.AI.Datos;

namespace UESAN.SHOPPING.AI.H7_Agente;

public sealed class EjecucionAgenteDto
{
    public Guid Id { get; set; }
    public int ClienteId { get; set; }
    public string Objetivo { get; set; } = "";
    public string MensajeInicial { get; set; } = "";
    public string Estado { get; set; } = "";
    public int Pasos { get; set; }
    public long TokensUsados { get; set; }
    public string HistorialJson { get; set; } = "[]";
    public string? PendienteJson { get; set; }
    public string? AprobacionesJson { get; set; }
    public string? RespuestaFinal { get; set; }
    public string? MotivoFin { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime FechaActualizacion { get; set; }
}

/// <summary>
/// H7 · Persiste el estado del agente para poder SUSPENDER y REANUDAR
/// (esperando evidencia, esperando aprobación) sin perder el progreso.
/// </summary>
public sealed class RepositorioEjecuciones(ConexionSql conexion)
{
    public async Task GuardarAsync(EjecucionAgenteDto e)
    {
        await using var cn = conexion.Crear();
        await cn.ExecuteAsync("""
            MERGE ai.EjecucionAgente AS destino
            USING (SELECT @Id AS Id) AS origen ON destino.Id = origen.Id
            WHEN MATCHED THEN UPDATE SET
                Estado = @Estado, Pasos = @Pasos, TokensUsados = @TokensUsados,
                HistorialJson = @HistorialJson, PendienteJson = @PendienteJson,
                AprobacionesJson = @AprobacionesJson, RespuestaFinal = @RespuestaFinal,
                MotivoFin = @MotivoFin, FechaActualizacion = SYSDATETIME()
            WHEN NOT MATCHED THEN INSERT
                (Id, ClienteId, Objetivo, MensajeInicial, Estado, Pasos, TokensUsados,
                 HistorialJson, PendienteJson, AprobacionesJson, RespuestaFinal, MotivoFin)
            VALUES
                (@Id, @ClienteId, @Objetivo, @MensajeInicial, @Estado, @Pasos, @TokensUsados,
                 @HistorialJson, @PendienteJson, @AprobacionesJson, @RespuestaFinal, @MotivoFin);
            """, e);
    }

    public async Task RegistrarPasoAsync(Guid ejecucionId, int numero, string tipo, string detalle)
    {
        await using var cn = conexion.Crear();
        await cn.ExecuteAsync("""
            INSERT ai.PasoAgente (EjecucionId, Numero, Tipo, Detalle) VALUES (@ejecucionId, @numero, @tipo, @detalle)
            """, new { ejecucionId, numero, tipo, detalle = detalle.Length > 4000 ? detalle[..4000] : detalle });
    }

    public async Task<EjecucionAgenteDto?> ObtenerAsync(Guid id)
    {
        await using var cn = conexion.Crear();
        return await cn.QuerySingleOrDefaultAsync<EjecucionAgenteDto>(
            "SELECT * FROM ai.EjecucionAgente WHERE Id = @id", new { id });
    }

    public async Task<IReadOnlyList<object>> PasosAsync(Guid ejecucionId)
    {
        await using var cn = conexion.Crear();
        return (await cn.QueryAsync(
            "SELECT Numero, Tipo, Detalle, Fecha FROM ai.PasoAgente WHERE EjecucionId = @ejecucionId ORDER BY Numero",
            new { ejecucionId })).Cast<object>().ToList();
    }

    public async Task<IReadOnlyList<EjecucionAgenteDto>> PendientesDeAprobacionAsync()
    {
        await using var cn = conexion.Crear();
        return (await cn.QueryAsync<EjecucionAgenteDto>(
            "SELECT * FROM ai.EjecucionAgente WHERE Estado = 'ESPERANDO_APROBACION' ORDER BY FechaInicio"))
            .AsList();
    }
}
