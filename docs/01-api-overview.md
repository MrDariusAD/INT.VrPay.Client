# VrPay eCommerce API Overview

## Introduction

The VrPay eCommerce API enables merchants to process payments through a Server-to-Server integration. This guide focuses on **synchronous payment** processing, where payments are processed in near real-time with the merchant in full control of collecting card and payment details.

## API Hosts

### Test Environment
```
https://test.vr-pay-ecommerce.de/
```

### Production Environment
```
https://vr-pay-ecommerce.de/
```

## API Version

The current API version is indicated in the request URL:
```
/v1/payments
```

All changes to the API are backwards compatible. Major breaking changes will be released under a new version.

## Core Concepts

### Server-to-Server Integration

In this integration model:
- **Merchant collects** all card and payment details directly from the customer
- **Full control** over the payment flow and user experience
- **Direct communication** between merchant server and VrPay API
- **Behind-the-scenes** processing ensures seamless customer experience
- **PCI-DSS compliance** required to collect card data

### Synchronous vs Asynchronous Payments

**Synchronous Payment** (Covered in this documentation):
- Payment processed in near real-time
- Immediate response from the payment gateway
- Suitable for: Credit/Debit cards, Direct Debit
- No customer redirection required

**Asynchronous Payment** (Not covered):
- Payment processed with account holder involvement
- Delayed confirmation
- Suitable for: 3D Secure, Online transfers, Virtual wallets
- May require customer redirection

## Transaction Flows

### Supported Payment Types

| Code | Type | Description |
|------|------|-------------|
| **PA** | Pre-authorization | Reserves funds on the customer's card. Must be followed by CP (Capture) |
| **DB** | Debit | Direct debit - debits customer account and credits merchant account immediately |
| **CP** | Capture | Captures previously pre-authorized (PA) funds |
| **RV** | Reversal | Reverses a PA, DB, or CD transaction (before cut-off time) |
| **RF** | Refund | Credits customer account referencing a prior DB or CD transaction |
| **RB** | Rebill | Rebills for additional products on a processed order |
| **CB** | Chargeback | Reflects chargeback processed by the bank |
| **CR** | Chargeback Reversal | Reflects chargeback reversal processed by the bank |

### Primary Transaction Flows (Synchronous)

This documentation covers two main flows:

#### 1. Pre-authorization + Capture Flow (PA + CP)
```
Step 1: Pre-authorize (PA) → Reserves funds
Step 2: Capture (CP)       → Transfers reserved funds
Step 3: Manage (Optional)  → RF, RB, CB, CR
```

**Use Case**: Reserve funds first, capture later (e.g., shipping delay scenarios)

#### 2. Direct Debit Flow (DB)
```
Step 1: Debit (DB)        → Immediate payment
Step 2: Manage (Optional) → RF, RB, CB, CR
```

**Use Case**: Immediate payment capture (e.g., digital goods, instant services)

## HTTP Communication

### Request Method
All payment requests use **HTTP POST** with parameters in the message body (not URL).

### Required Headers

```http
Authorization: Bearer <access-token>
Content-Type: application/x-www-form-urlencoded; charset=UTF-8
```

### Response Format
All responses are in **JSON format**.

### HTTP Status Codes

| Status | Meaning |
|--------|---------|
| **200** | Successful request |
| **307** | Temporary redirect (for asynchronous flows) |
| **400** | Bad request (invalid parameters or failed payment) |
| **401** | Invalid authorization header |
| **403** | Invalid access token |
| **404** | Resource/endpoint not found |

**Note**: For payment success/failure details, always check the `result.code` field in the response body, not just the HTTP status.

## Data Format

### Encoding
- **UTF-8** encoding required for all data
- Use Content-Type header: `application/x-www-form-urlencoded; charset=UTF-8`

### Decimal Numbers
- Decimal separator: **dot (.)** 
- Amount format: `N10.N2` (e.g., `92.00`, `1250.50`)
- Always use 2 decimal places for amounts

### Date Format
- Standard format: `yyyy-MM-dd` (e.g., `2024-12-25`)
- DateTime format: `yyyy-MM-dd HH:mm:ss` (e.g., `2024-12-25 14:30:00`)
- Timestamp format: `yyyy-MM-dd HH:mm:ss.SSS+0000` (e.g., `2024-12-25 14:30:00.752+0000`)

### Currency Codes
- Format: **ISO 4217** (3-letter alphabetic code)
- Examples: `EUR`, `USD`, `GBP`, `CHF`

### Country Codes
- Format: **ISO 3166-1** (2-letter code)
- Examples: `DE`, `AT`, `CH`, `US`, `GB`

## API Throttling

The API implements throttling to protect against overwhelming requests.

### Test Environment
- Throttling is configured with specific limits
- Throttled requests receive error code: `800.120.100`

### Live Environment
- Production throttling values apply
- Plan your integration to respect rate limits

### Throttle Error Response
```json
{
  "buildNumber": "b297e8ec4aa0888454578e292c67546d4c6a5c28@2018-08-30 06:31:46 +****",
  "id": "8ac9a4a8658afc790165a3f0e436198d",
  "ndc": "8acda4c9635ea2d90163636f0a462510_ebb07f3e26e942908d6eeed03a813237",
  "result": {
    "code": "800.120.100",
    "description": "Too many requests. Please try again later."
  },
  "timestamp": "2018-09-04 09:42:33+0000"
}
```

## Security Requirements

### SSL/TLS
- **All requests must be sent over SSL**
- Minimum TLS version: TLS 1.2 recommended

### Authentication
- Every request requires Authorization Bearer header
- Access token obtained from VrPay admin portal
- Token should be kept secure and never exposed client-side

### PCI-DSS Compliance
To collect and transmit card data, you must:
- Be PCI-DSS compliant
- Follow secure coding practices
- Implement proper encryption
- Consider using tokenization to reduce compliance scope

**Alternative**: Use the COPY+PAY widget to minimize PCI-DSS requirements by letting VrPay collect card data.

## Important Notes

### Future-Proof JSON Parsing
⚠️ **Critical**: The API response is in JSON format and may have new fields added at any time for new features. Your integration must:
- Parse JSON properly
- Handle unknown/new fields gracefully
- Not break when new response fields are added

### HTTP POST Parameters
⚠️ **Remember**: For HTTP POST requests, all parameters must be in the **message body**, NOT in the URL.

### Response Processing
Always check the `result.code` field for fine-grained transaction status, not just the HTTP status code.

## Next Steps

Continue to:
- [Authentication & Configuration](02-authentication-configuration.md) - Set up API access
- [Synchronous Payment Flow](03-synchronous-payment-flow.md) - Learn the payment workflow
