using CreativeCoders.HomeMatic.Core;
using Spectre.Console;

namespace CreativeCoders.HomeMatic.Tools.Cli.Base.Commanding;

public abstract class JsonExportCommandBase<T, TOptions>(
    IAnsiConsole console,
    IMultiCcuClient multiCcuClient) : IHomeMaticCliCommandWithOptions<TOptions>
    where TOptions : class
{
    public async Task<int> ExecuteAsync(TOptions options)
    {
        console.WriteLine("Export data to json file");

        if (!ValidateOptions(options))
        {
            console.WriteLine("Options are not valid");
            return -1;
        }

        await new JsonDataExporterBase<T, TOptions>(console, LoadDataAsync, TransformData)
            .ExportAsync(multiCcuClient, options, GetOutputFileName(options))
            .ConfigureAwait(false);

        return 0;
    }

    protected abstract object TransformData(T data);

    protected abstract Task<T> LoadDataAsync(IMultiCcuClient ccuClient, TOptions options);

    protected abstract bool ValidateOptions(TOptions options);

    protected abstract string GetOutputFileName(TOptions options);
}
