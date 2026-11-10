namespace WebScout.Web.Infrastructure.Navigation;

/// <summary>
/// Modos de contexto de trabajo para usuarios con roles duales (p. ej. Scouter con hijos en el grupo).
/// </summary>
public enum ScoutContextMode
{
    /// <summary>
    /// Modo de gestión de sección / kraal (censo, actividades, admisiones, pedidos de sección).
    /// </summary>
    Kraal,

    /// <summary>
    /// Modo de tutor legal / familia (fichas médicas de hijos, inscripciones familiares, compras de uniformidad).
    /// </summary>
    Familia
}

/// <summary>
/// Servicio con ciclo de vida Scoped (por circuito Blazor / sesión de usuario)
/// que gestiona el modo activo de navegación y emite notificaciones de cambio de contexto.
/// </summary>
public class UserContextModeService
{
    public ScoutContextMode CurrentMode { get; private set; } = ScoutContextMode.Kraal;

    public event Action? OnModeChanged;

    public void SetMode(ScoutContextMode mode)
    {
        if (CurrentMode != mode)
        {
            CurrentMode = mode;
            OnModeChanged?.Invoke();
        }
    }
}
