using CreativeCoders.Cli.Core;
using CreativeCoders.HomeMatic.Core;
using Spectre.Console;

namespace CreativeCoders.HomeMatic.Tools.Cli.Base.Commanding;

public abstract class JsonExportCommandBase<T, TOptions>(
    IAnsiConsole console,
    IMultiCcuClient multiCcuClient) : ICliCommand<TOptions>
    where TOptions : class
{
    public async Task<CommandResult> ExecuteAsync(TOptions options)
    {
        console.WriteLine("Export data to json file");

        await new JsonDataExporterBase<T, TOptions>(console, LoadDataAsync, TransformData)
            .ExportAsync(multiCcuClient, options, GetOutputFileName(options))
            .ConfigureAwait(false);

        return CommandResult.Success;
    }

    protected abstract object TransformData(T data);

    protected abstract Task<T> LoadDataAsync(IMultiCcuClient ccuClient, TOptions options);

    protected abstract string GetOutputFileName(TOptions options);
}
