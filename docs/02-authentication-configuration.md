# Authentication & Configuration

## Overview

All API requests to VrPay eCommerce must be authenticated using an Authorization Bearer token. This document covers authentication setup, configuration parameters, and security best practices.

## Authentication Method

### Bearer Token Authentication

All requests use the **Authorization Bearer** scheme in the HTTP header:

```http
Authorization: Bearer <access-token>
```

### Obtaining Access Token

Access tokens are obtained from the VrPay backend administration portal:

**Path**: Administration > Account data > Merchant / Channel Info

**Requirements**:
- Specific administration rights
- Access to VrPay backend UI

### Token Security

⚠️ **Critical Security Requirements**:
- **Never** expose access token client-side
- Store token securely in server configuration
- Use environment variables or secure key vaults
- Rotate tokens periodically
- Monitor token usage for anomalies

### Token Format

The access token is a base64-encoded string that typically looks like:
```
OGE4Mjk0MTc0ZTczNWQwYzAxNGU3OGJlYjZjNTE1NGZ8TnE3b0UlMmEyQldKcjQlJTZYZFk=
```

## Required Configuration Parameters

### Entity ID

The `entityId` parameter is required in every payment request.

**Parameter**: `entityId`  
**Format**: 32-character hexadecimal string  
**Pattern**: `[a-f0-9]{32}`  
**Example**: `8a8294174e735d0c014e78beb6b9154b`

**What to use**:
- **Channel Entity ID** - for standard merchants
- **Merchant Entity ID** - when channel dispatching is activated

**Where to find**: Administration > Account data > Merchant / Channel Info

### Test Mode (Test Environment Only)

The `testMode` parameter controls transaction routing in test environments.

**Parameter**: `testMode`  
**Format**: `INTERNAL` or `EXTERNAL`  
**Default**: `INTERNAL`

**Options**:
- `INTERNAL` - Transactions sent to VrPay simulators (default)
- `EXTERNAL` - Transactions forwarded to processor's test system for end-to-end testing

⚠️ **Production Restriction**: The `testMode` parameter is **NOT allowed** in production. Requests with `testMode` in production will be declined with error `600.200.701`.

## Environment Configuration

### Test Environment

**Base URL**: `https://test.vr-pay-ecommerce.de/`

**Configuration**:
```json
{
  "baseUrl": "https://test.vr-pay-ecommerce.de/",
  "entityId": "8a8294174e735d0c014e78beb6b9154b",
  "accessToken": "Bearer OGE4Mjk0MTc0ZTczNWQwYzAxNGU3OGJlYjZjNTE1NGZ8TnE3b0UlMmEyQldKcjQlJTZYZFk=",
  "testMode": "INTERNAL"
}
```

**Use Cases**:
- Development and testing
- Integration verification
- UAT (User Acceptance Testing)
- Simulator testing vs. end-to-end testing

### Production Environment

**Base URL**: `https://vr-pay-ecommerce.de/`

**Configuration**:
```json
{
  "baseUrl": "https://vr-pay-ecommerce.de/",
  "entityId": "8a8294174e735d0c014e78beb6b9154b",
  "accessToken": "Bearer [PRODUCTION_TOKEN]"
}
```

**Requirements**:
- Production credentials
- SSL certificate validation
- No `testMode` parameter
- Production merchant account

## HTTP Request Configuration

### Required Headers

Every API request must include these headers:

```http
Authorization: Bearer <access-token>
Content-Type: application/x-www-form-urlencoded; charset=UTF-8
```

### Optional Headers

```http
User-Agent: YourCompanyName/.NETClient/1.0.0
Accept: application/json
Accept-Charset: utf-8
```

### Complete Request Example

```http
POST https://test.vr-pay-ecommerce.de/v1/payments HTTP/1.1
Host: test.vr-pay-ecommerce.de
Authorization: Bearer OGE4Mjk0MTc0ZTczNWQwYzAxNGU3OGJlYjZjNTE1NGZ8TnE3b0UlMmEyQldKcjQlJTZYZFk=
Content-Type: application/x-www-form-urlencoded; charset=UTF-8
Content-Length: 250

entityId=8a8294174e735d0c014e78beb6b9154b&amount=92.00&currency=EUR&paymentBrand=VISA&paymentType=PA&card.number=4200000000000000&card.holder=Jane+Jones&card.expiryMonth=05&card.expiryYear=2034&card.cvv=123
```

## SSL/TLS Configuration

### Requirements

- **SSL Required**: All requests must use HTTPS
- **Minimum TLS Version**: TLS 1.2 or higher recommended
- **Certificate Validation**: Always validate server certificates
- **Certificate Pinning**: Consider implementing for enhanced security

### .NET HttpClient Configuration

```csharp
var handler = new HttpClientHandler
{
    SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13,
    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator // Only for development!
};

// Production: Use proper certificate validation
var handler = new HttpClientHandler
{
    SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13
};
```

## IP Whitelisting

### Merchant Source IP

For enhanced security, VrPay can restrict API access to specific IP addresses.

**Configuration**: Contact VrPay support to whitelist your server IPs

**Error Code**: `600.300.200` - "merchant source IP address not whitelisted"

### Callback URL Whitelisting

URLs used for callbacks must be whitelisted:
- `notificationUrl` - for asynchronous notifications (deprecated, use webhooks)
- `shopperResultUrl` - for customer redirects

**Error Codes**:
- `600.300.210` - merchant notificationUrl not whitelisted
- `600.300.211` - shopperResultUrl not whitelisted

## Configuration Best Practices

### 1. Environment Separation

Always maintain separate configurations for different environments:

```csharp
public class VrPayConfiguration
{
    public string BaseUrl { get; set; }
    public string EntityId { get; set; }
    public string AccessToken { get; set; }
    public string? TestMode { get; set; } // Only for test environment
}
```

### 2. Secure Token Storage

**Options**:
- Environment variables
- Azure Key Vault / AWS Secrets Manager
- Encrypted configuration files
- Secure configuration providers

**Example** (using .NET Configuration):
```json
{
  "VrPay": {
    "Test": {
      "BaseUrl": "https://test.vr-pay-ecommerce.de/",
      "EntityId": "8a8294174e735d0c014e78beb6b9154b"
    },
    "Production": {
      "BaseUrl": "https://vr-pay-ecommerce.de/",
      "EntityId": "[PRODUCTION_ENTITY_ID]"
    }
  }
}
```

Access token stored separately in environment variable:
```
VRPAY_ACCESS_TOKEN=Bearer OGE4Mjk0MTc0ZTczNWQwYzAxNGU3OGJlYjZjNTE1NGZ8TnE3b0UlMmEyQldKcjQlJTZYZFk=
```

### 3. Timeout Configuration

Configure appropriate timeouts for API requests:

```csharp
var httpClient = new HttpClient
{
    Timeout = TimeSpan.FromSeconds(30) // Recommended: 30 seconds
};
```

**Recommended Values**:
- Connection Timeout: 10 seconds
- Request Timeout: 30 seconds
- For test simulator: May be slightly slower

### 4. Retry Policy

Implement retry logic for transient failures:

```csharp
// Retry on specific HTTP status codes
// 500, 502, 503, 504 - Server errors
// 408 - Request timeout

// DO NOT retry on:
// 400 - Bad request (fix the request)
// 401, 403 - Authentication errors
// 404 - Not found
```

**Important**: Check result codes before retrying payments to avoid duplicate charges.

## Validation

### Entity ID Validation

```csharp
public static bool IsValidEntityId(string entityId)
{
    if (string.IsNullOrWhiteSpace(entityId))
        return false;
        
    // Must be exactly 32 hex characters
    return Regex.IsMatch(entityId, "^[a-f0-9]{32}$", RegexOptions.IgnoreCase);
}
```

### Access Token Validation

```csharp
public static bool IsValidAccessToken(string token)
{
    if (string.IsNullOrWhiteSpace(token))
        return false;
        
    // Should start with "Bearer " for proper formatting
    // Base64 encoded string follows
    return token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) 
           && token.Length > 7;
}
```

## Common Configuration Errors

### Error: 500.100.201
**Message**: "Channel/Merchant is disabled (no processing possible)"  
**Solution**: Contact VrPay to activate your channel/merchant account

### Error: 600.200.201
**Message**: "Channel/Merchant not configured for this payment method"  
**Solution**: Contact VrPay to enable required payment methods

### Error: 600.200.202
**Message**: "Channel/Merchant not configured for this payment type"  
**Solution**: Verify payment type (PA, DB, CP, etc.) is enabled for your account

### Error: 600.200.701
**Message**: "testMode not allowed on production"  
**Solution**: Remove `testMode` parameter when using production environment

### Error: 600.300.101
**Message**: "Merchant key not found"  
**Solution**: Verify `entityId` is correct

### Error: 800.900.100
**Message**: "sender authorization failed"  
**Solution**: Check Authorization header and access token

### Error: 800.900.302
**Message**: "Authorization failed"  
**Solution**: Verify access token is valid and not expired

## .NET Configuration Example

### appsettings.json

```json
{
  "VrPaySettings": {
    "BaseUrl": "https://test.vr-pay-ecommerce.de/",
    "EntityId": "8a8294174e735d0c014e78beb6b9154b",
    "TestMode": "INTERNAL",
    "TimeoutSeconds": 30,
    "MaxRetryAttempts": 3
  }
}
```

### Configuration Class

```csharp
public class VrPaySettings
{
    public string BaseUrl { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string? TestMode { get; set; }
    public int TimeoutSeconds { get; set; } = 30;
    public int MaxRetryAttempts { get; set; } = 3;
}
```

### Service Registration

```csharp
services.Configure<VrPaySettings>(configuration.GetSection("VrPaySettings"));

services.AddHttpClient("VrPayClient", (serviceProvider, client) =>
{
    var settings = serviceProvider.GetRequiredService<IOptions<VrPaySettings>>().Value;
    var accessToken = configuration["VRPAY_ACCESS_TOKEN"]; // From environment variable
    
    client.BaseAddress = new Uri(settings.BaseUrl);
    client.DefaultRequestHeaders.Add("Authorization", accessToken);
    client.Timeout = TimeSpan.FromSeconds(settings.TimeoutSeconds);
});
```

## Next Steps

Continue to:
- [Synchronous Payment Flow](03-synchronous-payment-flow.md) - Learn the payment workflow
- [Payment Models](04-payment-models.md) - Understand request/response structures
