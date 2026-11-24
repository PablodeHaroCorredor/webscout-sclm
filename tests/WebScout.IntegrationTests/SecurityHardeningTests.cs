using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging.Abstractions;
using Npgsql;
using WebScout.Web.Features.Audit.Domain;
using WebScout.Web.Features.Audit.Services;
using WebScout.Web.Features.Identity.Domain;
using WebScout.Web.Features.Inscriptions.Storage;
using WebScout.Web.Features.Participants.Domain;
using WebScout.Web.Infrastructure.Persistence;
using WebScout.Web.Infrastructure.Security;
using WebScout.Web.Infrastructure.Tenancy;
using Xunit;

namespace WebScout.IntegrationTests;

public class SecurityHardeningTests : IDisposable
{
    private readonly BaseDatosPostgresPruebas _baseDatos;
    private readonly DbContextOptions<WebScoutDbContext> _options;
    private readonly string _testTempDir;

    private static readonly Guid OrgLestonnac = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid OrgSanVicente = Guid.Parse("22222222-2222-2222-2222-222222222222");

    public SecurityHardeningTests()
    {
        _baseDatos = BaseDatosPostgresPruebas.Crear();

        _options = _baseDatos.Opciones;

        _testTempDir = Path.Combine(Path.GetTempPath(), "WebScout_SecurityTests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_testTempDir);
    }

    [Fact]
    public void CifradorDatosSensibles_ShouldEncryptWithVersionPrefixAndDecryptSuccessfully()
    {
        var cifrador = new CifradorDatosSensibles("ClavePrueba1234567890123456789012!");
        var textoOriginal = "Alergia mortal a los cacahuetes y frutos secos";

        var cifrado = cifrador.Cifrar(textoOriginal);

        Assert.NotNull(cifrado);
        Assert.StartsWith("enc:v1:", cifrado);
        Assert.NotEqual(textoOriginal, cifrado);

        var descifrado = cifrador.Descifrar(cifrado);
        Assert.Equal(textoOriginal, descifrado);
    }

    [Fact]
    public void CifradorDatosSensibles_ShouldPreserveUnencryptedLegacyData()
    {
        var cifrador = new CifradorDatosSensibles();
        var textoHistorico = "Sin alergias alimentarias registradas";

        var resultado = cifrador.Descifrar(textoHistorico);

        Assert.Equal(textoHistorico, resultado);
    }

    [Fact]
    public async Task FichaMedica_HealthFields_ShouldBePersistedEncryptedInDb()
    {
        var ctx = new TenantContext();
        ctx.SetTenant(OrgLestonnac, "lestonnac", "Lestonnac 330", 330);

        var educandoId = Guid.NewGuid();
        var fichaId = Guid.NewGuid();

        // 1. Guardar ficha con datos médicos vinculada a un educando y tutor válido
        using (var db = new WebScoutDbContext(_options, ctx))
        {
            var tutor = new UsuarioScout
            {
                Id = Guid.NewGuid(),
                OrganizacionId = OrgLestonnac,
                UserName = "tutor@lestonnac.es",
                Email = "tutor@lestonnac.es",
                NombreCompleto = "Tutor Prueba"
            };
            db.Users.Add(tutor);

            var educando = new Educando
            {
                Id = educandoId,
                OrganizacionId = OrgLestonnac,
                TutorLegalId = tutor.Id,
                Nombre = "Lucía",
                Apellidos = "García",
                FechaNacimiento = new DateOnly(2015, 5, 10),
                Sexo = "Femenino",
                RamaActual = RamaScout.Manada,
                Activo = true
            };
            db.Educandos.Add(educando);

            var ficha = new FichaMedica
            {
                Id = fichaId,
                OrganizacionId = OrgLestonnac,
                EducandoId = educandoId,
                NumeroSeguridadSocial = "CIP-CLM-998877",
                AlergiasAlimentarias = "Alérgica al marisco",
                EnfermedadesCronicas = "Asma bronquial",
                MedicacionActiva = "Ventolín",
                TelefonoEmergenciaPrincipal = "+34 600 111 222"
            };

            db.FichasMedicas.Add(ficha);
            await db.SaveChangesAsync();
        }

        // 2. Verificar que al leer a bajo nivel desde la base de datos, las columnas están cifradas con "enc:v1:"
        await using (var conexion = new NpgsqlConnection(_baseDatos.CadenaConexion))
        {
            await conexion.OpenAsync();
            await using var cmd = conexion.CreateCommand();
            cmd.CommandText = "SELECT \"NumeroSeguridadSocial\", \"AlergiasAlimentarias\", \"EnfermedadesCronicas\" FROM \"FichasMedicas\" LIMIT 1";

            using var reader = await cmd.ExecuteReaderAsync();
            Assert.True(await reader.ReadAsync());

            var rawNss = reader.GetString(0);
            var rawAlergias = reader.GetString(1);
            var rawEnfermedades = reader.GetString(2);

            Assert.StartsWith("enc:v1:", rawNss);
            Assert.StartsWith("enc:v1:", rawAlergias);
            Assert.StartsWith("enc:v1:", rawEnfermedades);
            Assert.DoesNotContain("CIP-CLM-998877", rawNss);
            Assert.DoesNotContain("marisco", rawAlergias);
        }

        // 3. Verificar que EF Core descifra automáticamente y de forma transparente para la aplicación
        using (var db = new WebScoutDbContext(_options, ctx))
        {
            var fichaLeida = await db.FichasMedicas.FindAsync(fichaId);
            Assert.NotNull(fichaLeida);
            Assert.Equal("CIP-CLM-998877", fichaLeida.NumeroSeguridadSocial);
            Assert.Equal("Alérgica al marisco", fichaLeida.AlergiasAlimentarias);
            Assert.Equal("Asma bronquial", fichaLeida.EnfermedadesCronicas);
            Assert.Equal("Ventolín", fichaLeida.MedicacionActiva);
        }
    }

    [Fact]
    public async Task TenantSessionGuardMiddleware_ShouldBlockCrossTenantApiRequests()
    {
        var middleware = new TenantSessionGuardMiddleware(
            _ => Task.CompletedTask,
            NullLogger<TenantSessionGuardMiddleware>.Instance);

        var context = new DefaultHttpContext();
        context.Request.Path = "/api/justificantes/descargar";

        // Usuario autenticado perteneciente a San Vicente
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new Claim("OrganizacionId", OrgSanVicente.ToString()),
            new Claim(ClaimTypes.Role, RolesScout.Scouter)
        };
        context.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));

        // Subdominio activo es Lestonnac
        var tenantContext = new TenantContext();
        tenantContext.SetTenant(OrgLestonnac, "lestonnac", "Lestonnac 330", 330);

        await middleware.InvokeAsync(context, tenantContext);

        Assert.Equal(StatusCodes.Status403Forbidden, context.Response.StatusCode);
    }

    [Fact]
    public async Task TenantSessionGuardMiddleware_ShouldAllowSuperAdminAcrossAnyTenant()
    {
        var nextExecuted = false;
        var middleware = new TenantSessionGuardMiddleware(
            _ => { nextExecuted = true; return Task.CompletedTask; },
            NullLogger<TenantSessionGuardMiddleware>.Instance);

        var context = new DefaultHttpContext();
        context.Request.Path = "/api/grupos";

        // SuperAdmin federativo (OrganizacionId vacía o cualquiera)
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Role, RolesScout.SuperAdmin)
        };
        context.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));

        var tenantContext = new TenantContext();
        tenantContext.SetTenant(OrgLestonnac, "lestonnac", "Lestonnac 330", 330);
        tenantContext.SetSuperAdmin(true);

        await middleware.InvokeAsync(context, tenantContext);

        Assert.True(nextExecuted);
        Assert.NotEqual(StatusCodes.Status403Forbidden, context.Response.StatusCode);
    }

    [Fact]
    public async Task AlmacenamientoJustificantesLocal_ShouldRejectFilesWithSpoofedExtension()
    {
        var env = new TestWebHostEnvironment { ContentRootPath = _testTempDir };

        var almacenamiento = new AlmacenamientoJustificantesLocal(
            env,
            NullLogger<AlmacenamientoJustificantesLocal>.Instance);

        // Archivo que declara ser PDF pero su contenido binario es texto o script plano (sin %PDF-)
        var contenidoFalso = Encoding.UTF8.GetBytes("echo 'malicious script'");
        using var streamFalso = new MemoryStream(contenidoFalso);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            almacenamiento.GuardarJustificanteAsync(
                OrgLestonnac,
                Guid.NewGuid(),
                "justificante.pdf",
                streamFalso));

        Assert.Contains("contenido binario", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task AlmacenamientoJustificantesLocal_ShouldAcceptValidPdfFile()
    {
        var env = new TestWebHostEnvironment { ContentRootPath = _testTempDir };

        var almacenamiento = new AlmacenamientoJustificantesLocal(
            env,
            NullLogger<AlmacenamientoJustificantesLocal>.Instance);

        // Archivo con cabecera %PDF- válida
        var contenidoValido = Encoding.UTF8.GetBytes("%PDF-1.4\n1 0 obj\n<<>>\nendobj\ntrailer\n<<>>\nstartxref\n10\n%%EOF");
        using var streamValido = new MemoryStream(contenidoValido);

        var rutaRelativa = await almacenamiento.GuardarJustificanteAsync(
            OrgLestonnac,
            Guid.NewGuid(),
            "recibo_bancario.pdf",
            streamValido);

        Assert.NotNull(rutaRelativa);
        Assert.Contains(OrgLestonnac.ToString(), rutaRelativa);
    }

    [Fact]
    public async Task ServicioAuditoria_ShouldPersistAuditEventInDatabase()
    {
        var ctx = new TenantContext();
        ctx.SetTenant(OrgLestonnac, "lestonnac", "Lestonnac 330", 330);

        var httpContext = new DefaultHttpContext();
        var scouterId = Guid.NewGuid();
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, scouterId.ToString()),
            new Claim(ClaimTypes.Email, "scouter.manada@scoutslestonnac.es")
        };
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
        httpContext.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("192.168.1.100");

        var accessor = new HttpContextAccessor { HttpContext = httpContext };

        using var db = new WebScoutDbContext(_options, ctx);
        var auditoria = new ServicioAuditoria(
            db,
            ctx,
            accessor,
            NullLogger<ServicioAuditoria>.Instance);

        var inscripcionId = Guid.NewGuid().ToString();
        await auditoria.RegistrarAsync(
            accion: "DescargaJustificante",
            tipoRecurso: "InscripcionActividad",
            recursoId: inscripcionId,
            detalles: "Descarga de comprobante de pago");

        var evento = await db.RegistrosAuditoria.FirstOrDefaultAsync(a => a.RecursoId == inscripcionId);
        Assert.NotNull(evento);
        Assert.Equal("DescargaJustificante", evento.Accion);
        Assert.Equal("InscripcionActividad", evento.TipoRecurso);
        Assert.Equal(OrgLestonnac, evento.OrganizacionId);
        Assert.Equal(scouterId, evento.UsuarioId);
        Assert.Equal("scouter.manada@scoutslestonnac.es", evento.EmailUsuario);
        Assert.Equal("192.168.1.100", evento.DireccionIp);
    }

    public void Dispose()
    {
        _baseDatos.Dispose();
        if (Directory.Exists(_testTempDir))
        {
            try { Directory.Delete(_testTempDir, true); } catch { }
        }
    }

    private class TestWebHostEnvironment : IWebHostEnvironment
    {
        public string ContentRootPath { get; set; } = string.Empty;
        public string WebRootPath { get; set; } = string.Empty;
        public IFileProvider WebRootFileProvider { get; set; } = null!;
        public string EnvironmentName { get; set; } = "Testing";
        public string ApplicationName { get; set; } = "WebScout";
        public IFileProvider ContentRootFileProvider { get; set; } = null!;
    }
}

