using WebScout.Web.Common.Domain;
using WebScout.Web.Features.Activities.Domain;
using WebScout.Web.Features.Identity.Domain;
using WebScout.Web.Features.Inscriptions.Domain;

namespace WebScout.Web.Features.Dossier.Domain;

/// <summary>
/// Modelo consolidado para el Dossier Operativo de Campamento o Salida Scout.
/// Agrupa en un único documento de campo toda la información sanitaria, logística,
/// de intendencia alimentaria, contactos de urgencia y control de asistencia.
/// </summary>
public class DossierOperativoActividad
{
    public required ActividadScout Actividad { get; set; }

    public IReadOnlyList<InscripcionActividad> Inscripciones { get; set; } = [];

    // Métricas globales de asistencia
    public int TotalInscritos => Inscripciones.Count;
    public int TotalConfirmados => Inscripciones.Count(i => i.Estado == EstadoInscripcion.Confirmada);
    public int TotalPresentesSalida => Inscripciones.Count(i => i.PresenteEnSalida);
    public int TotalPresentesRegreso => Inscripciones.Count(i => i.PresenteEnRegreso);

    // Balance económico de cuotas
    public decimal TotalPresupuestadoEuros => Inscripciones.Where(i => i.Estado != EstadoInscripcion.Cancelada).Sum(i => i.ImporteEuros);
    public decimal TotalRecaudadoEuros => Inscripciones.Where(i => i.Estado == EstadoInscripcion.Confirmada).Sum(i => i.ImporteEuros);
    public decimal TotalPendienteEuros => Inscripciones.Where(i => i.Estado == EstadoInscripcion.PendienteJustificante || i.Estado == EstadoInscripcion.RequiereSubsanacion).Sum(i => i.ImporteEuros);

    // Listados operativos especializados
    public List<ItemSanitarioDossier> RegistrosSanitarios { get; set; } = [];
    public List<ItemIntendenciaDossier> AlertasIntendencia { get; set; } = [];
    public List<ItemContactoEmergenciaDossier> DirectorioEmergencia { get; set; } = [];
}

/// <summary>
/// Resumen sanitario individualizado para el equipo de scouters y sanitarios de campamento.
/// </summary>
public class ItemSanitarioDossier
{
    public Guid EducandoId { get; set; }
    public required string NombreEducando { get; set; }
    public RamaScout Rama { get; set; }
    public int Edad { get; set; }

    /// <summary>Falso cuando el educando no tiene ficha médica: el resto de campos clínicos quedan vacíos (no se inventan).</summary>
    public bool TieneFicha { get; set; }
    public string? NumeroSeguridadSocial { get; set; }
    public string? EntidadAseguradora { get; set; }
    public bool? SabeNadar { get; set; }
    public bool? VacunacionAlDia { get; set; }
    public string? EnfermedadesCronicas { get; set; }
    public string? MedicacionActiva { get; set; }
    public string? ObservacionesMedicas { get; set; }

    /// <summary>Alertas sanitarias relevantes (todas las alergias, crónicas, medicación y dietas) ordenadas por gravedad.</summary>
    public IReadOnlyList<AlertaSanitaria> Alertas { get; set; } = [];

    public IEnumerable<AlertaSanitaria> Alergias => Alertas.Where(a => a.EsAlergia);
    public bool TieneAlertasMedicas => Alertas.Count > 0;
}

/// <summary>
/// Resumen nutricional y de alergias alimentarias para los cocineros y equipo de intendencia.
/// </summary>
public class ItemIntendenciaDossier
{
    public required string NombreEducando { get; set; }
    public RamaScout Rama { get; set; }
    public string? AlergiasAlimentarias { get; set; }
    public string? RegimenAlimenticio { get; set; }
    public string? ObservacionesIntendencia { get; set; }
}

/// <summary>
/// Directorio telefónico de emergencia con acceso rápido para scouters.
/// </summary>
public class ItemContactoEmergenciaDossier
{
    public required string NombreEducando { get; set; }
    public RamaScout Rama { get; set; }
    public required string TutorLegalNombre { get; set; }
    public required string TelefonoPrincipal { get; set; }
    public string? ContactoPrincipalNombre { get; set; }
    public string? TelefonoSecundario { get; set; }
    public string? ContactoSecundarioNombre { get; set; }
}
