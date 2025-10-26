using System.Text.RegularExpressions;

namespace INT.VrPay.Client.Services;

/// <summary>
/// Analyzes VrPay result codes to determine transaction status.
/// </summary>
public static partial class ResultCodeAnalyzer
{
    // Success patterns
    [GeneratedRegex(@"^(000\.000\.|000\.100\.1|000\.[36]|000\.400\.[1][12]0)")]
    private static partial Regex SuccessPattern();

    // Manual review patterns
    [GeneratedRegex(@"^(000\.400\.0[^3]|000\.400\.100)")]
    private static partial Regex ManualReviewPattern();

    // Pending patterns
    [GeneratedRegex(@"^(000\.200)")]
    private static partial Regex PendingPattern();

    // Soft decline patterns (can retry with 3DS or corrections)
    [GeneratedRegex(@"^(300\.100\.100)")]
    private static partial Regex SoftDeclinePattern();

    // Hard decline patterns (should not retry)
    [GeneratedRegex(@"^(800\.[17]00|800\.800\.[123])")]
    private static partial Regex HardDeclinePattern();

    // Validation error patterns
    [GeneratedRegex(@"^(100\.|200\.[123])")]
    private static partial Regex ValidationErrorPattern();

    // Communication error patterns
    [GeneratedRegex(@"^(000\.400\.[3]|900\.[1234]00|800\.900|100\.39[56])")]
    private static partial Regex CommunicationErrorPattern();

    // System error patterns
    [GeneratedRegex(@"^(800\.800\.[8]00|999\.)")]
    private static partial Regex SystemErrorPattern();

    /// <summary>
    /// Determines if the result code indicates a successful transaction.
    /// </summary>
    public static bool IsSuccess(string resultCode)
    {
        return SuccessPattern().IsMatch(resultCode) || ManualReviewPattern().IsMatch(resultCode);
    }

    /// <summary>
    /// Determines if the result code indicates manual review is required.
    /// </summary>
    public static bool RequiresManualReview(string resultCode)
    {
        return ManualReviewPattern().IsMatch(resultCode);
    }

    /// <summary>
    /// Gets the transaction status from the result code.
    /// </summary>
    public static Models.TransactionStatus GetStatus(string resultCode)
    {
        if (string.IsNullOrWhiteSpace(resultCode))
        {
            return Models.TransactionStatus.Unknown;
        }

        if (SuccessPattern().IsMatch(resultCode))
        {
            return Models.TransactionStatus.Success;
        }

        if (ManualReviewPattern().IsMatch(resultCode))
        {
            return Models.TransactionStatus.SuccessManualReview;
        }

        if (PendingPattern().IsMatch(resultCode))
        {
            return Models.TransactionStatus.Pending;
        }

        if (SoftDeclinePattern().IsMatch(resultCode))
        {
            return Models.TransactionStatus.SoftDecline;
        }

        if (HardDeclinePattern().IsMatch(resultCode))
        {
            return Models.TransactionStatus.HardDecline;
        }

        if (ValidationErrorPattern().IsMatch(resultCode))
        {
            return Models.TransactionStatus.ValidationError;
        }

        if (CommunicationErrorPattern().IsMatch(resultCode))
        {
            return Models.TransactionStatus.CommunicationError;
        }

        if (SystemErrorPattern().IsMatch(resultCode))
        {
            return Models.TransactionStatus.SystemError;
        }

        return Models.TransactionStatus.Unknown;
    }

    /// <summary>
    /// Determines if a transaction can be retried based on the result code.
    /// </summary>
    public static bool CanRetry(string resultCode)
    {
        var status = GetStatus(resultCode);
        return status is Models.TransactionStatus.SoftDecline 
            or Models.TransactionStatus.CommunicationError 
            or Models.TransactionStatus.SystemError;
    }
}
