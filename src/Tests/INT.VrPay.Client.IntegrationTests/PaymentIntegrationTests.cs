using FluentAssertions;
using INT.VrPay.Client.Exceptions;
using INT.VrPay.Client.Extensions;
using INT.VrPay.Client.Models;
using INT.VrPay.Client.Testing;

namespace INT.VrPay.Client.IntegrationTests;

/// <summary>
/// Integration tests for VrPay payment operations.
/// These tests require valid VrPay test credentials in appsettings.Development.json
/// </summary>
public class PaymentIntegrationTests : IClassFixture<IntegrationTestFixture>
{
    private readonly IntegrationTestFixture _fixture;

    public PaymentIntegrationTests(IntegrationTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task PreAuthorize_WithValidCard_ShouldSucceed()
    {
        // Arrange
        if (!_fixture.IsConfigured)
        {
            Assert.Fail("VrPay is not configured. Please set EntityId and AccessToken in appsettings.Development.json");
        }

        var request = TestData.CreateSuccessfulPaymentRequest(
            amount: 92.00m,
            currency: Currency.EUR,
            paymentBrand: PaymentBrand.Visa);
        request.MerchantTransactionId = $"TEST-PA-{Guid.NewGuid()}";

        // Act
        var response = await _fixture.VrPayClient.PreAuthorizeAsync(request);

        // Assert
        response.Should().NotBeNull();
        response.Id.Should().NotBeNullOrEmpty();
        response.PaymentType.Should().Be(PaymentType.PreAuthorization);
        response.Amount.Should().Be("92.00");
        response.Currency.Should().Be(Currency.EUR);
        response.IsSuccess().Should().BeTrue();
        response.Result.Code.Should().MatchRegex(@"^000\.");
    }

    [Fact]
    public async Task PreAuthorizeAndCapture_WithValidCard_ShouldSucceed()
    {
        // Arrange
        if (!_fixture.IsConfigured)
        {
            Assert.Fail("VrPay is not configured. Please set EntityId and AccessToken in appsettings.Development.json");
        }

        // Step 1: Pre-authorize
        var preAuthRequest = TestData.CreateSuccessfulPaymentRequest(
            amount: 50.00m,
            currency: Currency.EUR,
            paymentBrand: PaymentBrand.Visa);
        preAuthRequest.MerchantTransactionId = $"TEST-PA-CP-{Guid.NewGuid()}";

        var preAuthResponse = await _fixture.VrPayClient.PreAuthorizeAsync(preAuthRequest);

        // Wait a bit before capture (recommended in documentation)
        await Task.Delay(2000);

        // Step 2: Capture
        var captureResponse = await _fixture.VrPayClient.CaptureAsync(
            preAuthResponse.Id,
            50.00m,
            Currency.EUR);

        // Assert
        captureResponse.Should().NotBeNull();
        captureResponse.PaymentType.Should().Be(PaymentType.Capture);
        captureResponse.IsSuccess().Should().BeTrue();
    }

    [Fact]
    public async Task Debit_WithValidCard_ShouldSucceed()
    {
        // Arrange
        if (!_fixture.IsConfigured)
        {
            Assert.Fail("VrPay is not configured. Please set EntityId and AccessToken in appsettings.Development.json");
        }

        var request = TestData.CreateSuccessfulPaymentRequest(
            amount: 25.50m,
            currency: Currency.EUR,
            paymentBrand: PaymentBrand.Visa);
        request.MerchantTransactionId = $"TEST-DB-{Guid.NewGuid()}";

        // Act
        var response = await _fixture.VrPayClient.DebitAsync(request);

        // Assert
        response.Should().NotBeNull();
        response.PaymentType.Should().Be(PaymentType.Debit);
        response.IsSuccess().Should().BeTrue();
        response.Amount.Should().Be("25.50");
    }

    [Fact]
    public async Task PreAuthorize_WithDeclinedCard_ShouldThrowException()
    {
        // Arrange
        if (!_fixture.IsConfigured)
        {
            Assert.Fail("VrPay is not configured. Please set EntityId and AccessToken in appsettings.Development.json");
        }

        var request = TestData.CreateDeclinedPaymentRequest(
            amount: 100.00m,
            currency: Currency.EUR,
            paymentBrand: PaymentBrand.Visa);
        request.MerchantTransactionId = $"TEST-DECLINE-{Guid.NewGuid()}";

        // Act
        var act = async () => await _fixture.VrPayClient.PreAuthorizeAsync(request);

        // Assert
        await act.Should().ThrowAsync<VrPayPaymentDeclinedException>()
            .WithMessage("*declined*");
    }

    [Fact]
    public async Task Refund_AfterDebit_ShouldSucceed()
    {
        // Arrange
        if (!_fixture.IsConfigured)
        {
            Assert.Fail("VrPay is not configured. Please set EntityId and AccessToken in appsettings.Development.json");
        }

        // Step 1: Perform a debit
        var debitRequest = TestData.CreateSuccessfulPaymentRequest(
            amount: 15.00m,
            currency: Currency.EUR,
            paymentBrand: PaymentBrand.Visa);
        debitRequest.MerchantTransactionId = $"TEST-RF-{Guid.NewGuid()}";

        var debitResponse = await _fixture.VrPayClient.DebitAsync(debitRequest);

        // Wait a bit before refund
        await Task.Delay(2000);

        // Step 2: Refund
        var refundResponse = await _fixture.VrPayClient.RefundAsync(
            debitResponse.Id,
            15.00m,
            Currency.EUR);

        // Assert
        refundResponse.Should().NotBeNull();
        refundResponse.PaymentType.Should().Be(PaymentType.Refund);
        refundResponse.IsSuccess().Should().BeTrue();
    }

    [Fact]
    public async Task Reverse_AfterPreAuthorize_ShouldSucceed()
    {
        // Arrange
        if (!_fixture.IsConfigured)
        {
            Assert.Fail("VrPay is not configured. Please set EntityId and AccessToken in appsettings.Development.json");
        }

        // Step 1: Pre-authorize
        var preAuthRequest = TestData.CreateSuccessfulPaymentRequest(
            amount: 30.00m,
            currency: Currency.EUR,
            paymentBrand: PaymentBrand.Visa);
        preAuthRequest.MerchantTransactionId = $"TEST-RV-{Guid.NewGuid()}";

        var preAuthResponse = await _fixture.VrPayClient.PreAuthorizeAsync(preAuthRequest);

        // Wait a bit before reversal
        await Task.Delay(2000);

        // Step 2: Reverse
        var reversalResponse = await _fixture.VrPayClient.ReverseAsync(preAuthResponse.Id, 30.00m, Currency.EUR);

        // Assert
        reversalResponse.Should().NotBeNull();
        reversalResponse.PaymentType.Should().Be(PaymentType.Reversal);
        reversalResponse.IsSuccess().Should().BeTrue();
    }

    [Fact]
    public async Task PreAuthorize_WithInvalidData_ShouldThrowValidationException()
    {
        // Arrange
        var request = new PaymentRequest
        {
            // Missing required fields
            Amount = "50.00",
            Currency = Currency.EUR
            // Missing PaymentBrand and Card data
        };

        // Act
        var act = async () => await _fixture.VrPayClient.PreAuthorizeAsync(request);

        // Assert
        await act.Should().ThrowAsync<VrPayValidationException>();
    }

    [Fact]
    public async Task PreAuthorize_WithInvalidAmount_ShouldThrowValidationException()
    {
        // Arrange
        var request = new PaymentRequest
        {
            Amount = "invalid",
            Currency = Currency.EUR,
            PaymentBrand = PaymentBrand.Visa,
            Card = TestCards.VisaSuccess
        };

        // Act
        var act = async () => await _fixture.VrPayClient.PreAuthorizeAsync(request);

        // Assert
        await act.Should().ThrowAsync<VrPayValidationException>();
    }
}
