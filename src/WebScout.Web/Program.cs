using WebScout.Web.Components;
using WebScout.Web.Infrastructure.Tenancy;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Registrar Tenancy multitenant y Persistencia EF Core
builder.Services.AddScoutTenancy(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

// Middleware de resolución de grupo scout por subdominio
app.UseScoutTenancy();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Inicializar esquema y seed de base de datos si PostgreSQL está activo
await WebScout.Web.Infrastructure.Persistence.DatabaseInitializer.InitializeDatabaseAsync(app.Services);

app.Run();
