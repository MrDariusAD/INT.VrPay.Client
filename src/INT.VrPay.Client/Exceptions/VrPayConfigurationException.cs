namespace INT.VrPay.Client.Exceptions;

/// <summary>
/// Exception thrown when the VrPay configuration is invalid.
/// </summary>
public class VrPayConfigurationException : VrPayException
{
    /// <summary>
    /// Creates a new VrPayConfigurationException.
    /// </summary>
    public VrPayConfigurationException(string message) : base(message)
    {
    }

    /// <summary>
    /// Creates a new VrPayConfigurationException with an inner exception.
    /// </summary>
    public VrPayConfigurationException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
