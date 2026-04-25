using System;
using System.Collections.Generic;
using CreativeCoders.Net.XmlRpc.Definition;
using CreativeCoders.Net.XmlRpc.Model;
using CreativeCoders.Net.XmlRpc.Model.Values;

namespace CreativeCoders.HomeMatic.XmlRpc.Converters;

/// <summary>
/// Converts the space-separated link role strings returned by the HomeMatic CCU into a string array, and vice versa.
/// </summary>
/// <remarks>
/// The CCU encodes <c>LINK_SOURCE_ROLES</c> and <c>LINK_TARGET_ROLES</c> in a device description as a
/// single space-separated string (e.g. <c>"SWITCH DIMMER"</c>). This converter splits that string into
/// individual role identifiers on deserialization and joins them back on serialization.
/// </remarks>
public class LinkRolesValueConverter : IXmlRpcMemberValueConverter
{
    /// <summary>
    /// Converts a space-separated roles string from an <see cref="XmlRpcValue"/> into a <see langword="string"/> array.
    /// </summary>
    /// <param name="xmlRpcValue">The XML-RPC value containing the space-separated role names.</param>
    /// <returns>An array of individual role name strings, or <see langword="null"/> if the value has no string content.</returns>
    public object? ConvertFromValue(XmlRpcValue xmlRpcValue)
    {
        var text = xmlRpcValue.GetValue<string>();

        return text?.Split([" "], StringSplitOptions.RemoveEmptyEntries);
    }

    /// <summary>
    /// Converts a collection of role name strings into a space-separated <see cref="XmlRpcValue"/>.
    /// </summary>
    /// <param name="value">The role name collection to convert.</param>
    /// <returns>A <see cref="StringValue"/> with the roles joined by spaces, or an empty <see cref="StringValue"/> if the value is not a string collection.</returns>
    public XmlRpcValue ConvertFromObject(object value)
    {
        if (value is IEnumerable<string> textItems)
        {
            return new StringValue(string.Join(" ", textItems));
        }

        return new StringValue(string.Empty);
    }
}
