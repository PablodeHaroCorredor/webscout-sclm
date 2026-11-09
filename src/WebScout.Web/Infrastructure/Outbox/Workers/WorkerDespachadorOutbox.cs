using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WebScout.Web.Infrastructure.Outbox.Services;

namespace WebScout.Web.Infrastructure.Outbox.Workers;

/// <summary>
/// Worker de segundo plano que procesa periódicamente la cola Outbox persistida en base de datos (ADR07 / PA11).
/// Se ejecuta como un IHostedService / BackgroundService nativo de .NET 10.
/// </summary>
public class WorkerDespachadorOutbox : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<WorkerDespachadorOutbox> _logger;
    private readonly TimeSpan _intervaloVacio = TimeSpan.FromSeconds(5);
    private readonly TimeSpan _intervaloContinuo = TimeSpan.FromMilliseconds(500);

    public WorkerDespachadorOutbox(
        IServiceScopeFactory scopeFactory,
        ILogger<WorkerDespachadorOutbox> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Iniciando WorkerDespachadorOutbox...");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var servicioOutbox = scope.ServiceProvider.GetRequiredService<IServicioOutbox>();

                var procesados = await servicioOutbox.ProcesarLotePendienteAsync(tamanoLote: 20, stoppingToken);

                if (procesados > 0)
                {
                    // Si procesó mensajes, reevaluar rápidamente por si hay más en cola
                    await Task.Delay(_intervaloContinuo, stoppingToken);
                }
                else
                {
                    // Si la cola está vacía, esperar el intervalo regular
                    await Task.Delay(_intervaloVacio, stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en el ciclo de ejecución de WorkerDespachadorOutbox");
                await Task.Delay(_intervaloVacio, stoppingToken);
            }
        }

        _logger.LogInformation("WorkerDespachadorOutbox detenido.");
    }
}
