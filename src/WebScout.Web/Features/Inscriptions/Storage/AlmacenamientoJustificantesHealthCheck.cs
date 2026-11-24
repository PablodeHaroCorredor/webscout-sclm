using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace WebScout.Web.Features.Inscriptions.Storage;

/// <summary>
/// Comprobación de salud del almacén de justificantes. Se expone mediante <see cref="HealthCheckService"/>
/// para que el panel de diagnóstico la consulte sin depender directamente de este slice.
/// </summary>
public sealed class AlmacenamientoJustificantesHealthCheck(IAlmacenamientoJustificantes almacenamiento) : IHealthCheck
{
    public const string Nombre = "almacen-justificantes";

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var (disponible, detalle) = await almacenamiento.ComprobarDisponibilidadAsync();
        return disponible ? HealthCheckResult.Healthy(detalle) : HealthCheckResult.Unhealthy(detalle);
    }
}
