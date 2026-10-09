using CreativeCoders.HomeMatic.XmlRpc;

namespace CreativeCoders.HomeMatic.Tools.Cli.Commands.Ccu.Events;

/// <summary>
/// Represents the outcome of subscribing to the events of the CCU interfaces.
/// </summary>
/// <param name="Subscribed">The interfaces that were subscribed successfully.</param>
/// <param name="Failed">The interfaces that could not be subscribed, each with its error message.</param>
public sealed record SubscriptionResult(
    IReadOnlyList<CcuDeviceKind> Subscribed,
    IReadOnlyList<(CcuDeviceKind Kind, string Error)> Failed);
