namespace CreativeCoders.HomeMatic.FirmwareBackup.Internal;

/// <summary>
/// Performs JSON-RPC login/logout against a HomeMatic CCU to obtain a session id usable for
/// subsequent CGI calls (e.g. firmware backup download).
/// </summary>
internal interface ICcuSessionClient
{
    /// <summary>
    /// Logs in against the CCU and returns the session id.
    /// </summary>
    /// <param name="userName">Username used for login.</param>
    /// <param name="password">Password used for login.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The session id assigned by the CCU.</returns>
    Task<string> LoginAsync(string userName, string password, CancellationToken cancellationToken = default);

    /// <summary>
    /// Logs out the given session. Errors are swallowed so that this method can safely be called from a
    /// finally block.
    /// </summary>
    /// <param name="sessionId">Session id previously returned by <see cref="LoginAsync"/>.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task LogoutAsync(string sessionId, CancellationToken cancellationToken = default);
}
