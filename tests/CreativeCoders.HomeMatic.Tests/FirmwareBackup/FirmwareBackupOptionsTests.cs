using System.Net;
using AwesomeAssertions;
using CreativeCoders.HomeMatic.FirmwareBackup;

namespace CreativeCoders.HomeMatic.Tests.FirmwareBackup;

public class FirmwareBackupOptionsTests
{
    [Fact]
    public void Constructor_WithValidArgs_SetsProperties()
    {
        // Arrange
        var url = new Uri("https://ccu.example.local");
        var credential = new NetworkCredential("admin", "pwd");

        // Act
        var options = new FirmwareBackupOptions(url, credential);

        // Assert
        options.BaseUrl.Should().BeSameAs(url);
        options.Credential.Should().BeSameAs(credential);
        options.JsonRpcPath.Should().Be("/api/homematic.cgi");
        options.BackupCgiPath.Should().Be("/config/cp_security.cgi");
        options.BackupAction.Should().Be("create_backup");
        options.AcceptAnyServerCertificate.Should().BeTrue();
        options.Timeout.Should().Be(TimeSpan.FromMinutes(5));
    }

    [Fact]
    public void Constructor_WithNullUrl_Throws()
    {
        // Arrange
        var credential = new NetworkCredential("admin", "pwd");

        // Act
        var act = () => new FirmwareBackupOptions(null!, credential);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_WithNullCredential_Throws()
    {
        // Arrange
        var url = new Uri("https://ccu.example.local");

        // Act
        var act = () => new FirmwareBackupOptions(url, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }
}
