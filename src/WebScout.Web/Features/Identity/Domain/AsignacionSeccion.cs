using WebScout.Web.Common.Domain;

namespace WebScout.Web.Features.Identity.Domain;

/// <summary>
/// Función que desempeña un miembro del Kraal dentro de su sección.
/// </summary>
public enum FuncionSeccion
{
    /// <summary>Consulta el censo y revisa inscripciones de su sección.</summary>
    Scouter = 0,

    /// <summary>Además gestiona a los educandos de su sección (alta, edición, baja).</summary>
    Coordinador = 1
}

/// <summary>
/// Asignación de un miembro del Kraal a una sección (rama) concreta.
/// Es la fuente de verdad del alcance por sección: la autorización la consulta en cada operación,
/// de modo que revocarla surte efecto en la siguiente petición sin esperar a que caduque la sesión.
/// Cada persona tiene como máximo una asignación vigente (no revocada) por grupo:
/// un coordinador lo es de una única sección.
/// </summary>
public class AsignacionSeccion : IEntidadOrganizacion
{
    public Guid Id { get; set; }
    public Guid OrganizacionId { get; set; }

    public Guid UsuarioId { get; set; }
    public UsuarioScout? Usuario { get; set; }

    public RamaScout Rama { get; set; }
    public FuncionSeccion Funcion { get; set; } = FuncionSeccion.Scouter;

    public DateTime AsignadaUtc { get; set; } = DateTime.UtcNow;
    public Guid? AsignadaPorUsuarioId { get; set; }

    /// <summary>Fecha de revocación. Nula mientras la asignación está vigente.</summary>
    public DateTime? RevocadaUtc { get; set; }
    public Guid? RevocadaPorUsuarioId { get; set; }

    public bool Vigente => RevocadaUtc is null;
}
