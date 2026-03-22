using System.Text.Json;
using CreativeCoders.Cli.Core;
using Spectre.Console;

namespace CreativeCoders.HomeMatic.Tools.Cli.Base.Commanding;

public abstract class CliOutputCommandBase<TData, TOptions>(IAnsiConsole console)
    : ICliCommand<TOptions>
    where TOptions : class, IJsonOutputOptions
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public async Task<CommandResult> ExecuteAsync(TOptions options)
    {
        var data = await LoadDataAsync(options).ConfigureAwait(false);

        if (options.JsonOutput)
        {
            console.WriteLine(JsonSerializer.Serialize(ToJsonData(data, options), JsonOptions));
        }
        else
        {
            return await PrintDataAsync(data, options).ConfigureAwait(false);
        }

        return CommandResult.Success;
    }

    protected abstract Task<TData> LoadDataAsync(TOptions options);

    protected abstract Task<CommandResult> PrintDataAsync(TData data, TOptions options);

    protected virtual object ToJsonData(TData data, TOptions options) => data!;
}
