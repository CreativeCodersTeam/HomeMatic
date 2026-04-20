using System;
using System.Collections.Generic;
using CreativeCoders.HomeMatic.XmlRpc.Parameters;
using CreativeCoders.Net.XmlRpc.Definition;
using CreativeCoders.Net.XmlRpc.Model;
using CreativeCoders.Net.XmlRpc.Model.Values;
using JetBrains.Annotations;

namespace CreativeCoders.HomeMatic.XmlRpc.Converters;

/// <summary>
/// Converts the string-encoded parameter data type returned by the HomeMatic CCU into a <see cref="ParameterDataType"/> enum value.
/// </summary>
/// <remarks>
/// The CCU describes parameter types as uppercase strings (e.g. <c>"INTEGER"</c>, <c>"BOOL"</c>, <c>"FLOAT"</c>)
/// in a parameter set description. This converter maps those strings to the corresponding
/// <see cref="ParameterDataType"/> enum members. Any unrecognized type string is mapped to
/// <see cref="ParameterDataType.Unknown"/>.
/// </remarks>
[UsedImplicitly]
public class ParameterDataTypeValueConverter : IXmlRpcMemberValueConverter
{
    private static readonly IDictionary<string, ParameterDataType> DataTypeMapping = new Dictionary<string, ParameterDataType>
    {
        {"INTEGER", ParameterDataType.Integer},
        {"BOOL", ParameterDataType.Bool},
        {"FLOAT", ParameterDataType.Float},
        {"ACTION", ParameterDataType.Action},
        {"ENUM", ParameterDataType.Enum},
        {"STRING", ParameterDataType.String}
    };
        
    /// <summary>
    /// Converts an <see cref="XmlRpcValue"/> containing a type name string into a <see cref="ParameterDataType"/> value.
    /// </summary>
    /// <param name="xmlRpcValue">The XML-RPC value to convert.</param>
    /// <returns>The corresponding <see cref="ParameterDataType"/>, or <see cref="ParameterDataType.Unknown"/> if the value is not a recognized type string.</returns>
    public object ConvertFromValue(XmlRpcValue xmlRpcValue)
    {
        if (xmlRpcValue is not StringValue text)
        {
            return ParameterDataType.Unknown;
        }

        return DataTypeMapping.TryGetValue(text.Value, out var dataType)
            ? dataType
            : ParameterDataType.Unknown;
    }

    /// <summary>
    /// Converts a <see cref="ParameterDataType"/> value into an <see cref="XmlRpcValue"/>.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>This method is not implemented and always throws <see cref="NotImplementedException"/>.</returns>
    /// <exception cref="NotImplementedException">Always thrown; serialization of this type is not supported.</exception>
    public XmlRpcValue ConvertFromObject(object value)
    {
        throw new NotImplementedException();
    }
}