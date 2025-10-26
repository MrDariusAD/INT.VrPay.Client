# VrPay Client - Enum Refactoring & Test Helpers

## Summary of Changes

This document describes the enhancements made to the VrPay.Client library to improve type safety and developer experience.

## 1. Enum Types for String Constants

### Created Enums

All string-based fields that accept only specific values have been refactored to use strongly-typed enums:

#### **PaymentType** (`Models/PaymentType.cs`)
```csharp
public enum PaymentType
{
    [EnumMember(Value = "PA")] PreAuthorization,
    [EnumMember(Value = "DB")] Debit,
    [EnumMember(Value = "CP")] Capture,
    [EnumMember(Value = "RF")] Refund,
    [EnumMember(Value = "RV")] Reversal
}
```

#### **PaymentBrand** (`Models/PaymentBrand.cs`)
```csharp
public enum PaymentBrand
{
    [EnumMember(Value = "VISA")] Visa,
    [EnumMember(Value = "MASTER")] Master,
    [EnumMember(Value = "AMEX")] Amex,
    [EnumMember(Value = "DINERS")] Diners,
    [EnumMember(Value = "DISCOVER")] Discover,
    [EnumMember(Value = "JCB")] Jcb,
    [EnumMember(Value = "MAESTRO")] Maestro
}
```

#### **Currency** (`Models/Currency.cs`)
```csharp
public enum Currency
{
    [EnumMember(Value = "EUR")] EUR,
    [EnumMember(Value = "USD")] USD,
    [EnumMember(Value = "GBP")] GBP,
    [EnumMember(Value = "CHF")] CHF,
    // ... and more
}
```

#### **TestMode** (`Models/TestMode.cs`)
```csharp
public enum TestMode
{
    [EnumMember(Value = "EXTERNAL")] External,
    [EnumMember(Value = "INTERNAL")] Internal
}
```

### Benefits

✅ **Compile-Time Safety**: Type checking prevents invalid values  
✅ **IntelliSense Support**: IDEs provide autocomplete with all valid options  
✅ **Refactoring Support**: Rename/find usages work across the codebase  
✅ **Self-Documenting**: Clear what values are allowed

## 2. JSON Serialization with EnumMember

### Custom Converter

Created `EnumMemberConverter<TEnum>` that:
- Serializes enums using their `[EnumMember(Value = "...")]` attribute values
- Deserializes API string values back to enums
- Handles nullable enums with `NullableEnumMemberConverter<TEnum>`

### Updated Models

**PaymentRequest** now uses:
```csharp
[JsonConverter(typeof(EnumMemberConverter<Currency>))]
public Currency Currency { get; set; }

[JsonConverter(typeof(EnumMemberConverter<PaymentType>))]
public PaymentType PaymentType { get; set; }

[JsonConverter(typeof(NullableEnumMemberConverter<PaymentBrand>))]
public PaymentBrand? PaymentBrand { get; set; }
```

**PaymentResponse** uses the same converters for response deserialization.

## 3. Predefined Test Card Objects

### TestCards Class (`Testing/TestCards.cs`)

Provides ready-to-use card data for testing:

```csharp
// Successful cards
TestCards.VisaSuccess          // 4200000000000000
TestCards.MastercardSuccess    // 5200000000000007
TestCards.AmexSuccess          // 340000000000009

// Declined cards
TestCards.VisaDeclined         // 4000000000000002
TestCards.MastercardDeclined   // 5100000000000016

// Validation error cards
TestCards.InvalidCvv
TestCards.ExpiredCard
```

### TestData Class (`Testing/TestData.cs`)

Provides complete payment request objects:

```csharp
// Create a successful test payment
var request = TestData.CreateSuccessfulPaymentRequest(
    amount: 92.00m,
    currency: Currency.EUR,
    paymentBrand: PaymentBrand.Visa
);

// Create a declined test payment
var request = TestData.CreateDeclinedPaymentRequest(
    amount: 100.00m,
    currency: Currency.EUR,
    paymentBrand: PaymentBrand.Visa
);
```

Both methods include:
- Default customer data
- Default billing address
- Automatic merchant transaction ID generation
- Proper amount formatting (culture-invariant)

## 4. Configuration Updates

### VrPayConfiguration

Updated to use the new enum:

```csharp
public bool UseTestMode { get; set; } = true;
public TestMode TestModeValue { get; set; } = TestMode.External;
```

### Integration Test Configuration

Updated `appsettings.json` with default test values:
```json
{
  "VrPay": {
    "BaseUrl": "https://test.vr-pay-ecommerce.de/",
    "EntityId": "8a8294174b7ecb28014b9699220015ca",
    "AccessToken": "Bearer OGE4Mjk0MTc0YjdlY2IyODAxNGI5Njk5MjIwMDE1Y2N8c3k2S0pzVDg=",
    "UseTestMode": true,
    "TestModeValue": 0
  }
}
```

Note: These credentials are from VrPay's public documentation for sandbox testing.

## 5. Critical Bug Fixes

### Culture-Invariant Amount Formatting

**Problem**: Amount was formatted using current culture (e.g., "92,00" in German locale)  
**Solution**: Use `CultureInfo.InvariantCulture` for all decimal formatting

```csharp
Amount = amount.ToString("F2", CultureInfo.InvariantCulture)
```

This ensures amounts are always sent as "92.00" regardless of system locale.

## 6. Updated Integration Tests

All integration tests now use the new enums and test helpers:

```csharp
// Before
var request = new PaymentRequest
{
    Amount = "92.00",
    Currency = "EUR",
    PaymentBrand = "VISA",
    Card = new CardData { /* ... */ }
};

// After
var request = TestData.CreateSuccessfulPaymentRequest(
    amount: 92.00m,
    currency: Currency.EUR,
    paymentBrand: PaymentBrand.Visa
);
```

Assertions also use enums:
```csharp
response.PaymentType.Should().Be(PaymentType.PreAuthorization);
response.Currency.Should().Be(Currency.EUR);
```

## 7. Usage Examples

### Simple Payment with Enums

```csharp
var request = new PaymentRequest
{
    Amount = "50.00",
    Currency = Currency.EUR,
    PaymentType = PaymentType.Debit,
    PaymentBrand = PaymentBrand.Visa,
    Card = TestCards.VisaSuccess,
    Customer = TestData.DefaultCustomer
};

var response = await client.DebitAsync(request);
```

### Using Test Helpers

```csharp
// Quick test payment
var request = TestData.CreateSuccessfulPaymentRequest(
    amount: 25.50m,
    currency: Currency.USD,
    paymentBrand: PaymentBrand.Master
);
request.MerchantTransactionId = "MY-ORDER-123";

var response = await client.PreAuthorizeAsync(request);
```

### Capture with Enums

```csharp
var captureResponse = await client.CaptureAsync(
    preAuthId: "transaction-id",
    amount: 50.00m,
    currency: Currency.EUR
);
```

## 8. Test Results

All tests passing after refactoring:

```
✅ 54 tests passed
⏭️  6 tests skipped (require real credentials)
❌ 0 tests failed

Total: 60 tests
- Unit Tests: 52 passing
- Integration Tests: 2 passing, 6 skipped
```

## 9. Breaking Changes

### For Existing Code

If you were using string values directly, you'll need to update to enums:

```csharp
// Old
request.Currency = "EUR";
request.PaymentType = "PA";
request.PaymentBrand = "VISA";

// New
request.Currency = Currency.EUR;
request.PaymentType = PaymentType.PreAuthorization;
request.PaymentBrand = PaymentBrand.Visa;
```

### Interface Changes

```csharp
// Old
Task<PaymentResponse> CaptureAsync(string preAuthId, decimal amount, string currency, ...)

// New
Task<PaymentResponse> CaptureAsync(string preAuthId, decimal amount, Currency currency, ...)
```

## 10. Migration Guide

### Step 1: Update PaymentRequest Construction

Replace string literals with enum values:
```csharp
Currency = Currency.EUR
PaymentBrand = PaymentBrand.Visa
```

### Step 2: Update Method Calls

Update `CaptureAsync` and `RefundAsync` calls:
```csharp
await client.CaptureAsync(id, 50.00m, Currency.EUR);
```

### Step 3: Use Test Helpers (Optional)

Replace manual test data creation with helpers:
```csharp
var request = TestData.CreateSuccessfulPaymentRequest();
```

### Step 4: Update Assertions

In tests, compare against enums:
```csharp
response.PaymentType.Should().Be(PaymentType.Capture);
```

## Files Modified

### New Files
- `src/VrPay.Client/Models/PaymentType.cs`
- `src/VrPay.Client/Models/PaymentBrand.cs`
- `src/VrPay.Client/Models/Currency.cs`
- `src/VrPay.Client/Models/TestMode.cs`
- `src/VrPay.Client/Converters/EnumMemberConverter.cs`
- `src/VrPay.Client/Testing/TestCards.cs`
- `src/VrPay.Client/Testing/TestData.cs`
- `test-console/Program.cs` (test console app)

### Modified Files
- `src/VrPay.Client/Models/PaymentRequest.cs`
- `src/VrPay.Client/Models/PaymentResponse.cs`
- `src/VrPay.Client/Configuration/VrPayConfiguration.cs`
- `src/VrPay.Client/IVrPayClient.cs`
- `src/VrPay.Client/VrPayClient.cs`
- `tests/VrPay.Client.IntegrationTests/IntegrationTestFixture.cs`
- `tests/VrPay.Client.IntegrationTests/PaymentIntegrationTests.cs`
- `tests/VrPay.Client.IntegrationTests/appsettings.json`
- `tests/VrPay.Client.Tests/Configuration/VrPayConfigurationTests.cs`

## Conclusion

These changes significantly improve the developer experience by:

1. ✅ Providing compile-time type safety for all API parameters
2. ✅ Making code more maintainable and refactor-friendly
3. ✅ Offering ready-to-use test data for quick testing
4. ✅ Fixing critical culture-specific formatting issues
5. ✅ Improving IntelliSense support in IDEs

The library is now more robust, easier to use, and less prone to runtime errors from typos in string values.
