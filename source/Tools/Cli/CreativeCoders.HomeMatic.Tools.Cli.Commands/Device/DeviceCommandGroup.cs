using CreativeCoders.Cli.Core;
using CreativeCoders.HomeMatic.Tools.Cli.Commands.Device;

[assembly: CliCommandGroup([DeviceCommandGroup.Name], "Commands for managing CCU devices")]

namespace CreativeCoders.HomeMatic.Tools.Cli.Commands.Device;

public static class DeviceCommandGroup
{
    public const string Name = "device";
}
