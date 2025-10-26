using INT.VrPay.Client.Models;

namespace INT.VrPay.Client.Exceptions;

/// <summary>
/// Exception thrown when a payment is declined.
/// </summary>
public class VrPayPaymentDeclinedException : VrPayException
{
    /// <summary>
    /// The result code from the payment response.
    /// </summary>
    public string ResultCode { get; }

    /// <summary>
    /// The full payment response.
    /// </summary>
    public PaymentResponse Response { get; }

    /// <summary>
    /// Creates a new VrPayPaymentDeclinedException.
    /// </summary>
    public VrPayPaymentDeclinedException(string message, string resultCode, PaymentResponse response) 
        : base(message)
    {
        ResultCode = resultCode;
        Response = response;
    }
}
