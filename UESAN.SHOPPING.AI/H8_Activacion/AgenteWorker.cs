using Microsoft.Extensions.Logging;
using UESAN.SHOPPING.AI.H7_Agente;

namespace UESAN.SHOPPING.AI.H8_Activacion;

/// <summary>
/// H8 · Activación sin intervención humana: un BackgroundService revisa la cola
/// periódicamente (PeriodicTimer) y activa al agente por cada evento pendiente.
///
/// Un "cron" (revisión programada) es solo OTRO DISPARADOR del mismo agente de H7:
/// el ciclo observar-decidir-actuar-evaluar y sus límites no cambian.
/// </summary>
public sealed class AgenteWorker(
    IServiceScopeFactory scopeFactory, ILogger<AgenteWorker> log) : BackgroundService
{
    private static readonly TimeSpan Intervalo = TimeSpan.FromSeconds(15);

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        using var temporizador = new PeriodicTimer(Intervalo);
        do
        {
            try
            {
                await ProcesarPendientesAsync(ct);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                log.LogError(ex, "Error revisando la cola de eventos del agente.");
            }
        } while (await temporizador.WaitForNextTickAsync(ct));
    }

    private async Task ProcesarPendientesAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            using var scope = scopeFactory.CreateScope();
            var eventos = scope.ServiceProvider.GetRequiredService<RepositorioEventos>();
            var agente = scope.ServiceProvider.GetRequiredService<AgenteDevoluciones>();

            var evento = await eventos.TomarSiguientePendienteAsync();
            if (evento is null) return;   // cola vacía: espera al siguiente tick

            try
            {
                var clienteId = evento.ClienteId ?? throw new InvalidOperationException("El evento no tiene ClienteId.");
                var resultado = await agente.IniciarAsync(clienteId, evento.Payload, ct);
                await eventos.MarcarResultadoAsync(evento.Id, resultado.EjecucionId, null);
                log.LogInformation("Evento {Id} procesado -> ejecución {EjecucionId} ({Estado})",
                    evento.Id, resultado.EjecucionId, resultado.Estado);
            }
            catch (Exception ex)
            {
                await eventos.MarcarResultadoAsync(evento.Id, null, ex.Message);
                log.LogError(ex, "Error procesando el evento {Id}.", evento.Id);
            }
        }
    }
}
