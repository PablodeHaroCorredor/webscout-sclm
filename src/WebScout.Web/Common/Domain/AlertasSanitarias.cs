using System.Globalization;
using System.Text;

namespace WebScout.Web.Common.Domain;

/// <summary>
/// Tipos de alerta sanitaria que el Kraal debe ver de un vistazo en censos, dossieres y listados impresos.
/// El orden refleja la gravedad operativa: primero lo que puede provocar una reacción grave.
/// </summary>
public enum TipoAlertaSanitaria
{
    AlergiaMedicamentosa,
    AlergiaAlimentaria,
    AlergiaAmbiental,
    EnfermedadCronica,
    Medicacion,
    Dieta
}

/// <summary>
/// Alerta sanitaria derivada de la ficha médica de un educando.
/// </summary>
public sealed record AlertaSanitaria(TipoAlertaSanitaria Tipo, string Detalle)
{
    public string Etiqueta => Tipo switch
    {
        TipoAlertaSanitaria.AlergiaMedicamentosa => "Alergia a medicamentos",
        TipoAlertaSanitaria.AlergiaAlimentaria => "Alergia alimentaria",
        TipoAlertaSanitaria.AlergiaAmbiental => "Alergia ambiental",
        TipoAlertaSanitaria.EnfermedadCronica => "Enfermedad crónica",
        TipoAlertaSanitaria.Medicacion => "Medicación",
        TipoAlertaSanitaria.Dieta => "Dieta / intolerancia",
        _ => "Alerta"
    };

    /// <summary>Alergias de cualquier tipo: requieren atención inmediata.</summary>
    public bool EsAlergia => Tipo is TipoAlertaSanitaria.AlergiaMedicamentosa
        or TipoAlertaSanitaria.AlergiaAlimentaria
        or TipoAlertaSanitaria.AlergiaAmbiental;
}

/// <summary>
/// Regla única para interpretar los campos clínicos de texto libre de la ficha médica.
/// Las familias escriben a menudo "Sin alergias conocidas", "Ninguna" o "No" en lugar de dejar el campo vacío;
/// esos valores no son alertas y no deben pintarse en rojo ni contarse en los indicadores.
/// </summary>
public static class AlertasSanitarias
{
    private static readonly HashSet<string> ValoresNegativosExactos = new(StringComparer.Ordinal)
    {
        "no", "ninguna", "ninguno", "ningunas", "ningunos", "nada", "n/a", "na", "-", "--", "no aplica",
        "no consta", "no tiene", "no procede", "ninguna conocida", "ninguno conocido", "no conocidas", "no conocida",
        "normal", "dieta normal", "sin", "sin datos"
    };

    // "Sin ..." solo es negativo cuando niega la propia alerta; "Sin gluten" o "sin lactosa" son dietas reales.
    private static readonly string[] PrefijosNegativos =
    [
        "sin alergia", "sin intoleranc", "sin medicac", "sin tratamiento", "sin enfermedad", "sin patolog",
        "sin restriccion", "sin dieta especial", "no tiene alergia", "no tiene intoleranc", "no toma",
        "no padece", "no presenta", "ninguna alergia", "ninguna enfermedad", "ningun tratamiento"
    ];

    /// <summary>
    /// Indica si un campo clínico de texto libre contiene información relevante (no vacío ni negativo).
    /// </summary>
    public static bool EsRelevante(string? texto)
    {
        if (string.IsNullOrWhiteSpace(texto))
        {
            return false;
        }

        var normalizado = Normalizar(texto);
        if (normalizado.Length == 0 || ValoresNegativosExactos.Contains(normalizado))
        {
            return false;
        }

        return !PrefijosNegativos.Any(normalizado.StartsWith);
    }

    /// <summary>
    /// Obtiene las alertas sanitarias ordenadas por gravedad a partir de los campos clínicos de la ficha.
    /// </summary>
    public static IReadOnlyList<AlertaSanitaria> Obtener(
        string? alergiasMedicamentosas,
        string? alergiasAlimentarias,
        string? alergiasAmbientales,
        string? enfermedadesCronicas,
        string? medicacionActiva,
        string? regimenAlimenticio)
    {
        var alertas = new List<AlertaSanitaria>(6);
        Agregar(alertas, TipoAlertaSanitaria.AlergiaMedicamentosa, alergiasMedicamentosas);
        Agregar(alertas, TipoAlertaSanitaria.AlergiaAlimentaria, alergiasAlimentarias);
        Agregar(alertas, TipoAlertaSanitaria.AlergiaAmbiental, alergiasAmbientales);
        Agregar(alertas, TipoAlertaSanitaria.EnfermedadCronica, enfermedadesCronicas);
        Agregar(alertas, TipoAlertaSanitaria.Medicacion, medicacionActiva);
        Agregar(alertas, TipoAlertaSanitaria.Dieta, regimenAlimenticio);
        return alertas;
    }

    private static void Agregar(List<AlertaSanitaria> alertas, TipoAlertaSanitaria tipo, string? texto)
    {
        if (EsRelevante(texto))
        {
            alertas.Add(new AlertaSanitaria(tipo, texto!.Trim()));
        }
    }

    private static string Normalizar(string texto)
    {
        var descompuesto = texto.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(descompuesto.Length);
        foreach (var c in descompuesto)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
            {
                sb.Append(c);
            }
        }

        return sb.ToString().Normalize(NormalizationForm.FormC).TrimEnd('.', ',', ';', ' ');
    }
}
