using Microsoft.AspNetCore.Http;
using WebScout.Web.Common.Domain;
using WebScout.Web.Common.Wcag;
using WebScout.Web.Infrastructure.Tenancy;
using Xunit;

namespace WebScout.IntegrationTests;

public class PublicPortalTests
{
    [Fact]
    public void TenantContext_ResolucionLestonnac_AsignaPaletaCorrecta()
    {
        var tenantContext = new TenantContext();
        tenantContext.SetTenant(
            TenantContext.OrgLestonnac,
            "lestonnac",
            "Grupo Scout Lestonnac 330",
            330,
            PaletteConfig.DefaultLestonnac);

        Assert.NotNull(tenantContext.Palette);
        Assert.Equal("#1B5E20", tenantContext.Palette.PrimaryHex);
        Assert.Equal("#FBC02D", tenantContext.Palette.SecondaryHex);
        Assert.Equal("#FFFFFF", tenantContext.Palette.TextOnPrimaryHex);
        Assert.Equal("#1A1A1A", tenantContext.Palette.TextOnSecondaryHex);
    }

    [Fact]
    public void TenantContext_ResolucionSanVicente_AsignaPaletaCorrecta()
    {
        var sanVicentePalette = new PaletteConfig
        {
            PrimaryHex = "#0D47A1",
            SecondaryHex = "#D32F2F",
            AccentHex = "#F57C00",
            TextOnPrimaryHex = "#FFFFFF",
            TextOnSecondaryHex = "#FFFFFF",
            SurfaceHeaderHex = "#0D47A1"
        };

        var tenantContext = new TenantContext();
        tenantContext.SetTenant(
            TenantContext.OrgSanVicente,
            "sanvicente",
            "Grupo Scout San Vicente 412",
            412,
            sanVicentePalette);

        Assert.NotNull(tenantContext.Palette);
        Assert.Equal("#0D47A1", tenantContext.Palette.PrimaryHex);
        Assert.Equal("#D32F2F", tenantContext.Palette.SecondaryHex);
    }

    [Fact]
    public void ScoutMudTheme_ConPaletaTenant_PersonalizaColoresPrimarioYSecundario()
    {
        var palette = PaletteConfig.DefaultLestonnac;
        var theme = ScoutMudTheme.CreateTheme(palette);

        Assert.Equal(new MudBlazor.Utilities.MudColor("#1B5E20"), theme.PaletteLight.Primary);
        Assert.Equal(new MudBlazor.Utilities.MudColor("#FFFFFF"), theme.PaletteLight.PrimaryContrastText);
        Assert.Equal(new MudBlazor.Utilities.MudColor("#FBC02D"), theme.PaletteLight.Secondary);
        Assert.Equal(new MudBlazor.Utilities.MudColor("#1A1A1A"), theme.PaletteLight.SecondaryContrastText);
        Assert.Equal("12px", theme.LayoutProperties.DefaultBorderRadius);
    }

    [Fact]
    public void ScoutMudTheme_SinPaleta_AplicaPaletaInstitucionalSCLM()
    {
        var theme = ScoutMudTheme.CreateTheme(null);

        Assert.Equal(new MudBlazor.Utilities.MudColor("#1E3A8A"), theme.PaletteLight.Primary);
        Assert.Equal(new MudBlazor.Utilities.MudColor("#FFFFFF"), theme.PaletteLight.PrimaryContrastText);
        Assert.Equal(new MudBlazor.Utilities.MudColor("#475569"), theme.PaletteLight.Secondary);
    }

    [Fact]
    public void AislamientoPaletasEntreTenants_PA16()
    {
        var tenant1 = new TenantContext();
        tenant1.SetTenant(TenantContext.OrgLestonnac, "lestonnac", "Lestonnac 330", 330, PaletteConfig.DefaultLestonnac);

        var tenant2 = new TenantContext();
        tenant2.SetTenant(TenantContext.OrgSanVicente, "sanvicente", "San Vicente 412", 412, new PaletteConfig
        {
            PrimaryHex = "#0D47A1",
            SecondaryHex = "#D32F2F",
            AccentHex = "#F57C00",
            TextOnPrimaryHex = "#FFFFFF",
            TextOnSecondaryHex = "#FFFFFF",
            SurfaceHeaderHex = "#0D47A1"
        });

        // La paleta de Lestonnac debe permanecer inalterada y no contaminar San Vicente
        Assert.Equal("#1B5E20", tenant1.Palette!.PrimaryHex);
        Assert.Equal("#0D47A1", tenant2.Palette!.PrimaryHex);
        Assert.NotEqual(tenant1.Palette.PrimaryHex, tenant2.Palette.PrimaryHex);
    }

    [Theory]
    [InlineData("#9A3412", "#FFF7ED", "Castores")]
    [InlineData("#854D0E", "#FEFCE8", "Manada")]
    [InlineData("#075985", "#F0F9FF", "Tropa")]
    [InlineData("#991B1B", "#FEF2F2", "Escultas")]
    [InlineData("#166534", "#F0FDF4", "Clan")]
    public void WCAG22_TodasLasSeccionesPedagogicas_TienenContrasteValido(string textColor, string bgColor, string seccion)
    {
        // PA17: Todos los colores identificativos y secciones pedagógicas deben cumplir WCAG 2.2 Nivel AA (mínimo 4.5:1)
        var ratio = WcagContrastCalculator.CalculateContrastRatio(textColor, bgColor);
        var cumple = WcagContrastCalculator.MeetsWcagAaNormalText(textColor, bgColor);

        Assert.True(cumple, $"La sección {seccion} ({textColor} sobre {bgColor}) debe tener ratio >= 4.5:1. Obtenido: {ratio:F2}:1");
    }

    [Fact]
    public void WCAG22_PaletaLestonnacYSanVicente_CumplenContrasteAA()
    {
        // Lestonnac: Texto blanco sobre verde
        Assert.True(WcagContrastCalculator.MeetsWcagAaNormalText(
            PaletteConfig.DefaultLestonnac.TextOnPrimaryHex,
            PaletteConfig.DefaultLestonnac.PrimaryHex));

        // Lestonnac: Texto oscuro #1A1A1A sobre amarillo #FBC02D
        Assert.True(WcagContrastCalculator.MeetsWcagAaNormalText(
            PaletteConfig.DefaultLestonnac.TextOnSecondaryHex,
            PaletteConfig.DefaultLestonnac.SecondaryHex));

        // San Vicente: Texto blanco sobre azul marino #0D47A1
        Assert.True(WcagContrastCalculator.MeetsWcagAaNormalText("#FFFFFF", "#0D47A1"));
    }
}
