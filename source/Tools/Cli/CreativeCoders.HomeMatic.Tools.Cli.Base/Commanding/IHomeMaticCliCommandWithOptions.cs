namespace CreativeCoders.HomeMatic.Tools.Cli.Base.Commanding;

public interface IHomeMaticCliCommandWithOptions<in TOptions>
    where TOptions : class
{
    Task<int> ExecuteAsync(TOptions options);
}
