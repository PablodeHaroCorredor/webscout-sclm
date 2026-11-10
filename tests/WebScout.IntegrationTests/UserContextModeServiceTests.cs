using WebScout.Web.Infrastructure.Navigation;
using Xunit;

namespace WebScout.IntegrationTests;

public class UserContextModeServiceTests
{
    [Fact]
    public void InitialState_ShouldDefaultToKraalMode()
    {
        var service = new UserContextModeService();

        Assert.Equal(ScoutContextMode.Kraal, service.CurrentMode);
    }

    [Fact]
    public void SetMode_ToFamilia_ShouldUpdateModeAndTriggerEvent()
    {
        var service = new UserContextModeService();
        var eventFired = false;
        service.OnModeChanged += () => eventFired = true;

        service.SetMode(ScoutContextMode.Familia);

        Assert.Equal(ScoutContextMode.Familia, service.CurrentMode);
        Assert.True(eventFired, "El evento OnModeChanged debe dispararse al conmutar de Kraal a Familia.");
    }

    [Fact]
    public void SetMode_ToSameMode_ShouldNotTriggerEvent()
    {
        var service = new UserContextModeService();
        var eventCount = 0;
        service.OnModeChanged += () => eventCount++;

        // Intento de reasignar el mismo modo Kraal
        service.SetMode(ScoutContextMode.Kraal);

        Assert.Equal(0, eventCount);
        Assert.Equal(ScoutContextMode.Kraal, service.CurrentMode);
    }

    [Fact]
    public void ToggleModes_ShouldAlternateCorrectly()
    {
        var service = new UserContextModeService();
        var changeLog = new List<ScoutContextMode>();
        service.OnModeChanged += () => changeLog.Add(service.CurrentMode);

        service.SetMode(ScoutContextMode.Familia);
        service.SetMode(ScoutContextMode.Kraal);
        service.SetMode(ScoutContextMode.Familia);

        Assert.Equal(3, changeLog.Count);
        Assert.Equal([ScoutContextMode.Familia, ScoutContextMode.Kraal, ScoutContextMode.Familia], changeLog);
    }
}
