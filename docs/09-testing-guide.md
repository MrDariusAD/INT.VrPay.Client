# Testing Guide

## Testing Strategy

This guide covers comprehensive testing approaches for the VrPay .NET client, including unit tests, integration tests, and test environment usage.

## Test Data

### Test Card Numbers

VrPay provides test card numbers that simulate different scenarios in the test environment.

| Brand | Card Number (No 3DS) | CVV | Expiry | Result |
|-------|---------------------|-----|--------|--------|
| VISA | 4200000000000000 | Any 3 digits | Any future date | Success |
| VISA | 4000000000000002 | Any 3 digits | Any future date | Declined |
| MASTERCARD | 5454545454545454 | Any 3 digits | Any future date | Success |
| AMEX | 377777777777770 | Any 4 digits | Any future date | Success |

### Test Amounts

Different amounts can trigger different responses:

| Amount | Expected Result |
|--------|----------------|
| 92.00 | Successful transaction |
| 90.00 | Declined transaction |

### Test Mode Configuration

```json
{
  "VrPay": {
    "BaseUrl": "https://test.vr-pay-ecommerce.de/",
    "EntityId": "8a8294174e735d0c014e78beb6b9154b",
    "TestMode": "INTERNAL"  // or "EXTERNAL" for end-to-end testing
  }
}
```

**INTERNAL**: Transactions sent to VrPay simulators  
**EXTERNAL**: Transactions forwarded to processor's test system

## Unit Testing

### Test Setup

```csharp
public class VrPayClientTests : IDisposable
{
    private readonly Mock<HttpMessageHandler> _httpHandlerMock;
    private readonly Mock<ILogger<VrPayClient>> _loggerMock;
    private readonly VrPaySettings _settings;
    private readonly HttpClient _httpClient;
    private readonly VrPayClient _sut; // System Under Test
    
    public VrPayClientTests()
    {
        _httpHandlerMock = new Mock<HttpMessageHandler>();
        _loggerMock = new Mock<ILogger<VrPayClient>>();
        _settings = new VrPaySettings
        {
            BaseUrl = "https://test.vr-pay-ecommerce.de/",
            EntityId = "8a8294174e735d0c014e78beb6b9154b",
            TestMode = "INTERNAL",
            TimeoutSeconds = 30
        };
        
        _httpClient = new HttpClient(_httpHandlerMock.Object)
        {
            BaseAddress = new Uri(_settings.BaseUrl)
        };
        
        _sut = new VrPayClient(
            _httpClient,
            Options.Create(_settings),
            _loggerMock.Object);
    }
    
    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}
```

### Testing Successful Pre-Authorization

```csharp
[Fact]
public async Task PreAuthorizeAsync_WithValidRequest_ReturnsSuccessResponse()
{
    // Arrange
    var request = TestDataBuilder.CreateValidPaymentRequest();
    var expectedResponse = TestDataBuilder.CreateSuccessResponse();
    
    SetupHttpResponse(HttpStatusCode.OK, expectedResponse);
    
    // Act
    var result = await _sut.PreAuthorizeAsync(request);
    
    // Assert
    result.Should().NotBeNull();
    result.Id.Should().NotBeNullOrEmpty();
    result.Result.Code.Should().Be("000.100.110");
    result.IsSuccess().Should().BeTrue();
    result.Amount.Should().Be("92.00");
    result.Currency.Should().Be("EUR");
    result.PaymentType.Should().Be("PA");
}

private void SetupHttpResponse(HttpStatusCode statusCode, object responseBody)
{
    _httpHandlerMock
        .Protected()
        .Setup<Task<HttpResponseMessage>>(
            "SendAsync",
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>())
        .ReturnsAsync(new HttpResponseMessage
        {
            StatusCode = statusCode,
            Content = new StringContent(
                JsonSerializer.Serialize(responseBody),
                Encoding.UTF8,
                "application/json")
        });
}
```

### Testing Payment Decline

```csharp
[Fact]
public async Task PreAuthorizeAsync_WhenPaymentDeclined_ThrowsDeclinedException()
{
    // Arrange
    var request = TestDataBuilder.CreateValidPaymentRequest();
    var declinedResponse = new PaymentResponse
    {
        Id = Guid.NewGuid().ToString("N"),
        PaymentType = "PA",
        Amount = "92.00",
        Currency = "EUR",
        Result = new ResultData
        {
            Code = "800.100.153",
            Description = "Transaction declined (invalid CVV)"
        }
    };
    
    SetupHttpResponse(HttpStatusCode.OK, declinedResponse);
    
    // Act & Assert
    var exception = await Assert.ThrowsAsync<VrPayPaymentDeclinedException>(
        () => _sut.PreAuthorizeAsync(request));
    
    exception.ResultCode.Should().Be("800.100.153");
    exception.Response.Should().NotBeNull();
    exception.Response.Result.Code.Should().Be("800.100.153");
}
```

### Testing HTTP Errors

```csharp
[Theory]
[InlineData(HttpStatusCode.Unauthorized, typeof(VrPayConfigurationException))]
[InlineData(HttpStatusCode.Forbidden, typeof(VrPayConfigurationException))]
[InlineData(HttpStatusCode.NotFound, typeof(VrPayException))]
[InlineData(HttpStatusCode.InternalServerError, typeof(VrPayCommunicationException))]
public async Task PreAuthorizeAsync_WithHttpError_ThrowsAppropriateException(
    HttpStatusCode statusCode,
    Type expectedExceptionType)
{
    // Arrange
    var request = TestDataBuilder.CreateValidPaymentRequest();
    
    _httpHandlerMock
        .Protected()
        .Setup<Task<HttpResponseMessage>>(
            "SendAsync",
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>())
        .ReturnsAsync(new HttpResponseMessage
        {
            StatusCode = statusCode,
            Content = new StringContent("{}")
        });
    
    // Act & Assert
    await Assert.ThrowsAsync(expectedExceptionType,
        () => _sut.PreAuthorizeAsync(request));
}
```

### Testing Capture

```csharp
[Fact]
public async Task CaptureAsync_WithValidPreAuthId_ReturnsSuccessResponse()
{
    // Arrange
    var preAuthId = "8ac7a4a289d210fc0189d5b4e9572cc8";
    var amount = 92.00m;
    var currency = "EUR";
    
    var expectedResponse = new PaymentResponse
    {
        Id = Guid.NewGuid().ToString("N"),
        PaymentType = "CP",
        Amount = "92.00",
        Currency = "EUR",
        Result = new ResultData
        {
            Code = "000.100.110",
            Description = "Request successfully processed"
        },
        ReferencedId = preAuthId
    };
    
    SetupHttpResponse(HttpStatusCode.OK, expectedResponse);
    
    // Act
    var result = await _sut.CaptureAsync(preAuthId, amount, currency);
    
    // Assert
    result.Should().NotBeNull();
    result.PaymentType.Should().Be("CP");
    result.IsSuccess().Should().BeTrue();
}
```

### Testing Validation

```csharp
[Theory]
[InlineData("", "EUR", "Amount is required")]
[InlineData("92.5", "EUR", "Amount must have 2 decimal places")]
[InlineData("92.00", "", "Currency is required")]
[InlineData("92.00", "EURO", "Invalid currency code")]
public async Task PreAuthorizeAsync_WithInvalidRequest_ThrowsValidationException(
    string amount,
    string currency,
    string expectedError)
{
    // Arrange
    var request = new PaymentRequest
    {
        Amount = decimal.TryParse(amount, out var amt) ? amt : 0,
        Currency = currency,
        Card = TestDataBuilder.CreateTestCard()
    };
    
    // Act & Assert
    await Assert.ThrowsAsync<VrPayValidationException>(
        () => _sut.PreAuthorizeAsync(request));
}
```

## Integration Testing

### Integration Test Setup

```csharp
public class VrPayIntegrationTests : IClassFixture<VrPayTestFixture>
{
    private readonly VrPayTestFixture _fixture;
    private readonly IVrPayClient _client;
    
    public VrPayIntegrationTests(VrPayTestFixture fixture)
    {
        _fixture = fixture;
        _client = fixture.CreateClient();
    }
}

public class VrPayTestFixture : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    
    public VrPayTestFixture()
    {
        var services = new ServiceCollection();
        
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.test.json")
            .AddEnvironmentVariables()
            .Build();
        
        services.AddVrPayClient(configuration);
        services.AddLogging(builder => builder.AddConsole());
        
        _serviceProvider = services.BuildServiceProvider();
    }
    
    public IVrPayClient CreateClient()
    {
        return _serviceProvider.GetRequiredService<IVrPayClient>();
    }
    
    public void Dispose()
    {
        _serviceProvider?.Dispose();
    }
}
```

### End-to-End Pre-Authorization Test

```csharp
[Fact]
[Trait("Category", "Integration")]
public async Task PreAuthorize_AndCapture_FullFlow_Succeeds()
{
    // Arrange
    var request = new PaymentRequest
    {
        Amount = 92.00m,
        Currency = "EUR",
        PaymentBrand = "VISA",
        MerchantTransactionId = $"TEST-{Guid.NewGuid()}",
        Card = new CardData
        {
            Number = "4200000000000000",
            Holder = "Jane Jones",
            ExpiryMonth = "05",
            ExpiryYear = "2034",
            Cvv = "123"
        },
        Customer = new CustomerData
        {
            Email = "test@example.com",
            Ip = "127.0.0.1"
        }
    };
    
    // Act - Pre-authorize
    var preAuthResponse = await _client.PreAuthorizeAsync(request);
    
    // Assert - Pre-auth successful
    preAuthResponse.Should().NotBeNull();
    preAuthResponse.IsSuccess().Should().BeTrue();
    preAuthResponse.Id.Should().NotBeNullOrEmpty();
    
    // Act - Capture
    var captureResponse = await _client.CaptureAsync(
        preAuthResponse.Id,
        92.00m,
        "EUR");
    
    // Assert - Capture successful
    captureResponse.Should().NotBeNull();
    captureResponse.IsSuccess().Should().BeTrue();
    captureResponse.PaymentType.Should().Be("CP");
}
```

### Testing Decline Scenarios

```csharp
[Fact]
[Trait("Category", "Integration")]
public async Task PreAuthorize_WithInvalidCard_ReturnsDecline()
{
    // Arrange - Use a card number known to decline
    var request = TestDataBuilder.CreateValidPaymentRequest();
    request.Card.Number = "4000000000000002"; // Known decline card
    
    // Act & Assert
    var exception = await Assert.ThrowsAsync<VrPayPaymentDeclinedException>(
        () => _client.PreAuthorizeAsync(request));
    
    exception.Response.Result.Code.Should().StartWith("800.");
}
```

## Test Data Builders

### Payment Request Builder

```csharp
public class TestDataBuilder
{
    public static PaymentRequest CreateValidPaymentRequest()
    {
        return new PaymentRequest
        {
            Amount = 92.00m,
            Currency = "EUR",
            PaymentBrand = "VISA",
            MerchantTransactionId = $"TEST-{Guid.NewGuid().ToString("N")[..12]}",
            Card = CreateTestCard(),
            Customer = CreateTestCustomer(),
            Billing = CreateTestAddress()
        };
    }
    
    public static CardData CreateTestCard()
    {
        return new CardData
        {
            Number = "4200000000000000",
            Holder = "Jane Jones",
            ExpiryMonth = "05",
            ExpiryYear = DateTime.Now.AddYears(2).Year.ToString(),
            Cvv = "123"
        };
    }
    
    public static CustomerData CreateTestCustomer()
    {
        return new CustomerData
        {
            GivenName = "Jane",
            Surname = "Jones",
            Email = $"test-{Guid.NewGuid().ToString("N")[..8]}@example.com",
            Ip = "127.0.0.1"
        };
    }
    
    public static AddressData CreateTestAddress()
    {
        return new AddressData
        {
            Street1 = "123 Test Street",
            City = "Berlin",
            Postcode = "10115",
            Country = "DE"
        };
    }
    
    public static PaymentResponse CreateSuccessResponse(string paymentType = "PA")
    {
        return new PaymentResponse
        {
            Id = Guid.NewGuid().ToString("N"),
            PaymentType = paymentType,
            PaymentBrand = "VISA",
            Amount = "92.00",
            Currency = "EUR",
            Result = new ResultData
            {
                Code = "000.100.110",
                Description = "Request successfully processed in 'Merchant in Integrator Test Mode'"
            },
            Card = new CardResponseData
            {
                Bin = "420000",
                Last4Digits = "0000",
                Holder = "Jane Jones",
                ExpiryMonth = "05",
                ExpiryYear = "2034"
            },
            Timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fffzzz")
        };
    }
}
```

## Testing Best Practices

### 1. Arrange-Act-Assert Pattern

```csharp
[Fact]
public async Task MethodName_StateUnderTest_ExpectedBehavior()
{
    // Arrange - Set up test data and dependencies
    var request = TestDataBuilder.CreateValidPaymentRequest();
    SetupHttpResponse(HttpStatusCode.OK, expectedResponse);
    
    // Act - Execute the method being tested
    var result = await _sut.PreAuthorizeAsync(request);
    
    // Assert - Verify the expected outcome
    result.Should().NotBeNull();
    result.IsSuccess().Should().BeTrue();
}
```

### 2. Test Naming Convention

```
MethodName_StateUnderTest_ExpectedBehavior

Examples:
- PreAuthorizeAsync_WithValidRequest_ReturnsSuccessResponse
- CaptureAsync_WithInvalidAmount_ThrowsValidationException
- DebitAsync_WhenDeclined_ThrowsPaymentDeclinedException
```

### 3. Use Test Categories

```csharp
[Trait("Category", "Unit")]
public class UnitTests { }

[Trait("Category", "Integration")]
public class IntegrationTests { }

// Run only unit tests:
// dotnet test --filter "Category=Unit"
```

### 4. Isolation

```csharp
// ✅ Good - Each test is independent
[Fact]
public async Task Test1() 
{
    var client = CreateClient();
    // Test logic
}

[Fact]
public async Task Test2() 
{
    var client = CreateClient();
    // Test logic
}

// ❌ Bad - Tests depend on shared state
private IVrPayClient _sharedClient;

[Fact]
public async Task Test1() 
{
    // Modifies _sharedClient state
}

[Fact]
public async Task Test2() 
{
    // Depends on Test1's modifications
}
```

## Performance Testing

### Load Testing Setup

```csharp
[Fact]
[Trait("Category", "Performance")]
public async Task PreAuthorize_UnderLoad_MaintainsPerformance()
{
    // Arrange
    var requests = Enumerable.Range(0, 100)
        .Select(_ => TestDataBuilder.CreateValidPaymentRequest())
        .ToList();
    
    var stopwatch = Stopwatch.StartNew();
    
    // Act
    var tasks = requests.Select(r => _client.PreAuthorizeAsync(r));
    var results = await Task.WhenAll(tasks);
    
    stopwatch.Stop();
    
    // Assert
    results.Should().HaveCount(100);
    results.Should().OnlyContain(r => r.IsSuccess());
    stopwatch.ElapsedMilliseconds.Should().BeLessThan(10000); // < 10s for 100 requests
    
    _testOutputHelper.WriteLine($"Processed 100 requests in {stopwatch.ElapsedMilliseconds}ms");
    _testOutputHelper.WriteLine($"Average: {stopwatch.ElapsedMilliseconds / 100.0}ms per request");
}
```

## Mocking Best Practices

### Using Moq for HTTP Mocking

```csharp
private void SetupSuccessfulResponse()
{
    _httpHandlerMock
        .Protected()
        .Setup<Task<HttpResponseMessage>>(
            "SendAsync",
            ItExpr.Is<HttpRequestMessage>(req => 
                req.Method == HttpMethod.Post &&
                req.RequestUri.AbsolutePath.Contains("/v1/payments")),
            ItExpr.IsAny<CancellationToken>())
        .ReturnsAsync(new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(
                JsonSerializer.Serialize(TestDataBuilder.CreateSuccessResponse()))
        });
}
```

## Summary

### Test Coverage Goals

- **Unit Tests**: 80%+ code coverage
- **Integration Tests**: Critical user journeys
- **End-to-End Tests**: Full payment flows

### Running Tests

```bash
# All tests
dotnet test

# Unit tests only
dotnet test --filter "Category=Unit"

# Integration tests only
dotnet test --filter "Category=Integration"

# With coverage
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

### CI/CD Integration

```yaml
# .github/workflows/test.yml
name: Tests

on: [push, pull_request]

jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v2
      - name: Setup .NET
        uses: actions/setup-dotnet@v1
        with:
          dotnet-version: '8.0.x'
      - name: Restore dependencies
        run: dotnet restore
      - name: Build
        run: dotnet build --no-restore
      - name: Run Unit Tests
        run: dotnet test --no-build --filter "Category=Unit"
      - name: Run Integration Tests
        run: dotnet test --no-build --filter "Category=Integration"
        env:
          VRPAY_ACCESS_TOKEN: ${{ secrets.VRPAY_TEST_TOKEN }}
```

## Conclusion

This testing guide provides a comprehensive approach to testing the VrPay .NET client. Combine unit tests for fast feedback with integration tests for confidence in real-world scenarios.
