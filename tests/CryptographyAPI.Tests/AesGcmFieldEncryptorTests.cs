using System.Security.Cryptography;
using CryptographyAPI.Security;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CryptographyAPI.Tests;

[TestClass]
public sealed class AesGcmFieldEncryptorTests
{
    [TestMethod]
    public void Encrypt_and_decrypt_preserve_the_original_value()
    {
        using var encryptor = CreateEncryptor();

        var encrypted = encryptor.Encrypt("12345678900");
        var decrypted = encryptor.Decrypt(encrypted);

        Assert.AreNotEqual("12345678900", encrypted);
        StringAssert.StartsWith(encrypted, "v1.");
        Assert.AreEqual("12345678900", decrypted);
    }

    [TestMethod]
    public void Equal_values_produce_different_ciphertexts()
    {
        using var encryptor = CreateEncryptor();

        var first = encryptor.Encrypt("same-value");
        var second = encryptor.Encrypt("same-value");

        Assert.AreNotEqual(first, second);
    }

    [TestMethod]
    public void Modified_ciphertext_is_rejected()
    {
        using var encryptor = CreateEncryptor();
        var parts = encryptor.Encrypt("sensitive-value").Split('.');
        var ciphertext = Convert.FromBase64String(parts[3]);
        ciphertext[0] ^= 1;
        parts[3] = Convert.ToBase64String(ciphertext);

        Assert.ThrowsExactly<AuthenticationTagMismatchException>(() =>
            encryptor.Decrypt(string.Join('.', parts)));
    }

    internal static AesGcmFieldEncryptor CreateEncryptor()
    {
        var key = RandomNumberGenerator.GetBytes(EncryptionOptions.KeySizeInBytes);
        var options = Options.Create(new EncryptionOptions
        {
            Key = Convert.ToBase64String(key)
        });

        return new AesGcmFieldEncryptor(options);
    }
}
