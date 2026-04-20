using System.Text.Json;
using System.Text.Json.Serialization;
using CreativeCoders.Core.IO;
using CreativeCoders.Core.Text;
using CreativeCoders.HomeMatic.Core;
using Spectre.Console;

namespace CreativeCoders.HomeMatic.Tools.Cli.Base.Commanding;

public class JsonDataExporterBase<T, TOptions>(
    IAnsiConsole console,
    Func<IMultiCcuClient, TOptions, Task<T>> loadDataAsyncFunc,
    Func<T, object> transformData)
    where TOptions : class
{
    public async Task ExportAsync(IMultiCcuClient ccuClient, TOptions options, string outputFileName)
    {
        console.WriteLine("Load data for export");

        var data = await loadDataAsyncFunc(ccuClient, options).ConfigureAwait(false);

        console.WriteLine("Transform data for export");

        var outputData = transformData(data);

        console.WriteLine($"Write data to json file '{outputFileName}'");

        await FileSys.File.WriteAllTextAsync(outputFileName,
                outputData.ToJson(new JsonSerializerOptions
                {
                    WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                }))
            .ConfigureAwait(false);
    }
}
