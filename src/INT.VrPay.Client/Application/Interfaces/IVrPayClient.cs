using INT.VrPay.Client.Models;

namespace INT.VrPay.Client;

/// <summary>
/// Interface for the VrPay payment client.
/// </summary>
public interface IVrPayClient
{
    /// <summary>
    /// Performs a pre-authorization (PA) to reserve funds on a card.
    /// Funds are not captured until a Capture operation is performed.
    /// </summary>
    /// <param name="request">The payment request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The payment response containing the transaction ID and result.</returns>
    Task<PaymentResponse> PreAuthorizeAsync(PaymentRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Performs a direct debit (DB) to immediately charge the card.
    /// This is a one-step process combining authorization and capture.
    /// </summary>
    /// <param name="request">The payment request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The payment response.</returns>
    Task<PaymentResponse> DebitAsync(PaymentRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Captures a previously pre-authorized payment.
    /// </summary>
    /// <param name="preAuthId">The ID from the pre-authorization response.</param>
    /// <param name="amount">The amount to capture (can be less than pre-authorized amount for partial capture).</param>
    /// <param name="currency">The currency code.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The capture response.</returns>
    Task<PaymentResponse> CaptureAsync(string preAuthId, decimal amount, Currency currency, CancellationToken cancellationToken = default);

    /// <summary>
    /// Refunds a captured payment (full or partial).
    /// </summary>
    /// <param name="captureId">The ID from the capture or debit response.</param>
    /// <param name="amount">The amount to refund.</param>
    /// <param name="currency">The currency code.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The refund response.</returns>
    Task<PaymentResponse> RefundAsync(string captureId, decimal amount, Currency currency, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reverses (cancels) a pre-authorization that has not been captured.
    /// </summary>
    /// <param name="preAuthId">The ID from the pre-authorization response.</param>
    /// <param name="amount">The amount to reverse (typically the same as the pre-authorized amount).</param>
    /// <param name="currency">The currency code.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The reversal response.</returns>
    Task<PaymentResponse> ReverseAsync(string preAuthId, decimal amount, Currency currency, CancellationToken cancellationToken = default);
}
