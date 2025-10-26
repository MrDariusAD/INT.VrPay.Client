using System.Runtime.Serialization;

namespace INT.VrPay.Client.Models;

/// <summary>
/// Payment brands supported by VrPay.
/// </summary>
public enum PaymentBrand
{
    /// <summary>
    /// Visa card.
    /// </summary>
    [EnumMember(Value = "VISA")]
    Visa,

    /// <summary>
    /// Mastercard.
    /// </summary>
    [EnumMember(Value = "MASTER")]
    Master,

    /// <summary>
    /// American Express.
    /// </summary>
    [EnumMember(Value = "AMEX")]
    Amex,

    /// <summary>
    /// Diners Club.
    /// </summary>
    [EnumMember(Value = "DINERS")]
    Diners,

    /// <summary>
    /// Discover card.
    /// </summary>
    [EnumMember(Value = "DISCOVER")]
    Discover,

    /// <summary>
    /// JCB card.
    /// </summary>
    [EnumMember(Value = "JCB")]
    Jcb,

    /// <summary>
    /// Maestro card.
    /// </summary>
    [EnumMember(Value = "MAESTRO")]
    Maestro
}
