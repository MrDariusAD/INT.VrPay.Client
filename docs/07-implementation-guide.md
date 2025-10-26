# .NET Client Implementation Guide

## Step-by-Step Implementation

This guide provides a complete walkthrough for implementing the VrPay .NET client for synchronous payments.

## Phase 1: Project Setup

### 1.1 Create Project Structure

```powershell
# Create solution and projects
dotnet new sln -n VrPay.Client
dotnet new classlib -n VrPay.Client -f net8.0
dotnet new classlib -n VrPay.Client.Models -f net8.0
dotnet new xunit -n VrPay.Client.Tests -f net8.0

# Add projects to solution
dotnet sln add VrPay.Client/VrPay.Client.csproj
dotnet sln add VrPay.Client.Models/VrPay.Client.Models.csproj
dotnet sln add VrPay.Client.Tests/VrPay.Client.Tests.csproj

# Add project references
dotnet add VrPay.Client/VrPay.Client.csproj reference VrPay.Client.Models/VrPay.Client.Models.csproj
dotnet add VrPay.Client.Tests/VrPay.Client.Tests.csproj reference VrPay.Client/VrPay.Client.csproj
```

### 1.2 Install NuGet Packages

```xml
<!-- VrPay.Client.csproj -->
<ItemGroup>
  <PackageReference Include="Microsoft.Extensions.Http" Version="8.0.0" />
  <PackageReference Include="Microsoft.Extensions.Options" Version="8.0.0" />
  <PackageReference Include="Microsoft.Extensions.Logging.Abstractions" Version="8.0.0" />
  <PackageReference Include="System.Text.Json" Version="8.0.0" />
  <PackageReference Include="Polly.Extensions.Http" Version="3.0.0" />
</ItemGroup>

<!-- VrPay.Client.Tests.csproj -->
<ItemGroup>
  <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.8.0" />
  <PackageReference Include="xunit" Version="2.6.2" />
  <PackageReference Include="xunit.runner.visualstudio" Version="2.5.4" />
  <PackageReference Include="Moq" Version="4.20.69" />
  <PackageReference Include="FluentAssertions" Version="6.12.0" />
</ItemGroup>
```

## Phase 2: Configuration

### 2.1 Create Configuration Models

**File**: `VrPay.Client/Configuration/VrPaySettings.cs`

```csharp
namespace VrPay.Client.Configuration;

public class VrPaySettings
{
    public const string SectionName = "VrPay";
    
    public string BaseUrl { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string? TestMode { get; set; }
    public int TimeoutSeconds { get; set; } = 30;
    public int MaxRetryAttempts { get; set; } = 3;
    
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(BaseUrl))
            throw new ArgumentException("BaseUrl is required");
            
        if (string.IsNullOrWhiteSpace(EntityId))
            throw new ArgumentException("EntityId is required");
            
        if (!Regex.IsMatch(EntityId, "^[a-f0-9]{32}$", RegexOptions.IgnoreCase))
            throw new ArgumentException("EntityId must be 32 hex characters");
    }
}
```

### 2.2 Configuration Files

**appsettings.json**:
```json
{
  "VrPay": {
    "BaseUrl": "https://test.vr-pay-ecommerce.de/",
    "EntityId": "8a8294174e735d0c014e78beb6b9154b",
    "TestMode": "INTERNAL",
    "TimeoutSeconds": 30,
    "MaxRetryAttempts": 3
  }
}
```

**Environment Variable** (for access token):
```
VRPAY_ACCESS_TOKEN=Bearer OGE4Mjk0MTc0ZTczNWQwYzAxNGU3OGJlYjZjNTE1NGZ8TnE3b0UlMmEyQldKcjQlJTZYZFk=
```

## Phase 3: Models

### 3.1 Request Models

**File**: `VrPay.Client.Models/Requests/PaymentRequest.cs`

```csharp
namespace VrPay.Client.Models.Requests;

public class PaymentRequest
{
    [Required]
    public string EntityId { get; set; } = string.Empty;
    
    [Required]
    public decimal Amount { get; set; }
    
    [Required]
    public string Currency { get; set; } = string.Empty;
    
    [Required]
    public string PaymentType { get; set; } = string.Empty;
    
    public string? PaymentBrand { get; set; }
    public string? MerchantTransactionId { get; set; }
    public string? TestMode { get; set; }
    
    public CardData? Card { get; set; }
    public CustomerData? Customer { get; set; }
    public AddressData? Billing { get; set; }
    
    public Dictionary<string, string> ToFormData()
    {
        var data = new Dictionary<string, string>
        {
            ["entityId"] = EntityId,
            ["amount"] = Amount.ToString("F2", CultureInfo.InvariantCulture),
            ["currency"] = Currency,
            ["paymentType"] = PaymentType
        };
        
        if (!string.IsNullOrEmpty(PaymentBrand))
            data["paymentBrand"] = PaymentBrand;
            
        if (!string.IsNullOrEmpty(MerchantTransactionId))
            data["merchantTransactionId"] = MerchantTransactionId;
            
        if (!string.IsNullOrEmpty(TestMode))
            data["testMode"] = TestMode;
            
        Card?.AddToFormData(data, "card");
        Customer?.AddToFormData(data, "customer");
        Billing?.AddToFormData(data, "billing");
        
        return data;
    }
}
```

### 3.2 Response Models

**File**: `VrPay.Client.Models/Responses/PaymentResponse.cs`

```csharp
namespace VrPay.Client.Models.Responses;

public class PaymentResponse
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
    
    [JsonPropertyName("paymentType")]
    public string PaymentType { get; set; } = string.Empty;
    
    [JsonPropertyName("paymentBrand")]
    public string? PaymentBrand { get; set; }
    
    [JsonPropertyName("amount")]
    public string Amount { get; set; } = string.Empty;
    
    [JsonPropertyName("currency")]
    public string Currency { get; set; } = string.Empty;
    
    [JsonPropertyName("result")]
    public ResultData Result { get; set; } = new();
    
    [JsonPropertyName("card")]
    public CardResponseData? Card { get; set; }
    
    [JsonPropertyName("timestamp")]
    public string Timestamp { get; set; } = string.Empty;
    
    public bool IsSuccess() => ResultCodeAnalyzer.IsSuccess(Result.Code);
    public TransactionStatus GetStatus() => ResultCodeAnalyzer.AnalyzeResultCode(Result.Code);
}

public class ResultData
{
    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;
    
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;
}
```

## Phase 4: Core Client

### 4.1 Interface

**File**: `VrPay.Client/IVrPayClient.cs`

```csharp
namespace VrPay.Client;

public interface IVrPayClient
{
    Task<PaymentResponse> PreAuthorizeAsync(
        PaymentRequest request, 
        CancellationToken cancellationToken = default);
    
    Task<PaymentResponse> CaptureAsync(
        string preAuthId,
        decimal amount,
        string currency,
        CancellationToken cancellationToken = default);
    
    Task<PaymentResponse> DebitAsync(
        PaymentRequest request,
        CancellationToken cancellationToken = default);
    
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

### 4.2 Implementation

**File**: `VrPay.Client/VrPayClient.cs`

```csharp
namespace VrPay.Client;

public class VrPayClient : IVrPayClient
{
    private readonly HttpClient _httpClient;
    private readonly VrPaySettings _settings;
    private readonly ILogger<VrPayClient> _logger;
    
    public VrPayClient(
        HttpClient httpClient,
        IOptions<VrPaySettings> settings,
        ILogger<VrPayClient> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        _settings.Validate();
    }
    
    public async Task<PaymentResponse> PreAuthorizeAsync(
        PaymentRequest request,
        CancellationToken cancellationToken = default)
    {
        request.EntityId = _settings.EntityId;
        request.PaymentType = "PA";
        request.TestMode = _settings.TestMode;
        
        _logger.LogInformation("Pre-authorizing payment: {MerchantTxId}", 
            request.MerchantTransactionId);
        
        return await SendPaymentRequestAsync(request, "/v1/payments", cancellationToken);
    }
    
    public async Task<PaymentResponse> CaptureAsync(
        string preAuthId,
        decimal amount,
        string currency,
        CancellationToken cancellationToken = default)
    {
        var request = new PaymentRequest
        {
            EntityId = _settings.EntityId,
            Amount = amount,
            Currency = currency,
            PaymentType = "CP"
        };
        
        _logger.LogInformation("Capturing payment: {PaymentId}", preAuthId);
        
        return await SendPaymentRequestAsync(
            request, 
            $"/v1/payments/{preAuthId}", 
            cancellationToken);
    }
    
    private async Task<PaymentResponse> SendPaymentRequestAsync(
        PaymentRequest request,
        string endpoint,
        CancellationToken cancellationToken)
    {
        try
        {
            var formData = request.ToFormData();
            var content = new FormUrlEncodedContent(formData);
            
            var httpResponse = await _httpClient.PostAsync(endpoint, content, cancellationToken);
            var responseBody = await httpResponse.Content.ReadAsStringAsync(cancellationToken);
            
            if (!httpResponse.IsSuccessStatusCode)
            {
                _logger.LogError("HTTP error {StatusCode}: {Response}", 
                    httpResponse.StatusCode, responseBody);
                HandleHttpError(httpResponse.StatusCode, responseBody);
            }
            
            var paymentResponse = JsonSerializer.Deserialize<PaymentResponse>(responseBody);
            
            if (paymentResponse == null)
                throw new VrPayException("Failed to deserialize payment response");
            
            _logger.LogInformation("Payment response: {ResultCode} - {PaymentId}",
                paymentResponse.Result.Code, paymentResponse.Id);
            
            return HandlePaymentResult(paymentResponse);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error during payment request");
            throw new VrPayCommunicationException("Network error occurred", ex);
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Payment request timeout");
            throw new VrPayCommunicationException("Request timeout", ex);
        }
    }
    
    // See error handling document for HandleHttpError and HandlePaymentResult methods
}
```

## Phase 5: Dependency Injection Setup

### 5.1 Service Registration

**File**: `VrPay.Client/Extensions/ServiceCollectionExtensions.cs`

```csharp
namespace VrPay.Client.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddVrPayClient(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Configure settings
        services.Configure<VrPaySettings>(
            configuration.GetSection(VrPaySettings.SectionName));
        
        // Get access token from environment
        var accessToken = configuration["VRPAY_ACCESS_TOKEN"] 
            ?? throw new InvalidOperationException("VRPAY_ACCESS_TOKEN not configured");
        
        // Register HTTP client with policies
        services.AddHttpClient<IVrPayClient, VrPayClient>((serviceProvider, client) =>
        {
            var settings = serviceProvider
                .GetRequiredService<IOptions<VrPaySettings>>()
                .Value;
            
            client.BaseAddress = new Uri(settings.BaseUrl);
            client.DefaultRequestHeaders.Add("Authorization", accessToken);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.Timeout = TimeSpan.FromSeconds(settings.TimeoutSeconds);
        })
        .AddPolicyHandler(HttpPolicies.GetRetryPolicy())
        .AddPolicyHandler(HttpPolicies.GetCircuitBreakerPolicy());
        
        return services;
    }
}
```

## Phase 6: Testing

### 6.1 Unit Tests

**File**: `VrPay.Client.Tests/VrPayClientTests.cs`

```csharp
public class VrPayClientTests
{
    private readonly Mock<ILogger<VrPayClient>> _loggerMock;
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly VrPaySettings _settings;
    private readonly VrPayClient _client;
    
    public VrPayClientTests()
    {
        _loggerMock = new Mock<ILogger<VrPayClient>>();
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        _settings = new VrPaySettings
        {
            BaseUrl = "https://test.vr-pay-ecommerce.de/",
            EntityId = "8a8294174e735d0c014e78beb6b9154b",
            TestMode = "INTERNAL"
        };
        
        var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri(_settings.BaseUrl)
        };
        
        _client = new VrPayClient(
            httpClient,
            Options.Create(_settings),
            _loggerMock.Object);
    }
    
    [Fact]
    public async Task PreAuthorizeAsync_Success_ReturnsPaymentResponse()
    {
        // Arrange
        var request = CreateTestPaymentRequest();
        var expectedResponse = CreateSuccessResponse();
        
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(expectedResponse))
            });
        
        // Act
        var result = await _client.PreAuthorizeAsync(request);
        
        // Assert
        result.Should().NotBeNull();
        result.Result.Code.Should().Be("000.100.110");
        result.IsSuccess().Should().BeTrue();
    }
}
```

## Phase 7: Usage Example

### 7.1 ASP.NET Core Integration

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddVrPayClient(builder.Configuration);

var app = builder.Build();

// Controller
public class PaymentController : ControllerBase
{
    private readonly IVrPayClient _vrPayClient;
    
    public PaymentController(IVrPayClient vrPayClient)
    {
        _vrPayClient = vrPayClient;
    }
    
    [HttpPost("checkout")]
    public async Task<IActionResult> Checkout([FromBody] CheckoutRequest request)
    {
        try
        {
            var paymentRequest = new PaymentRequest
            {
                Amount = request.Amount,
                Currency = "EUR",
                PaymentBrand = "VISA",
                MerchantTransactionId = Guid.NewGuid().ToString(),
                Card = new CardData
                {
                    Number = request.CardNumber,
                    Holder = request.CardHolder,
                    ExpiryMonth = request.ExpiryMonth,
                    ExpiryYear = request.ExpiryYear,
                    Cvv = request.Cvv
                }
            };
            
            var response = await _vrPayClient.PreAuthorizeAsync(paymentRequest);
            
            // Store payment ID for later capture
            await StorePaymentIdAsync(response.Id);
            
            return Ok(new { paymentId = response.Id });
        }
        catch (VrPayPaymentDeclinedException ex)
        {
            return BadRequest(new { error = ex.Message, code = ex.ResultCode });
        }
    }
}
```

## Next Steps

Continue to:
- [.NET Client Architecture](08-dotnet-client-architecture.md) - Detailed architecture documentation
- [Testing Guide](09-testing-guide.md) - Comprehensive testing strategies
