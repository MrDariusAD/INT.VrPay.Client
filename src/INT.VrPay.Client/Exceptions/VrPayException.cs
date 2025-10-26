namespace INT.VrPay.Client.Exceptions;

/// <summary>
/// Base exception for all VrPay client errors.
/// </summary>
public class VrPayException : Exception
{
    /// <summary>
    /// Creates a new VrPayException.
    /// </summary>
    public VrPayException(string message) : base(message)
    {
    }

    /// <summary>
    /// Creates a new VrPayException with an inner exception.
    /// </summary>
    public VrPayException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
