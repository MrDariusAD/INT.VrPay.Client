# Domain-Driven Design Architecture

## Overview

The INT.VrPay.Client library follows Domain-Driven Design (DDD) principles to create a well-structured, maintainable payment integration solution.

## Architecture Layers

### 1. Domain Layer (`Domain/`)

The Domain layer contains the core business logic and domain concepts. It has no dependencies on other layers.

#### **Domain/Entities** 
Core domain entities representing payment transactions:
- `PaymentRequest.cs` - Represents a payment transaction request (Aggregate Root)
- `PaymentResponse.cs` - Represents a payment transaction response

#### **Domain/ValueObjects**
Immutable value objects that describe domain concepts:
- `CardData.cs` - Credit/debit card information
- `CustomerData.cs` - Customer personal information
- `AddressData.cs` - Billing/shipping address information

#### **Domain/Enums**
Domain-specific enumerations:
- `PaymentType.cs` - Transaction types (PreAuthorization, Debit, Capture, Refund, Reversal)
- `PaymentBrand.cs` - Card brands (Visa, Mastercard, Amex, etc.)
- `Currency.cs` - ISO 4217 currency codes
- `TestMode.cs` - VrPay test mode values
- `TransactionStatus.cs` - Transaction status codes

#### **Domain/Services**
Domain services containing business logic:
- `ResultCodeAnalyzer.cs` - Analyzes VrPay result codes and determines transaction status

### 2. Application Layer (`Application/`)

The Application layer orchestrates the flow of data and coordinates domain objects.

#### **Application/Interfaces**
Application service contracts:
- `IVrPayClient.cs` - Main client interface for payment operations

#### **Application/Services**
Application services would go here (currently the VrPayClient implementation is in Infrastructure as it's HTTP-focused).

### 3. Infrastructure Layer (`Infrastructure/`)

The Infrastructure layer contains implementations that deal with external concerns.

#### **Infrastructure/Http**
HTTP communication implementations:
- `VrPayClient.cs` - HttpClient-based implementation of IVrPayClient

#### **Infrastructure/Serialization**
Serialization concerns:
- `EnumMemberConverter.cs` - JSON converters for enum serialization

### 4. Cross-Cutting Concerns

#### **Configuration**
Configuration models:
- `VrPayConfiguration.cs` - Client configuration settings

#### **Exceptions**
Exception hierarchy:
- `VrPayException.cs` - Base exception
- `VrPayConfigurationException.cs` - Configuration errors
- `VrPayPaymentDeclinedException.cs` - Payment declined
- `VrPayValidationException.cs` - Validation errors  
- `VrPayCommunicationException.cs` - Communication failures

#### **Extensions**
Extension methods:
- `ServiceCollectionExtensions.cs` - Dependency injection configuration
- `PaymentResponseExtensions.cs` - Response helper methods

#### **Testing**
Test helpers and utilities:
- `TestCards.cs` - Predefined test card data
- `TestData.cs` - Test data factory methods

## Dependency Flow

```
┌─────────────────────────────────────┐
│         Domain Layer                │
│  (Entities, Value Objects, Enums,   │
│   Domain Services)                  │
│  NO DEPENDENCIES                    │
└─────────────────────────────────────┘
                  ▲
                  │
                  │
┌─────────────────────────────────────┐
│      Application Layer              │
│  (Interfaces, Application Services) │
│  Depends on: Domain                 │
└─────────────────────────────────────┘
                  ▲
                  │
                  │
┌─────────────────────────────────────┐
│     Infrastructure Layer            │
│  (HTTP, Serialization, External)    │
│  Depends on: Domain, Application    │
└─────────────────────────────────────┘
```

## DDD Patterns Used

### Aggregate Root
- `PaymentRequest` acts as an aggregate root with factory methods for creating valid payment requests
- Ensures business rules are enforced at creation time

### Value Objects
- `CardData`, `CustomerData`, `AddressData` are immutable value objects
- Define equality based on values, not identity
- Encapsulate validation rules

### Domain Services
- `ResultCodeAnalyzer` provides domain logic that doesn't naturally fit in entities or value objects
- Stateless service for analyzing result codes

### Enums as Domain Concepts
- Type-safe enumerations for payment types, brands, currencies
- Use `EnumMember` attributes for API contract mapping

### Factory Methods
- `PaymentRequest.Create()` factory method ensures valid payment requests
- Encapsulates complex creation logic

### Specification Pattern
- `ResultCodeAnalyzer` uses regex patterns as specifications for result code analysis

## Naming Conventions

### Namespaces
- `VrPay.Client.Domain.Entities`
- `VrPay.Client.Domain.ValueObjects`
- `VrPay.Client.Domain.Enums`
- `VrPay.Client.Domain.Services`
- `VrPay.Client.Application.Interfaces`
- `VrPay.Client.Infrastructure.Http`
- `VrPay.Client.Infrastructure.Serialization`

### File Organization
- Files are organized by layer and then by concept
- Each folder represents a specific architectural layer or concern

## Benefits of This Architecture

1. **Separation of Concerns**: Each layer has a clear responsibility
2. **Testability**: Domain logic can be tested independently of infrastructure
3. **Maintainability**: Changes to infrastructure don't affect domain logic
4. **Flexibility**: Easy to swap implementations (e.g., different HTTP clients)
5. **Domain Focus**: Business logic is explicit and centralized in the Domain layer

## Future Enhancements

Potential DDD patterns to consider:

1. **Repository Pattern**: If we need to add persistence for payment history
2. **Unit of Work**: For managing transactions across multiple operations
3. **Domain Events**: For publishing payment events (success, failure, etc.)
4. **CQRS**: Separate read/write models if complexity increases

## Testing Strategy

### Unit Tests
- Domain entities and value objects: Test business rules and validation
- Domain services: Test business logic independently
- Application services: Test orchestration logic with mocked dependencies

### Integration Tests
- Infrastructure implementations: Test actual HTTP communication with VrPay API
- End-to-end scenarios: Test complete payment flows

## References

- Eric Evans - "Domain-Driven Design: Tackling Complexity in the Heart of Software"
- Vaughn Vernon - "Implementing Domain-Driven Design"
- Martin Fowler - "Patterns of Enterprise Application Architecture"
