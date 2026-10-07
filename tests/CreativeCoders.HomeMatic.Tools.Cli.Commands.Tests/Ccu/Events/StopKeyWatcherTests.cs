using System.Text;
using AwesomeAssertions;
using CreativeCoders.HomeMatic.Tools.Cli.Commands.Ccu.Events;
using FakeItEasy;
using Spectre.Console;

namespace CreativeCoders.HomeMatic.Tools.Cli.Commands.Tests.Ccu.Events;

public class StopKeyWatcherTests
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromMilliseconds(1);

    private static readonly TimeSpan TestTimeout = TimeSpan.FromSeconds(10);

    [Theory]
    [InlineData('Q', ConsoleKey.Q)]
    [InlineData('q', ConsoleKey.Q)]
    [InlineData('\u001b', ConsoleKey.Escape)]
    public async Task RunAsync_StopKeyPressed_CancelsStopSource(char keyChar, ConsoleKey key)
    {
        // Arrange
        var context = new ConsoleContext(interactive: true);
        context.QueueKeys(new ConsoleKeyInfo(keyChar, key, char.IsUpper(keyChar), false, false));
        using var stopSource = new CancellationTokenSource();
        var sut = new StopKeyWatcher(context.Console, PollInterval);

        // Act
        await sut.RunAsync(stopSource, stopSource.Token).WaitAsync(TestTimeout);

        // Assert
        stopSource.IsCancellationRequested.Should().BeTrue();
        A.CallTo(() => context.Input.ReadKey(true)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task RunAsync_OtherKeysBeforeStopKey_KeepsPollingUntilStopKey()
    {
        // Arrange
        var context = new ConsoleContext(interactive: true);
        context.QueueKeys(
            new ConsoleKeyInfo('a', ConsoleKey.A, false, false, false),
            new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false),
            new ConsoleKeyInfo('q', ConsoleKey.Q, false, false, false));
        using var stopSource = new CancellationTokenSource();
        var sut = new StopKeyWatcher(context.Console, PollInterval);

        // Act
        await sut.RunAsync(stopSource, stopSource.Token).WaitAsync(TestTimeout);

        // Assert
        stopSource.IsCancellationRequested.Should().BeTrue();
        A.CallTo(() => context.Input.ReadKey(true)).MustHaveHappened(3, Times.Exactly);
    }

    [Fact]
    public async Task RunAsync_OnlyOtherKeys_DoesNotCancelStopSource()
    {
        // Arrange
        var context = new ConsoleContext(interactive: true);
        context.QueueKeys(
            new ConsoleKeyInfo('a', ConsoleKey.A, false, false, false),
            new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false));
        using var stopSource = new CancellationTokenSource();
        using var tokenSource = new CancellationTokenSource();
        var sut = new StopKeyWatcher(context.Console, PollInterval);

        // Act
        var run = sut.RunAsync(stopSource, tokenSource.Token);
        await context.AllKeysRead.WaitAsync(TestTimeout);
        await tokenSource.CancelAsync();
        await run.WaitAsync(TestTimeout);

        // Assert
        stopSource.IsCancellationRequested.Should().BeFalse();
        A.CallTo(() => context.Input.ReadKey(true)).MustHaveHappened(2, Times.Exactly);
    }

    [Fact]
    public async Task RunAsync_ReadKeyReturnsNull_DoesNotCancelStopSource()
    {
        // Arrange
        var context = new ConsoleContext(interactive: true);
        var readCount = 0;
        A.CallTo(() => context.Input.IsKeyAvailable()).Returns(true);
        A.CallTo(() => context.Input.ReadKey(true)).ReturnsLazily(() =>
        {
            readCount++;
            return readCount == 1 ? null : new ConsoleKeyInfo('q', ConsoleKey.Q, false, false, false);
        });
        using var stopSource = new CancellationTokenSource();
        var sut = new StopKeyWatcher(context.Console, PollInterval);

        // Act
        await sut.RunAsync(stopSource, stopSource.Token).WaitAsync(TestTimeout);

        // Assert
        readCount.Should().Be(2);
        stopSource.IsCancellationRequested.Should().BeTrue();
    }

    [Fact]
    public async Task RunAsync_NoKeyAvailable_DoesNotReadKey()
    {
        // Arrange
        var context = new ConsoleContext(interactive: true);
        using var stopSource = new CancellationTokenSource();
        using var tokenSource = new CancellationTokenSource();
        var polled = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        A.CallTo(() => context.Input.IsKeyAvailable()).ReturnsLazily(() =>
        {
            polled.TrySetResult();
            return false;
        });
        var sut = new StopKeyWatcher(context.Console, PollInterval);

        // Act
        var run = sut.RunAsync(stopSource, tokenSource.Token);
        await polled.Task.WaitAsync(TestTimeout);
        await tokenSource.CancelAsync();
        await run.WaitAsync(TestTimeout);

        // Assert
        A.CallTo(() => context.Input.ReadKey(A<bool>._)).MustNotHaveHappened();
        stopSource.IsCancellationRequested.Should().BeFalse();
    }

    [Fact]
    public async Task RunAsync_NotInteractive_CompletesWithoutTouchingInput()
    {
        // Arrange
        var context = new ConsoleContext(interactive: false);
        using var stopSource = new CancellationTokenSource();
        var sut = new StopKeyWatcher(context.Console, PollInterval);

        // Act
        var run = sut.RunAsync(stopSource, stopSource.Token);

        // Assert
        run.IsCompletedSuccessfully.Should().BeTrue();
        await run;
        stopSource.IsCancellationRequested.Should().BeFalse();
        A.CallTo(() => context.Input.IsKeyAvailable()).MustNotHaveHappened();
        A.CallTo(() => context.Input.ReadKey(A<bool>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task RunAsync_TokenCancelled_CompletesWithoutThrowing()
    {
        // Arrange
        var context = new ConsoleContext(interactive: true);
        using var stopSource = new CancellationTokenSource();
        using var tokenSource = new CancellationTokenSource();
        var sut = new StopKeyWatcher(context.Console, TimeSpan.FromMinutes(5));

        // Act
        var run = sut.RunAsync(stopSource, tokenSource.Token);
        await tokenSource.CancelAsync();
        var act = () => run.WaitAsync(TestTimeout);

        // Assert
        await act.Should().NotThrowAsync();
        stopSource.IsCancellationRequested.Should().BeFalse();
    }

    [Fact]
    public async Task RunAsync_TokenAlreadyCancelled_CompletesWithoutThrowing()
    {
        // Arrange
        var context = new ConsoleContext(interactive: true);
        using var stopSource = new CancellationTokenSource();
        using var tokenSource = new CancellationTokenSource();
        await tokenSource.CancelAsync();
        var sut = new StopKeyWatcher(context.Console, TimeSpan.FromMinutes(5));

        // Act
        var act = () => sut.RunAsync(stopSource, tokenSource.Token).WaitAsync(TestTimeout);

        // Assert
        await act.Should().NotThrowAsync();
        stopSource.IsCancellationRequested.Should().BeFalse();
    }

    public static TheoryData<Exception> InputFailures =>
        [new InvalidOperationException("input redirected"), new IOException("input broken")];

    [Theory]
    [MemberData(nameof(InputFailures))]
    public async Task RunAsync_IsKeyAvailableThrows_CompletesWithoutThrowingOrCancelling(Exception failure)
    {
        // Arrange
        var context = new ConsoleContext(interactive: true);
        A.CallTo(() => context.Input.IsKeyAvailable()).Throws(failure);
        using var stopSource = new CancellationTokenSource();
        var sut = new StopKeyWatcher(context.Console, PollInterval);

        // Act
        var act = () => sut.RunAsync(stopSource, stopSource.Token).WaitAsync(TestTimeout);

        // Assert
        await act.Should().NotThrowAsync();
        stopSource.IsCancellationRequested.Should().BeFalse();
    }

    [Theory]
    [MemberData(nameof(InputFailures))]
    public async Task RunAsync_ReadKeyThrows_CompletesWithoutThrowingOrCancelling(Exception failure)
    {
        // Arrange
        var context = new ConsoleContext(interactive: true);
        A.CallTo(() => context.Input.IsKeyAvailable()).Returns(true);
        A.CallTo(() => context.Input.ReadKey(true)).Throws(failure);
        using var stopSource = new CancellationTokenSource();
        var sut = new StopKeyWatcher(context.Console, PollInterval);

        // Act
        var act = () => sut.RunAsync(stopSource, stopSource.Token).WaitAsync(TestTimeout);

        // Assert
        await act.Should().NotThrowAsync();
        stopSource.IsCancellationRequested.Should().BeFalse();
    }

    [Fact]
    public async Task RunAsync_InputThrowsUnexpectedException_PropagatesWithoutCancelling()
    {
        // Arrange
        var context = new ConsoleContext(interactive: true);
        A.CallTo(() => context.Input.IsKeyAvailable()).Throws(new NotSupportedException("unexpected"));
        using var stopSource = new CancellationTokenSource();
        var sut = new StopKeyWatcher(context.Console, PollInterval);

        // Act
        var act = () => sut.RunAsync(stopSource, stopSource.Token).WaitAsync(TestTimeout);

        // Assert
        await act.Should().ThrowAsync<NotSupportedException>();
        stopSource.IsCancellationRequested.Should().BeFalse();
    }

    [Fact]
    public void Ctor_NullConsole_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new StopKeyWatcher(null!, PollInterval);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task RunAsync_NullStopSource_ThrowsArgumentNullException()
    {
        // Arrange
        var sut = new StopKeyWatcher(new ConsoleContext(interactive: true).Console, PollInterval);

        // Act
        var act = () => sut.RunAsync(null!, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    private sealed class ConsoleContext
    {
        private readonly TaskCompletionSource _allKeysRead =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public ConsoleContext(bool interactive)
        {
            var profile = new Profile(
                new AnsiConsoleOutput(TextWriter.Null),
                new Capabilities { Interactive = interactive },
                Encoding.UTF8);

            A.CallTo(() => Console.Profile).Returns(profile);
            A.CallTo(() => Console.Input).Returns(Input);
        }

        public IAnsiConsole Console { get; } = A.Fake<IAnsiConsole>();

        public IAnsiConsoleInput Input { get; } = A.Fake<IAnsiConsoleInput>();

        public Task AllKeysRead => _allKeysRead.Task;

        public void QueueKeys(params ConsoleKeyInfo[] keys)
        {
            var queue = new Queue<ConsoleKeyInfo>(keys);

            A.CallTo(() => Input.IsKeyAvailable()).ReturnsLazily(() => queue.Count > 0);
            A.CallTo(() => Input.ReadKey(true)).ReturnsLazily(() =>
            {
                var key = queue.Dequeue();

                if (queue.Count == 0)
                {
                    _allKeysRead.TrySetResult();
                }

                return key;
            });
        }
    }
}
