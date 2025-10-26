using System.Globalization;
using System.Text.Json.Serialization;
using INT.VrPay.Client.Converters;

namespace INT.VrPay.Client.Models;

/// <summary>
/// Represents a payment request to the VrPay API.
/// </summary>
public class PaymentRequest
{
    /// <summary>
    /// The entity ID for the payment channel (will be set from configuration).
    /// </summary>
    [JsonPropertyName("entityId")]
    public string EntityId { get; set; } = string.Empty;

    /// <summary>
    /// The payment amount with 2 decimal places (e.g., 92.00).
    /// </summary>
    [JsonPropertyName("amount")]
    public string Amount { get; set; } = string.Empty;

    /// <summary>
    /// The currency code (ISO 4217).
    /// </summary>
    [JsonPropertyName("currency")]
    [JsonConverter(typeof(EnumMemberConverter<Currency>))]
    public Currency Currency { get; set; }

    /// <summary>
    /// The type of payment transaction.
    /// </summary>
    [JsonPropertyName("paymentType")]
    [JsonConverter(typeof(EnumMemberConverter<PaymentType>))]
    public PaymentType PaymentType { get; set; }

    /// <summary>
    /// The payment brand (e.g., VISA, MASTER, AMEX).
    /// </summary>
    [JsonPropertyName("paymentBrand")]
    [JsonConverter(typeof(NullableEnumMemberConverter<PaymentBrand>))]
    public PaymentBrand? PaymentBrand { get; set; }

    /// <summary>
    /// Merchant transaction ID for tracking (optional but recommended).
    /// </summary>
    [JsonPropertyName("merchantTransactionId")]
    public string? MerchantTransactionId { get; set; }

    /// <summary>
    /// Card data for card payments.
    /// </summary>
    [JsonPropertyName("card")]
    public CardData? Card { get; set; }

    /// <summary>
    /// Customer data (optional but recommended).
    /// </summary>
    [JsonPropertyName("customer")]
    public CustomerData? Customer { get; set; }

    /// <summary>
    /// Billing address data (optional).
    /// </summary>
    [JsonPropertyName("billing")]
    public AddressData? Billing { get; set; }

    /// <summary>
    /// URL to redirect the customer after completing 3D Secure authentication.
    /// Required for 3D Secure payments.
    /// </summary>
    [JsonPropertyName("shopperResultUrl")]
    public string? ShopperResultUrl { get; set; }

    /// <summary>
    /// Test mode parameter (EXTERNAL or INTERNAL).
    /// </summary>
    [JsonPropertyName("testMode")]
    [JsonConverter(typeof(NullableEnumMemberConverter<TestMode>))]
    public TestMode? TestMode { get; set; }

    /// <summary>
    /// Creates a payment request with the specified amount and currency.
    /// </summary>
    public static PaymentRequest Create(decimal amount, Currency currency, PaymentType paymentType)
    {
        return new PaymentRequest
        {
            Amount = amount.ToString("F2", CultureInfo.InvariantCulture),
            Currency = currency,
            PaymentType = paymentType
        };
    }
}
