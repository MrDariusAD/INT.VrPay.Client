namespace INT.VrPay.Client.Models;

/// <summary>
/// Transaction status based on result code analysis.
/// </summary>
public enum TransactionStatus
{
    /// <summary>
    /// Transaction was successful.
    /// </summary>
    Success,

    /// <summary>
    /// Transaction was successful but requires manual review.
    /// </summary>
    SuccessManualReview,

    /// <summary>
    /// Transaction is pending (asynchronous payment).
    /// </summary>
    Pending,

    /// <summary>
    /// Transaction was declined but can be retried (soft decline).
    /// </summary>
    SoftDecline,

    /// <summary>
    /// Transaction was declined and should not be retried (hard decline).
    /// </summary>
    HardDecline,

    /// <summary>
    /// Validation error in the request data.
    /// </summary>
    ValidationError,

    /// <summary>
    /// Communication error with the payment provider.
    /// </summary>
    CommunicationError,

    /// <summary>
    /// System error occurred.
    /// </summary>
    SystemError,

    /// <summary>
    /// Unknown status.
    /// </summary>
    Unknown
}
