namespace CryptographyAPI.Security;

public sealed class EncryptionOptions
{
    public const string SectionName = "Encryption";
    public const int KeySizeInBytes = 32;

    public string Key { get; init; } = string.Empty;

    public bool HasValidKey()
    {
        try
        {
            return Convert.FromBase64String(Key).Length == KeySizeInBytes;
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
