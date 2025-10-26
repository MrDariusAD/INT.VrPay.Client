# VrPay eCommerce .NET Client Documentation

This documentation provides comprehensive information for implementing a .NET client for VrPay eCommerce Server to Server Payment Integration with support for synchronous payments.

## Documentation Structure

1. **[API Overview](01-api-overview.md)** - Introduction to VrPay eCommerce API
2. **[Authentication & Configuration](02-authentication-configuration.md)** - API authentication and setup
3. **[Synchronous Payment Flow](03-synchronous-payment-flow.md)** - Complete workflow for synchronous payments
4. **[Payment Models](04-payment-models.md)** - All data structures and models
5. **[Result Codes](05-result-codes.md)** - Comprehensive result code reference
6. **[Error Handling](06-error-handling.md)** - Error handling strategies
7. **[Implementation Guide](07-implementation-guide.md)** - Step-by-step .NET implementation
8. **[.NET Client Architecture](08-dotnet-client-architecture.md)** - Client design and structure
9. **[Testing Guide](09-testing-guide.md)** - Testing strategies and test data

## Quick Start

For a quick start, follow this reading order:
1. Read [API Overview](01-api-overview.md)
2. Understand [Authentication & Configuration](02-authentication-configuration.md)
3. Study [Synchronous Payment Flow](03-synchronous-payment-flow.md)
4. Review [Payment Models](04-payment-models.md)
5. Follow [Implementation Guide](07-implementation-guide.md)

## Feature Support

This documentation currently covers:
- ✅ Synchronous Payment (Pre-authorization + Capture)
- ✅ Direct Debit (DB) transactions
- ❌ Asynchronous Payments (Future)
- ❌ 3D Secure (Future)
- ❌ Tokenization/Registration (Future)

## Important Notes

⚠️ **PCI-DSS Compliance**: Collecting card data requires PCI-DSS compliance. Consider using COPY+PAY widget to minimize compliance requirements.

⚠️ **SSL Required**: All API requests must be sent over SSL.

⚠️ **UTF-8 Encoding**: All data must be sent encoded in UTF-8.

## References

- Last Updated: October 26, 2025
- API Version: v1
- Based on VrPay eCommerce Documentation (Last updated: December 27, 2024)
