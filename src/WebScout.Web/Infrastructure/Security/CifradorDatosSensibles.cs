using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace WebScout.Web.Infrastructure.Security;

public interface IServicioCifrado
{
    string? Cifrar(string? textoPlano);
    string? Descifrar(string? textoCifrado);
}

/// <summary>
/// Cifrador criptográfico autenticado con AES-256-GCM para datos de salud y categorías especiales (Art. 9 RGPD).
/// Almacena el payload con el prefijo "enc:v1:" concatenando nonce (12 bytes) + tag de autenticación (16 bytes) + texto cifrado.
/// Garantiza compatibilidad retroactiva: si un registro histórico no tiene el prefijo, se devuelve tal cual.
/// La clave procede de configuración (<see cref="ClaveConfiguracion"/>); fuera de Development es obligatoria.
/// </summary>
public class CifradorDatosSensibles : IServicioCifrado
{
    /// <summary>Clave de configuración. Como variable de entorno: <c>Seguridad__ClaveCifradoDatosSensibles</c>.</summary>
    public const string ClaveConfiguracion = "Seguridad:ClaveCifradoDatosSensibles";

    /// <summary>Longitud mínima fuera de Development. Recomendado: 32 bytes aleatorios en Base64 (44 caracteres).</summary>
    public const int LongitudMinimaClave = 32;

    /// <summary>
    /// Clave PÚBLICA (está en el repositorio): solo protege datos sintéticos en Development.
    /// Es la misma cadena que usaban las versiones anteriores, para que las bases locales ya sembradas sigan descifrándose.
    /// Cualquier otro entorno rechaza arrancar con ella.
    /// </summary>
    public const string ClaveSoloDesarrollo = "WebScoutSCLM_MasterEncryptionKey_2026_SecureRGPDDefaultKey!";

    private readonly byte[] _key;
    private const string Prefijo = "enc:v1:";

    public CifradorDatosSensibles(string clave)
    {
        if (string.IsNullOrWhiteSpace(clave))
        {
            throw new ArgumentException("La clave de cifrado no puede estar vacía.", nameof(clave));
        }

        _key = SHA256.HashData(Encoding.UTF8.GetBytes(clave));
    }

    /// <summary>Cifrador con la clave pública de desarrollo. Solo para Development y pruebas con datos sintéticos.</summary>
    public static CifradorDatosSensibles ParaDesarrollo() => new(ClaveSoloDesarrollo);

    /// <summary>
    /// Construye el cifrador a partir de la configuración. En Development, si no hay clave configurada,
    /// usa <see cref="ClaveSoloDesarrollo"/>. En cualquier otro entorno exige una clave propia y suficientemente
    /// larga, y lanza una excepción si falta o si es la clave pública de desarrollo, para que la aplicación no arranque.
    /// </summary>
    public static CifradorDatosSensibles DesdeConfiguracion(IConfiguration configuracion, IHostEnvironment entorno)
    {
        var clave = configuracion[ClaveConfiguracion];

        if (entorno.IsDevelopment())
        {
            return string.IsNullOrWhiteSpace(clave) ? ParaDesarrollo() : new CifradorDatosSensibles(clave);
        }

        if (string.IsNullOrWhiteSpace(clave))
        {
            throw new InvalidOperationException(
                $"Falta la clave de cifrado de datos sensibles ('{ClaveConfiguracion}'). " +
                $"Defínala con la variable de entorno 'Seguridad__ClaveCifradoDatosSensibles' o un gestor de secretos; " +
                $"el entorno '{entorno.EnvironmentName}' no arranca sin ella.");
        }

        if (clave == ClaveSoloDesarrollo)
        {
            throw new InvalidOperationException(
                $"La clave de cifrado configurada es la clave pública de desarrollo. " +
                $"El entorno '{entorno.EnvironmentName}' necesita una clave propia que no esté en el repositorio.");
        }

        if (clave.Length < LongitudMinimaClave)
        {
            throw new InvalidOperationException(
                $"La clave de cifrado de datos sensibles debe tener al menos {LongitudMinimaClave} caracteres " +
                $"(recomendado: 32 bytes aleatorios codificados en Base64).");
        }

        return new CifradorDatosSensibles(clave);
    }

    public string? Cifrar(string? textoPlano)
    {
        if (string.IsNullOrEmpty(textoPlano)) return textoPlano;
        if (textoPlano.StartsWith(Prefijo, StringComparison.Ordinal)) return textoPlano; // Ya cifrado

        var bytesPlano = Encoding.UTF8.GetBytes(textoPlano);
        var nonce = new byte[AesGcm.NonceByteSizes.MaxSize]; // 12 bytes
        RandomNumberGenerator.Fill(nonce);

        var ciphertext = new byte[bytesPlano.Length];
        var tag = new byte[AesGcm.TagByteSizes.MaxSize]; // 16 bytes

        using var aes = new AesGcm(_key, AesGcm.TagByteSizes.MaxSize);
        aes.Encrypt(nonce, bytesPlano, ciphertext, tag);

        var payload = new byte[nonce.Length + tag.Length + ciphertext.Length];
        Buffer.BlockCopy(nonce, 0, payload, 0, nonce.Length);
        Buffer.BlockCopy(tag, 0, payload, nonce.Length, tag.Length);
        Buffer.BlockCopy(ciphertext, 0, payload, nonce.Length + tag.Length, ciphertext.Length);

        return Prefijo + Convert.ToBase64String(payload);
    }

    public string? Descifrar(string? textoCifrado)
    {
        if (string.IsNullOrEmpty(textoCifrado)) return textoCifrado;

        if (!textoCifrado.StartsWith(Prefijo, StringComparison.Ordinal))
        {
            // Registro histórico o de prueba sin cifrar
            return textoCifrado;
        }

        try
        {
            var base64 = textoCifrado.Substring(Prefijo.Length);
            var payload = Convert.FromBase64String(base64);

            var nonceSize = AesGcm.NonceByteSizes.MaxSize;
            var tagSize = AesGcm.TagByteSizes.MaxSize;

            if (payload.Length < nonceSize + tagSize)
            {
                return textoCifrado;
            }

            var nonce = new byte[nonceSize];
            var tag = new byte[tagSize];
            var cipherLength = payload.Length - nonceSize - tagSize;
            var ciphertext = new byte[cipherLength];

            Buffer.BlockCopy(payload, 0, nonce, 0, nonceSize);
            Buffer.BlockCopy(payload, nonceSize, tag, 0, tagSize);
            Buffer.BlockCopy(payload, nonceSize + tagSize, ciphertext, 0, cipherLength);

            var bytesPlano = new byte[cipherLength];
            using var aes = new AesGcm(_key, AesGcm.TagByteSizes.MaxSize);
            aes.Decrypt(nonce, ciphertext, tag, bytesPlano);

            return Encoding.UTF8.GetString(bytesPlano);
        }
        catch
        {
            // Fallback ante payload corrupto o clave incorrecta
            return textoCifrado;
        }
    }
}
