using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;

namespace CryptographyAPI.Security;

public sealed class AesGcmFieldEncryptor : IFieldEncryptor, IDisposable
{
    private const string FormatVersion = "v1";
    private const int NonceSizeInBytes = 12;
    private const int TagSizeInBytes = 16;

    private readonly byte[] _key;

    public AesGcmFieldEncryptor(IOptions<EncryptionOptions> options)
    {
        _key = Convert.FromBase64String(options.Value.Key);

        if (_key.Length != EncryptionOptions.KeySizeInBytes)
        {
            throw new OptionsValidationException(
                EncryptionOptions.SectionName,
                typeof(EncryptionOptions),
                ["Encryption:Key must be a 32-byte Base64 value."]);
        }
    }

    public string Encrypt(string plaintext)
    {
        ArgumentNullException.ThrowIfNull(plaintext);

        var plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
        var nonce = RandomNumberGenerator.GetBytes(NonceSizeInBytes);
        var ciphertext = new byte[plaintextBytes.Length];
        var tag = new byte[TagSizeInBytes];

        using var aes = new AesGcm(_key, TagSizeInBytes);
        aes.Encrypt(nonce, plaintextBytes, ciphertext, tag);

        return string.Join(
            '.',
            FormatVersion,
            Convert.ToBase64String(nonce),
            Convert.ToBase64String(tag),
            Convert.ToBase64String(ciphertext));
    }

    public string Decrypt(string encryptedValue)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(encryptedValue);

        var parts = encryptedValue.Split('.');
        if (parts.Length != 4 || parts[0] != FormatVersion)
        {
            throw new CryptographicException("The encrypted value has an unsupported format.");
        }

        try
        {
            var nonce = Convert.FromBase64String(parts[1]);
            var tag = Convert.FromBase64String(parts[2]);
            var ciphertext = Convert.FromBase64String(parts[3]);

            if (nonce.Length != NonceSizeInBytes || tag.Length != TagSizeInBytes)
            {
                throw new CryptographicException("The encrypted value has invalid metadata.");
            }

            var plaintext = new byte[ciphertext.Length];
            using var aes = new AesGcm(_key, TagSizeInBytes);
            aes.Decrypt(nonce, ciphertext, tag, plaintext);

            return Encoding.UTF8.GetString(plaintext);
        }
        catch (FormatException exception)
        {
            throw new CryptographicException("The encrypted value has invalid encoding.", exception);
        }
    }

    public void Dispose() => CryptographicOperations.ZeroMemory(_key);
}
