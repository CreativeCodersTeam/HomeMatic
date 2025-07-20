using System.Text.Json;
using CreativeCoders.Core.IO;
using CreativeCoders.Core.Text;

namespace CreativeCoders.HomeMatic.Tools.Cli.Base.Commanding;

public class JsonDataExporterBase<T>(Func<Task<T>> loadDataAsyncFunc, Func<T, object> transformData)
{
    public async Task ExportAsync(string outputFileName)
    {
        var data = await loadDataAsyncFunc().ConfigureAwait(false);

        var outputData = transformData(data);

        await FileSys.File.WriteAllTextAsync(outputFileName,
                outputData.ToJson(new JsonSerializerOptions { WriteIndented = true }))
            .ConfigureAwait(false);
    }
}
