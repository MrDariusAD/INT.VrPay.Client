using System.Text.Json.Serialization;

namespace INT.VrPay.Client.Models;

/// <summary>
/// Card data for card payments.
/// </summary>
public class CardData
{
    /// <summary>
    /// Card number (PAN).
    /// </summary>
    [JsonPropertyName("number")]
    public string Number { get; set; } = string.Empty;

    /// <summary>
    /// Cardholder name.
    /// </summary>
    [JsonPropertyName("holder")]
    public string? Holder { get; set; }

    /// <summary>
    /// Card expiry month (01-12).
    /// </summary>
    [JsonPropertyName("expiryMonth")]
    public string ExpiryMonth { get; set; } = string.Empty;

    /// <summary>
    /// Card expiry year (4 digits, e.g., 2034).
    /// </summary>
    [JsonPropertyName("expiryYear")]
    public string ExpiryYear { get; set; } = string.Empty;

    /// <summary>
    /// Card CVV/CVC code.
    /// </summary>
    [JsonPropertyName("cvv")]
    public string? Cvv { get; set; }
}
