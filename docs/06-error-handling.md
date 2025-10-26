# Error Handling Guide

## Exception Handling Strategy

### Exception Hierarchy

```csharp
public class VrPayException : Exception
{
    public string? ResultCode { get; set; }
    public VrPayException(string message) : base(message) { }
    public VrPayException(string message, string resultCode) : base(message)
    {
        ResultCode = resultCode;
    }
}

public class VrPayValidationException : VrPayException
{
    public Dictionary<string, string> ValidationErrors { get; set; }
    public VrPayValidationException(string message, Dictionary<string, string> errors) 
        : base(message)
    {
        ValidationErrors = errors;
    }
}

public class VrPayPaymentDeclinedException : VrPayException
{
    public PaymentResponse Response { get; set; }
    public VrPayPaymentDeclinedException(string message, PaymentResponse response) 
        : base(message, response.Result.Code)
    {
        Response = response;
    }
}

public class VrPayCommunicationException : VrPayException
{
    public HttpStatusCode? StatusCode { get; set; }
    public VrPayCommunicationException(string message, HttpStatusCode? statusCode = null) 
        : base(message)
    {
        StatusCode = statusCode;
    }
}

public class VrPayConfigurationException : VrPayException
{
    public VrPayConfigurationException(string message) : base(message) { }
}
```

## Error Handling Patterns

### HTTP Error Handling

```csharp
public async Task<PaymentResponse> ProcessPaymentAsync(PaymentRequest request)
{
    try
    {
        var response = await _httpClient.PostAsync("/v1/payments", content);
        
        // Check HTTP status
        if (!response.IsSuccessStatusCode)
        {
            await HandleHttpErrorAsync(response);
        }
        
        var responseBody = await response.Content.ReadAsStringAsync();
        var paymentResponse = JsonSerializer.Deserialize<PaymentResponse>(responseBody);
        
        // Check result code
        return HandlePaymentResult(paymentResponse);
    }
    catch (HttpRequestException ex)
    {
        throw new VrPayCommunicationException("Network error occurred", ex);
    }
    catch (TaskCanceledException ex)
    {
        throw new VrPayCommunicationException("Request timeout", ex);
    }
}

private async Task HandleHttpErrorAsync(HttpResponseMessage response)
{
    var content = await response.Content.ReadAsStringAsync();
    
    switch (response.StatusCode)
    {
        case HttpStatusCode.BadRequest:
            throw new VrPayValidationException("Bad request", 
                ParseValidationErrors(content));
        
        case HttpStatusCode.Unauthorized:
            throw new VrPayConfigurationException(
                "Invalid authorization header");
        
        case HttpStatusCode.Forbidden:
            throw new VrPayConfigurationException(
                "Invalid access token");
        
        case HttpStatusCode.NotFound:
            throw new VrPayException(
                "Resource not found - check endpoint and payment ID");
        
        default:
            throw new VrPayCommunicationException(
                $"HTTP error: {response.StatusCode}", response.StatusCode);
    }
}
```

### Result Code Handling

```csharp
private PaymentResponse HandlePaymentResult(PaymentResponse response)
{
    var status = ResultCodeAnalyzer.AnalyzeResultCode(response.Result.Code);
    
    switch (status)
    {
        case TransactionStatus.Success:
            return response;
            
        case TransactionStatus.SuccessManualReview:
            // Log for manual review
            _logger.LogWarning("Transaction {Id} requires manual review: {Code}", 
                response.Id, response.Result.Code);
            return response;
            
        case TransactionStatus.PendingShortTerm:
        case TransactionStatus.PendingLongTerm:
            // Return pending status
            return response;
            
        case TransactionStatus.SoftDecline:
            // Can retry with 3DS
            throw new VrPayPaymentDeclinedException(
                "Soft decline - retry with 3D Secure authentication", response);
            
        case TransactionStatus.HardDecline:
            // Don't retry
            throw new VrPayPaymentDeclinedException(
                $"Payment declined: {response.Result.Description}", response);
            
        case TransactionStatus.ValidationError:
            throw new VrPayValidationException(
                response.Result.Description, new Dictionary<string, string>());
                
        case TransactionStatus.CommunicationError:
            throw new VrPayCommunicationException(
                "Communication error with payment provider");
            
        default:
            throw new VrPayException(
                $"Unknown error: {response.Result.Description}", 
                response.Result.Code);
    }
}
```

## Retry Logic

### Retry Policy

```csharp
public class RetryPolicy
{
    public static readonly int[] RetryableHttpStatusCodes = 
    {
        408, // Request Timeout
        500, // Internal Server Error
        502, // Bad Gateway
        503, // Service Unavailable
        504  // Gateway Timeout
    };
    
    public static readonly string[] RetryableResultCodePatterns = 
    {
        @"^(900\.100\.[1256]00)", // Communication/timeout errors
        @"^(900\.300\.600)",       // Session timeout
        @"^(800\.800\.400)"        // System under maintenance
    };
    
    public static bool ShouldRetry(string resultCode, int attemptNumber, int maxAttempts)
    {
        if (attemptNumber >= maxAttempts)
            return false;
            
        return RetryableResultCodePatterns.Any(pattern => 
            Regex.IsMatch(resultCode, pattern));
    }
    
    public static TimeSpan GetRetryDelay(int attemptNumber)
    {
        // Exponential backoff: 1s, 2s, 4s, 8s
        return TimeSpan.FromSeconds(Math.Pow(2, attemptNumber - 1));
    }
}
```

### Implementation with Polly

```csharp
using Polly;
using Polly.Extensions.Http;

public static IHttpClientBuilder AddVrPayHttpClient(
    this IServiceCollection services, 
    IConfiguration configuration)
{
    return services.AddHttpClient<IVrPayClient, VrPayClient>()
        .AddPolicyHandler(GetRetryPolicy())
        .AddPolicyHandler(GetCircuitBreakerPolicy());
}

private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .OrResult(msg => RetryPolicy.RetryableHttpStatusCodes.Contains((int)msg.StatusCode))
        .WaitAndRetryAsync(
            retryCount: 3,
            sleepDurationProvider: attemptNumber => RetryPolicy.GetRetryDelay(attemptNumber),
            onRetry: (outcome, timespan, attemptNumber, context) =>
            {
                // Log retry
            });
}

private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .CircuitBreakerAsync(
            handledEventsAllowedBeforeBreaking: 5,
            durationOfBreak: TimeSpan.FromSeconds(30));
}
```

## Validation

### Pre-request Validation

```csharp
public class PaymentRequestValidator
{
    public ValidationResult Validate(PaymentRequest request)
    {
        var errors = new Dictionary<string, string>();
        
        // Entity ID
        if (!Regex.IsMatch(request.EntityId, @"^[a-f0-9]{32}$"))
            errors["entityId"] = "Invalid entity ID format";
            
        // Amount
        if (!Regex.IsMatch(request.Amount, @"^[0-9]{1,10}\.[0-9]{2}$"))
            errors["amount"] = "Amount must have exactly 2 decimal places";
            
        if (decimal.Parse(request.Amount) <= 0)
            errors["amount"] = "Amount must be greater than zero";
            
        // Currency
        if (!Regex.IsMatch(request.Currency, @"^[A-Z]{3}$"))
            errors["currency"] = "Invalid ISO 4217 currency code";
            
        // Card validation
        if (request.Card != null)
        {
            ValidateCard(request.Card, errors);
        }
        
        return new ValidationResult 
        { 
            IsValid = errors.Count == 0,
            Errors = errors
        };
    }
    
    private void ValidateCard(CardData card, Dictionary<string, string> errors)
    {
        // Luhn algorithm for card number
        if (!LuhnCheck(card.Number))
            errors["card.number"] = "Invalid card number";
            
        // Expiry validation
        var expiryMonth = int.Parse(card.ExpiryMonth);
        var expiryYear = int.Parse(card.ExpiryYear);
        var expiry = new DateTime(expiryYear, expiryMonth, 1).AddMonths(1).AddDays(-1);
        
        if (expiry < DateTime.Today)
            errors["card.expiry"] = "Card has expired";
    }
    
    private bool LuhnCheck(string cardNumber)
    {
        int sum = 0;
        bool alternate = false;
        
        for (int i = cardNumber.Length - 1; i >= 0; i--)
        {
            int n = int.Parse(cardNumber[i].ToString());
            
            if (alternate)
            {
                n *= 2;
                if (n > 9) n -= 9;
            }
            
            sum += n;
            alternate = !alternate;
        }
        
        return sum % 10 == 0;
    }
}
```

## Logging

### Structured Logging

```csharp
public class VrPayClient
{
    private readonly ILogger<VrPayClient> _logger;
    
    public async Task<PaymentResponse> PreAuthorizeAsync(PaymentRequest request)
    {
        using (_logger.BeginScope(new Dictionary<string, object>
        {
            ["MerchantTransactionId"] = request.MerchantTransactionId,
            ["Amount"] = request.Amount,
            ["Currency"] = request.Currency
        }))
        {
            _logger.LogInformation("Starting pre-authorization");
            
            try
            {
                var response = await SendPaymentRequestAsync(request);
                
                _logger.LogInformation(
                    "Pre-authorization completed: {ResultCode} - {PaymentId}",
                    response.Result.Code,
                    response.Id);
                    
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
}
```

### Sensitive Data Masking

```csharp
public class SensitiveDataMasker
{
    public static string MaskCardNumber(string cardNumber)
    {
        if (string.IsNullOrEmpty(cardNumber) || cardNumber.Length < 12)
            return "****";
            
        var first6 = cardNumber.Substring(0, 6);
        var last4 = cardNumber.Substring(cardNumber.Length - 4);
        var masked = new string('*', cardNumber.Length - 10);
        
        return $"{first6}{masked}{last4}";
    }
    
    public static string MaskCvv(string cvv)
    {
        return "***";
    }
}

// Usage in logging
_logger.LogInformation("Processing card: {CardNumber}", 
    SensitiveDataMasker.MaskCardNumber(request.Card.Number));
```

## Error Recovery

### Idempotency

```csharp
public class IdempotencyManager
{
    private readonly IDistributedCache _cache;
    
    public async Task<PaymentResponse> ExecuteIdempotentAsync(
        string idempotencyKey,
        Func<Task<PaymentResponse>> operation)
    {
        // Check cache
        var cached = await _cache.GetStringAsync(idempotencyKey);
        if (cached != null)
        {
            return JsonSerializer.Deserialize<PaymentResponse>(cached);
        }
        
        // Execute operation
        var response = await operation();
        
        // Store in cache (24 hour expiry)
        await _cache.SetStringAsync(
            idempotencyKey, 
            JsonSerializer.Serialize(response),
            new DistributedCacheEntryOptions 
            { 
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24) 
            });
            
        return response;
    }
}
```

## Next Steps

Continue to:
- [Implementation Guide](07-implementation-guide.md) - Complete implementation walkthrough
- [.NET Client Architecture](08-dotnet-client-architecture.md) - Architecture and design patterns
