using System.Diagnostics.CodeAnalysis;
using CreativeCoders.HomeMatic.Core;

namespace CreativeCoders.HomeMatic;

[ExcludeFromCodeCoverage]
public class CcuDeviceUri
{
    public required string CcuHost { get; init; }

    public string CcuName { get; init; } = string.Empty;

    public required CcuDeviceKind Kind { get; init; }

    public required string Address { get; init; }

    public string HostDisplayName => string.IsNullOrWhiteSpace(CcuName) ? CcuHost : CcuName;

    public override string ToString()
    {
        return $"{Kind}://{CcuHost}/{Address}";
    }
}
