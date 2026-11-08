using MudBlazor.Services;
using WebScout.Web.Components;
using WebScout.Web.Features.Identity;
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

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Inicializar esquema, migraciones y seed de base de datos
await WebScout.Web.Infrastructure.Persistence.DatabaseInitializer.InitializeDatabaseAsync(app.Services);

app.Run();
