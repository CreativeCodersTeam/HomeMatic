using JetBrains.Annotations;

namespace CreativeCoders.HomeMatic.Core.Parameters;

[PublicAPI]
public class ParamSetKey
{
    public const string Master = "MASTER";

    public const string Values = "VALUES";

    public const string Link = "LINK";

    public const string Service = "SERVICE";

    public static readonly string[] ParamSetKeys = {Master, Values, Link, Service};

    public static ParameterKind StringToParameterKind(string text)
    {
        return text.ToUpper() switch
        {
            Master => ParameterKind.Master,
            Values => ParameterKind.Values,
            Link => ParameterKind.Link,
            Service => ParameterKind.Service,
            _ => ParameterKind.Undefined
        };
    }
}