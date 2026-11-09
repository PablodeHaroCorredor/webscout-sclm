using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WebScout.Web.Common.Domain;
using WebScout.Web.Features.Dossier.Domain;
using WebScout.Web.Features.Identity.Domain;
using WebScout.Web.Features.Inscriptions.Domain;
using WebScout.Web.Infrastructure.Persistence;
using WebScout.Web.Infrastructure.Tenancy;

namespace WebScout.Web.Features.Dossier.Services;

/// <summary>
/// Implementación del servicio de dossier operativo de campamentos, fichero sanitario y control de asistencia.
/// </summary>
public class ServicioDossierAsistencia : IServicioDossierAsistencia
{
    private readonly WebScoutDbContext _dbContext;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<ServicioDossierAsistencia> _logger;

    public ServicioDossierAsistencia(
        WebScoutDbContext dbContext,
        ITenantContext tenantContext,
        ILogger<ServicioDossierAsistencia> logger)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<DossierOperativoActividad> ObtenerDossierActividadAsync(Guid actividadId)
    {
        ValidarTenantActivo();

        var actividad = await _dbContext.Actividades
            .FirstOrDefaultAsync(a => a.Id == actividadId);

        if (actividad == null)
        {
            throw new InvalidOperationException("La actividad no existe o no pertenece a su grupo scout.");
        }

        var inscripciones = await _dbContext.Inscripciones
            .AsNoTracking()
            .Include(i => i.Educando)
                .ThenInclude(e => e.FichaMedica)
            .Include(i => i.TutorLegal)
            .Include(i => i.RevisadoPorUsuario)
            .Where(i => i.ActividadId == actividadId && i.Estado != EstadoInscripcion.Cancelada)
            .OrderBy(i => i.Educando.RamaActual)
            .ThenBy(i => i.Educando.Apellidos)
            .ToListAsync();

        var dossier = new DossierOperativoActividad
        {
            Actividad = actividad,
            Inscripciones = inscripciones
        };

        foreach (var ins in inscripciones)
        {
            var edu = ins.Educando;
            var fm = edu.FichaMedica;
            var tutor = ins.TutorLegal;

            // 1. Registro Sanitario
            dossier.RegistrosSanitarios.Add(new ItemSanitarioDossier
            {
                EducandoId = edu.Id,
                NombreEducando = edu.NombreCompleto,
                Rama = edu.RamaActual,
                Edad = edu.CalcularEdad(),
                NumeroSeguridadSocial = fm?.NumeroSeguridadSocial ?? "No registrada",
                EntidadAseguradora = fm?.EntidadAseguradora ?? "SESCAM",
                SabeNadar = fm?.SabeNadar ?? false,
                VacunacionAlDia = fm?.VacunacionAlDia ?? false,
                EnfermedadesCronicas = fm?.EnfermedadesCronicas,
                MedicacionActiva = fm?.MedicacionActiva,
                ObservacionesMedicas = fm?.ObservacionesAdicionales
            });

            // 2. Alertas de Intendencia / Alergias Alimentarias
            if (fm != null && (!string.IsNullOrWhiteSpace(fm.AlergiasAlimentarias) || !string.IsNullOrWhiteSpace(fm.RegimenAlimenticio)))
            {
                dossier.AlertasIntendencia.Add(new ItemIntendenciaDossier
                {
                    NombreEducando = edu.NombreCompleto,
                    Rama = edu.RamaActual,
                    AlergiasAlimentarias = fm.AlergiasAlimentarias,
                    RegimenAlimenticio = fm.RegimenAlimenticio,
                    ObservacionesIntendencia = fm.ObservacionesAdicionales
                });
            }

            // 3. Directorio de Emergencia
            dossier.DirectorioEmergencia.Add(new ItemContactoEmergenciaDossier
            {
                NombreEducando = edu.NombreCompleto,
                Rama = edu.RamaActual,
                TutorLegalNombre = tutor.NombreCompleto,
                TelefonoPrincipal = fm?.TelefonoEmergenciaPrincipal ?? tutor.PhoneNumber ?? "No disponible",
                ContactoPrincipalNombre = fm?.NombreContactoEmergenciaPrincipal ?? tutor.NombreCompleto,
                TelefonoSecundario = fm?.TelefonoEmergenciaSecundario,
                ContactoSecundarioNombre = fm?.NombreContactoEmergenciaSecundario
            });
        }

        _logger.LogInformation(
            "Dossier generado para actividad {ActividadId} ({Titulo}): {Count} educandos inscritos.",
            actividad.Id, actividad.Titulo, inscripciones.Count);

        return dossier;
    }

    public async Task RegistrarCheckInSalidaAsync(Guid inscripcionId, bool presente)
    {
        ValidarTenantActivo();

        var inscripcion = await _dbContext.Inscripciones
            .FirstOrDefaultAsync(i => i.Id == inscripcionId);

        if (inscripcion == null)
        {
            throw new InvalidOperationException("La inscripción no existe o no pertenece a su grupo scout.");
        }

        inscripcion.PresenteEnSalida = presente;
        inscripcion.FechaCheckInSalidaUtc = presente ? DateTime.UtcNow : null;

        await _dbContext.SaveChangesAsync();

        _logger.LogInformation(
            "Check-in de salida para inscripción {InscripcionId}: Presente={Presente}",
            inscripcionId, presente);
    }

    public async Task RegistrarCheckInRegresoAsync(Guid inscripcionId, bool presente)
    {
        ValidarTenantActivo();

        var inscripcion = await _dbContext.Inscripciones
            .FirstOrDefaultAsync(i => i.Id == inscripcionId);

        if (inscripcion == null)
        {
            throw new InvalidOperationException("La inscripción no existe o no pertenece a su grupo scout.");
        }

        inscripcion.PresenteEnRegreso = presente;
        inscripcion.FechaCheckInRegresoUtc = presente ? DateTime.UtcNow : null;

        await _dbContext.SaveChangesAsync();

        _logger.LogInformation(
            "Check-in de regreso para inscripción {InscripcionId}: Entregado={Presente}",
            inscripcionId, presente);
    }

    public async Task ActualizarObservacionAsistenciaAsync(Guid inscripcionId, string? observacion)
    {
        ValidarTenantActivo();

        var inscripcion = await _dbContext.Inscripciones
            .FirstOrDefaultAsync(i => i.Id == inscripcionId);

        if (inscripcion == null)
        {
            throw new InvalidOperationException("La inscripción no existe o no pertenece a su grupo scout.");
        }

        inscripcion.ObservacionesAsistencia = observacion;
        await _dbContext.SaveChangesAsync();
    }

    public async Task<string> GenerarCsvAsistenciaAsync(Guid actividadId)
    {
        var dossier = await ObtenerDossierActividadAsync(actividadId);

        var sb = new StringBuilder();
        // Encabezados con separador de lista estándar para Excel en español (punto y coma)
        sb.AppendLine("Educando;Rama;Edad;Tutor Legal;Telefono Emergencia Principal;Salida Presente;Hora Salida;Regreso Entregado;Hora Regreso;Estado Inscripcion;Importe Euros;Alergias Alimentarias;Medicacion Activa;Observaciones Asistencia");

        foreach (var ins in dossier.Inscripciones)
        {
            var edu = ins.Educando;
            var fm = edu.FichaMedica;
            var tutor = ins.TutorLegal;

            var linea = string.Join(";",
                SanitizarCsv(edu.NombreCompleto),
                SanitizarCsv(edu.RamaActual.ObtenerNombre()),
                edu.CalcularEdad().ToString(),
                SanitizarCsv(tutor.NombreCompleto),
                SanitizarCsv(fm?.TelefonoEmergenciaPrincipal ?? tutor.PhoneNumber ?? ""),
                ins.PresenteEnSalida ? "SI" : "NO",
                ins.FechaCheckInSalidaUtc?.ToLocalTime().ToString("HH:mm") ?? "",
                ins.PresenteEnRegreso ? "SI" : "NO",
                ins.FechaCheckInRegresoUtc?.ToLocalTime().ToString("HH:mm") ?? "",
                ins.Estado.ToString(),
                ins.ImporteEuros.ToString("N2"),
                SanitizarCsv(fm?.AlergiasAlimentarias ?? "Ninguna"),
                SanitizarCsv(fm?.MedicacionActiva ?? "Ninguna"),
                SanitizarCsv(ins.ObservacionesAsistencia ?? "")
            );

            sb.AppendLine(linea);
        }

        return sb.ToString();
    }

    private static string SanitizarCsv(string texto)
    {
        if (string.IsNullOrWhiteSpace(texto)) return "";
        var limpio = texto.Replace(";", ",").Replace("\r", " ").Replace("\n", " ").Trim();
        if (limpio.Contains('"'))
        {
            limpio = limpio.Replace("\"", "\"\"");
        }
        return $"\"{limpio}\"";
    }

    private void ValidarTenantActivo()
    {
        if (!_tenantContext.IsResolved || _tenantContext.CurrentTenantId == Guid.Empty)
        {
            throw new InvalidOperationException("Operación no permitida sin una Organización activa.");
        }
    }
}
