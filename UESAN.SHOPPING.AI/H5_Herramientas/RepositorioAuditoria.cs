using Dapper;
using UESAN.SHOPPING.AI.Datos;

namespace UESAN.SHOPPING.AI.H5_Herramientas;

/// <summary>H5 · Trazabilidad: cada invocación de herramienta queda registrada.</summary>
public sealed class RepositorioAuditoria(ConexionSql conexion)
{
    public async Task RegistrarAsync(ContextoEjecucion ctx, string herramienta, string parametros, string resultado, bool exitoso)
    {
        await using var cn = conexion.Crear();
        await cn.ExecuteAsync("""
            INSERT ai.Auditoria (Usuario, Origen, Herramienta, Parametros, Resultado, Exitoso)
            VALUES (@Usuario, @Origen, @Herramienta, @Parametros, @Resultado, @Exitoso)
            """,
            new
            {
                Usuario = $"cliente:{ctx.ClienteId}",
                ctx.Origen,
                Herramienta = herramienta,
                Parametros = parametros,
                Resultado = resultado.Length > 4000 ? resultado[..4000] : resultado,
                Exitoso = exitoso
            });
    }

    public async Task<IReadOnlyList<object>> UltimasAsync(int max = 50)
    {
        await using var cn = conexion.Crear();
        return (await cn.QueryAsync(
            "SELECT TOP (@max) Id, Fecha, Usuario, Origen, Herramienta, Parametros, Exitoso FROM ai.Auditoria ORDER BY Id DESC",
            new { max })).Cast<object>().ToList();
    }
}
