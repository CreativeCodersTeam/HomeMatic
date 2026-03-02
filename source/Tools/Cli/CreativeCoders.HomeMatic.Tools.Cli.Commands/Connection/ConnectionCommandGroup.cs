using CreativeCoders.Cli.Core;
using CreativeCoders.HomeMatic.Tools.Cli.Commands.Connection;

[assembly: CliCommandGroup([ConnectionCommandGroup.Name], "Commands for managing CCU connections")]

namespace CreativeCoders.HomeMatic.Tools.Cli.Commands.Connection;

public static class ConnectionCommandGroup
{
    public const string Name = "connection";
}
