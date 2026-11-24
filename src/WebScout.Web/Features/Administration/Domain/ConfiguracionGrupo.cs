using WebScout.Web.Common.Domain;

namespace WebScout.Web.Features.Administration.Domain;

/// <summary>
/// Configuración operativa y ajustes por áreas del grupo scout (RF64 / PA16).
/// Permite el guardado parcial atómico por áreas sin sobreescrituras cruzadas.
/// </summary>
public class ConfiguracionGrupo : IEntidadOrganizacion
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Organización / Grupo scout al que pertenecen los ajustes (aislamiento multitenant estricto).
    /// </summary>
    public Guid OrganizacionId { get; set; }

    // Área 1: Institucional y ronda solar
    public string? Cif { get; set; }
    public string? DireccionBase { get; set; }
    public string? TelefonoContacto { get; set; }
    public string? EmailOficial { get; set; }
    public string RondaSolarActiva { get; set; } = "2026/2027";
    public DateOnly FechaInicioRonda { get; set; } = new(2026, 10, 1);
    public DateOnly FechaFinRonda { get; set; } = new(2027, 7, 31);

    // Área 2: Económica y cuenta bancaria
    public string? IbanCuentaBancaria { get; set; }
    public string? TitularCuenta { get; set; }
    public string? ConceptoCuotaSugerido { get; set; }

    // Área 3: Secretaría y admisiones
    public bool AdmisionesAbiertas { get; set; } = true;
    public string? MensajeBienvenidaAdmisiones { get; set; }

    // Área 4: Operación y respaldo (PA19 / RF65)
    public DateTime? FechaUltimoBackupUtc { get; set; }
    public string? EstadoUltimoBackup { get; set; }
    public int ObjetivoRpoHoras { get; set; } = 1;
    public int ObjetivoRtoHoras { get; set; } = 4;

    public DateTime FechaModificacionUtc { get; set; } = DateTime.UtcNow;
}
