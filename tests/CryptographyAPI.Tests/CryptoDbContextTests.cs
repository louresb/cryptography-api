using CryptographyAPI.Data;
using CryptographyAPI.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CryptographyAPI.Tests;

[TestClass]
public sealed class CryptoDbContextTests
{
    [TestMethod]
    public async Task Sensitive_fields_are_encrypted_in_storage_and_decrypted_by_the_context()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<CryptoDbContext>()
            .UseSqlite(connection)
            .Options;

        using var encryptor = AesGcmFieldEncryptorTests.CreateEncryptor();
        await using var context = new CryptoDbContext(options, encryptor);
        await context.Database.EnsureCreatedAsync();

        context.CryptEntities.Add(new CryptEntity
        {
            UserDocument = "12345678900",
            CreditCardToken = "card-token",
            Value = 5999
        });
        await context.SaveChangesAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT UserDocument, CreditCardToken FROM CryptEntities LIMIT 1";
        await using var reader = await command.ExecuteReaderAsync();
        Assert.IsTrue(await reader.ReadAsync());
        Assert.AreNotEqual("12345678900", reader.GetString(0));
        Assert.AreNotEqual("card-token", reader.GetString(1));
        await reader.CloseAsync();

        context.ChangeTracker.Clear();
        var entity = await context.CryptEntities.SingleAsync();

        Assert.AreEqual("12345678900", entity.UserDocument);
        Assert.AreEqual("card-token", entity.CreditCardToken);
    }
}
