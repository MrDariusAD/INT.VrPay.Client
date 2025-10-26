# Payment Models

## Overview

This document provides complete reference for all data structures used in VrPay eCommerce API requests and responses.

## Request Models

### Basic Payment Parameters

#### Required Fields

| Field | Description | Format | Required For | Example |
|-------|-------------|--------|--------------|---------|
| `entityId` | Channel/Merchant entity identifier | `[a-f0-9]{32}` | All | `8a8294174e735d0c014e78beb6b9154b` |
| `amount` | Payment amount (2 decimals) | `[0-9]{1,10}\\.[0-9]{2}` | All | `92.00` |
| `currency` | ISO 4217 currency code | `[A-Z]{3}` | All | `EUR` |
| `paymentType` | Transaction type | `PA\|DB\|CP\|RV\|RF\|RB\|CB\|CR` | All | `PA` |

#### Optional but Recommended

| Field | Description | Format | Example |
|-------|-------------|--------|---------|
| `paymentBrand` | Card brand (required for cards) | `VISA\|MASTER\|AMEX` | `VISA` |
| `merchantTransactionId` | Your unique transaction ID | `[\\s\\S]{8,255}` | `ORDER-12345-2024` |
| `descriptor` | Statement descriptor | `[\\s\\S]{1,127}` | `MyStore Purchase` |
| `merchantInvoiceId` | Invoice number | `[\\s\\S]{8,255}` | `INV-2024-001` |

### Card Account Data

#### Required for Card Payments

| Field | Description | Format | Example |
|-------|-------------|--------|---------|
| `card.number` | Card number (PAN) | `[0-9]{8,32}` | `4200000000000000` |
| `card.expiryMonth` | Expiry month | `(0[1-9]\|1[0-2])` | `05` |
| `card.expiryYear` | Expiry year | `(19\|20)([0-9]{2})` | `2034` |

#### Conditional

| Field | Description | Format | Example |
|-------|-------------|--------|---------|
| `card.holder` | Cardholder name | `{3,128}` | `Jane Jones` |
| `card.cvv` | CVV/CVC code | `[0-9]{3,4}` | `123` |

### Customer Data

| Field | Description | Format | Example |
|-------|-------------|--------|---------|
| `customer.givenName` | First name | `[\\s\\S]{1,50}` | `Jane` |
| `customer.surname` | Last name | `[\\s\\S]{1,50}` | `Jones` |
| `customer.email` | Email address | `[\\s\\S]{6,128}` | `jane@example.com` |
| `customer.phone` | Phone number | `[+0-9][0-9 \\.()/-]{5,25}` | `+49 30 12345678` |
| `customer.ip` | Customer IP address | Valid IP | `192.168.1.100` |
| `customer.birthDate` | Birth date | `yyyy-MM-dd` | `1985-03-15` |
| `customer.merchantCustomerId` | Your customer ID | `[\\s\\S]{1,255}` | `CUST-12345` |

### Billing Address

| Field | Description | Format | Example |
|-------|-------------|--------|---------|
| `billing.street1` | Street address | `[\\s\\S]{1,100}` | `123 Main Street` |
| `billing.city` | City | `[\\s\\S]{1,80}` | `Berlin` |
| `billing.state` | State/Region | `[a-zA-Z0-9\\.]{1,50}` | `BE` |
| `billing.postcode` | Postal code | `[A-Za-z0-9]{1,16}` | `10115` |
| `billing.country` | ISO 3166-1 country | `[A-Z]{2}` | `DE` |

### Shipping Address

Same structure as billing, prefixed with `shipping.` instead of `billing.`

Additional shipping fields:

| Field | Description | Format | Example |
|-------|-------------|--------|---------|
| `shipping.cost` | Shipping cost | `[0-9]{1,10}\\.[0-9]{2}` | `5.99` |
| `shipping.method` | Shipping method | See enum below | `EXPEDITED` |

**Shipping Methods**: `CARRIER_DESIGNATED_BY_CUSTOMER`, `ELECTRONIC_DELIVERY`, `EXPEDITED`, `GROUND`, `INTERNATIONAL`, `LOWEST_COST`, `MILITARY`, `NEXT_DAY_OVERNIGHT`, `OTHER`, `POINT_PICKUP`, `SAME_DAY_SERVICE`, `STORE_PICKUP`, `THREE_DAY_SERVICE`, `TWO_DAY_SERVICE`

### Cart Items

Cart items use indexed notation: `cart.items[n].fieldName` where n starts at 0.

| Field | Description | Format | Example |
|-------|-------------|--------|---------|
| `cart.items[0].name` | Item name | `[\\s\\S]{1,255}` | `Premium Widget` |
| `cart.items[0].merchantItemId` | Your item SKU | `[\\s\\S]{1,255}` | `SKU-12345` |
| `cart.items[0].description` | Item description | `[\\s\\S]{1,2048}` | `High quality widget` |
| `cart.items[0].price` | Unit price (incl tax) | `[0-9]{1,10}\\.[0-9]{2}` | `29.99` |
| `cart.items[0].quantity` | Quantity | `[0-9]{1,12}(\\.[0-9]{0,3})?` | `2` |
| `cart.items[0].totalAmount` | Total (price × quantity) | `[0-9]{1,10}\\.[0-9]{2}` | `59.98` |
| `cart.items[0].type` | Item type | See below | `PHYSICAL` |
| `cart.items[0].currency` | ISO 4217 currency | `[A-Z]{3}` | `EUR` |

**Item Types**: `PHYSICAL`, `DIGITAL`, `MIXED`, `ANONYMOUS_DONATION`, `AUTHORITIES_PAYMENT`

## Response Models

### Payment Response Structure

```json
{
  "id": "string",
  "paymentType": "string",
  "paymentBrand": "string",
  "amount": "string",
  "currency": "string",
  "descriptor": "string",
  "result": {
    "code": "string",
    "description": "string",
    "avsResponse": "string",
    "cvvResponse": "string"
  },
  "resultDetails": {
    "ExtendedDescription": "string",
    "clearingInstituteName": "string",
    "ConnectorTxID1": "string",
    "ConnectorTxID2": "string",
    "ConnectorTxID3": "string",
    "AcquirerResponse": "string"
  },
  "card": {
    "bin": "string",
    "last4Digits": "string",
    "holder": "string",
    "expiryMonth": "string",
    "expiryYear": "string"
  },
  "buildNumber": "string",
  "timestamp": "string",
  "ndc": "string"
}
```

### Result Object

| Field | Description | Format |
|-------|-------------|--------|
| `code` | Result code | `[0-9\\.]{2,11}` |
| `description` | Description | `[\\s\\S]{0,255}` |
| `avsResponse` | AVS check result | `[A-Z]{1}` |
| `cvvResponse` | CVV check result | `[A-Z]{1}` |

**AVS Response Codes**:
- `A` - Address matches, zip does not
- `Z` - Zip matches, address does not  
- `N` - Neither matches
- `U` - Technical error or not applicable
- `F` - Full match

**CVV Response Codes**:
- `M` - CVV2 Match
- `N` - CVV2 does not match
- `P` - Not Processed
- `S` - CVV should be present but is not
- `U` - Unsupported by issuer

### Card Response Object

| Field | Description | Example |
|-------|-------------|---------|
| `bin` | First 6 digits | `420000` |
| `last4Digits` | Last 4 digits | `0000` |
| `holder` | Cardholder name | `Jane Jones` |
| `expiryMonth` | Expiry month | `05` |
| `expiryYear` | Expiry year | `2034` |

## Test Data

### Test Card Numbers

| Brand | Number (no 3DS) | CVV | Expiry |
|-------|-----------------|-----|--------|
| VISA | 4200000000000000 | Any 3 digits | Any future date |
| MASTERCARD | 5454545454545454 | Any 3 digits | Any future date |
| AMEX | 377777777777770 | Any 4 digits | Any future date |

### Test Amounts

Different amounts trigger different responses in test mode:

| Amount | Result |
|--------|--------|
| 92.00 | Success |
| 90.00 | Declined |

## .NET Model Classes

### Request Models

```csharp
public class PaymentRequest
{
    [Required]
    public string EntityId { get; set; }
    
    [Required]
    [RegularExpression(@"^[0-9]{1,10}\.[0-9]{2}$")]
    public string Amount { get; set; }
    
    [Required]
    [RegularExpression(@"^[A-Z]{3}$")]
    public string Currency { get; set; }
    
    [Required]
    public string PaymentType { get; set; }
    
    public string? PaymentBrand { get; set; }
    public string? MerchantTransactionId { get; set; }
    public CardData? Card { get; set; }
    public CustomerData? Customer { get; set; }
    public AddressData? Billing { get; set; }
    public AddressData? Shipping { get; set; }
}

public class CardData
{
    [Required]
    [RegularExpression(@"^[0-9]{8,32}$")]
    public string Number { get; set; }
    
    [Required]
    [RegularExpression(@"^(0[1-9]|1[0-2])$")]
    public string ExpiryMonth { get; set; }
    
    [Required]
    [RegularExpression(@"^(19|20)([0-9]{2})$")]
    public string ExpiryYear { get; set; }
    
    [RegularExpression(@"^[0-9]{3,4}$")]
    public string? Cvv { get; set; }
    
    [StringLength(128, MinimumLength = 3)]
    public string? Holder { get; set; }
}

public class CustomerData
{
    [StringLength(50)]
    public string? GivenName { get; set; }
    
    [StringLength(50)]
    public string? Surname { get; set; }
    
    [EmailAddress]
    [StringLength(128, MinimumLength = 6)]
    public string? Email { get; set; }
    
    [Phone]
    [StringLength(25, MinimumLength = 7)]
    public string? Phone { get; set; }
    
    public string? Ip { get; set; }
}

public class AddressData
{
    [StringLength(100)]
    public string? Street1 { get; set; }
    
    [StringLength(80)]
    public string? City { get; set; }
    
    [StringLength(16)]
    public string? Postcode { get; set; }
    
    [RegularExpression(@"^[A-Z]{2}$")]
    public string? Country { get; set; }
}
```

### Response Models

```csharp
public class PaymentResponse
{
    public string Id { get; set; }
    public string PaymentType { get; set; }
    public string? PaymentBrand { get; set; }
    public string Amount { get; set; }
    public string Currency { get; set; }
    public Result Result { get; set; }
    public Dictionary<string, string>? ResultDetails { get; set; }
    public CardResponse? Card { get; set; }
    public string BuildNumber { get; set; }
    public string Timestamp { get; set; }
    public string Ndc { get; set; }
}

public class Result
{
    public string Code { get; set; }
    public string Description { get; set; }
    public string? AvsResponse { get; set; }
    public string? CvvResponse { get; set; }
}

public class CardResponse
{
    public string Bin { get; set; }
    public string Last4Digits { get; set; }
    public string? Holder { get; set; }
    public string ExpiryMonth { get; set; }
    public string ExpiryYear { get; set; }
}
```

## Next Steps

Continue to:
- [Result Codes](05-result-codes.md) - Complete result code reference
- [Error Handling](06-error-handling.md) - Error handling strategies
