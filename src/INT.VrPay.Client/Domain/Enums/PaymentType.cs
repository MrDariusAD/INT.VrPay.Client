using System.Runtime.Serialization;

namespace INT.VrPay.Client.Models;

/// <summary>
/// Payment transaction types supported by VrPay.
/// </summary>
public enum PaymentType
{
    /// <summary>
    /// Pre-Authorization - reserves the amount but doesn't capture it.
    /// </summary>
    [EnumMember(Value = "PA")]
    PreAuthorization,

    /// <summary>
    /// Debit - directly charges the payment method.
    /// </summary>
    [EnumMember(Value = "DB")]
    Debit,

    /// <summary>
    /// Capture - captures a previously pre-authorized payment.
    /// </summary>
    [EnumMember(Value = "CP")]
    Capture,

    /// <summary>
    /// Refund - refunds a captured payment.
    /// </summary>
    [EnumMember(Value = "RF")]
    Refund,

    /// <summary>
    /// Reversal - cancels a pre-authorization.
    /// </summary>
    [EnumMember(Value = "RV")]
    Reversal
}
