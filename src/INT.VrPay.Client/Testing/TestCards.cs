using INT.VrPay.Client.Models;

namespace INT.VrPay.Client.Testing;

/// <summary>
/// Predefined test card data for VrPay testing environment.
/// </summary>
public static class TestCards
{
    /// <summary>
    /// Visa test card that will result in a successful transaction.
    /// </summary>
    public static readonly CardData VisaSuccess = new()
    {
        Number = "4200000000000000",
        Holder = "John Doe",
        ExpiryMonth = "12",
        ExpiryYear = "2034",
        Cvv = "123"
    };

    /// <summary>
    /// Visa test card that will be declined.
    /// </summary>
    public static readonly CardData VisaDeclined = new()
    {
        Number = "4000000000000002",
        Holder = "Jane Smith",
        ExpiryMonth = "12",
        ExpiryYear = "2034",
        Cvv = "123"
    };

    /// <summary>
    /// Mastercard test card that will result in a successful transaction.
    /// </summary>
    public static readonly CardData MastercardSuccess = new()
    {
        Number = "5200000000000007",
        Holder = "John Doe",
        ExpiryMonth = "12",
        ExpiryYear = "2034",
        Cvv = "123"
    };

    /// <summary>
    /// Mastercard test card that will be declined.
    /// </summary>
    public static readonly CardData MastercardDeclined = new()
    {
        Number = "5100000000000016",
        Holder = "Jane Smith",
        ExpiryMonth = "12",
        ExpiryYear = "2034",
        Cvv = "123"
    };

    /// <summary>
    /// American Express test card that will result in a successful transaction.
    /// </summary>
    public static readonly CardData AmexSuccess = new()
    {
        Number = "340000000000009",
        Holder = "John Doe",
        ExpiryMonth = "12",
        ExpiryYear = "2034",
        Cvv = "1234"
    };

    /// <summary>
    /// Card data with invalid CVV (will trigger validation error).
    /// </summary>
    public static readonly CardData InvalidCvv = new()
    {
        Number = "4200000000000000",
        Holder = "Test User",
        ExpiryMonth = "12",
        ExpiryYear = "2034",
        Cvv = "12" // Invalid - too short
    };

    /// <summary>
    /// Card data with expired card (will trigger validation error).
    /// </summary>
    public static readonly CardData ExpiredCard = new()
    {
        Number = "4200000000000000",
        Holder = "Test User",
        ExpiryMonth = "01",
        ExpiryYear = "2020",
        Cvv = "123"
    };
}
