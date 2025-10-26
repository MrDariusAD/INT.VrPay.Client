# INT.VrPay.Client# VrPay.Client



A robust .NET client library for the VR Payment Gateway API with support for pre-authorization, debit, capture, refund, and reversal operations. Built with Domain-Driven Design principles and production-ready features.A .NET 9.0 client library for the VrPay eCommerce Server-to-Server Payment integration.



[![.NET 9.0](https://img.shields.io/badge/.NET-9.0-512BD4)](https://dotnet.microsoft.com/download/dotnet/9.0)[![CI/CD](https://github.com/your-org/INT.VrPay/actions/workflows/ci-cd.yml/badge.svg)](https://github.com/your-org/INT.VrPay/actions/workflows/ci-cd.yml)

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)[![NuGet](https://img.shields.io/nuget/v/VrPay.Client.svg)](https://www.nuget.org/packages/VrPay.Client/)



## Features## Features



- ✅ **Complete Payment Operations**: Pre-authorization, Debit, Capture, Refund, and Reversal✅ **Synchronous Payment Support**

- 🏗️ **Domain-Driven Design**: Clean architecture with Domain, Application, and Infrastructure layers- Pre-Authorization (PA)

- 🔒 **Type-Safe**: Strongly-typed models with enum converters for API communication- Direct Debit (DB)

- 🔄 **Resilient**: Built-in retry policies using Polly for transient failures- Capture (CP)

- 📝 **Comprehensive Logging**: Integrated logging support via `ILogger`- Refund (RF)

- 🧪 **Well-Tested**: Extensive unit and integration test coverage- Reversal (RV)

- ⚡ **Async/Await**: Fully asynchronous API for optimal performance

- 🎯 **Dependency Injection**: Native support for ASP.NET Core DI container✅ **Modern .NET 9.0**

- 📦 **NuGet Package**: Easy installation and version management- Built with latest C# 12 features

- Native AOT compatible

## Table of Contents- Nullable reference types enabled



- [Installation](#installation)✅ **Production Ready**

- [Quick Start](#quick-start)- Comprehensive error handling

- [Configuration](#configuration)- Built-in retry logic with Polly

- [Usage Examples](#usage-examples)- Extensive logging support

  - [Pre-Authorization](#pre-authorization)- Full XML documentation

  - [Direct Debit](#direct-debit)

  - [Capture](#capture)✅ **Developer Friendly**

  - [Refund](#refund)- Dependency Injection support

  - [Reversal](#reversal)- Fluent API

- [Test Cards](#test-cards)- Strong typing

- [Error Handling](#error-handling)- Extension methods

- [Domain Model](#domain-model)

- [Testing](#testing)## Installation

- [Contributing](#contributing)

- [License](#license)Install via NuGet Package Manager:



## Installation```bash

dotnet add package VrPay.Client

Install the package via NuGet Package Manager:```



```bashOr via Package Manager Console:

dotnet add package INT.VrPay.Client

``````powershell

Install-Package VrPay.Client

Or via Package Manager Console:```



```powershell## Quick Start

Install-Package INT.VrPay.Client

```### 1. Configuration



Or add directly to your `.csproj` file:Add VrPay configuration to your `appsettings.json`:



```xml```json

<PackageReference Include="INT.VrPay.Client" Version="1.0.0" />{

```  "VrPay": {

    "BaseUrl": "https://test.vr-pay-ecommerce.de/",

## Quick Start    "EntityId": "YOUR_ENTITY_ID",

    "AccessToken": "Bearer YOUR_ACCESS_TOKEN",

### 1. Configure in `appsettings.json`    "TimeoutSeconds": 30,

    "TestMode": true,

```json    "TestModeValue": "EXTERNAL"

{  }

  "VrPay": {}

    "BaseUrl": "https://test.vr-pay-ecommerce.de",```

    "EntityId": "your-entity-id",

    "AccessToken": "Bearer your-access-token",Set your access token as an environment variable (recommended for production):

    "UseTestMode": true,

    "TestModeValue": "EXTERNAL",```bash

    "TimeoutSeconds": 30,# Windows

    "MaxRetries": 3$env:VRPAY_ACCESS_TOKEN="Bearer your-token-here"

  }

}# Linux/Mac

```export VRPAY_ACCESS_TOKEN="Bearer your-token-here"

```

### 2. Register in `Program.cs` (ASP.NET Core)

### 2. Register Services

```csharp

using INT.VrPay.Client.Extensions;In your `Program.cs` or `Startup.cs`:



var builder = WebApplication.CreateBuilder(args);```csharp

using VrPay.Client.Extensions;

// Add VrPay client with configuration

builder.Services.AddVrPayClient(// Add VrPay client

    builder.Configuration.GetSection("VrPay"));builder.Services.AddVrPayClient(builder.Configuration);

```

var app = builder.Build();

```### 3. Use the Client



### 3. Inject and UseInject `IVrPayClient` into your services:



```csharp```csharp

using INT.VrPay.Client;using VrPay.Client;

using INT.VrPay.Client.Models;using VrPay.Client.Models;

using VrPay.Client.Extensions;

public class PaymentService

{public class PaymentService

    private readonly IVrPayClient _vrPayClient;{

    private readonly IVrPayClient _vrPayClient;

    public PaymentService(IVrPayClient vrPayClient)

    {    public PaymentService(IVrPayClient vrPayClient)

        _vrPayClient = vrPayClient;    {

    }        _vrPayClient = vrPayClient;

    }

    public async Task<string> ProcessPaymentAsync(decimal amount)

    {    public async Task<string> ProcessPaymentAsync()

        var request = new PaymentRequest    {

        {        // Create payment request

            Amount = amount.ToString("F2"),        var request = new PaymentRequest

            Currency = Currency.EUR,        {

            PaymentBrand = PaymentBrand.Visa,            Amount = "92.00",

            Card = new CardData            Currency = "EUR",

            {            PaymentBrand = "VISA",

                Number = "4200000000000000",            MerchantTransactionId = $"ORDER-{Guid.NewGuid()}",

                Holder = "Jane Doe",            Card = new CardData

                ExpiryMonth = "12",            {

                ExpiryYear = "2025",                Number = "4200000000000000",

                Cvv = "123"                Holder = "John Doe",

            },                ExpiryMonth = "12",

            Customer = new CustomerData                ExpiryYear = "2034",

            {                Cvv = "123"

                GivenName = "Jane",            },

                Surname = "Doe",            Customer = new CustomerData

                Email = "jane.doe@example.com",            {

                Ip = "127.0.0.1"                GivenName = "John",

            }                Surname = "Doe",

        };                Email = "john.doe@example.com"

            }

        var response = await _vrPayClient.DebitAsync(request);        };

        return response.Id;

    }        // Pre-authorize payment

}        var response = await _vrPayClient.PreAuthorizeAsync(request);

```

        // Check if successful

## Configuration        if (response.IsSuccess())

        {

### VrPayConfiguration Properties            // Store transaction ID for later capture

            return response.Id;

| Property | Type | Required | Description |        }

|----------|------|----------|-------------|

| `BaseUrl` | `string` | ✅ | VrPay API base URL |        throw new Exception($"Payment failed: {response.Result.Description}");

| `EntityId` | `string` | ✅ | Your merchant entity ID |    }

| `AccessToken` | `string` | ✅ | Bearer access token |

| `UseTestMode` | `bool` | ❌ | Enable test mode (default: `false`) |    public async Task CapturePaymentAsync(string preAuthId, decimal amount)

| `TestModeValue` | `string` | ❌ | Test mode type: `EXTERNAL` or `INTERNAL` |    {

| `TimeoutSeconds` | `int` | ❌ | HTTP timeout in seconds (default: `30`) |        // Capture the pre-authorized payment

| `MaxRetries` | `int` | ❌ | Number of retry attempts (default: `3`) |        var response = await _vrPayClient.CaptureAsync(preAuthId, amount, "EUR");



### Environment-Specific Configuration        if (!response.IsSuccess())

        {

**appsettings.json** (base configuration):            throw new Exception($"Capture failed: {response.Result.Description}");

```json        }

{    }

  "VrPay": {}

    "BaseUrl": "https://test.vr-pay-ecommerce.de",```

    "EntityId": "",

    "AccessToken": "",## Payment Flows

    "UseTestMode": true

  }### Two-Step Payment (Pre-Authorization + Capture)

}

``````csharp

// Step 1: Pre-authorize

**appsettings.Development.json** (with secrets):var preAuthResponse = await vrPayClient.PreAuthorizeAsync(request);

```jsonvar transactionId = preAuthResponse.Id;

{

  "VrPay": {// Step 2: Capture (can be partial)

    "EntityId": "8a8294174e735d0c014e78beb6b9154b",var captureResponse = await vrPayClient.CaptureAsync(transactionId, 50.00m, "EUR");

    "AccessToken": "Bearer OGE4Mjk0MTc0ZTczNWQwYzAxNGU3OGJlYjZjNTE1NGZ8TnE3b0UlMmEyQldKcjQlJTZYZFk="```

  }

}### One-Step Payment (Direct Debit)

```

```csharp

## Usage Examples// Direct debit - immediately charges the card

var response = await vrPayClient.DebitAsync(request);

### Pre-Authorization```



Reserve funds without capturing them immediately:### Refund



```csharp```csharp

using INT.VrPay.Client.Models;// Refund a captured payment

var refundResponse = await vrPayClient.RefundAsync(captureId, 50.00m, "EUR");

var request = new PaymentRequest```

{

    Amount = "92.00",### Reversal

    Currency = Currency.EUR,

    PaymentBrand = PaymentBrand.Visa,```csharp

    MerchantTransactionId = $"ORDER-{Guid.NewGuid()}",// Cancel a pre-authorization

    Card = new CardDatavar reversalResponse = await vrPayClient.ReverseAsync(preAuthId);

    {```

        Number = "4200000000000000",

        Holder = "John Doe",## Error Handling

        ExpiryMonth = "05",

        ExpiryYear = "2026",The client provides specific exceptions for different error scenarios:

        Cvv = "123"

    },```csharp

    Customer = new CustomerDatausing VrPay.Client.Exceptions;

    {

        GivenName = "John",try

        Surname = "Doe",{

        Email = "john.doe@example.com",    var response = await vrPayClient.PreAuthorizeAsync(request);

        Ip = "192.168.1.1"}

    },catch (VrPayPaymentDeclinedException ex)

    Billing = new AddressData{

    {    // Payment was declined by the bank

        Street1 = "123 Main St",    Console.WriteLine($"Declined: {ex.ResultCode} - {ex.Response.Result.Description}");

        City = "Berlin",}

        Postcode = "10115",catch (VrPayValidationException ex)

        Country = "DE"{

    }    // Request validation failed

};    foreach (var error in ex.ValidationErrors)

    {

var response = await vrPayClient.PreAuthorizeAsync(request);        Console.WriteLine($"Validation Error: {error}");

    }

Console.WriteLine($"Pre-Auth ID: {response.Id}");}

Console.WriteLine($"Status: {response.GetStatus()}");catch (VrPayCommunicationException ex)

Console.WriteLine($"Result: {response.Result.Description}");{

```    // Network or API communication error

    Console.WriteLine($"Communication Error: {ex.Message}");

### Direct Debit}

catch (VrPayConfigurationException ex)

Immediately charge a card:{

    // Configuration is invalid

```csharp    Console.WriteLine($"Configuration Error: {ex.Message}");

var request = new PaymentRequest}

{```

    Amount = "25.50",

    Currency = Currency.EUR,## Result Code Analysis

    PaymentBrand = PaymentBrand.Master,

    Card = new CardDataThe client automatically analyzes result codes:

    {

        Number = "5454545454545454",```csharp

        Holder = "Jane Smith",var response = await vrPayClient.PreAuthorizeAsync(request);

        ExpiryMonth = "12",

        ExpiryYear = "2025",// Check status

        Cvv = "456"var status = response.GetStatus();

    }

};switch (status)

{

var response = await vrPayClient.DebitAsync(request);    case TransactionStatus.Success:

        // Payment successful

if (response.IsSuccess())        break;

{    case TransactionStatus.SuccessManualReview:

    Console.WriteLine($"Payment successful: {response.Id}");        // Successful but requires manual review

}        break;

```    case TransactionStatus.SoftDecline:

        // Can be retried (e.g., with 3D Secure)

### Capture        break;

    case TransactionStatus.HardDecline:

Capture a previously pre-authorized payment:        // Should not be retried

        break;

```csharp}

// After pre-authorization

string preAuthId = response.Id;// Or simple check

if (response.IsSuccess())

// Capture the full amount{

var captureResponse = await vrPayClient.CaptureAsync(    // Process payment

    preAuthId, }

    amount: 92.00m, 

    currency: Currency.EUR// Check if requires manual review

);if (response.RequiresManualReview())

{

// Or capture a partial amount    // Flag for manual review

var partialCapture = await vrPayClient.CaptureAsync(}

    preAuthId, ```

    amount: 50.00m, 

    currency: Currency.EUR## Test Cards

);

```For testing in the VrPay test environment:



### Refund| Card Number          | Result   |

|---------------------|----------|

Refund a captured payment:| 4200000000000000    | ✅ Success |

| 4000000000000002    | ❌ Declined |

```csharp

// After debit or capture## Development

string captureId = response.Id;

### Prerequisites

// Full refund

var refundResponse = await vrPayClient.RefundAsync(- .NET 9.0 SDK or later

    captureId, - Visual Studio 2022 (17.8+), VS Code, or Rider

    amount: 25.50m, 

    currency: Currency.EUR### Building

);

```bash

// Partial refund# Build the solution

var partialRefund = await vrPayClient.RefundAsync(./scripts/build.ps1

    captureId, 

    amount: 10.00m, # Or manually

    currency: Currency.EURdotnet build src/INT.VrPay.Client.slnx

);```

```

### Testing

### Reversal

```bash

Cancel a pre-authorization before capture:# Run unit tests

./scripts/test.ps1 -TestType Unit

```csharp

// After pre-authorization# Run all tests

string preAuthId = response.Id;./scripts/test.ps1 -TestType All



var reversalResponse = await vrPayClient.ReverseAsync(# Or manually

    preAuthId,dotnet test src/INT.VrPay.Client.slnx

    amount: 92.00m,```

    currency: Currency.EUR

);### Creating a Package



Console.WriteLine($"Reversal successful: {reversalResponse.Id}");```bash

```# Create NuGet package

./scripts/package.ps1

## Test Cards

# Or manually

The library includes predefined test cards for development:dotnet pack --configuration Release

```

```csharp

using INT.VrPay.Client.Testing;## Documentation



// Successful transactionsComprehensive documentation is available in the [docs](./docs) folder:

var visaSuccess = TestCards.VisaSuccess;        // 4200000000000000

var masterSuccess = TestCards.MastercardSuccess; // 5454545454545454- [Quick Reference](./docs/QUICK-REFERENCE.md)

var amexSuccess = TestCards.AmexSuccess;         // 378282246310005- [Implementation Plan](./docs/IMPLEMENTATION-PLAN.md)

- [API Overview](./docs/01-api-overview.md)

// Declined transactions- [Authentication & Configuration](./docs/02-authentication-configuration.md)

var visaDeclined = TestCards.VisaDeclined;       // 4000300011112220- [Payment Flows](./docs/03-synchronous-payment-flow.md)

var masterDeclined = TestCards.MastercardDeclined; // 5555444433331111- [Payment Models](./docs/04-payment-models.md)

- [Result Codes](./docs/05-result-codes.md)

// Use in requests- [Error Handling](./docs/06-error-handling.md)

var request = TestData.CreateSuccessfulPaymentRequest(- [Implementation Guide](./docs/07-implementation-guide.md)

    amount: 50.00m,- [Architecture](./docs/08-dotnet-client-architecture.md)

    currency: Currency.EUR,- [Testing Guide](./docs/09-testing-guide.md)

    paymentBrand: PaymentBrand.Visa

);## Security Considerations

```

⚠️ **Important Security Notes:**

## Error Handling

- **Never log or store CVV codes** after the initial authorization

The library provides specific exception types for different error scenarios:- **Use HTTPS only** for all API communications

- **Store access tokens securely** (use environment variables or secret management)

```csharp- **Implement PCI-DSS compliance** if handling card data directly

using INT.VrPay.Client.Exceptions;- **Follow tokenization best practices** for recurring payments



try## License

{

    var response = await vrPayClient.DebitAsync(request);This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

}

catch (VrPayPaymentDeclinedException ex)## Support

{

    // Payment was declined by the bankFor issues and questions:

    Console.WriteLine($"Payment declined: {ex.ResultCode}");- GitHub Issues: [https://github.com/your-org/INT.VrPay/issues](https://github.com/your-org/INT.VrPay/issues)

    Console.WriteLine($"Message: {ex.Message}");- VrPay Documentation: [https://vr-pay-ecommerce.docs.oppwa.com/](https://vr-pay-ecommerce.docs.oppwa.com/)

    Console.WriteLine($"Transaction ID: {ex.Response?.Id}");

}## Contributing

catch (VrPayValidationException ex)

{Contributions are welcome! Please feel free to submit a Pull Request.

    // Request validation failed

    Console.WriteLine("Validation errors:");## Changelog

    foreach (var error in ex.ValidationErrors)

    {### Version 1.0.0

        Console.WriteLine($"  - {error}");- Initial release

    }- Support for synchronous payments (PA, DB, CP, RF, RV)

}- Comprehensive error handling

catch (VrPayCommunicationException ex)- Full .NET 9.0 support

{- Integration tests

    // Network or HTTP error- Complete documentation

    Console.WriteLine($"Communication error: {ex.Message}");
    Console.WriteLine($"Status Code: {ex.StatusCode}");
}
catch (VrPayConfigurationException ex)
{
    // Configuration error
    Console.WriteLine($"Configuration error: {ex.Message}");
}
catch (VrPayException ex)
{
    // General VrPay error
    Console.WriteLine($"Error: {ex.Message}");
}
```

## Domain Model

The library follows Domain-Driven Design principles:

### Domain Layer
- **Entities**: `PaymentRequest`, `PaymentResponse`
- **Value Objects**: `CardData`, `CustomerData`, `AddressData`
- **Enums**: `PaymentType`, `PaymentBrand`, `Currency`, `TransactionStatus`, `TestMode`
- **Services**: `ResultCodeAnalyzer`

### Application Layer
- **Interfaces**: `IVrPayClient`

### Infrastructure Layer
- **HTTP**: `VrPayClient`
- **Serialization**: `EnumMemberConverter`

### Key Enums

**PaymentType**:
- `PreAuthorization` (PA)
- `Debit` (DB)
- `Capture` (CP)
- `Refund` (RF)
- `Reversal` (RV)

**PaymentBrand**:
- `Visa`
- `Master`
- `Amex`
- `Maestro`
- And more...

**Currency**:
- `EUR`, `USD`, `GBP`, `CHF`, `JPY`, etc. (ISO 4217)

**TransactionStatus**:
- `Success`
- `Pending`
- `Declined`
- `Error`
- `Unknown`

## Testing

The project includes comprehensive tests:

### Run All Tests

```bash
dotnet test
```

### Run Unit Tests Only

```bash
dotnet test --filter "FullyQualifiedName~INT.VrPay.Client.Tests"
```

### Run Integration Tests Only

```bash
dotnet test --filter "FullyQualifiedName~INT.VrPay.Client.IntegrationTests"
```

### Test Coverage

- **Unit Tests**: 54+ tests covering configuration, models, and services
- **Integration Tests**: 8 tests with real API calls to test environment
- All tests use FluentAssertions for readable assertions

## Project Structure

```
INT.VrPay/
├── src/
│   ├── INT.VrPay.Client/              # Main library
│   │   ├── Application/               # Application layer (interfaces)
│   │   ├── Domain/                    # Domain layer (entities, value objects, enums)
│   │   ├── Infrastructure/            # Infrastructure layer (HTTP, serialization)
│   │   ├── Configuration/             # Configuration models
│   │   ├── Exceptions/                # Custom exceptions
│   │   ├── Extensions/                # Extension methods
│   │   └── Testing/                   # Test helpers and data
│   └── Tests/
│       ├── INT.VrPay.Client.Tests/            # Unit tests
│       └── INT.VrPay.Client.IntegrationTests/ # Integration tests
├── samples/
│   └── INT.VrPay.Samples.Console/     # Console application sample
├── docs/                               # Documentation
├── scripts/                            # Build and deployment scripts
└── README.md                           # This file
```

## Requirements

- .NET 9.0 or higher
- Visual Studio 2022 17.8+ or Rider 2024.1+

## Dependencies

- `Microsoft.Extensions.Http` - HTTP client factory
- `Microsoft.Extensions.Http.Polly` - Resilience policies
- `Polly` - Retry and circuit breaker policies
- `System.Text.Json` - JSON serialization

## Contributing

Contributions are welcome! Please follow these steps:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

### Development Setup

```bash
# Clone the repository
git clone https://github.com/yourusername/INT.VrPay.Client.git
cd INT.VrPay.Client

# Restore dependencies
dotnet restore

# Build the solution
dotnet build src/INT.VrPay.Client.slnx

# Run tests
dotnet test src/INT.VrPay.Client.slnx

# Run sample console app
dotnet run --project samples/INT.VrPay.Samples.Console
```

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Support

For issues, questions, or contributions, please:
- 🐛 [Report bugs](https://github.com/yourusername/INT.VrPay.Client/issues)
- 💡 [Request features](https://github.com/yourusername/INT.VrPay.Client/issues)
- 📖 [Read the docs](https://github.com/yourusername/INT.VrPay.Client/wiki)

## Changelog

### Version 1.0.0 (Initial Release)
- Complete payment operations (PA, DB, CP, RF, RV)
- Domain-Driven Design architecture
- Comprehensive error handling
- Test card support
- Full async/await support
- Integration with ASP.NET Core DI
- Extensive test coverage

---

Made with ❤️ by INT
