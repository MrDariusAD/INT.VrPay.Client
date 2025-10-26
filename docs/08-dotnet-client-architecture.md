# .NET Client Architecture

## Architecture Overview

The VrPay .NET Client follows a layered architecture with clean separation of concerns, dependency injection, and robust error handling.

```
┌─────────────────────────────────────────────────────────┐
│                    Consumer Layer                        │
│  (ASP.NET Core Controllers, Services, Console Apps)     │
└────────────────────┬────────────────────────────────────┘
                     │
┌────────────────────┴────────────────────────────────────┐
│                    Client Layer                          │
│                  (IVrPayClient)                         │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐ │
│  │ PreAuthorize │  │   Capture    │  │    Debit     │ │
│  └──────────────┘  └──────────────┘  └──────────────┘ │
└────────────────────┬────────────────────────────────────┘
                     │
┌────────────────────┴────────────────────────────────────┐
│                  HTTP Layer                              │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐    │
│  │   Polly     │  │   Logging   │  │   Metrics   │    │
│  │  Policies   │  │             │  │             │    │
│  └─────────────┘  └─────────────┘  └─────────────┘    │
└────────────────────┬────────────────────────────────────┘
                     │
┌────────────────────┴────────────────────────────────────┐
│              VrPay eCommerce API                         │
│           https://vr-pay-ecommerce.de/                   │
└──────────────────────────────────────────────────────────┘
```

## Project Structure

```
VrPay.Client.sln
│
├── VrPay.Client/                           # Core client library
│   ├── Configuration/
│   │   └── VrPaySettings.cs               # Configuration model
│   ├── Exceptions/
│   │   ├── VrPayException.cs              # Base exception
│   │   ├── VrPayPaymentDeclinedException.cs
│   │   ├── VrPayCommunicationException.cs
│   │   └── VrPayValidationException.cs
│   ├── Extensions/
│   │   └── ServiceCollectionExtensions.cs  # DI registration
│   ├── Policies/
│   │   └── HttpPolicies.cs                 # Polly retry policies
│   ├── Services/
│   │   ├── ResultCodeAnalyzer.cs           # Result code analysis
│   │   └── PaymentRequestValidator.cs      # Input validation
│   ├── IVrPayClient.cs                     # Client interface
│   └── VrPayClient.cs                      # Client implementation
│
├── VrPay.Client.Models/                    # Shared models
│   ├── Enums/
│   │   ├── PaymentType.cs
│   │   ├── PaymentBrand.cs
│   │   └── TransactionStatus.cs
│   ├── Requests/
│   │   ├── PaymentRequest.cs
│   │   ├── CardData.cs
│   │   ├── CustomerData.cs
│   │   └── AddressData.cs
│   └── Responses/
│       ├── PaymentResponse.cs
│       ├── ResultData.cs
│       └── CardResponseData.cs
│
└── VrPay.Client.Tests/                     # Unit tests
    ├── VrPayClientTests.cs
    ├── ResultCodeAnalyzerTests.cs
    └── Fixtures/
        └── TestDataBuilder.cs
```

## Key Components

### 1. IVrPayClient Interface

```csharp
public interface IVrPayClient
{
    // Pre-authorization + Capture flow
    Task<PaymentResponse> PreAuthorizeAsync(
        PaymentRequest request, 
        CancellationToken cancellationToken = default);
    
    Task<PaymentResponse> CaptureAsync(
        string preAuthId,
        decimal amount,
        string currency,
        CancellationToken cancellationToken = default);
    
    // Direct debit flow
    Task<PaymentResponse> DebitAsync(
        PaymentRequest request,
        CancellationToken cancellationToken = default);
    
    // Back-office operations
    Task<PaymentResponse> RefundAsync(
        string transactionId,
        decimal amount,
        string currency,
        CancellationToken cancellationToken = default);
    
    Task<PaymentResponse> ReverseAsync(
        string transactionId,
        CancellationToken cancellationToken = default);
}
```

### 2. VrPayClient Implementation

**Responsibilities**:
- Execute HTTP requests to VrPay API
- Handle request/response serialization
- Apply entity ID and test mode from configuration
- Delegate error handling
- Structured logging

**Key Methods**:
```csharp
private async Task<PaymentResponse> SendPaymentRequestAsync(
    PaymentRequest request,
    string endpoint,
    CancellationToken cancellationToken)
{
    // 1. Convert request to form data
    // 2. Send HTTP POST request
    // 3. Handle HTTP errors
    // 4. Deserialize response
    // 5. Analyze result code
    // 6. Return or throw exception
}
```

### 3. Result Code Analyzer

```csharp
public class ResultCodeAnalyzer
{
    public static TransactionStatus AnalyzeResultCode(string code)
    {
        // Pattern matching using regex
        // Returns appropriate TransactionStatus enum
    }
    
    public static bool IsSuccess(string code)
    {
        return Regex.IsMatch(code, SuccessPattern);
    }
    
    public static bool RequiresManualReview(string code)
    {
        return Regex.IsMatch(code, ManualReviewPattern);
    }
}
```

### 4. HTTP Policies (Polly)

```csharp
public static class HttpPolicies
{
    public static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(msg => ShouldRetryStatusCode(msg.StatusCode))
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: attemptNumber => 
                    TimeSpan.FromSeconds(Math.Pow(2, attemptNumber - 1)),
                onRetry: (outcome, timespan, attemptNumber, context) =>
                {
                    // Log retry attempt
                });
    }
    
    public static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromSeconds(30));
    }
}
```

## Design Patterns

### 1. Repository Pattern (Optional)

For storing and retrieving payment records:

```csharp
public interface IPaymentRepository
{
    Task<Payment> GetByIdAsync(string paymentId);
    Task SaveAsync(Payment payment);
    Task<IEnumerable<Payment>> GetByMerchantTransactionIdAsync(string txId);
}

public class Payment
{
    public string Id { get; set; }
    public string MerchantTransactionId { get; set; }
    public string PaymentType { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; }
    public string Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public PaymentResponse? Response { get; set; }
}
```

### 2. Builder Pattern

For constructing complex payment requests:

```csharp
public class PaymentRequestBuilder
{
    private readonly PaymentRequest _request = new();
    
    public PaymentRequestBuilder WithAmount(decimal amount, string currency)
    {
        _request.Amount = amount;
        _request.Currency = currency;
        return this;
    }
    
    public PaymentRequestBuilder WithCard(
        string number, 
        string holder, 
        string expiryMonth, 
        string expiryYear, 
        string cvv)
    {
        _request.Card = new CardData
        {
            Number = number,
            Holder = holder,
            ExpiryMonth = expiryMonth,
            ExpiryYear = expiryYear,
            Cvv = cvv
        };
        return this;
    }
    
    public PaymentRequestBuilder WithCustomer(string email, string ip)
    {
        _request.Customer = new CustomerData
        {
            Email = email,
            Ip = ip
        };
        return this;
    }
    
    public PaymentRequest Build()
    {
        Validate();
        return _request;
    }
}
```

### 3. Strategy Pattern

For different payment strategies:

```csharp
public interface IPaymentStrategy
{
    Task<PaymentResponse> ExecuteAsync(
        PaymentRequest request, 
        CancellationToken cancellationToken);
}

public class PreAuthStrategy : IPaymentStrategy
{
    private readonly IVrPayClient _client;
    
    public async Task<PaymentResponse> ExecuteAsync(
        PaymentRequest request, 
        CancellationToken cancellationToken)
    {
        return await _client.PreAuthorizeAsync(request, cancellationToken);
    }
}

public class DirectDebitStrategy : IPaymentStrategy
{
    private readonly IVrPayClient _client;
    
    public async Task<PaymentResponse> ExecuteAsync(
        PaymentRequest request, 
        CancellationToken cancellationToken)
    {
        return await _client.DebitAsync(request, cancellationToken);
    }
}
```

## Configuration Management

### Multi-Environment Setup

```csharp
public class VrPayEnvironmentSettings
{
    public Dictionary<string, EnvironmentConfig> Environments { get; set; }
    
    public EnvironmentConfig GetEnvironment(string name)
    {
        return Environments.TryGetValue(name, out var config) 
            ? config 
            : throw new InvalidOperationException($"Environment '{name}' not found");
    }
}

public class EnvironmentConfig
{
    public string BaseUrl { get; set; }
    public string EntityId { get; set; }
    public string? TestMode { get; set; }
}
```

**appsettings.json**:
```json
{
  "VrPay": {
    "Environments": {
      "Test": {
        "BaseUrl": "https://test.vr-pay-ecommerce.de/",
        "EntityId": "8a8294174e735d0c014e78beb6b9154b",
        "TestMode": "INTERNAL"
      },
      "Production": {
        "BaseUrl": "https://vr-pay-ecommerce.de/",
        "EntityId": "[PRODUCTION_ENTITY_ID]"
      }
    },
    "ActiveEnvironment": "Test"
  }
}
```

## Logging Strategy

### Structured Logging with Scopes

```csharp
public async Task<PaymentResponse> PreAuthorizeAsync(PaymentRequest request, ...)
{
    using (_logger.BeginScope(new Dictionary<string, object>
    {
        ["OperationType"] = "PreAuthorization",
        ["MerchantTxId"] = request.MerchantTransactionId,
        ["Amount"] = request.Amount,
        ["Currency"] = request.Currency,
        ["CorrelationId"] = Guid.NewGuid()
    }))
    {
        _logger.LogInformation("Starting pre-authorization");
        
        try
        {
            var response = await SendPaymentRequestAsync(request, ...);
            
            _logger.LogInformation(
                "Pre-authorization successful: {PaymentId}, Result: {ResultCode}",
                response.Id,
                response.Result.Code);
                
            return response;
        }
        catch (VrPayException ex)
        {
            _logger.LogError(ex, 
                "Pre-authorization failed: {ResultCode}",
                ex.ResultCode);
            throw;
        }
    }
}
```

## Testing Architecture

### Test Doubles

```csharp
// Mock HTTP message handler for testing
public class MockHttpMessageHandler : HttpMessageHandler
{
    public Queue<HttpResponseMessage> Responses { get; } = new();
    public List<HttpRequestMessage> Requests { get; } = new();
    
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, 
        CancellationToken cancellationToken)
    {
        Requests.Add(request);
        return Task.FromResult(Responses.Dequeue());
    }
}

// Test data builder
public class TestDataBuilder
{
    public static PaymentRequest CreateValidPaymentRequest()
    {
        return new PaymentRequest
        {
            Amount = 92.00m,
            Currency = "EUR",
            PaymentBrand = "VISA",
            MerchantTransactionId = Guid.NewGuid().ToString(),
            Card = CreateTestCard()
        };
    }
    
    public static PaymentResponse CreateSuccessResponse()
    {
        return new PaymentResponse
        {
            Id = Guid.NewGuid().ToString("N"),
            PaymentType = "PA",
            Amount = "92.00",
            Currency = "EUR",
            Result = new ResultData
            {
                Code = "000.100.110",
                Description = "Request successfully processed"
            }
        };
    }
}
```

## Security Considerations

### 1. Sensitive Data Protection

```csharp
public class SensitiveDataProtector
{
    // Never log card numbers, CVV, or full PAN
    public static PaymentRequest SanitizeForLogging(PaymentRequest request)
    {
        var sanitized = request with { }; // Copy
        
        if (sanitized.Card != null)
        {
            sanitized.Card.Number = MaskCardNumber(sanitized.Card.Number);
            sanitized.Card.Cvv = "***";
        }
        
        return sanitized;
    }
}
```

### 2. PCI-DSS Compliance

- **Never store CVV** after authorization
- **Never log full card numbers**
- Use **tokenization** when possible
- Implement **encryption at rest** for stored payment data
- Follow **PCI-DSS SAQ** requirements

## Performance Optimization

### 1. HTTP Client Reuse

```csharp
// ✅ Good: Single HttpClient instance per lifetime
services.AddHttpClient<IVrPayClient, VrPayClient>()
    .SetHandlerLifetime(TimeSpan.FromMinutes(5));

// ❌ Bad: Creating new HttpClient per request
var client = new HttpClient(); // Don't do this!
```

### 2. Connection Pooling

```csharp
services.AddHttpClient<IVrPayClient, VrPayClient>()
    .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
    {
        PooledConnectionLifetime = TimeSpan.FromMinutes(2),
        MaxConnectionsPerServer = 10
    });
```

## Next Steps

Continue to:
- [Testing Guide](09-testing-guide.md) - Comprehensive testing strategies
