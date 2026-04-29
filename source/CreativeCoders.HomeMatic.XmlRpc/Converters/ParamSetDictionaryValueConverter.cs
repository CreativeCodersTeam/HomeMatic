using System;
using System.Collections.Generic;
using System.Linq;
using CreativeCoders.Net.XmlRpc.Definition;
using CreativeCoders.Net.XmlRpc.Model;
using CreativeCoders.Net.XmlRpc.Model.Values;

namespace CreativeCoders.HomeMatic.XmlRpc.Converters;

/// <summary>
/// Converts an XML-RPC value representing a parameter set into a
/// <see cref="Dictionary{TKey,TValue}"/> with string keys and object values.
/// </summary>
/// <remarks>
/// HomeMatic / homegear CCUs occasionally return the <c>SENDER_PARAMSET</c> and
/// <c>RECEIVER_PARAMSET</c> members of a <c>getLinks</c> entry as an empty
/// <see cref="StringValue"/> instead of an empty <see cref="StructValue"/> when the
/// corresponding paramset flag was not requested. The default
/// <see cref="IXmlRpcMemberValueConverter"/> implementation rejects this with an
/// <see cref="InvalidOperationException"/>. This converter tolerates the deviation
/// and returns an empty dictionary for any non-struct value.
/// </remarks>
public class ParamSetDictionaryValueConverter : IXmlRpcMemberValueConverter
{
    /// <summary>
    /// Converts an <see cref="XmlRpcValue"/> into a <see cref="Dictionary{TKey,TValue}"/>.
    /// </summary>
    /// <param name="xmlRpcValue">The XML-RPC value to convert.</param>
    /// <returns>
    /// A dictionary mapping member names to their underlying data when
    /// <paramref name="xmlRpcValue"/> is a <see cref="StructValue"/>; an empty dictionary
    /// otherwise.
    /// </returns>
    public object ConvertFromValue(XmlRpcValue xmlRpcValue)
    {
        if (xmlRpcValue is StructValue structValue)
        {
            return structValue.Value
                .ToDictionary(member => member.Key, member => member.Value.Data);
        }

        return new Dictionary<string, object>();
    }

    /// <summary>
    /// Converts a dictionary value into an <see cref="XmlRpcValue"/>.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>This method is not implemented and always throws <see cref="NotImplementedException"/>.</returns>
    /// <exception cref="NotImplementedException">Always thrown; serialization of paramset dictionaries is not supported.</exception>
    public XmlRpcValue ConvertFromObject(object value)
    {
        throw new NotImplementedException();
    }
}
