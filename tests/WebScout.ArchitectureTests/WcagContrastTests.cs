using WebScout.Web.Common.Components;
using WebScout.Web.Common.Domain;
using WebScout.Web.Common.Wcag;
using Xunit;

namespace WebScout.ArchitectureTests;

public class WcagContrastTests
{
    [Fact]
    public void PureBlackAndWhite_ShouldHaveMaximumContrastRatio()
    {
        // El contraste entre blanco y negro debe ser 21:1 según la especificación WCAG
        var ratio = WcagContrastCalculator.CalculateContrastRatio("#FFFFFF", "#000000");
        Assert.True(ratio >= 20.9 && ratio <= 21.1);
    }

    [Fact]
    public void Lestonnac330_WhiteTextOnGreenBackground_ShouldComplyWithWcagAa()
    {
        var primaryColor = PaletteConfig.DefaultLestonnac.PrimaryHex;
        
        // Texto blanco sobre verde bosque #1B5E20
        var ratio = WcagContrastCalculator.CalculateContrastRatio("#FFFFFF", primaryColor);
        var passes = WcagContrastCalculator.MeetsWcagAaNormalText("#FFFFFF", primaryColor);

        Assert.True(passes, $"El ratio de contraste {ratio:F2}:1 debe ser >= 4.5:1 para cumplir WCAG 2.2 AA");
    }

    [Fact]
    public void SanVicente412_WhiteTextOnNavyBackground_ShouldComplyWithWcagAa()
    {
        const string navyColor = "#0D47A1";
        
        // Texto blanco sobre azul marino #0D47A1
        var ratio = WcagContrastCalculator.CalculateContrastRatio("#FFFFFF", navyColor);
        var passes = WcagContrastCalculator.MeetsWcagAaNormalText("#FFFFFF", navyColor);

        Assert.True(passes, $"El ratio de contraste {ratio:F2}:1 debe ser >= 4.5:1");
    }

    [Fact]
    public void YellowBackground_ShouldAutomaticallySelectDarkText()
    {
        // Fondo amarillo #FBC02D (secundario Lestonnac)
        var recommendedText = WcagContrastCalculator.GetAccessibleTextColor(PaletteConfig.DefaultLestonnac.SecondaryHex);

        Assert.Equal("#1A1A1A", recommendedText);
    }

    [Fact]
    public void SegmentedControl_InactiveTextOnSlateBackground_ShouldComplyWithWcagAa()
    {
        // Texto inactivo #1E293B sobre contenedor #E2E8F0 del conmutador de entorno
        var ratio = WcagContrastCalculator.CalculateContrastRatio("#1E293B", "#E2E8F0");
        var passes = WcagContrastCalculator.MeetsWcagAaNormalText("#1E293B", "#E2E8F0");

        Assert.True(passes, $"El ratio de contraste {ratio:F2}:1 debe ser >= 4.5:1 para cumplir WCAG 2.2 AA");
    }

    [Fact]
    public void FamilyDashboard_ActionButtons_ShouldComplyWithWcagAa()
    {
        // Botón rojo de subsanación (#FFFFFF sobre #DC2626)
        var ratioRojo = WcagContrastCalculator.CalculateContrastRatio("#FFFFFF", "#DC2626");
        Assert.True(WcagContrastCalculator.MeetsWcagAaNormalText("#FFFFFF", "#DC2626"), 
            $"El botón de subsanación ({ratioRojo:F2}:1) debe cumplir WCAG AA");

        // Botón ámbar de justificante (#FFFFFF sobre #B45309)
        var ratioAmbar = WcagContrastCalculator.CalculateContrastRatio("#FFFFFF", "#B45309");
        Assert.True(WcagContrastCalculator.MeetsWcagAaNormalText("#FFFFFF", "#B45309"), 
            $"El botón de justificante ({ratioAmbar:F2}:1) debe cumplir WCAG AA");
    }

    [Fact]
    public void FamilyDashboard_AlertHeadings_ShouldComplyWithWcagAa()
    {
        // Texto rojo oscuro #991B1B sobre fondo pastel #FEF2F2
        var ratioRojo = WcagContrastCalculator.CalculateContrastRatio("#991B1B", "#FEF2F2");
        Assert.True(WcagContrastCalculator.MeetsWcagAaNormalText("#991B1B", "#FEF2F2"),
            $"El texto de alerta roja ({ratioRojo:F2}:1) debe cumplir WCAG AA");

        // Texto ámbar oscuro #92400E sobre fondo pastel #FFFBEB
        var ratioAmbar = WcagContrastCalculator.CalculateContrastRatio("#92400E", "#FFFBEB");
        Assert.True(WcagContrastCalculator.MeetsWcagAaNormalText("#92400E", "#FFFBEB"),
            $"El texto de alerta ámbar ({ratioAmbar:F2}:1) debe cumplir WCAG AA");
    }

    [Fact]
    public void DrawerActiveLink_GreenOnLightGreenPastel_ShouldComplyWithWcagAa()
    {
        // Enlace activo del menú lateral: verde bosque #1B5E20 sobre fondo pastel #F0FDF4
        var ratio = WcagContrastCalculator.CalculateContrastRatio("#1B5E20", "#F0FDF4");
        Assert.True(WcagContrastCalculator.MeetsWcagAaNormalText("#1B5E20", "#F0FDF4"),
            $"El enlace activo en el menú lateral ({ratio:F2}:1) debe cumplir WCAG 2.2 AA (>= 4.5:1)");
    }

    [Fact]
    public void ScoutStatusBadge_AllSemanticStyles_MustComplyWithWcagAaNormalText()
    {
        var estilos = ScoutStatusBadgeStyleResolver.ObtenerTodosLosEstilos();
        
        foreach (var estilo in estilos)
        {
            var ratio = WcagContrastCalculator.CalculateContrastRatio(estilo.TextColorHex, estilo.BackgroundColorHex);
            var cumple = WcagContrastCalculator.MeetsWcagAaNormalText(estilo.TextColorHex, estilo.BackgroundColorHex);

            Assert.True(cumple, 
                $"El estilo semántico '{estilo.Text}' ({estilo.TextColorHex} sobre {estilo.BackgroundColorHex}) tiene un ratio {ratio:F2}:1 y debe ser >= 4.5:1 para cumplir WCAG 2.2 AA");
        }
    }
}
