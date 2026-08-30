using CryptographyAPI.Data;
using CryptographyAPI.Security;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services
    .AddOptions<EncryptionOptions>()
    .Bind(builder.Configuration.GetSection(EncryptionOptions.SectionName))
    .Validate(options => options.HasValidKey(), "Encryption:Key must be a 32-byte Base64 value.")
    .ValidateOnStart();

builder.Services.AddSingleton<IFieldEncryptor, AesGcmFieldEncryptor>();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "ConnectionStrings:DefaultConnection must be configured with User Secrets or an environment variable.");

builder.Services.AddDbContext<CryptoDbContext>(options =>
    options.UseSqlServer(connectionString));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();

public partial class Program;
