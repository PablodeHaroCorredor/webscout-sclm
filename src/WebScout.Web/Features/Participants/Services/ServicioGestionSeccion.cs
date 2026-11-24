using Microsoft.EntityFrameworkCore;
using WebScout.Web.Features.Audit.Services;
using WebScout.Web.Features.Identity.Services;
using WebScout.Web.Features.Participants.Domain;
using WebScout.Web.Infrastructure.Persistence;

namespace WebScout.Web.Features.Participants.Services;

/// <summary>Datos del educando que puede corregir quien gestiona su sección. La rama y la ficha médica quedan fuera.</summary>
public sealed record DatosEducandoEditables(
    string Nombre,
    string Apellidos,
    DateOnly FechaNacimiento,
    string? Dni,
    string? Sexo);

/// <summary>
/// Gestión de los educandos de una sección: corregir datos, dar de baja y reactivar.
/// Reservada al Coordinador de esa sección y a quien tiene alcance global (Coordinación, Administración).
/// </summary>
public interface IServicioGestionSeccion
{
    /// <summary>Educandos dados de baja visibles para el usuario actual (su sección o todas con alcance global).</summary>
    Task<IReadOnlyList<Educando>> ObtenerBajasAsync();

    Task ActualizarDatosAsync(Guid educandoId, DatosEducandoEditables datos);

    Task DarDeBajaAsync(Guid educandoId, string motivo);

    Task ReactivarAsync(Guid educandoId);
}

public sealed class ServicioGestionSeccion(
    WebScoutDbContext db,
    IServicioAlcanceSeccion servicioAlcance,
    IServicioAuditoria auditoria) : IServicioGestionSeccion
{
    // Misma respuesta para «no existe» y «no es de su sección»: no se confirma la existencia de educandos ajenos
    private const string NoDisponible = "El educando no existe o no pertenece a su sección.";

    public async Task<IReadOnlyList<Educando>> ObtenerBajasAsync()
    {
        var alcance = await servicioAlcance.ObtenerAlcanceAsync();
        if (!alcance.TieneAccesoKraal)
        {
            return [];
        }

        var consulta = db.Educandos.AsNoTracking()
            .Include(e => e.TutorLegal)
            .Where(e => !e.Activo);

        if (!alcance.EsGlobal)
        {
            consulta = consulta.Where(e => e.RamaActual == alcance.Rama);
        }

        return await consulta
            .OrderBy(e => e.RamaActual)
            .ThenBy(e => e.Apellidos)
            .ThenBy(e => e.Nombre)
            .ToListAsync();
    }

    public async Task ActualizarDatosAsync(Guid educandoId, DatosEducandoEditables datos)
    {
        Validar(datos);
        var educando = await CargarGestionableAsync(educandoId);

        educando.Nombre = datos.Nombre.Trim();
        educando.Apellidos = datos.Apellidos.Trim();
        educando.FechaNacimiento = datos.FechaNacimiento;
        educando.Dni = string.IsNullOrWhiteSpace(datos.Dni) ? null : datos.Dni.Trim().ToUpperInvariant();
        educando.Sexo = string.IsNullOrWhiteSpace(datos.Sexo) ? null : datos.Sexo.Trim();
        educando.FechaActualizacionUtc = DateTime.UtcNow;

        await db.SaveChangesAsync();
        // Sin datos personales en el detalle: el registro identifica el recurso por su Id
        await auditoria.RegistrarAsync("EducandoDatosActualizados", nameof(Educando), educando.Id.ToString(),
            $"Rama: {educando.RamaActual}");
    }

    public async Task DarDeBajaAsync(Guid educandoId, string motivo)
    {
        if (string.IsNullOrWhiteSpace(motivo))
        {
            throw new ArgumentException("Indique el motivo de la baja.", nameof(motivo));
        }

        var educando = await CargarGestionableAsync(educandoId);
        if (!educando.Activo)
        {
            return;
        }

        educando.Activo = false;
        educando.FechaActualizacionUtc = DateTime.UtcNow;

        await db.SaveChangesAsync();
        await auditoria.RegistrarAsync("EducandoBaja", nameof(Educando), educando.Id.ToString(),
            $"Rama: {educando.RamaActual}. Motivo: {Recortar(motivo.Trim(), 500)}");
    }

    public async Task ReactivarAsync(Guid educandoId)
    {
        var educando = await CargarGestionableAsync(educandoId);
        if (educando.Activo)
        {
            return;
        }

        educando.Activo = true;
        educando.FechaActualizacionUtc = DateTime.UtcNow;

        await db.SaveChangesAsync();
        await auditoria.RegistrarAsync("EducandoReactivado", nameof(Educando), educando.Id.ToString(),
            $"Rama: {educando.RamaActual}");
    }

    /// <summary>Carga el educando solo si el usuario actual puede gestionar su sección.</summary>
    private async Task<Educando> CargarGestionableAsync(Guid educandoId)
    {
        var educando = await db.Educandos.FirstOrDefaultAsync(e => e.Id == educandoId);
        var alcance = await servicioAlcance.ObtenerAlcanceAsync();

        if (educando == null || !alcance.PuedeGestionar(educando.RamaActual))
        {
            throw new InvalidOperationException(NoDisponible);
        }

        return educando;
    }

    private static void Validar(DatosEducandoEditables datos)
    {
        if (string.IsNullOrWhiteSpace(datos.Nombre) || datos.Nombre.Trim().Length > 100)
        {
            throw new ArgumentException("El nombre es obligatorio (máximo 100 caracteres).");
        }

        if (string.IsNullOrWhiteSpace(datos.Apellidos) || datos.Apellidos.Trim().Length > 150)
        {
            throw new ArgumentException("Los apellidos son obligatorios (máximo 150 caracteres).");
        }

        if (datos.FechaNacimiento >= DateOnly.FromDateTime(DateTime.UtcNow))
        {
            throw new ArgumentException("La fecha de nacimiento debe ser anterior a hoy.");
        }

        if (datos.Dni?.Trim().Length > 20 || datos.Sexo?.Trim().Length > 20)
        {
            throw new ArgumentException("El DNI y el sexo admiten como máximo 20 caracteres.");
        }
    }

    private static string Recortar(string texto, int maximo) => texto.Length <= maximo ? texto : texto[..maximo];
}
