using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using WebScout.Web.Features.Identity;
using WebScout.Web.Infrastructure.Security;
using WebScout.Web.Infrastructure.Tenancy;
using Xunit;

namespace WebScout.IntegrationTests;

/// <summary>
/// Protecciones que dependen del entorno: el acceso rápido sin contraseña y la clave de cifrado
/// de datos de salud no pueden llegar a un entorno distinto de Development.
/// </summary>
public class ProteccionPorEntornoTests
{
    private const string ClaveProduccionValida = "q7N2xV9pL4sT8wK1mB6zR3cF5hJ0dG2aE9uY4iO7tQ=";

    // ---- Acceso rápido (/auth/quick-login) ----

    [Theory]
    [InlineData("Production")]
    [InlineData("Staging")]
    public async Task AccesoRapido_FueraDeDevelopment_NoSeRegistra(string entorno)
    {
        var rutas = await ObtenerRutasIdentidadAsync(entorno);

        Assert.Contains(rutas, r => r.EndsWith("/login", StringComparison.Ordinal));
        Assert.DoesNotContain(rutas, r => r.Contains("quick-login", StringComparison.Ordinal));
    }

    [Fact]
    public async Task AccesoRapido_EnDevelopment_SeRegistra()
    {
        var rutas = await ObtenerRutasIdentidadAsync(Environments.Development);

        Assert.Contains(rutas, r => r.EndsWith("/quick-login", StringComparison.Ordinal));
    }

    // ---- Redirección tras login (returnUrl) ----

    [Theory]
    [InlineData("/familias", true)]
    [InlineData("/kraal/inscripciones?rama=Manada", true)]
    [InlineData("/", true)]
    [InlineData("//evil.example", false)]
    [InlineData("/\\evil.example", false)]
    [InlineData("https://evil.example/familias", false)]
    [InlineData("familias", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void RutaDeRetorno_SoloAceptaRutasLocales(string? url, bool esperado)
    {
        Assert.Equal(esperado, IdentityEndpoints.EsRutaLocalSegura(url));
    }

    // ---- Clave de cifrado de datos de salud ----

    [Fact]
    public void Cifrador_EnProduccionSinClave_ImpideArrancar()
    {
        var ex = Assert.Throws<InvalidOperationException>(() =>
            CifradorDatosSensibles.DesdeConfiguracion(Configuracion(null), new EntornoPrueba("Production")));

        Assert.Contains(CifradorDatosSensibles.ClaveConfiguracion, ex.Message);
    }

    [Fact]
    public void Cifrador_EnProduccionConClavePublicaDeDesarrollo_ImpideArrancar()
    {
        Assert.Throws<InvalidOperationException>(() =>
            CifradorDatosSensibles.DesdeConfiguracion(
                Configuracion(CifradorDatosSensibles.ClaveSoloDesarrollo), new EntornoPrueba("Production")));
    }

    [Fact]
    public void Cifrador_EnProduccionConClaveCorta_ImpideArrancar()
    {
        Assert.Throws<InvalidOperationException>(() =>
            CifradorDatosSensibles.DesdeConfiguracion(Configuracion("demasiado-corta"), new EntornoPrueba("Staging")));
    }

    [Fact]
    public void Cifrador_EnProduccionConClavePropia_NoDescifraDatosDeLaClaveDeDesarrollo()
    {
        var cifradorProduccion = CifradorDatosSensibles.DesdeConfiguracion(
            Configuracion(ClaveProduccionValida), new EntornoPrueba("Production"));
        const string dato = "Alergia grave a frutos secos";

        Assert.Equal(dato, cifradorProduccion.Descifrar(cifradorProduccion.Cifrar(dato)));

        var cifradoConClavePublica = CifradorDatosSensibles.ParaDesarrollo().Cifrar(dato);
        Assert.NotEqual(dato, cifradorProduccion.Descifrar(cifradoConClavePublica));
    }

    [Fact]
    public void Cifrador_EnDevelopmentSinClave_DescifraLosDatosSembradosPorVersionesAnteriores()
    {
        // Cadena usada como clave por defecto antes de este cambio: las bases locales ya sembradas dependen de ella.
        var cifradorAnterior = new CifradorDatosSensibles("WebScoutSCLM_MasterEncryptionKey_2026_SecureRGPDDefaultKey!");
        var cifradoAnterior = cifradorAnterior.Cifrar("Celiaquía");

        var cifradorDesarrollo = CifradorDatosSensibles.DesdeConfiguracion(
            Configuracion(null), new EntornoPrueba(Environments.Development));

        Assert.Equal("Celiaquía", cifradorDesarrollo.Descifrar(cifradoAnterior));
    }

    // ---- Utilidades ----

    private static async Task<List<string>> ObtenerRutasIdentidadAsync(string entorno)
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = entorno });
        builder.Services.AddScoutTenancy(builder.Configuration);
        builder.Services.AddScoutIdentity();

        await using var app = builder.Build();
        app.MapIdentityEndpoints();

        return ((IEndpointRouteBuilder)app).DataSources
            .SelectMany(ds => ds.Endpoints)
            .OfType<RouteEndpoint>()
            .Select(e => e.RoutePattern.RawText ?? string.Empty)
            .ToList();
    }

    private static IConfiguration Configuracion(string? clave) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [CifradorDatosSensibles.ClaveConfiguracion] = clave
            })
            .Build();

    private sealed class EntornoPrueba(string nombre) : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = nombre;
        public string ApplicationName { get; set; } = "WebScout.Web";
        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
