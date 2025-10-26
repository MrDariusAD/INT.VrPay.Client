using INT.VrPay.Client.Models;

namespace INT.VrPay.Client.Configuration;

/// <summary>
/// Configuration for the VrPay client.
/// </summary>
public class VrPayConfiguration
{
    /// <summary>
    /// The base URL for the VrPay API.
    /// Test: https://test.vr-pay-ecommerce.de/
    /// Production: https://vr-pay-ecommerce.de/
    /// </summary>
    public string BaseUrl { get; set; } = "https://test.vr-pay-ecommerce.de/";

    /// <summary>
    /// The entity ID for the payment channel.
    /// </summary>
    public string EntityId { get; set; } = string.Empty;

    /// <summary>
    /// The access token for authentication (Bearer token).
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// The timeout in seconds for HTTP requests.
    /// Default is 30 seconds.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Whether to use test mode.
    /// When true, sets testMode in requests.
    /// </summary>
    public bool UseTestMode { get; set; } = true;

    /// <summary>
    /// The test mode value to send in requests.
    /// </summary>
    public TestMode TestModeValue { get; set; } = TestMode.External;

    /// <summary>
    /// Validates the configuration.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when configuration is invalid.</exception>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(BaseUrl))
        {
            throw new InvalidOperationException("BaseUrl is required.");
        }

        if (string.IsNullOrWhiteSpace(EntityId))
        {
            throw new InvalidOperationException("EntityId is required.");
        }

        if (string.IsNullOrWhiteSpace(AccessToken))
        {
            throw new InvalidOperationException("AccessToken is required.");
        }

        if (!Uri.TryCreate(BaseUrl, UriKind.Absolute, out _))
        {
            throw new InvalidOperationException("BaseUrl must be a valid absolute URL.");
        }

        if (TimeoutSeconds <= 0)
        {
            throw new InvalidOperationException("TimeoutSeconds must be greater than 0.");
        }
    }
}
