# Quick Reference Guide

## Quick Start

### 1. Configuration

```json
{
  "VrPay": {
    "BaseUrl": "https://test.vr-pay-ecommerce.de/",
    "EntityId": "YOUR_ENTITY_ID",
    "TestMode": "INTERNAL",
    "TimeoutSeconds": 30
  }
}
```

Environment variable:
```
VRPAY_ACCESS_TOKEN=Bearer YOUR_ACCESS_TOKEN
```

### 2. Service Registration

```csharp
services.AddVrPayClient(configuration);
```

### 3. Pre-Authorization

```csharp
var request = new PaymentRequest
{
    Amount = 92.00m,
    Currency = "EUR",
    PaymentBrand = "VISA",
    MerchantTransactionId = "ORDER-12345",
    Card = new CardData
    {
        Number = "4200000000000000",
        Holder = "Jane Jones",
        ExpiryMonth = "05",
        ExpiryYear = "2034",
        Cvv = "123"
    }
};

var response = await vrPayClient.PreAuthorizeAsync(request);
// Store response.Id for capture
```

### 4. Capture

```csharp
var captureResponse = await vrPayClient.CaptureAsync(
    preAuthId: response.Id,
    amount: 92.00m,
    currency: "EUR");
```

## Result Code Quick Check

```csharp
if (response.IsSuccess())
{
    // Process successful payment
}
else
{
    var status = response.GetStatus();
    switch (status)
    {
        case TransactionStatus.SuccessManualReview:
            // Review before fulfillment
            break;
        case TransactionStatus.HardDecline:
            // Payment declined - don't retry
            break;
        // ... other cases
    }
}
```

## Common Result Codes

| Code | Meaning | Action |
|------|---------|--------|
| 000.100.110 | Success (test mode) | ✅ Process payment |
| 000.400.000 | Success - review manually | ⚠️ Manual review |
| 300.100.100 | Soft decline - needs 3DS | 🔁 Retry with 3DS |
| 800.100.153 | Invalid CVV | ❌ Declined |
| 800.100.160 | Card blocked | ❌ Declined |

## Test Cards

| Brand | Number | CVV | Expiry | Result |
|-------|--------|-----|--------|--------|
| VISA | 4200000000000000 | 123 | Any future | ✅ Success |
| VISA | 4000000000000002 | 123 | Any future | ❌ Declined |

## Endpoints

| Operation | Endpoint |
|-----------|----------|
| Pre-auth/Debit | POST `/v1/payments` |
| Capture/Refund/Reverse | POST `/v1/payments/{id}` |

## Error Handling

```csharp
try
{
    var response = await vrPayClient.PreAuthorizeAsync(request);
}
catch (VrPayPaymentDeclinedException ex)
{
    // Payment declined by bank
    // ex.ResultCode, ex.Response
}
catch (VrPayValidationException ex)
{
    // Invalid request data
    // ex.ValidationErrors
}
catch (VrPayCommunicationException ex)
{
    // Network/timeout error
    // Retry may be possible
}
catch (VrPayConfigurationException ex)
{
    // Configuration issue
    // Check entityId and access token
}
```

## Required Fields

### All Requests
- ✅ entityId
- ✅ amount (2 decimals)
- ✅ currency (ISO 4217)
- ✅ paymentType (PA, DB, CP, RF, RV)

### Card Payments
- ✅ paymentBrand (VISA, MASTER, AMEX)
- ✅ card.number
- ✅ card.expiryMonth
- ✅ card.expiryYear
- ⚠️ card.cvv (required for most cards)
- ⚠️ card.holder (recommended)

## Regex Patterns

```csharp
// Success
/^(000\.000\.|000\.100\.1|000\.[36]|000\.400\.[1][12]0)/

// Manual Review
/^(000\.400\.0[^3]|000\.400\.100)/

// Pending
/^(000\.200)/

// Soft Decline (can retry)
/^(300\.100\.100)/

// Hard Decline (don't retry)
/^(800\.[17]00|800\.800\.[123])/
```

## Best Practices

✅ **DO**:
- Validate input before sending
- Store transaction IDs
- Log with sensitive data masked
- Use retry logic for transient errors
- Check result codes, not just HTTP status
- Use HTTPS only

❌ **DON'T**:
- Store CVV after authorization
- Log full card numbers
- Retry hard declines
- Ignore manual review flags
- Use test mode in production
- Create new HttpClient instances

## Useful Links

- [Full Documentation](README.md)
- [Implementation Guide](07-implementation-guide.md)
- [Payment Flow](03-synchronous-payment-flow.md)
- [Result Codes](05-result-codes.md)
- [Testing Guide](09-testing-guide.md)
