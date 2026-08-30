namespace CryptographyAPI.Security;

public interface IFieldEncryptor
{
    string Encrypt(string plaintext);
    string Decrypt(string encryptedValue);
}
