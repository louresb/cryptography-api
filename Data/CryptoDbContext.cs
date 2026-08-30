using CryptographyAPI.Models;
using CryptographyAPI.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace CryptographyAPI.Data;

public sealed class CryptoDbContext(
    DbContextOptions<CryptoDbContext> options,
    IFieldEncryptor fieldEncryptor) : DbContext(options)
{
    public DbSet<CryptEntity> CryptEntities => Set<CryptEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var encryptedConverter = new ValueConverter<string, string>(
            value => fieldEncryptor.Encrypt(value),
            value => fieldEncryptor.Decrypt(value));

        modelBuilder.Entity<CryptEntity>(entity =>
        {
            entity.Property(value => value.UserDocument)
                .HasConversion(encryptedConverter);

            entity.Property(value => value.CreditCardToken)
                .HasConversion(encryptedConverter);
        });
    }
}
