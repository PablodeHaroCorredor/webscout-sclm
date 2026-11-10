using WebScout.Web.Features.Administration.Domain;

namespace WebScout.Web.Features.Administration.Services;

public record AjustesInstitucionalesDto(
    string? Cif,
    string? DireccionBase,
    string? TelefonoContacto,
    string? EmailOficial,
    string RondaSolarActiva,
    DateOnly FechaInicioRonda,
    DateOnly FechaFinRonda
);

public record AjustesEconomicosDto(
    string? IbanCuentaBancaria,
    string? TitularCuenta,
    string? ConceptoCuotaSugerido
);

public record AjustesAdmisionesDto(
    bool AdmisionesAbiertas,
    string? MensajeBienvenidaAdmisiones
);

public interface IServicioConfiguracionGrupo
{
    Task<ConfiguracionGrupo> ObtenerConfiguracionAsync(CancellationToken cancellationToken = default);

    Task<ConfiguracionGrupo> GuardarAreaInstitucionalAsync(
        AjustesInstitucionalesDto dto,
        CancellationToken cancellationToken = default);

    Task<ConfiguracionGrupo> GuardarAreaEconomicaAsync(
        AjustesEconomicosDto dto,
        CancellationToken cancellationToken = default);

    Task<ConfiguracionGrupo> GuardarAreaAdmisionesAsync(
        AjustesAdmisionesDto dto,
        CancellationToken cancellationToken = default);

    Task<ConfiguracionGrupo> RegistrarSimulacionBackupAsync(
        string? estadoDetalle = null,
        CancellationToken cancellationToken = default);
}
