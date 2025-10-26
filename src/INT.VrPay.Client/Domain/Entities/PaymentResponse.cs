using System.Text.Json.Serialization;
using INT.VrPay.Client.Converters;

namespace INT.VrPay.Client.Models;

/// <summary>
/// Response from a payment operation.
/// </summary>
public class PaymentResponse
{
    /// <summary>
    /// Unique identifier for this payment transaction.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// The payment type that was performed.
    /// </summary>
    [JsonPropertyName("paymentType")]
    [JsonConverter(typeof(EnumMemberConverter<PaymentType>))]
    public PaymentType PaymentType { get; set; }

    /// <summary>
    /// The payment amount.
    /// </summary>
    [JsonPropertyName("amount")]
    public string Amount { get; set; } = string.Empty;

    /// <summary>
    /// The currency code.
    /// </summary>
    [JsonPropertyName("currency")]
    [JsonConverter(typeof(EnumMemberConverter<Currency>))]
    public Currency Currency { get; set; }

    /// <summary>
    /// The payment brand used.
    /// </summary>
    [JsonPropertyName("paymentBrand")]
    [JsonConverter(typeof(NullableEnumMemberConverter<PaymentBrand>))]
    public PaymentBrand? PaymentBrand { get; set; }

    /// <summary>
    /// Result data containing the result code and description.
    /// </summary>
    [JsonPropertyName("result")]
    public ResultData Result { get; set; } = new();

    /// <summary>
    /// Merchant transaction ID if provided in the request.
    /// </summary>
    [JsonPropertyName("merchantTransactionId")]
    public string? MerchantTransactionId { get; set; }

    /// <summary>
    /// Card information (masked).
    /// </summary>
    [JsonPropertyName("card")]
    public CardInfo? Card { get; set; }

    /// <summary>
    /// Timestamp of the transaction.
    /// </summary>
    [JsonPropertyName("timestamp")]
    public string? Timestamp { get; set; }

    /// <summary>
    /// Redirect URL if 3D Secure or other redirect is required.
    /// </summary>
    [JsonPropertyName("redirectUrl")]
    public string? RedirectUrl { get; set; }
}

/// <summary>
/// Result data from the payment operation.
/// </summary>
public class ResultData
{
    /// <summary>
    /// Result code indicating success or failure.
    /// Pattern: 000.000.000
    /// </summary>
    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable description of the result.
    /// </summary>
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// Card information in the response (masked for security).
/// </summary>
public class CardInfo
{
    /// <summary>
    /// Masked card number (e.g., 420000******0000).
    /// </summary>
    [JsonPropertyName("bin")]
    public string? Bin { get; set; }

    /// <summary>
    /// Last 4 digits of the card.
    /// </summary>
    [JsonPropertyName("last4Digits")]
    public string? Last4Digits { get; set; }

    /// <summary>
    /// Card holder name.
    /// </summary>
    [JsonPropertyName("holder")]
    public string? Holder { get; set; }

    /// <summary>
    /// Card expiry month.
    /// </summary>
    [JsonPropertyName("expiryMonth")]
    public string? ExpiryMonth { get; set; }

    /// <summary>
    /// Card expiry year.
    /// </summary>
    [JsonPropertyName("expiryYear")]
    public string? ExpiryYear { get; set; }
}
