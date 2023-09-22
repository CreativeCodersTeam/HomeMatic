using JetBrains.Annotations;

namespace CreativeCoders.HomeMatic.Core.Parameters;

[PublicAPI]
public enum ParameterDataType
{
    Unknown,
    Integer,
    Bool,
    Float,
    Action,
    Enum,
    String
}