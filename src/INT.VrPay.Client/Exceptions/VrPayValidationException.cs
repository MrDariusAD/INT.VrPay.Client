namespace INT.VrPay.Client.Exceptions;

/// <summary>
/// Exception thrown when request validation fails.
/// </summary>
public class VrPayValidationException : VrPayException
{
    /// <summary>
    /// List of validation errors.
    /// </summary>
    public List<string> ValidationErrors { get; }

    /// <summary>
    /// Creates a new VrPayValidationException.
    /// </summary>
    public VrPayValidationException(string message, List<string> validationErrors) : base(message)
    {
        ValidationErrors = validationErrors;
    }

    /// <summary>
    /// Creates a new VrPayValidationException with a single error.
    /// </summary>
    public VrPayValidationException(string message) : base(message)
    {
        ValidationErrors = new List<string> { message };
    }
}
