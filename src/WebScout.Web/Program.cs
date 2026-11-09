using MudBlazor.Services;
using WebScout.Web.Components;
using WebScout.Web.Features.Activities;
using WebScout.Web.Features.Dossier;
using WebScout.Web.Features.Dossier.Endpoints;
using WebScout.Web.Features.Identity;
using WebScout.Web.Features.Inscriptions;
using WebScout.Web.Features.Inscriptions.Endpoints;
using WebScout.Web.Features.Participants;
using WebScout.Web.Infrastructure.Tenancy;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Registrar MudBlazor y componentes UI
builder.Services.AddMudServices();

// Registrar Tenancy multitenant y Persistencia EF Core
builder.Services.AddScoutTenancy(builder.Configuration);

// Registrar Identity, Roles y Cookies de Autenticación
builder.Services.AddScoutIdentity();

// Registrar Dominio de Educandos y Salud
builder.Services.AddParticipantFeatures();

// Registrar Dominio de Actividades y Campamentos
builder.Services.AddActivityFeatures();

// Registrar Dominio de Inscripciones y Justificantes Bancarios
builder.Services.AddInscriptionFeatures();

// Registrar Dominio de Dossier Operativo y Asistencia
builder.Services.AddDossierFeatures();

var app = builder.Build();

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

app.UseAntiforgery();

app.MapStaticAssets();

// Endpoints de autenticación (Login, Logout, QuickLogin)
app.MapIdentityEndpoints();

// Endpoints de descarga segura de justificantes bancarios
app.MapJustificanteEndpoints();

// Endpoints de exportación de asistencia
app.MapDossierEndpoints();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Inicializar esquema, migraciones y seed de base de datos
await WebScout.Web.Infrastructure.Persistence.DatabaseInitializer.InitializeDatabaseAsync(app.Services);

app.Run();
