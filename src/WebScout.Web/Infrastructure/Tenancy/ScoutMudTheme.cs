using MudBlazor;

namespace WebScout.Web.Infrastructure.Tenancy;

/// <summary>
/// Provee el tema visual sobrio, neutro y profesional de MudBlazor para WebScout SCLM.
/// Evita estridencias cromáticas y garantiza máxima legibilidad y usabilidad.
/// </summary>
public static class ScoutMudTheme
{
    public static MudTheme CreateTheme()
    {
        return new MudTheme
        {
            PaletteLight = new PaletteLight
            {
                Primary = "#1E3A8A",            // Azul marino profesional
                PrimaryContrastText = "#FFFFFF",
                Secondary = "#475569",          // Pizarra neutro
                SecondaryContrastText = "#FFFFFF",
                AppbarBackground = "#FFFFFF",   // Barra superior blanca limpia
                AppbarText = "#0F172A",         // Texto oscuro de alto contraste
                Background = "#F8FAFC",         // Fondo gris claro moderno
                Surface = "#FFFFFF",
                TextPrimary = "#0F172A",
                TextSecondary = "#64748B",
                DrawerBackground = "#FFFFFF",
                DrawerText = "#0F172A",
                DrawerIcon = "#1E3A8A",
                ActionDefault = "#64748B",
                Success = "#15803D",
                Warning = "#D97706",
                Error = "#DC2626",
                Info = "#0284C7"
            },
            LayoutProperties = new LayoutProperties
            {
                DefaultBorderRadius = "8px",
                AppbarHeight = "64px"
            }
        };
    }
}
