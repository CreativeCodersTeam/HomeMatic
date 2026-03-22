using System;
using CreativeCoders.Net.XmlRpc.Definition;
using CreativeCoders.Net.XmlRpc.Model;
using CreativeCoders.Net.XmlRpc.Model.Values;

namespace CreativeCoders.HomeMatic.XmlRpc.Converters;

/// <summary>
/// Converts an XML-RPC integer value representing a bitmask into a flags enum of type <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The flags enum type to convert to.</typeparam>
/// <remarks>
/// Several HomeMatic device description fields (e.g. <c>FLAGS</c>, <c>RX_MODE</c>) are returned as
/// integer bitmasks. This converter casts the raw integer directly to the target enum type so that
/// the individual flag bits can be tested with standard enum / bitwise operations.
/// </remarks>
public class FlagsMemberValueConverter<T> : IXmlRpcMemberValueConverter
{
    /// <summary>
    /// Converts an <see cref="XmlRpcValue"/> containing an integer bitmask into a value of type <typeparamref name="T"/>.
    /// </summary>
    /// <param name="xmlRpcValue">The XML-RPC value to convert.</param>
    /// <returns>The bitmask cast to <typeparamref name="T"/>, or the raw data when the value is not an integer.</returns>
    public object ConvertFromValue(XmlRpcValue xmlRpcValue)
    {
        if (xmlRpcValue is IntegerValue intValue)
        {
            return (T) intValue.Data;
        }

        return xmlRpcValue.Data;
    }

    /// <summary>
    /// Converts a <typeparamref name="T"/> flags value into an <see cref="XmlRpcValue"/>.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>This method is not implemented and always throws <see cref="NotImplementedException"/>.</returns>
    /// <exception cref="NotImplementedException">Always thrown; serialization of flags enums is not supported.</exception>
    public XmlRpcValue ConvertFromObject(object value)
    {
        throw new NotImplementedException();
    }
}