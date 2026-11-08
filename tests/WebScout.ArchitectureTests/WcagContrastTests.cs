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
        var lestonnac = PanoletaConfig.Lestonnac330;
        
        // Texto blanco sobre verde bosque #1B5E20
        var ratio = WcagContrastCalculator.CalculateContrastRatio("#FFFFFF", lestonnac.BaseColorHex);
        var passes = WcagContrastCalculator.MeetsWcagAaNormalText("#FFFFFF", lestonnac.BaseColorHex);

        Assert.True(passes, $"El ratio de contraste {ratio:F2}:1 debe ser >= 4.5:1 para cumplir WCAG 2.2 AA");
    }

    [Fact]
    public void SanVicente412_WhiteTextOnNavyBackground_ShouldComplyWithWcagAa()
    {
        var sanVicente = PanoletaConfig.SanVicente412;
        
        // Texto blanco sobre azul marino #0D47A1
        var ratio = WcagContrastCalculator.CalculateContrastRatio("#FFFFFF", sanVicente.BaseColorHex);
        var passes = WcagContrastCalculator.MeetsWcagAaNormalText("#FFFFFF", sanVicente.BaseColorHex);

        Assert.True(passes, $"El ratio de contraste {ratio:F2}:1 debe ser >= 4.5:1");
    }

    [Fact]
    public void YellowBackground_ShouldAutomaticallySelectDarkText()
    {
        // Fondo amarillo #FBC02D (ribete Lestonnac)
        var recommendedText = WcagContrastCalculator.GetAccessibleTextColor("#FBC02D");

        Assert.Equal("#1A1A1A", recommendedText);
    }
}
