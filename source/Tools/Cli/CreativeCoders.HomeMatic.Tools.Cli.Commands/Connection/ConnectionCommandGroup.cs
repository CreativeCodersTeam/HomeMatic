using CreativeCoders.Cli.Core;

[assembly: CliCommandGroup(["connection"], "Commands for managing CCU connections")]

namespace CreativeCoders.HomeMatic.Tools.Cli.Commands.Connection;

public static class ConnectionCommandGroup
{
    public const string Name = "connection";
}
