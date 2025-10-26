using System.Text.Json.Serialization;

namespace INT.VrPay.Client.Models;

/// <summary>
/// Address data for billing or shipping.
/// </summary>
public class AddressData
{
    /// <summary>
    /// Street address line 1.
    /// </summary>
    [JsonPropertyName("street1")]
    public string? Street1 { get; set; }

    /// <summary>
    /// Street address line 2 (optional).
    /// </summary>
    [JsonPropertyName("street2")]
    public string? Street2 { get; set; }

    /// <summary>
    /// City name.
    /// </summary>
    [JsonPropertyName("city")]
    public string? City { get; set; }

    /// <summary>
    /// State or province code.
    /// </summary>
    [JsonPropertyName("state")]
    public string? State { get; set; }

    /// <summary>
    /// Postal or ZIP code.
    /// </summary>
    [JsonPropertyName("postcode")]
    public string? Postcode { get; set; }

    /// <summary>
    /// Country code (ISO 3166-1 alpha-2, e.g., DE, US).
    /// </summary>
    [JsonPropertyName("country")]
    public string? Country { get; set; }
}
