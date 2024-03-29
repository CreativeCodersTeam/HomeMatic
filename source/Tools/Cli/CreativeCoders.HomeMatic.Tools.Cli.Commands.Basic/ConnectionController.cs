﻿using CreativeCoders.Core;
using CreativeCoders.HomeMatic.Tools.Cli.Base.Commanding;
using CreativeCoders.HomeMatic.Tools.Cli.Commands.Basic.Connections.AddConnection;
using CreativeCoders.HomeMatic.Tools.Cli.Commands.Basic.Connections.ListConnections;
using CreativeCoders.HomeMatic.Tools.Cli.Commands.Basic.Connections.RemoveConnection;
using CreativeCoders.SysConsole.Cli.Actions;
using CreativeCoders.SysConsole.Cli.Actions.Definition;
using JetBrains.Annotations;

namespace CreativeCoders.HomeMatic.Tools.Cli.Commands.Basic;

[UsedImplicitly]
[CliController("connection")]
public class ConnectionController
{
    private readonly ICliCommandExecutor _commandExecutor;

    public ConnectionController(ICliCommandExecutor commandExecutor)
    {
        _commandExecutor = Ensure.NotNull(commandExecutor);
    }
    
    // [CliAction("select", HelpText = "Selects the connection to use")]
    // public Task<CliActionResult> SelectConnectionAsync(SelectOptions options)
    // {
    //     return _commandExecutor.ExecuteAsync<SelectCommand, SelectOptions>(options);
    // }
    
    // [CliAction("status", HelpText = "Show current connection status")]
    // public Task<CliActionResult> ShowStatusAsync()
    // {
    //     return _commandExecutor.ExecuteAsync<StatusCommand>();
    // }
    
    // [CliAction("list-devices", HelpText = "List devices")]
    // public Task<CliActionResult> ListDevicesAsync(ListDevicesOptions options)
    //     => _commandExecutor.ExecuteAsync<ListDevicesCommand, ListDevicesOptions>(options);
    
    [CliAction("add", HelpText = "Adds a new CCU connection to the available connections")]
    public Task<CliActionResult> AddAsync(AddConnectionOptions options)
        => _commandExecutor.ExecuteAsync<AddConnectionCommand, AddConnectionOptions>(options);
    
    [CliAction("remove", HelpText = "Removes a CCU connection from the available connections")]
    public Task<CliActionResult> RemoveAsync(RemoveConnectionOptions options)
        => _commandExecutor.ExecuteAsync<RemoveConnectionCommand, RemoveConnectionOptions>(options);
    
    [CliAction("list", HelpText = "Lists all available CCU connections")]
    public Task<CliActionResult> ListAsync(ListConnectionsOptions options)
        => _commandExecutor.ExecuteAsync<ListConnectionsCommand, ListConnectionsOptions>(options);
}
