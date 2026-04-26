using CreativeCoders.Cli.Core;
using CreativeCoders.Core;
using CreativeCoders.HomeMatic.Tools.Cli.Base.Commands.Output;
using CreativeCoders.HomeMatic.Tools.Cli.Base.Commands.Serialization;
using Spectre.Console;

namespace CreativeCoders.HomeMatic.Tools.Cli.Base.Commands;

/// <summary>
/// Generic base class for CLI commands that load arbitrary data, serialize it as JSON or YAML
/// and write the result either to a file or to stdout.
/// </summary>
/// <typeparam name="TData">The type of data produced by <see cref="LoadDataAsync"/>.</typeparam>
/// <typeparam name="TOptions">The type of CLI options. Must implement <see cref="IDataOutputOptions"/>.</typeparam>
public abstract class DataOutputCommandBase<TData, TOptions> : ICliCommand<TOptions>
    where TOptions : class, IDataOutputOptions
{
    private readonly IAnsiConsole _console;

    private readonly IDataSerializerFactory _serializerFactory;

    private readonly IDataOutputWriter _outputWriter;

    /// <summary>
    /// Initializes a new instance of the <see cref="DataOutputCommandBase{TData, TOptions}"/> class.
    /// </summary>
    /// <param name="console">The console used for status messages and stdout output.</param>
    /// <param name="serializerFactory">The factory that resolves serializers for a format.</param>
    /// <param name="outputWriter">The writer that targets file or stdout.</param>
    protected DataOutputCommandBase(
        IAnsiConsole console,
        IDataSerializerFactory serializerFactory,
        IDataOutputWriter outputWriter)
    {
        _console = Ensure.NotNull(console);
        _serializerFactory = Ensure.NotNull(serializerFactory);
        _outputWriter = Ensure.NotNull(outputWriter);
    }

    /// <summary>
    /// Gets the console available to subclasses for additional output.
    /// </summary>
    protected IAnsiConsole Console => _console;

    /// <inheritdoc />
    public async Task<CommandResult> ExecuteAsync(TOptions options)
    {
        Ensure.NotNull(options);

        var format = _outputWriter.ResolveFormat(options.OutputFormat, options.OutputFile);

        await OnBeforeLoadAsync(options).ConfigureAwait(false);

        var data = await LoadDataAsync(options).ConfigureAwait(false);

        var transformed = TransformData(data, options);

        Ensure.NotNull(transformed);

        var serializer = _serializerFactory.Create(format);
        var content = serializer.Serialize(transformed);

        await OnBeforeWriteAsync(options, format).ConfigureAwait(false);

        await _outputWriter.WriteAsync(content, options.OutputFile).ConfigureAwait(false);

        await OnAfterWriteAsync(options, format).ConfigureAwait(false);

        return CommandResult.Success;
    }

    /// <summary>
    /// Loads the data to be serialized.
    /// </summary>
    /// <param name="options">The CLI options.</param>
    /// <returns>The loaded data.</returns>
    protected abstract Task<TData> LoadDataAsync(TOptions options);

    /// <summary>
    /// Transforms the loaded data into the object that is actually serialized. The default
    /// implementation returns <paramref name="data"/> unchanged.
    /// </summary>
    /// <param name="data">The loaded data.</param>
    /// <param name="options">The CLI options.</param>
    /// <returns>The object passed to the serializer.</returns>
    protected virtual object TransformData(TData data, TOptions options) => data!;

    /// <summary>
    /// Hook invoked before data is loaded. The default implementation writes a status message
    /// to the console.
    /// </summary>
    /// <param name="options">The CLI options.</param>
    /// <returns>A task that completes when the hook is finished.</returns>
    protected virtual Task OnBeforeLoadAsync(TOptions options)
    {
        _console.WriteLine("Loading data...");

        return Task.CompletedTask;
    }

    /// <summary>
    /// Hook invoked after serialization but before writing the result. The default implementation
    /// writes a status message describing the resolved target.
    /// </summary>
    /// <param name="options">The CLI options.</param>
    /// <param name="resolvedFormat">The resolved output format.</param>
    /// <returns>A task that completes when the hook is finished.</returns>
    protected virtual Task OnBeforeWriteAsync(TOptions options, DataOutputFormat resolvedFormat)
    {
        var target = string.IsNullOrWhiteSpace(options.OutputFile)
            ? "stdout"
            : $"file '{options.OutputFile}'";

        _console.WriteLine($"Writing {resolvedFormat} output to {target}");

        return Task.CompletedTask;
    }

    /// <summary>
    /// Hook invoked after the result has been written. The default implementation is a no-op.
    /// </summary>
    /// <param name="options">The CLI options.</param>
    /// <param name="resolvedFormat">The resolved output format.</param>
    /// <returns>A task that completes when the hook is finished.</returns>
    protected virtual Task OnAfterWriteAsync(TOptions options, DataOutputFormat resolvedFormat)
        => Task.CompletedTask;
}
