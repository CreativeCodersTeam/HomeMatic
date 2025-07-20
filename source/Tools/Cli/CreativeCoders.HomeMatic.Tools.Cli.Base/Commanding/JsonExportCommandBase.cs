using CreativeCoders.HomeMatic.Abstractions;
using CreativeCoders.HomeMatic.Tools.Cli.Base.Connections;
using Spectre.Console;

namespace CreativeCoders.HomeMatic.Tools.Cli.Base.Commanding;

public abstract class JsonExportCommandBase<T, TOptions>(
    IAnsiConsole console,
    ICliHomeMaticClientBuilder cliHomeMaticClientBuilder,
    Func<IMultiCcuClient, TOptions, Task<T>> loadDataAsyncFunc,
    Func<T, object> transformData) : IHomeMaticCliCommandWithOptions<TOptions>
    where TOptions : class
{
    public async Task<int> ExecuteAsync(TOptions options)
    {
        var ccuClient = await cliHomeMaticClientBuilder.BuildMultiCcuClientAsync().ConfigureAwait(false);

        console.WriteLine("Export data to json file");

        if (!ValidateOptions(options))
        {
            console.WriteLine("Options are not valid");
            return -1;
        }

        await new JsonDataExporterBase<T, TOptions>(console, loadDataAsyncFunc, transformData)
            .ExportAsync(ccuClient, options, GetOutputFileName(options))
            .ConfigureAwait(false);

        return 0;
    }

    protected abstract bool ValidateOptions(TOptions options);

    protected abstract string GetOutputFileName(TOptions options);
}
