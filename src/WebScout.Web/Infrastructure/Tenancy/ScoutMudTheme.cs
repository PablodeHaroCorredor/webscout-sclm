using MudBlazor;
using WebScout.Web.Common.Domain;

namespace WebScout.Web.Infrastructure.Tenancy;

/// <summary>
/// Provee el tema visual sobrio, neutro y profesional de MudBlazor para WebScout SCLM.
/// Soporta la inyección reactiva de la paleta identificativa del grupo activo (PaletteConfig)
/// garantizando cumplimiento estricto de accesibilidad WCAG 2.2 AA (mínimo 4.5:1).
/// </summary>
public static class ScoutMudTheme
{
    public static MudTheme CreateTheme(PaletteConfig? palette = null)
    {
        var primaryColor = palette?.PrimaryHex ?? "#1E3A8A";
        var primaryContrast = palette?.TextOnPrimaryHex ?? "#FFFFFF";
        var secondaryColor = palette?.SecondaryHex ?? "#475569";
        var secondaryContrast = palette?.TextOnSecondaryHex ?? "#FFFFFF";

        return new MudTheme
        {
            PaletteLight = new PaletteLight
            {
                Primary = primaryColor,
                PrimaryContrastText = primaryContrast,
                Secondary = secondaryColor,
                SecondaryContrastText = secondaryContrast,
                Tertiary = "#0284C7",
                AppbarBackground = "#FFFFFF",
                AppbarText = "#0F172A",
                Background = "#F8FAFC",
                Surface = "#FFFFFF",
                TextPrimary = "#0F172A",
                TextSecondary = "#475569",
                DrawerBackground = "#FFFFFF",
                DrawerText = "#0F172A",
                DrawerIcon = primaryColor,
                ActionDefault = "#475569",
                Divider = "#E2E8F0",
                TableLines = "#E2E8F0",
                Success = "#15803D",
                Warning = "#D97706",
                Error = "#DC2626",
                Info = "#0284C7"
            },
            LayoutProperties = new LayoutProperties
            {
                DefaultBorderRadius = "12px",
                AppbarHeight = "64px"
            },
            Typography = new Typography
            {
                Default = new DefaultTypography
                {
                    FontFamily = ["Inter", "-apple-system", "BlinkMacSystemFont", "Segoe UI", "Roboto", "Helvetica", "Arial", "sans-serif"],
                    FontSize = "0.9375rem",
                    FontWeight = "400",
                    LineHeight = "1.5"
                },
                H1 = new H1Typography
                {
                    FontSize = "2.5rem",
                    FontWeight = "800",
                    LineHeight = "1.2"
                },
                H2 = new H2Typography
                {
                    FontSize = "2rem",
                    FontWeight = "800",
                    LineHeight = "1.25"
                },
                H3 = new H3Typography
                {
                    FontSize = "1.5rem",
                    FontWeight = "700",
                    LineHeight = "1.3"
                },
                H4 = new H4Typography
                {
                    FontSize = "1.25rem",
                    FontWeight = "700",
                    LineHeight = "1.35"
                },
                Button = new ButtonTypography
                {
                    FontWeight = "600",
                    TextTransform = "none"
                }
            }
        };
    }
}
