using MudBlazor.Services;
using WebScout.Web.Components;
using WebScout.Web.Features.Activities;
using WebScout.Web.Features.Administration;
using WebScout.Web.Features.Admissions;
using WebScout.Web.Features.Audit;
using WebScout.Web.Features.Dossier;
using WebScout.Web.Features.Dossier.Endpoints;
using WebScout.Web.Features.Identity;
using WebScout.Web.Features.Inscriptions;
using WebScout.Web.Features.Inscriptions.Endpoints;
using WebScout.Web.Features.Orders;
using WebScout.Web.Features.Orders.Endpoints;
using WebScout.Web.Features.Participants;
using WebScout.Web.Infrastructure.Outbox;
using WebScout.Web.Infrastructure.Security;
using WebScout.Web.Infrastructure.Tenancy;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Registrar MudBlazor y componentes UI
builder.Services.AddMudServices();
builder.Services.AddScoped<WebScout.Web.Infrastructure.Navigation.UserContextModeService>();

// Registrar Tenancy multitenant y Persistencia EF Core
builder.Services.AddScoutTenancy(builder.Configuration);

// Registrar Identity, Roles y Cookies de Autenticación
builder.Services.AddScoutIdentity();

// Registrar Servicios de Seguridad y Cifrado RGPD (Art. 9)
builder.Services.AddSingleton<IServicioCifrado, CifradorDatosSensibles>();
builder.Services.AddScoutAudit();

// Registrar Dominio de Educandos y Salud
builder.Services.AddParticipantFeatures();

// Registrar Dominio de Actividades y Campamentos
builder.Services.AddActivityFeatures();

// Registrar Dominio de Inscripciones y Justificantes Bancarios
builder.Services.AddInscriptionFeatures();

// Registrar Dominio de Dossier Operativo y Asistencia
builder.Services.AddDossierFeatures();

// Registrar Dominio de Admisiones y Contacto Público (BL03)
builder.Services.AddAdmissionFeatures();

// Registrar Dominio de Administración, Configuración y Diagnóstico (BL10)
builder.Services.AddAdministrationFeatures();

// Registrar Dominio de Pedidos y Catálogo de Material Scout (BL07)
builder.Services.AddOrderFeatures();

// Registrar Motor Outbox Transaccional y Transporte SMTP (ADR07 / PA11)
builder.Services.AddOutboxInfrastructure(builder.Configuration);

var app = builder.Build();

// Cabeceras HTTP de Seguridad Defensiva (OWASP / Hardening)
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    context.Response.Headers.Append("Permissions-Policy", "camera=(), microphone=(), geolocation=()");
    await next();
});

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

// Middleware de resolución de grupo scout por subdominio
app.UseScoutTenancy();

app.UseAuthentication();
app.UseAuthorization();

// Guardia de sesión cross-tenant: previene manipulación o reutilización de cookies entre grupos
app.UseTenantSessionGuard();

app.UseAntiforgery();

app.MapStaticAssets();

// Endpoints de autenticación (Login, Logout, QuickLogin)
app.MapIdentityEndpoints();

// Endpoints de descarga segura de justificantes bancarios
app.MapJustificanteEndpoints();

// Endpoints de exportación de asistencia
app.MapDossierEndpoints();

// Endpoints de pedidos y aprovisionamiento de material scout (BL07)
app.MapOrderEndpoints();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Inicializar esquema, migraciones y seed de base de datos
await WebScout.Web.Infrastructure.Persistence.DatabaseInitializer.InitializeDatabaseAsync(app.Services);

app.Run();
