using INT.VrPay.Client.Models;
using INT.VrPay.Client.Services;

namespace INT.VrPay.Client.Extensions;

/// <summary>
/// Extension methods for PaymentResponse.
/// </summary>
public static class PaymentResponseExtensions
{
    /// <summary>
    /// Determines if the payment was successful.
    /// </summary>
    public static bool IsSuccess(this PaymentResponse response)
    {
        return ResultCodeAnalyzer.IsSuccess(response.Result.Code);
    }

    /// <summary>
    /// Determines if the payment requires manual review.
    /// </summary>
    public static bool RequiresManualReview(this PaymentResponse response)
    {
        return ResultCodeAnalyzer.RequiresManualReview(response.Result.Code);
    }

    /// <summary>
    /// Gets the transaction status.
    /// </summary>
    public static TransactionStatus GetStatus(this PaymentResponse response)
    {
        return ResultCodeAnalyzer.GetStatus(response.Result.Code);
    }

    /// <summary>
    /// Determines if the transaction can be retried.
    /// </summary>
    public static bool CanRetry(this PaymentResponse response)
    {
        return ResultCodeAnalyzer.CanRetry(response.Result.Code);
    }
}
