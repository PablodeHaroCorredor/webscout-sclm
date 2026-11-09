using System.Security.Cryptography;
using System.Text;

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
/// </summary>
public class CifradorDatosSensibles : IServicioCifrado
{
    private readonly byte[] _key;
    private const string Prefijo = "enc:v1:";

    public CifradorDatosSensibles(string? clave = null)
    {
        var claveEfectiva = !string.IsNullOrWhiteSpace(clave)
            ? clave
            : "WebScoutSCLM_MasterEncryptionKey_2026_SecureRGPDDefaultKey!";

        _key = SHA256.HashData(Encoding.UTF8.GetBytes(claveEfectiva));
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
