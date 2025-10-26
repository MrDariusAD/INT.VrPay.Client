using System.Runtime.Serialization;

namespace INT.VrPay.Client.Models;

/// <summary>
/// Test mode values for VrPay testing.
/// </summary>
public enum TestMode
{
    /// <summary>
    /// External test mode.
    /// </summary>
    [EnumMember(Value = "EXTERNAL")]
    External,

    /// <summary>
    /// Internal test mode.
    /// </summary>
    [EnumMember(Value = "INTERNAL")]
    Internal
}
