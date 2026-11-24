using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WebScout.Web.Common.Domain;
using WebScout.Web.Features.Dossier.Domain;
using WebScout.Web.Features.Identity.Domain;
using WebScout.Web.Features.Identity.Services;
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
    private readonly IServicioAlcanceSeccion _alcance;

    public ServicioDossierAsistencia(
        WebScoutDbContext dbContext,
        ITenantContext tenantContext,
        ILogger<ServicioDossierAsistencia> logger,
        IServicioAlcanceSeccion alcance)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
        _logger = logger;
        _alcance = alcance;
    }

    /// <summary>
    /// Dossier de la actividad (sanitario, intendencia, emergencias) limitado a las secciones del alcance
    /// del usuario actual. También lo usa la exportación CSV, que hereda el mismo límite.
    /// </summary>
    public async Task<DossierOperativoActividad> ObtenerDossierActividadAsync(Guid actividadId)
    {
        ValidarTenantActivo();

        var alcance = await _alcance.ObtenerAlcanceAsync();
        var actividad = await _dbContext.Actividades
            .FirstOrDefaultAsync(a => a.Id == actividadId);

        if (actividad == null || !alcance.TieneAccesoKraal)
        {
            throw new InvalidOperationException("La actividad no existe o no pertenece a su grupo scout.");
        }

        var consulta = _dbContext.Inscripciones
            .AsNoTracking()
            .Include(i => i.Educando)
                .ThenInclude(e => e.FichaMedica)
            .Include(i => i.TutorLegal)
            .Include(i => i.RevisadoPorUsuario)
            .Where(i => i.ActividadId == actividadId && i.Estado != EstadoInscripcion.Cancelada);

        if (!alcance.EsGlobal)
        {
            consulta = consulta.Where(i => i.Educando.RamaActual == alcance.Rama);
        }

        var inscripciones = await consulta
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

            // 1. Registro Sanitario: sin ficha no se rellenan valores por defecto para no presentar datos inventados.
            var alertas = fm?.ObtenerAlertasSanitarias() ?? [];
            dossier.RegistrosSanitarios.Add(new ItemSanitarioDossier
            {
                EducandoId = edu.Id,
                NombreEducando = edu.NombreCompleto,
                Rama = edu.RamaActual,
                Edad = edu.CalcularEdad(),
                TieneFicha = fm != null,
                NumeroSeguridadSocial = fm?.NumeroSeguridadSocial,
                EntidadAseguradora = fm?.EntidadAseguradora,
                SabeNadar = fm?.SabeNadar,
                VacunacionAlDia = fm?.VacunacionAlDia,
                EnfermedadesCronicas = AlertasSanitarias.EsRelevante(fm?.EnfermedadesCronicas) ? fm!.EnfermedadesCronicas : null,
                MedicacionActiva = AlertasSanitarias.EsRelevante(fm?.MedicacionActiva) ? fm!.MedicacionActiva : null,
                ObservacionesMedicas = fm?.ObservacionesAdicionales,
                Alertas = alertas
            });

            // 2. Alertas de Intendencia: solo alergias alimentarias o dietas reales (no "Sin alergias conocidas").
            var alergiaAlimentaria = alertas.FirstOrDefault(a => a.Tipo == TipoAlertaSanitaria.AlergiaAlimentaria);
            var dieta = alertas.FirstOrDefault(a => a.Tipo == TipoAlertaSanitaria.Dieta);
            if (alergiaAlimentaria != null || dieta != null)
            {
                dossier.AlertasIntendencia.Add(new ItemIntendenciaDossier
                {
                    NombreEducando = edu.NombreCompleto,
                    Rama = edu.RamaActual,
                    AlergiasAlimentarias = alergiaAlimentaria?.Detalle,
                    RegimenAlimenticio = dieta?.Detalle,
                    ObservacionesIntendencia = fm!.ObservacionesAdicionales
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

        var inscripcion = await CargarInscripcionConAlcanceAsync(inscripcionId);

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

        var inscripcion = await CargarInscripcionConAlcanceAsync(inscripcionId);

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

        var inscripcion = await CargarInscripcionConAlcanceAsync(inscripcionId);

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
                ins.FechaCheckInSalidaUtc?.AHoraEspana().ToString("HH:mm") ?? "",
                ins.PresenteEnRegreso ? "SI" : "NO",
                ins.FechaCheckInRegresoUtc?.AHoraEspana().ToString("HH:mm") ?? "",
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

        // Mitigación de CSV Formula Injection (CWE-1236):
        // Desactiva la ejecución de fórmulas en hojas de cálculo (Excel, Calc) prefijando con apóstrofe
        if (limpio.StartsWith('=') || limpio.StartsWith('+') || limpio.StartsWith('-') || 
            limpio.StartsWith('@') || limpio.StartsWith('\t') || limpio.StartsWith('%'))
        {
            limpio = "'" + limpio;
        }

        if (limpio.Contains('"'))
        {
            limpio = limpio.Replace("\"", "\"\"");
        }
        return $"\"{limpio}\"";
    }

    /// <summary>
    /// Carga una inscripción para registrar asistencia solo si su educando está en el alcance del usuario;
    /// si no, responde igual que si no existiera.
    /// </summary>
    private async Task<InscripcionActividad> CargarInscripcionConAlcanceAsync(Guid inscripcionId)
    {
        var inscripcion = await _dbContext.Inscripciones
            .Include(i => i.Educando)
            .FirstOrDefaultAsync(i => i.Id == inscripcionId);

        var alcance = await _alcance.ObtenerAlcanceAsync();
        if (inscripcion == null || !alcance.PuedeVer(inscripcion.Educando.RamaActual))
        {
            throw new InvalidOperationException("La inscripción no existe o no pertenece a su grupo scout.");
        }

        return inscripcion;
    }

    private void ValidarTenantActivo()
    {
        if (!_tenantContext.IsResolved || _tenantContext.CurrentTenantId == Guid.Empty)
        {
            throw new InvalidOperationException("Operación no permitida sin una Organización activa.");
        }
    }
}
