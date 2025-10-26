using System.Globalization;
using INT.VrPay.Client.Models;

namespace INT.VrPay.Client.Testing;

/// <summary>
/// Predefined test data for VrPay testing environment.
/// </summary>
public static class TestData
{
    /// <summary>
    /// Default test customer data.
    /// </summary>
    public static readonly CustomerData DefaultCustomer = new()
    {
        GivenName = "John",
        Surname = "Doe",
        Email = "john.doe@example.com",
        Ip = "127.0.0.1",
        MerchantCustomerId = "CUST-12345"
    };

    /// <summary>
    /// Default test billing address.
    /// </summary>
    public static readonly AddressData DefaultBillingAddress = new()
    {
        Street1 = "123 Test Street",
        City = "Test City",
        State = "TC",
        Postcode = "12345",
        Country = "DE"
    };

    /// <summary>
    /// Creates a complete test payment request with successful card.
    /// </summary>
    /// <param name="amount">Payment amount</param>
    /// <param name="currency">Payment currency</param>
    /// <param name="paymentBrand">Payment brand</param>
    /// <returns>A configured payment request</returns>
    public static PaymentRequest CreateSuccessfulPaymentRequest(
        decimal amount = 92.00m,
        Currency currency = Currency.EUR,
        PaymentBrand paymentBrand = PaymentBrand.Visa)
    {
        return new PaymentRequest
        {
            Amount = amount.ToString("F2", CultureInfo.InvariantCulture),
            Currency = currency,
            PaymentBrand = paymentBrand,
            MerchantTransactionId = $"TEST-{Guid.NewGuid():N}",
            Card = paymentBrand switch
            {
                PaymentBrand.Visa => TestCards.VisaSuccess,
                PaymentBrand.Master => TestCards.MastercardSuccess,
                PaymentBrand.Amex => TestCards.AmexSuccess,
                _ => TestCards.VisaSuccess
            },
            Customer = DefaultCustomer,
            Billing = DefaultBillingAddress
        };
    }

    /// <summary>
    /// Creates a test payment request that will be declined.
    /// </summary>
    /// <param name="amount">Payment amount</param>
    /// <param name="currency">Payment currency</param>
    /// <param name="paymentBrand">Payment brand</param>
    /// <returns>A configured payment request that will be declined</returns>
    public static PaymentRequest CreateDeclinedPaymentRequest(
        decimal amount = 92.00m,
        Currency currency = Currency.EUR,
        PaymentBrand paymentBrand = PaymentBrand.Visa)
    {
        return new PaymentRequest
        {
            Amount = amount.ToString("F2", CultureInfo.InvariantCulture),
            Currency = currency,
            PaymentBrand = paymentBrand,
            MerchantTransactionId = $"TEST-{Guid.NewGuid():N}",
            Card = paymentBrand == PaymentBrand.Master 
                ? TestCards.MastercardDeclined 
                : TestCards.VisaDeclined,
            Customer = DefaultCustomer,
            Billing = DefaultBillingAddress,
            ShopperResultUrl = "https://example.com/payment/result" // Required for 3D Secure
        };
    }
}
