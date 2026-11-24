using System.Net.Mail;
using System.Numerics;
using System.Text.RegularExpressions;

namespace WebScout.Web.Features.Administration.Domain;

/// <summary>
/// Validación de los ajustes del grupo. La usan el formulario (aviso junto al campo) y el servicio (regla en servidor).
/// Cada método devuelve el mensaje de error o null si el valor es válido. Los campos vacíos son válidos: son opcionales.
/// </summary>
public static partial class ValidadorAjustesGrupo
{
    public static string? ValidarEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email)) return null;

        var valor = email.Trim();
        return MailAddress.TryCreate(valor, out var direccion) && direccion.Address == valor && valor.Contains('.', StringComparison.Ordinal)
            ? null
            : "Introduce un email válido, por ejemplo coordinacion@migrupo.es.";
    }

    public static string? ValidarTelefono(string? telefono)
    {
        if (string.IsNullOrWhiteSpace(telefono)) return null;

        var digitos = telefono.Count(char.IsDigit);
        return TelefonoPermitido().IsMatch(telefono.Trim()) && digitos is >= 9 and <= 15
            ? null
            : "Introduce un teléfono válido (9 a 15 cifras; se admiten espacios y prefijo +34).";
    }

    /// <summary>CIF de asociación: letra, 7 cifras y carácter de control (ej. G02123456).</summary>
    public static string? ValidarCif(string? cif)
    {
        if (string.IsNullOrWhiteSpace(cif)) return null;

        return FormatoCif().IsMatch(Compactar(cif))
            ? null
            : "Introduce un CIF válido: una letra, siete cifras y un carácter de control (ej. G02123456).";
    }

    /// <summary>IBAN con comprobación del dígito de control (ISO 13616, módulo 97).</summary>
    public static string? ValidarIban(string? iban)
    {
        if (string.IsNullOrWhiteSpace(iban)) return null;

        var valor = Compactar(iban);
        if (!FormatoIban().IsMatch(valor) || (valor.StartsWith("ES", StringComparison.Ordinal) && valor.Length != 24))
        {
            return "Introduce un IBAN válido (en España, ES seguido de 22 cifras).";
        }

        var reordenado = valor[4..] + valor[..4];
        var numerico = string.Concat(reordenado.Select(c => char.IsLetter(c) ? (c - 'A' + 10).ToString() : c.ToString()));
        return BigInteger.Parse(numerico) % 97 == 1
            ? null
            : "El IBAN no es correcto: revisa los dígitos de control.";
    }

    public static string? ValidarRonda(DateOnly inicio, DateOnly fin) =>
        fin > inicio ? null : "La fecha de fin de ronda debe ser posterior a la de inicio.";

    private static string Compactar(string valor) => valor.Replace(" ", string.Empty).Replace("-", string.Empty).ToUpperInvariant();

    [GeneratedRegex(@"^\+?[0-9 ]+$")]
    private static partial Regex TelefonoPermitido();

    [GeneratedRegex(@"^[ABCDEFGHJNPQRSUVW][0-9]{7}[0-9A-J]$")]
    private static partial Regex FormatoCif();

    [GeneratedRegex(@"^[A-Z]{2}[0-9]{2}[A-Z0-9]{11,30}$")]
    private static partial Regex FormatoIban();
}
