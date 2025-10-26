# Synchronous Payment Flow

## Overview

Synchronous payments are processed in near real-time with immediate responses. This document covers the complete workflow for synchronous credit/debit card payments using the Server-to-Server integration.

## Transaction Flows

### Flow 1: Pre-authorization + Capture (PA + CP)

This two-step flow allows you to reserve funds first and capture them later.

```
┌─────────────┐        ┌─────────────┐        ┌─────────────┐
│  Merchant   │        │   VrPay     │        │    Bank     │
│   Server    │        │   Gateway   │        │   (Issuer)  │
└──────┬──────┘        └──────┬──────┘        └──────┬──────┘
       │                      │                      │
       │ 1. PA Request        │                      │
       │─────────────────────>│                      │
       │                      │ Authorize            │
       │                      │─────────────────────>│
       │                      │<─────────────────────│
       │ 2. PA Response (id)  │   Funds Reserved     │
       │<─────────────────────│                      │
       │                      │                      │
       │ ... Time passes ...  │                      │
       │                      │                      │
       │ 3. CP Request (ref)  │                      │
       │─────────────────────>│                      │
       │                      │ Capture Funds        │
       │                      │─────────────────────>│
       │                      │<─────────────────────│
       │ 4. CP Response       │   Funds Transferred  │
       │<─────────────────────│                      │
       │                      │                      │
```

**Use Cases**:
- **Delayed Shipping**: Reserve funds at order, capture when item ships
- **Hotel Bookings**: Reserve at booking, capture at check-in/check-out
- **Rental Services**: Reserve at reservation, capture after service completion
- **Split Payments**: Multiple captures against single authorization

**Benefits**:
- Funds guaranteed but not yet transferred
- Merchant flexibility in timing
- Customer sees pending charge
- Can reverse before capture

### Flow 2: Direct Debit (DB)

Single-step flow that immediately debits the customer and credits the merchant.

```
┌─────────────┐        ┌─────────────┐        ┌─────────────┐
│  Merchant   │        │   VrPay     │        │    Bank     │
│   Server    │        │   Gateway   │        │   (Issuer)  │
└──────┬──────┘        └──────┬──────┘        └──────┬──────┘
       │                      │                      │
       │ 1. DB Request        │                      │
       │─────────────────────>│                      │
       │                      │ Charge               │
       │                      │─────────────────────>│
       │                      │<─────────────────────│
       │ 2. DB Response (id)  │   Payment Complete   │
       │<─────────────────────│                      │
       │                      │                      │
```

**Use Cases**:
- **Digital Goods**: Immediate delivery requires immediate payment
- **Instant Services**: Payment and service completion simultaneous
- **Simple Checkouts**: No need for two-step process
- **Subscriptions**: Recurring charges

**Benefits**:
- Immediate payment
- Simpler workflow
- Faster settlement
- Direct completion

## Step-by-Step: Pre-authorization + Capture

### Step 1: Pre-authorize Payment (PA)

#### 1.1 Request

**Endpoint**: `POST /v1/payments`

**Required Parameters**:

| Parameter | Description | Format | Example |
|-----------|-------------|--------|---------|
| `entityId` | Channel/Merchant entity ID | `[a-f0-9]{32}` | `8a8294174e735d0c014e78beb6b9154b` |
| `amount` | Payment amount | `[0-9]{1,10}\\.[0-9]{2}` | `92.00` |
| `currency` | Currency code (ISO 4217) | `[A-Z]{3}` | `EUR` |
| `paymentBrand` | Card brand | `AN32` | `VISA` |
| `paymentType` | Transaction type | `PA` | `PA` |
| `card.number` | Card number (PAN) | `[0-9]{8,32}` | `4200000000000000` |
| `card.holder` | Cardholder name | `{3,128}` | `Jane Jones` |
| `card.expiryMonth` | Card expiry month | `(0[1-9]\\|1[0-2])` | `05` |
| `card.expiryYear` | Card expiry year | `(19\\|20)([0-9]{2})` | `2034` |
| `card.cvv` | Card security code | `[0-9]{3,4}` | `123` |

**Optional but Recommended**:

| Parameter | Description | Example |
|-----------|-------------|---------|
| `merchantTransactionId` | Your unique order/transaction ID | `ORDER-12345-2024` |
| `customer.email` | Customer email | `customer@example.com` |
| `customer.ip` | Customer IP address | `192.168.1.100` |
| `billing.street1` | Billing address street | `123 Main Street` |
| `billing.city` | Billing city | `Berlin` |
| `billing.postcode` | Billing postal code | `10115` |
| `billing.country` | Billing country (ISO 3166-1) | `DE` |

**Example Request** (application/x-www-form-urlencoded):

```
POST https://test.vr-pay-ecommerce.de/v1/payments HTTP/1.1
Authorization: Bearer OGE4Mjk0MTc0ZTczNWQwYzAxNGU3OGJlYjZjNTE1NGZ8TnE3b0UlMmEyQldKcjQlJTZYZFk=
Content-Type: application/x-www-form-urlencoded; charset=UTF-8

entityId=8a8294174e735d0c014e78beb6b9154b
&amount=92.00
&currency=EUR
&paymentBrand=VISA
&paymentType=PA
&card.number=4200000000000000
&card.holder=Jane Jones
&card.expiryMonth=05
&card.expiryYear=2034
&card.cvv=123
&merchantTransactionId=ORDER-12345-2024
&customer.email=customer@example.com
```

**C# Example**:

```csharp
var parameters = new Dictionary<string, string>
{
    { "entityId", "8a8294174e735d0c014e78beb6b9154b" },
    { "amount", "92.00" },
    { "currency", "EUR" },
    { "paymentBrand", "VISA" },
    { "paymentType", "PA" },
    { "card.number", "4200000000000000" },
    { "card.holder", "Jane Jones" },
    { "card.expiryMonth", "05" },
    { "card.expiryYear", "2034" },
    { "card.cvv", "123" },
    { "merchantTransactionId", "ORDER-12345-2024" }
};

var content = new FormUrlEncodedContent(parameters);
var response = await httpClient.PostAsync("/v1/payments", content);
```

#### 1.2 Response

**Success Response** (HTTP 200):

```json
{
  "id": "8ac7a4a289d210fc0189d5b4e9572cc8",
  "paymentType": "PA",
  "paymentBrand": "VISA",
  "amount": "92.00",
  "currency": "EUR",
  "descriptor": "7766.2840.5126 OPP_Channel",
  "result": {
    "code": "000.100.110",
    "description": "Request successfully processed in 'Merchant in Integrator Test Mode'"
  },
  "resultDetails": {
    "ExtendedDescription": "Authorized",
    "clearingInstituteName": "Your Clearing Institute Name",
    "ConnectorTxID1": "00000000776628405126",
    "ConnectorTxID2": "012416",
    "ConnectorTxID3": "0170|085",
    "AcquirerResponse": "0000"
  },
  "card": {
    "bin": "420000",
    "last4Digits": "0000",
    "holder": "Jane Jones",
    "expiryMonth": "05",
    "expiryYear": "2034"
  },
  "risk": {
    "score": "100"
  },
  "buildNumber": "5ce30a257f96b238fa8ecc669ffdc2a77040ba35@2023-08-08 07:36:04 +0000",
  "timestamp": "2023-08-08 15:12:30.752+0000",
  "ndc": "8a8294174b7ecb28014b9699220015ca_83281e556b1e4216bef5c3bc1f63c45c"
}
```

**Key Response Fields**:

| Field | Description | Example |
|-------|-------------|---------|
| `id` | **Transaction ID** - Required for capture | `8ac7a4a289d210fc0189d5b4e9572cc8` |
| `result.code` | Result status code | `000.100.110` |
| `result.description` | Human-readable description | `Request successfully processed` |
| `paymentType` | Echoed payment type | `PA` |
| `amount` | Echoed amount | `92.00` |
| `currency` | Echoed currency | `EUR` |
| `card.bin` | First 6 digits of card | `420000` |
| `card.last4Digits` | Last 4 digits of card | `0000` |

**⚠️ Critical**: Save the `id` field! You'll need it for the capture request.

#### 1.3 Evaluate Result

Check the `result.code` to determine success:

**Success Codes** (Regex: `/^(000\\.000\\.|000\\.100\\.1|000\\.[36])/`):
- `000.000.000` - Transaction succeeded
- `000.100.110` - Successfully processed in test mode
- `000.100.111`, `000.100.112` - Test mode variations

**Manual Review Required** (Regex: `/^(000\\.400\\.0[^3]|000\\.400\\.100)/`):
- `000.400.000` - Succeeded but review manually (fraud suspicion)
- `000.400.010` - Review manually (AVS return code)
- `000.400.020` - Review manually (CVV return code)

**Pending** (Regex: `/^(000\\.200)/`):
- `000.200.000` - Transaction pending

**Declined** - Any other code (see [Result Codes](05-result-codes.md))

### Step 2: Capture Payment (CP)

After successful pre-authorization, capture the funds when ready (e.g., when shipping).

#### 2.1 Request

**Endpoint**: `POST /v1/payments/{id}`

Where `{id}` is the transaction ID from the PA response.

**Required Parameters**:

| Parameter | Description | Format | Example |
|-----------|-------------|--------|---------|
| `entityId` | Same entity ID as PA | `[a-f0-9]{32}` | `8a8294174e735d0c014e78beb6b9154b` |
| `amount` | Amount to capture | `[0-9]{1,10}\\.[0-9]{2}` | `92.00` |
| `currency` | Currency (must match PA) | `[A-Z]{3}` | `EUR` |
| `paymentType` | Transaction type | `CP` | `CP` |

**Example Request**:

```
POST https://test.vr-pay-ecommerce.de/v1/payments/8ac7a4a289d210fc0189d5b4e9572cc8 HTTP/1.1
Authorization: Bearer OGE4Mjk0MTc0ZTczNWQwYzAxNGU3OGJlYjZjNTE1NGZ8TnE3b0UlMmEyQldKcjQlJTZYZFk=
Content-Type: application/x-www-form-urlencoded; charset=UTF-8

entityId=8a8294174e735d0c014e78beb6b9154b
&amount=92.00
&currency=EUR
&paymentType=CP
```

**C# Example**:

```csharp
var preAuthId = "8ac7a4a289d210fc0189d5b4e9572cc8"; // From PA response

var parameters = new Dictionary<string, string>
{
    { "entityId", "8a8294174e735d0c014e78beb6b9154b" },
    { "amount", "92.00" },
    { "currency", "EUR" },
    { "paymentType", "CP" }
};

var content = new FormUrlEncodedContent(parameters);
var response = await httpClient.PostAsync($"/v1/payments/{preAuthId}", content);
```

#### 2.2 Response

**Success Response**:

```json
{
  "id": "8ac7a4a289d210fc0189d5b4e9583cd9",
  "paymentType": "CP",
  "paymentBrand": "VISA",
  "amount": "92.00",
  "currency": "EUR",
  "descriptor": "7766.2840.5126 OPP_Channel",
  "result": {
    "code": "000.100.110",
    "description": "Request successfully processed in 'Merchant in Integrator Test Mode'"
  },
  "referencedId": "8ac7a4a289d210fc0189d5b4e9572cc8",
  "buildNumber": "5ce30a257f96b238fa8ecc669ffdc2a77040ba35@2023-08-08 07:36:04 +0000",
  "timestamp": "2023-08-08 15:15:12.348+0000",
  "ndc": "8a8294174b7ecb28014b9699220015ca_83281e556b1e4216bef5c3bc1f63c45c"
}
```

**Key Difference**: 
- New `id` for the capture transaction
- `referencedId` contains the original PA transaction ID
- `paymentType` is now `CP`

### Step 3: Manage Payment (Optional)

After capture, you can perform back-office operations.

#### 3.1 Refund (RF)

Refund captured funds to the customer.

**Endpoint**: `POST /v1/payments/{id}`

Where `{id}` is either the CP or DB transaction ID.

**Parameters**:

```
entityId=8a8294174e735d0c014e78beb6b9154b
&amount=92.00
&currency=EUR
&paymentType=RF
```

**Partial Refund**:
```
amount=50.00  // Refund only part of the original amount
```

#### 3.2 Reversal (RV)

Reverse a pre-authorization or debit before cut-off time.

**Parameters**:

```
entityId=8a8294174e735d0c014e78beb6b9154b
&paymentType=RV
```

**Note**: Amount is automatically the full amount of the referenced transaction.

#### 3.3 Chargeback (CB)

Reflect a chargeback initiated by the bank.

**Note**: Chargebacks are typically created internally by the payment system, not via API.

## Step-by-Step: Direct Debit

### Step 1: Debit Payment (DB)

Single-step immediate payment.

#### 1.1 Request

**Endpoint**: `POST /v1/payments`

**Required Parameters**: Same as PA, but with `paymentType=DB`

```
entityId=8a8294174e735d0c014e78beb6b9154b
&amount=92.00
&currency=EUR
&paymentBrand=VISA
&paymentType=DB
&card.number=4200000000000000
&card.holder=Jane Jones
&card.expiryMonth=05
&card.expiryYear=2034
&card.cvv=123
```

#### 1.2 Response

```json
{
  "id": "8ac7a4a289d210fc0189d5b4e9572cc8",
  "paymentType": "DB",
  "paymentBrand": "VISA",
  "amount": "92.00",
  "currency": "EUR",
  "result": {
    "code": "000.100.110",
    "description": "Request successfully processed"
  },
  "card": {
    "bin": "420000",
    "last4Digits": "0000",
    "holder": "Jane Jones",
    "expiryMonth": "05",
    "expiryYear": "2034"
  },
  "timestamp": "2023-08-08 15:12:30.752+0000"
}
```

**That's it!** Payment is complete. No capture needed.

### Step 2: Manage Payment (Optional)

You can refund or perform other back-office operations as needed.

## Partial Captures

You can perform multiple partial captures against a single pre-authorization.

**Example**:
1. Pre-authorize: €100.00
2. First capture: €60.00
3. Second capture: €40.00

**Configuration**: Set `numberOfCaptures` parameter in PA request:

```
numberOfCaptures=2  // Allow up to 2 captures
```

**Limitations**:
- Maximum 98 captures per pre-authorization
- Total captured amount cannot exceed pre-authorized amount
- Check acquirer support for multiple captures

## Important Notes

### Cut-off Times

- **Reversal (RV)**: Only possible before connector-specific cut-off time
- **After Cut-off**: Use Refund (RF) instead
- Cut-off times vary by acquirer/connector

### Amount Validation

- Capture amount **cannot exceed** pre-authorized amount
- Capture amount **can be less** than pre-authorized amount
- Currency **must match** original transaction

### Transaction States

**PA (Pre-authorization)**:
- `NEW` → `OPEN` (successful) or `REJECTED` (failed)

**CP (Capture)**:
- Requires PA in `OPEN` state
- Changes PA state to `CLOSED`

**RV (Reversal)**:
- Requires PA/DB/CP in `OPEN` state
- Changes state to `REVERSED`

**RF (Refund)**:
- Requires CP/DB in `CLOSED` state
- Creates separate refund transaction

### Error Handling

Always check `result.code` before proceeding:

```csharp
if (response.Result.Code.StartsWith("000."))
{
    // Success or pending
    if (Regex.IsMatch(response.Result.Code, @"^(000\.000\.|000\.100\.1|000\.[36])"))
    {
        // Definite success
    }
    else if (Regex.IsMatch(response.Result.Code, @"^(000\.400\.0[^3]|000\.400\.100)"))
    {
        // Success but requires manual review
    }
}
else
{
    // Failed - check specific error code
}
```

## Next Steps

Continue to:
- [Payment Models](04-payment-models.md) - Complete data structure reference
- [Result Codes](05-result-codes.md) - All result codes and meanings
- [Error Handling](06-error-handling.md) - Comprehensive error handling strategies
