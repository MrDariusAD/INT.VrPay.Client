using System.Runtime.Serialization;

namespace INT.VrPay.Client.Models;

/// <summary>
/// Common currency codes (ISO 4217).
/// </summary>
public enum Currency
{
    /// <summary>
    /// Euro.
    /// </summary>
    [EnumMember(Value = "EUR")]
    EUR,

    /// <summary>
    /// US Dollar.
    /// </summary>
    [EnumMember(Value = "USD")]
    USD,

    /// <summary>
    /// British Pound.
    /// </summary>
    [EnumMember(Value = "GBP")]
    GBP,

    /// <summary>
    /// Swiss Franc.
    /// </summary>
    [EnumMember(Value = "CHF")]
    CHF,

    /// <summary>
    /// Japanese Yen.
    /// </summary>
    [EnumMember(Value = "JPY")]
    JPY,

    /// <summary>
    /// Canadian Dollar.
    /// </summary>
    [EnumMember(Value = "CAD")]
    CAD,

    /// <summary>
    /// Australian Dollar.
    /// </summary>
    [EnumMember(Value = "AUD")]
    AUD,

    /// <summary>
    /// Polish Zloty.
    /// </summary>
    [EnumMember(Value = "PLN")]
    PLN,

    /// <summary>
    /// Czech Koruna.
    /// </summary>
    [EnumMember(Value = "CZK")]
    CZK,

    /// <summary>
    /// Danish Krone.
    /// </summary>
    [EnumMember(Value = "DKK")]
    DKK,

    /// <summary>
    /// Swedish Krona.
    /// </summary>
    [EnumMember(Value = "SEK")]
    SEK,

    /// <summary>
    /// Norwegian Krone.
    /// </summary>
    [EnumMember(Value = "NOK")]
    NOK
}
