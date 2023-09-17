using System.Net;
using CreativeCoders.HomeMatic.JsonRpc.Api;
using CreativeCoders.Net.JsonRpc;
using FakeItEasy;
using FluentAssertions;

namespace CreativeCoders.HomeMatic.JsonRpc.Tests;

public class HomeMaticJsonRpcClientTests
{
    [Fact]
    public async Task LoginAsync_CredentialsGiven_LoginIsDoneAndSubsequentCallsAreMadeWithSessionId()
    {
        // Arrange
        const string expectedUserName = "user1";
        const string expectedPassword = "password1";
        const string expectedSessionId = "ABCD1234";
        
        var api = A.Fake<IHomeMaticJsonRpcApi>();

        A.CallTo(() => api.LoginAsync(expectedUserName, expectedPassword))
            .Returns(Task.FromResult(new JsonRpcResponse<string> { Result = expectedSessionId }));

        var client = new HomeMaticJsonRpcClient(api);
        client.Credential = new NetworkCredential(expectedUserName, expectedPassword);

        // Act
        await client.LoginAsync().ConfigureAwait(false);
        await client.ListAllDetailsAsync().ConfigureAwait(false);

        // Assert
        A.CallTo(() => api.LoginAsync(expectedUserName, expectedPassword)).MustHaveHappenedOnceExactly();
        A.CallTo(() => api.ListAllDetailsAsync(expectedSessionId)).MustHaveHappenedOnceExactly();
    }
    
    [Fact]
    public async Task LoginAsync_NoCredentialsGiven_ExceptionIsThrown()
    {
        // Arrange
        var api = A.Fake<IHomeMaticJsonRpcApi>();

        var client = new HomeMaticJsonRpcClient(api);

        // Act
        var act = () => client.LoginAsync();

        // Assert
        await act
            .Should()
            .ThrowWithinAsync<InvalidOperationException>(TimeSpan.MaxValue);
        
        A.CallTo(() => api.LoginAsync(A<string>.Ignored, A<string>.Ignored)).MustNotHaveHappened();
    }
    
    [Fact]
    public async Task ListAllDetailsAsync_CredentialsGivenNotLoggedIn_LoginIsDoneAndListAllDetailsAsyncIsCalled()
    {
        // Arrange
        const string expectedUserName = "user1";
        const string expectedPassword = "password1";
        const string expectedSessionId = "ABCD1234";
        
        var api = A.Fake<IHomeMaticJsonRpcApi>();

        A.CallTo(() => api.LoginAsync(expectedUserName, expectedPassword))
            .Returns(Task.FromResult(new JsonRpcResponse<string> { Result = expectedSessionId }));

        var client = new HomeMaticJsonRpcClient(api);
        client.Credential = new NetworkCredential(expectedUserName, expectedPassword);

        // Act
        await client.ListAllDetailsAsync().ConfigureAwait(false);

        // Assert
        A.CallTo(() => api.LoginAsync(expectedUserName, expectedPassword)).MustHaveHappenedOnceExactly();
        A.CallTo(() => api.ListAllDetailsAsync(expectedSessionId)).MustHaveHappenedOnceExactly();
    }
}
