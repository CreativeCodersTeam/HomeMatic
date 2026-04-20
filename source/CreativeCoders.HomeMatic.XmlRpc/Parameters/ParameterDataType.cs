using JetBrains.Annotations;

namespace CreativeCoders.HomeMatic.XmlRpc.Parameters;

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