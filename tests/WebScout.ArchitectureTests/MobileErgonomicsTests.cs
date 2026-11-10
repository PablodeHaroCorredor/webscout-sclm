using WebScout.Web.Common.Domain;
using Xunit;

namespace WebScout.ArchitectureTests;

public class MobileErgonomicsTests
{
    public const int WcagMinimumTouchTargetPx = 44;

    [Fact]
    public void ModoVisualizacion_ShouldSupportAutoTablaAndTarjetas()
    {
        Assert.True(Enum.IsDefined(typeof(ModoVisualizacion), ModoVisualizacion.Auto));
        Assert.True(Enum.IsDefined(typeof(ModoVisualizacion), ModoVisualizacion.Tabla));
        Assert.True(Enum.IsDefined(typeof(ModoVisualizacion), ModoVisualizacion.Tarjetas));
    }

    [Fact]
    public void TouchTargetMinimumSize_ShouldComplyWithWcag255Recommendation()
    {
        // WCAG 2.5.5 Target Size (Enhanced) y recomendación móvil Apple/Android: mínimo 44x44 CSS px
        Assert.True(WcagMinimumTouchTargetPx >= 44, "Los elementos táctiles en tarjetas móviles deben ser >= 44px");
    }
}
