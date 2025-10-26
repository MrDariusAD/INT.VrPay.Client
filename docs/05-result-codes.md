# Result Codes Reference

## Overview

Result codes indicate the outcome of each transaction. Each code follows the format `ddd.ddd.ddd` where each segment represents progressively more specific information.

## Success Detection

### Successful Transactions

**Regex Pattern**: `/^(000\\.000\\.|000\\.100\\.1|000\\.[36]|000\\.400\\.[1][12]0)/`

| Code | Description |
|------|-------------|
| 000.000.000 | Transaction succeeded |
| 000.100.110 | Request successfully processed in 'Merchant in Integrator Test Mode' |
| 000.100.111 | Request successfully processed in 'Merchant in Validator Test Mode' |
| 000.100.112 | Request successfully processed in 'Merchant in Connector Test Mode' |
| 000.300.000 | Two-step transaction succeeded |
| 000.400.110 | Authentication successful (frictionless flow) |
| 000.400.120 | Authentication successful (data only flow) |

**C# Success Check**:
```csharp
public static bool IsSuccess(string resultCode)
{
    return Regex.IsMatch(resultCode, @"^(000\.000\.|000\.100\.1|000\.[36]|000\.400\.[1][12]0)");
}
```

### Manual Review Required

**Regex Pattern**: `/^(000\\.400\\.0[^3]|000\\.400\\.100)/`

| Code | Description | Action |
|------|-------------|--------|
| 000.400.000 | Transaction succeeded (review manually - fraud suspicion) | Verify before fulfillment |
| 000.400.010 | Transaction succeeded (review manually - AVS) | Check address |
| 000.400.020 | Transaction succeeded (review manually - CVV) | Verify CVV |
| 000.400.060 | Transaction succeeded (approved at merchant's risk) | Manual review |

### Pending Transactions

**Short-term Pending** - Regex: `/^(000\\.200)/`

| Code | Description | Action |
|------|-------------|--------|
| 000.200.000 | Transaction pending | Wait ~30 minutes |
| 000.200.100 | Successfully created checkout | Continue workflow |

**Long-term Pending** - Regex: `/^(800\\.400\\.5|100\\.400\\.500)/`

| Code | Description | Action |
|------|-------------|--------|
| 100.400.500 | Waiting for external risk | Monitor for updates |
| 800.400.500 | Waiting for confirmation of non-instant payment | Don't retry |

## Common Decline Codes

### Bank/Issuer Declines

**Regex Pattern**: `/^(800\\.[17]00|800\\.800\\.[123])/`

| Code | Description | Action |
|------|-------------|--------|
| 800.100.100 | Transaction declined (unknown reason) | Try another card |
| 800.100.152 | Transaction declined by authorization system | Contact bank |
| 800.100.153 | Transaction declined (invalid CVV) | Check CVV |
| 800.100.155 | Transaction declined (amount exceeds credit) | Insufficient funds |
| 800.100.157 | Transaction declined (wrong expiry date) | Check expiry |
| 800.100.159 | Transaction declined (stolen card) | Do not retry |
| 800.100.160 | Transaction declined (card blocked) | Do not retry |
| 800.100.162 | Transaction declined (limit exceeded) | Try smaller amount |
| 800.100.165 | Transaction declined (card lost) | Do not retry |
| 800.100.170 | Transaction declined (transaction not permitted) | Check with bank |

### Soft Declines (Retry Possible)

**Regex Pattern**: `/^(300\\.100\\.100)/`

| Code | Description | Action |
|------|-------------|--------|
| 300.100.100 | Transaction declined (additional authentication required) | Retry with 3DS |

## Validation Errors

### Account Validation

**Regex Pattern**: `/^(100\\.100|100\\.2[01])/`

| Code | Description | Fix |
|------|-------------|-----|
| 100.100.100 | No creditcard/account number | Provide card number |
| 100.100.101 | Invalid creditcard/account number | Check card number |
| 100.100.200 | Request contains no month | Provide expiry month |
| 100.100.300 | Request contains no year | Provide expiry year |
| 100.100.303 | Card expired | Use valid card |
| 100.100.600 | Empty CVV not allowed | Provide CVV |
| 100.100.700 | Invalid cc number/brand combination | Check brand |

### Amount Validation

**Regex Pattern**: `/^(100\\.55)/`

| Code | Description | Fix |
|------|-------------|-----|
| 100.550.300 | No amount or too low | Provide valid amount |
| 100.550.301 | Amount too large | Reduce amount |
| 100.550.303 | Amount format invalid | Use 2 decimals |
| 100.550.400 | Request contains no currency | Provide currency |
| 100.550.401 | Invalid currency | Use ISO 4217 code |

### Configuration Errors

**Regex Pattern**: `/^(600\\.[23]|500\\.[12]|800\\.121)/`

| Code | Description | Fix |
|------|-------------|-----|
| 500.100.201 | Channel/Merchant is disabled | Contact VrPay |
| 600.200.201 | Not configured for this payment method | Contact VrPay |
| 600.200.202 | Not configured for this payment type | Contact VrPay |
| 600.200.701 | testMode not allowed on production | Remove testMode |
| 600.300.101 | Merchant key not found | Check entityId |

### Reference Errors

**Regex Pattern**: `/^(700\\.[1345][05]0)/`

| Code | Description | Fix |
|------|-------------|-----|
| 700.100.100 | Reference id does not exist | Check ID |
| 700.100.200 | Non matching reference amount | Check amount |
| 700.100.400 | Referenced payment method does not match | Check method |
| 700.300.100 | Referenced tx cannot be refunded/captured | Check status |
| 700.400.100 | Cannot capture (PA value exceeded) | Check amount |

## Communication Errors

**Regex Pattern**: `/^(900\\.[1234]00|000\\.400\\.030)/`

| Code | Description | Action |
|------|-------------|--------|
| 900.100.100 | Unexpected communication error | Retry |
| 900.100.300 | Timeout, uncertain result | Check status before retry |
| 900.100.500 | Timeout (try later) | Retry later |
| 900.100.600 | Connector/acquirer down | Retry later |

## System Errors

**Regex Pattern**: `/^(800\\.[56]|999\\.|600\\.1|800\\.800\\.[84])/`

| Code | Description | Action |
|------|-------------|--------|
| 800.800.800 | Payment system unavailable | Contact support |
| 999.999.999 | Undefined connector/acquirer error | Contact support |

## Risk Management Errors

### External Risk Checks

**Regex Pattern**: `/^(100\\.400\\.[0-3]|100\\.380\\.100|100\\.380\\.11|100\\.380\\.4|100\\.380\\.5)/`

| Code | Description |
|------|-------------|
| 100.400.140 | Transaction declined by GateKeeper |
| 100.400.141 | Challenge by ReD Shield |
| 100.400.142 | Deny by ReD Shield |
| 100.400.319 | Transaction declined by risk system |

### Blacklist

**Regex Pattern**: `/^(800\\.[32])/`

| Code | Description |
|------|-------------|
| 800.300.101 | Account or user is blacklisted |
| 800.300.200 | Email is blacklisted |
| 800.300.301 | IP blacklisted |

## Chargeback Codes

**Regex Pattern**: `/^(000\\.100\\.2)/`

| Code | Description |
|------|-------------|
| 000.100.220 | Fraudulent Transaction |
| 000.100.221 | Merchandise Not Received |
| 000.100.222 | Transaction Not Recognized By Cardholder |
| 000.100.224 | Duplicate Processing |

## .NET Implementation

### Result Code Analyzer

```csharp
public class ResultCodeAnalyzer
{
    public static TransactionStatus AnalyzeResultCode(string code)
    {
        // Success
        if (Regex.IsMatch(code, @"^(000\.000\.|000\.100\.1|000\.[36]|000\.400\.[1][12]0)"))
            return TransactionStatus.Success;
            
        // Manual review required
        if (Regex.IsMatch(code, @"^(000\.400\.0[^3]|000\.400\.100)"))
            return TransactionStatus.SuccessManualReview;
            
        // Pending
        if (Regex.IsMatch(code, @"^(000\.200)"))
            return TransactionStatus.PendingShortTerm;
            
        if (Regex.IsMatch(code, @"^(800\.400\.5|100\.400\.500)"))
            return TransactionStatus.PendingLongTerm;
            
        // Soft decline
        if (Regex.IsMatch(code, @"^(300\.100\.100)"))
            return TransactionStatus.SoftDecline;
            
        // Hard decline
        if (Regex.IsMatch(code, @"^(800\.[17]00|800\.800\.[123])"))
            return TransactionStatus.HardDecline;
            
        // Communication error
        if (Regex.IsMatch(code, @"^(900\.[1234]00|000\.400\.030)"))
            return TransactionStatus.CommunicationError;
            
        // Validation error
        if (Regex.IsMatch(code, @"^(200\.[123]|100\.[53][07]|800\.900|100\.[69]00\.500)"))
            return TransactionStatus.ValidationError;
            
        return TransactionStatus.Unknown;
    }
}

public enum TransactionStatus
{
    Success,
    SuccessManualReview,
    PendingShortTerm,
    PendingLongTerm,
    SoftDecline,
    HardDecline,
    CommunicationError,
    ValidationError,
    Unknown
}
```

## Next Steps

Continue to:
- [Error Handling](06-error-handling.md) - Comprehensive error handling strategies
- [Implementation Guide](07-implementation-guide.md) - Step-by-step implementation
