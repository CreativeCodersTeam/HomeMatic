using CreativeCoders.Cli.Core;
using CreativeCoders.HomeMatic.Tools.Cli.Commands.Ccu;

[assembly: CliCommandGroup([CcuCommandGroup.Name], "Commands for working with a HomeMatic CCU")]

namespace CreativeCoders.HomeMatic.Tools.Cli.Commands.Ccu;

public static class CcuCommandGroup
{
    public const string Name = "ccu";
}
