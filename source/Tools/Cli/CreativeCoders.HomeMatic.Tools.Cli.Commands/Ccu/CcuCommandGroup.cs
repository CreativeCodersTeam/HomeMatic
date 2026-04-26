using CreativeCoders.Cli.Core;
using CreativeCoders.HomeMatic.Tools.Cli.Commands.Ccu;

[assembly: CliCommandGroup([CcuCommandGroup.Name], "Commands for CCU operations")]

namespace CreativeCoders.HomeMatic.Tools.Cli.Commands.Ccu;

/// <summary>
/// Defines the command group for CCU operations.
/// </summary>
public static class CcuCommandGroup
{
    /// <summary>
    /// The name of the CCU command group.
    /// </summary>
    public const string Name = "ccu";
}
