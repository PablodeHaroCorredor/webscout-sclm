using MudBlazor;
using WebScout.Web.Common.Domain;
using WebScout.Web.Common.Wcag;

namespace WebScout.Web.Infrastructure.Tenancy;

public static class ScoutMudTheme
{
    public static MudTheme CreateTheme(PanoletaConfig panoleta)
    {
        var textColorOnPrimary = WcagContrastCalculator.GetAccessibleTextColor(panoleta.BaseColorHex);
        var textColorOnSecondary = WcagContrastCalculator.GetAccessibleTextColor(panoleta.BorderColorHex);

        return new MudTheme
        {
            PaletteLight = new PaletteLight
            {
                Primary = panoleta.BaseColorHex,
                PrimaryContrastText = textColorOnPrimary,
                Secondary = panoleta.BorderColorHex,
                SecondaryContrastText = textColorOnSecondary,
                AppbarBackground = panoleta.BaseColorHex,
                AppbarText = textColorOnPrimary,
                Background = "#F8FAFC",
                Surface = "#FFFFFF",
                TextPrimary = "#0F172A",
                TextSecondary = "#475569",
                DrawerBackground = "#FFFFFF",
                DrawerText = "#0F172A",
                DrawerIcon = panoleta.BaseColorHex,
                ActionDefault = "#475569",
                Success = "#166534",
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
