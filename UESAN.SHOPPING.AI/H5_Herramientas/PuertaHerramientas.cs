using Microsoft.Extensions.Logging;
using UESAN.SHOPPING.AI.Comun;

namespace UESAN.SHOPPING.AI.H5_Herramientas;

/// <summary>
/// H5 · LA PUERTA. El modelo solo SOLICITA una herramienta; el código decide si se ejecuta.
/// Responsabilidades del código (ver diapositiva "Responsabilidad del modelo y del código"):
///   1. comprobar que la herramienta esté permitida (allowlist del contexto)
///   2. validar parámetros            ┐
///   3. autenticar y autorizar        ├─ dentro de cada herramienta (ReglaNegocioException)
///   4. ejecutar la operación         ┘
///   5. devolver el resultado al modelo (JSON)
///   6. registrar la trazabilidad (ai.Auditoria)
/// </summary>
public sealed class PuertaHerramientas(RepositorioAuditoria auditoria, ILogger<PuertaHerramientas> log)
{
    public async Task<string> EjecutarAsync(
        ContextoEjecucion ctx, string herramienta, object parametros, Func<Task<object?>> accion)
    {
        var parametrosJson = JsonOpciones.Serializar(parametros);

        if (!ctx.HerramientasPermitidas.Contains(herramienta))
            return await RechazarAsync(ctx, herramienta, parametrosJson, "Herramienta no permitida en este contexto.");

        try
        {
            var resultado = JsonOpciones.Serializar(await accion());
            await auditoria.RegistrarAsync(ctx, herramienta, parametrosJson, resultado, exitoso: true);
            ctx.Invocaciones.Add(new InvocacionHerramienta(herramienta, parametrosJson, true, resultado));
            log.LogInformation("Herramienta {Herramienta} OK ({Origen}) {Parametros}", herramienta, ctx.Origen, parametrosJson);
            return resultado;
        }
        catch (ReglaNegocioException ex)
        {
            return await RechazarAsync(ctx, herramienta, parametrosJson, ex.Message);
        }
    }

    private async Task<string> RechazarAsync(ContextoEjecucion ctx, string herramienta, string parametros, string motivo)
    {
        var resultado = JsonOpciones.Serializar(new { error = motivo });
        await auditoria.RegistrarAsync(ctx, herramienta, parametros, resultado, exitoso: false);
        ctx.Invocaciones.Add(new InvocacionHerramienta(herramienta, parametros, false, resultado));
        log.LogWarning("Herramienta {Herramienta} RECHAZADA ({Origen}): {Motivo}", herramienta, ctx.Origen, motivo);
        return resultado;
    }
}
