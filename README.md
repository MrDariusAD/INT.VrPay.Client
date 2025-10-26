# INT.VrPay.Client

A .NET client library for the VR Payment Gateway API with support for pre-authorization, debit, capture, refund, and reversal operations.

[![.NET 9.0](https://img.shields.io/badge/.NET-9.0-512BD4)](https://dotnet.microsoft.com/download/dotnet/9.0)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

## Features

- **Complete Payment Operations**: Pre-authorization, Debit, Capture, Refund, and Reversal
- **Domain-Driven Design**: Clean architecture with Domain, Application, and Infrastructure layers
- **Type-Safe**: Strongly-typed models with enum converters
- **Resilient**: Built-in retry policies using Polly
- **Comprehensive Logging**: Integrated logging support
- **Well-Tested**: Extensive unit and integration test coverage
- **Async/Await**: Fully asynchronous API
- **Dependency Injection**: Native support for ASP.NET Core DI

## Installation

Add the GitHub Packages source to your NuGet configuration:

```bash
dotnet nuget add source --username YOUR_GITHUB_USERNAME --password YOUR_GITHUB_PAT --store-password-in-clear-text --name github "https://nuget.pkg.github.com/OWNER/index.json"
```

Install the package:

```bash
dotnet add package INT.VrPay.Client
```

Or add to your `.csproj`:

```xml
<PackageReference Include="INT.VrPay.Client" Version="1.0.0" />
```

## Quick Start

### 1. Configuration

Add VrPay settings to `appsettings.json`:

```json
{
  "VrPay": {
    "BaseUrl": "https://test.vr-pay-ecommerce.de",
    "EntityId": "your-entity-id",
    "AccessToken": "Bearer your-access-token",
    "UseTestMode": true,
    "TestModeValue": "EXTERNAL",
    "TimeoutSeconds": 30,
    "MaxRetries": 3
  }
}
```

### 2. Register Services

In `Program.cs`:

```csharp
using INT.VrPay.Client.Extensions;

builder.Services.AddVrPayClient(
    builder.Configuration.GetSection("VrPay"));
```

### 3. Use the Client

```csharp
using INT.VrPay.Client;
using INT.VrPay.Client.Models;

public class PaymentService
{
    private readonly IVrPayClient _vrPayClient;

    public PaymentService(IVrPayClient vrPayClient)
    {
        _vrPayClient = vrPayClient;
    }

    public async Task<string> ProcessPaymentAsync(decimal amount)
    {
        var request = new PaymentRequest
        {
            Amount = amount.ToString("F2"),
            Currency = Currency.EUR,
            PaymentBrand = PaymentBrand.Visa,
            Card = new CardData
            {
                Number = "4200000000000000",
                Holder = "Jane Doe",
                ExpiryMonth = "12",
                ExpiryYear = "2025",
                Cvv = "123"
            },
            Customer = new CustomerData
            {
                GivenName = "Jane",
                Surname = "Doe",
                Email = "jane.doe@example.com",
                Ip = "127.0.0.1"
            }
        };

        var response = await _vrPayClient.DebitAsync(request);
        return response.Id;
    }
}
```

## Usage Examples

### Pre-Authorization

Reserve funds without capturing them:

```csharp
var request = new PaymentRequest
{
    Amount = "92.00",
    Currency = Currency.EUR,
    PaymentBrand = PaymentBrand.Visa,
    MerchantTransactionId = $"ORDER-{Guid.NewGuid()}",
    Card = new CardData
    {
        Number = "4200000000000000",
        Holder = "John Doe",
        ExpiryMonth = "05",
        ExpiryYear = "2026",
        Cvv = "123"
    },
    Customer = new CustomerData
    {
        GivenName = "John",
        Surname = "Doe",
        Email = "john.doe@example.com",
        Ip = "192.168.1.1"
    }
};

var response = await vrPayClient.PreAuthorizeAsync(request);
Console.WriteLine($"Pre-Auth ID: {response.Id}");
```

### Direct Debit

Immediately charge a card:

```csharp
var request = new PaymentRequest
{
    Amount = "25.50",
    Currency = Currency.EUR,
    PaymentBrand = PaymentBrand.Master,
    Card = new CardData
    {
        Number = "5454545454545454",
        Holder = "Jane Smith",
        ExpiryMonth = "12",
        ExpiryYear = "2025",
        Cvv = "456"
    }
};

var response = await vrPayClient.DebitAsync(request);
```

### Capture

Capture a pre-authorized payment:

```csharp
// Full capture
var captureResponse = await vrPayClient.CaptureAsync(
    preAuthId, 
    amount: 92.00m, 
    currency: Currency.EUR
);

// Partial capture
var partialCapture = await vrPayClient.CaptureAsync(
    preAuthId, 
    amount: 50.00m, 
    currency: Currency.EUR
);
```

### Refund

Refund a captured payment:

```csharp
// Full refund
var refundResponse = await vrPayClient.RefundAsync(
    captureId, 
    amount: 25.50m, 
    currency: Currency.EUR
);
```

### Reversal

Cancel a pre-authorization:

```csharp
var reversalResponse = await vrPayClient.ReverseAsync(
    preAuthId, 
    amount: 92.00m, 
    currency: Currency.EUR
);
```

## Test Cards

For testing in the VrPay test environment:

| Card Number | Brand | Result |
|-------------|-------|--------|
| 4200000000000000 | Visa | Success |
| 5454545454545454 | Mastercard | Success |
| 378282246310005 | Amex | Success |
| 4000300011112220 | Visa | Declined |
| 5555444433331111 | Mastercard | Declined |

## Error Handling

The library provides specific exception types:

```csharp
using INT.VrPay.Client.Exceptions;

try
{
    var response = await vrPayClient.DebitAsync(request);
}
catch (VrPayPaymentDeclinedException ex)
{
    Console.WriteLine($"Declined: {ex.ResultCode}");
}
catch (VrPayValidationException ex)
{
    foreach (var error in ex.ValidationErrors)
    {
        Console.WriteLine($"Error: {error}");
    }
}
catch (VrPayCommunicationException ex)
{
    Console.WriteLine($"Communication error: {ex.Message}");
}
catch (VrPayConfigurationException ex)
{
    Console.WriteLine($"Config error: {ex.Message}");
}
```

## Domain Model

### Key Enums

**PaymentType**:
- `PreAuthorization` (PA)
- `Debit` (DB)
- `Capture` (CP)
- `Refund` (RF)
- `Reversal` (RV)

**PaymentBrand**:
- `Visa`, `Master`, `Amex`, `Maestro`, and more

**Currency**:
- `EUR`, `USD`, `GBP`, `CHF`, `JPY`, etc. (ISO 4217)

**TransactionStatus**:
- `Success`, `Pending`, `Declined`, `Error`, `Unknown`

## Development

### Building

```bash
dotnet build src/INT.VrPay.Client.slnx
```

### Testing

```bash
# Run all tests
dotnet test src/INT.VrPay.Client.slnx

# Unit tests only
dotnet test --filter "FullyQualifiedName~INT.VrPay.Client.Tests"

# Integration tests only
dotnet test --filter "FullyQualifiedName~INT.VrPay.Client.IntegrationTests"
```

### Creating a Package

```bash
dotnet pack --configuration Release
```

## Requirements

- .NET 9.0 or higher
- Visual Studio 2022 17.8+ or Rider 2024.1+

## Dependencies

- Microsoft.Extensions.Http
- Microsoft.Extensions.Http.Polly
- Polly
- System.Text.Json

## Contributing

Contributions are welcome! Please:

1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push to the branch
5. Open a Pull Request

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Support

- [Report bugs](https://github.com/yourusername/INT.VrPay.Client/issues)
- [Request features](https://github.com/yourusername/INT.VrPay.Client/issues)
- [VrPay Documentation](https://vr-pay-ecommerce.docs.oppwa.com/)
