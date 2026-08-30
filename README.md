# CryptographyAPI

[![.NET Build](https://github.com/louresb/cryptography-api/actions/workflows/dotnet-build.yml/badge.svg)](https://github.com/louresb/cryptography-api/actions/workflows/dotnet-build.yml)
![Development Status Badge](https://img.shields.io/badge/Status-Completed-green)

This API is a completed solution for a [public challenge](https://github.com/backend-br/desafios) offered by [Back-End Brasil](https://github.com/backend-br), the official community hub for Brazilian back-end developers.

The project demonstrates transparent encryption of sensitive entity properties between the application and database layers. Clients work with plaintext values while Entity Framework Core persists authenticated ciphertext.

## Technologies

- C# and .NET 10
- ASP.NET Core Web API
- Entity Framework Core
- AES-256-GCM authenticated encryption
- Microsoft SQL Server
- MSTest and SQLite for automated verification

## Challenge

The challenge is to implement encryption in a service transparently for both the API and service layers. Sensitive entity fields must not be directly visible in database columns because encryption and decryption occur during persistence conversion.

The example entity contains the following fields:

| Field | Type | Storage |
| --- | --- | --- |
| `id` | `long` | Plaintext |
| `userDocument` | `string` | Encrypted |
| `creditCardToken` | `string` | Encrypted |
| `value` | `long` | Plaintext |

## Implementation

Entity Framework Core value converters apply encryption and decryption without exposing cryptographic operations to controllers or clients.

- AES-256-GCM provides confidentiality and integrity validation.
- Each encrypted value receives a cryptographically random nonce.
- The encryption key and database connection string remain outside source control.
- Versioned ciphertext allows the persisted format to evolve explicitly.

## Screenshots

<div align="center">

### Post new user

![Post](https://github.com/louresb/CryptographyAPI/assets/103293696/3706150b-543a-4f6b-a391-8568c35e2672) ![Post 200](https://github.com/louresb/CryptographyAPI/assets/103293696/cc769d26-0c25-4492-a34b-56bbe15ec291)

### Sensitive fields encrypted in the database

![Db encrypted](https://github.com/louresb/CryptographyAPI/assets/103293696/15ad45a8-d973-4d09-bffb-ee509837dc69)

### Data decrypted transparently by the API

![Get](https://github.com/louresb/CryptographyAPI/assets/103293696/803e27b3-a0b2-4f13-94aa-5d3e493ee74a)

</div>

## Run locally

Requirements:

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Microsoft SQL Server

Clone the repository and configure the local secrets from the project directory:

```powershell
$encryptionKey = [Convert]::ToBase64String(
    [Security.Cryptography.RandomNumberGenerator]::GetBytes(32)
)

dotnet user-secrets set "Encryption:Key" $encryptionKey
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<SQL Server connection string>"
```

Create the database and start the API:

```powershell
dotnet ef database update
dotnet run
```

Open the Swagger UI using the URL shown in the terminal.

Environment variables can also be used with the names `Encryption__Key` and `ConnectionStrings__DefaultConnection`.

## Tests

The test suite verifies encryption round trips, unique ciphertext, tamper detection and transparent database conversion.

```powershell
dotnet test --configuration Release
```

## License

[MIT License](LICENSE) © [Bruno Loures](https://github.com/louresb)
