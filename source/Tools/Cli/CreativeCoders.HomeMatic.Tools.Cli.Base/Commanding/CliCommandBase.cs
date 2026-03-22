namespace CreativeCoders.HomeMatic.Tools.Cli.Base.Commanding;

public abstract class CliDataCommandBase<TOptions, TData> : IHomeMaticCliCommandWithOptions<TOptions>
    where TOptions : class
{
    public Task<int> ExecuteAsync(TOptions options)
    {
        throw new NotImplementedException();
    }

    protected abstract Task<TData> LoadDataAsync();
}
