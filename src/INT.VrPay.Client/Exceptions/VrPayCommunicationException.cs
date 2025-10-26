namespace INT.VrPay.Client.Exceptions;

/// <summary>
/// Exception thrown when communication with the VrPay API fails.
/// </summary>
public class VrPayCommunicationException : VrPayException
{
    /// <summary>
    /// The HTTP status code if available.
    /// </summary>
    public int? StatusCode { get; }

    /// <summary>
    /// Creates a new VrPayCommunicationException.
    /// </summary>
    public VrPayCommunicationException(string message) : base(message)
    {
    }

    /// <summary>
    /// Creates a new VrPayCommunicationException with an inner exception.
    /// </summary>
    public VrPayCommunicationException(string message, Exception innerException) : base(message, innerException)
    {
    }

    /// <summary>
    /// Creates a new VrPayCommunicationException with a status code.
    /// </summary>
    public VrPayCommunicationException(string message, int statusCode) : base(message)
    {
        StatusCode = statusCode;
    }

    /// <summary>
    /// Creates a new VrPayCommunicationException with a status code and inner exception.
    /// </summary>
    public VrPayCommunicationException(string message, int statusCode, Exception innerException) 
        : base(message, innerException)
    {
        StatusCode = statusCode;
    }
}
