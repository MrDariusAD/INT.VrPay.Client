using System.Text.Json.Serialization;

namespace INT.VrPay.Client.Models;

/// <summary>
/// Customer data for payment requests.
/// </summary>
public class CustomerData
{
    /// <summary>
    /// Customer's given name (first name).
    /// </summary>
    [JsonPropertyName("givenName")]
    public string? GivenName { get; set; }

    /// <summary>
    /// Customer's surname (last name).
    /// </summary>
    [JsonPropertyName("surname")]
    public string? Surname { get; set; }

    /// <summary>
    /// Customer's email address.
    /// </summary>
    [JsonPropertyName("email")]
    public string? Email { get; set; }

    /// <summary>
    /// Customer's IP address.
    /// </summary>
    [JsonPropertyName("ip")]
    public string? Ip { get; set; }

    /// <summary>
    /// Merchant's customer identifier.
    /// </summary>
    [JsonPropertyName("merchantCustomerId")]
    public string? MerchantCustomerId { get; set; }
}
