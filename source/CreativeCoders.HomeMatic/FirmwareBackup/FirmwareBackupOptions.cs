using System.Net;
using CreativeCoders.Core;
using JetBrains.Annotations;

namespace CreativeCoders.HomeMatic.FirmwareBackup;

/// <summary>
/// Connection and behavior options used to create a firmware backup of a HomeMatic CCU.
/// </summary>
[PublicAPI]
public class FirmwareBackupOptions
{
    /// <summary>
    /// Initializes a new instance of <see cref="FirmwareBackupOptions"/>.
    /// </summary>
    /// <param name="baseUrl">Base URL of the CCU (e.g. <c>https://homematic-ccu.local</c>).</param>
    /// <param name="credential">Credentials of a CCU user that is allowed to download backups.</param>
    public FirmwareBackupOptions(Uri baseUrl, NetworkCredential credential)
    {
        BaseUrl = Ensure.NotNull(baseUrl);
        Credential = Ensure.NotNull(credential);
    }

    /// <summary>
    /// Gets the base URL of the CCU.
    /// </summary>
    public Uri BaseUrl { get; }

    /// <summary>
    /// Gets the credentials used to log in against the CCU.
    /// </summary>
    public NetworkCredential Credential { get; }

    /// <summary>
    /// Gets or sets the relative path of the JSON-RPC endpoint used for login/logout. Default: <c>/api/homematic.cgi</c>.
    /// </summary>
    public string JsonRpcPath { get; set; } = "/api/homematic.cgi";

    /// <summary>
    /// Gets or sets the relative path of the CGI endpoint that produces the firmware backup file.
    /// Default: <c>/config/cp_security.cgi</c>.
    /// </summary>
    public string BackupCgiPath { get; set; } = "/config/cp_security.cgi";

    /// <summary>
    /// Gets or sets the form action value sent to the backup CGI endpoint. Default: <c>create_backup</c>.
    /// </summary>
    public string BackupAction { get; set; } = "create_backup";

    /// <summary>
    /// Gets or sets a value indicating whether the HTTP client should accept any (including self-signed)
    /// server certificate. CCU devices typically use a self-signed certificate, therefore the default is
    /// <see langword="true"/>.
    /// </summary>
    public bool AcceptAnyServerCertificate { get; set; } = true;

    /// <summary>
    /// Gets or sets the request timeout used for both the JSON-RPC and the CGI download call.
    /// Default: 5 minutes (creating a backup on the CCU can take a while).
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromMinutes(5);
}
