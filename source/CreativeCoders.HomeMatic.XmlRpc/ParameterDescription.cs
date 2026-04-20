using System.Collections.Generic;
using CreativeCoders.HomeMatic.XmlRpc.Parameters;
using CreativeCoders.HomeMatic.XmlRpc.Converters;
using CreativeCoders.Net.XmlRpc.Definition;
using JetBrains.Annotations;

namespace CreativeCoders.HomeMatic.XmlRpc;

/// <summary>
/// Describes a single parameter within a HomeMatic parameter set.
/// </summary>
/// <remarks>
/// A <c>ParameterDescription</c> is returned as part of the result of
/// <see cref="Client.IHomeMaticXmlRpcApi.GetParameterDescriptionAsync"/>. It provides metadata
/// about a device parameter such as its data type, allowed value range, unit, and display hints
/// for user interfaces.
/// <para>
/// For <c>ENUM</c> parameters, <see cref="ValuesList"/> contains the symbolic names for each possible
/// integer value. For <c>FLOAT</c> and <c>INTEGER</c> parameters, <see cref="SpecialValues"/> may
/// contain discrete values with special meaning outside the normal range.
/// </para>
/// </remarks>
[PublicAPI]
public class ParameterDescription
{
    /// <summary>
    /// Gets or sets the identifier of this parameter within its parameter set.
    /// </summary>
    /// <value>The parameter name (e.g. <c>SET_TEMPERATURE</c>), or <see langword="null"/> if not provided.</value>
    [XmlRpcStructMember("ID")]
    public string? Id { get; set; }

    /// <summary>
    /// Gets or sets the default value of the parameter.
    /// </summary>
    /// <value>The default value; the data type matches the parameter's <see cref="DataType"/>.</value>
    [XmlRpcStructMember("DEFAULT")]
    public object? DefaultValue { get; set; }

    /// <summary>
    /// Gets or sets the minimum allowed value of the parameter.
    /// </summary>
    /// <value>The minimum value; the data type matches the parameter's <see cref="DataType"/>.</value>
    [XmlRpcStructMember("MIN")]
    public object? MinValue { get; set; }

    /// <summary>
    /// Gets or sets the maximum allowed value of the parameter.
    /// </summary>
    /// <value>The maximum value; the data type matches the parameter's <see cref="DataType"/>.</value>
    [XmlRpcStructMember("MAX")]
    public object? MaxValue { get; set; }

    /// <summary>
    /// Gets or sets the raw type string of the parameter as returned by the XML-RPC interface.
    /// </summary>
    /// <value>
    /// One of <c>FLOAT</c>, <c>INTEGER</c>, <c>BOOL</c>, <c>ENUM</c>, <c>STRING</c>, or <c>ACTION</c>;
    /// or <see langword="null"/> if not provided.
    /// </value>
    [XmlRpcStructMember("TYPE")]
    public string? Type { get; set; }

    /// <summary>
    /// Gets or sets the data type of the parameter as a typed enum value.
    /// </summary>
    /// <value>One of the <see cref="ParameterDataType"/> enumeration values.</value>
    [XmlRpcStructMember("TYPE", Converter = typeof(ParameterDataTypeValueConverter))]
    public ParameterDataType DataType { get; set; }

    /// <summary>
    /// Gets or sets the unit of measurement for this parameter.
    /// </summary>
    /// <value>The unit string (e.g. <c>°C</c>, <c>%</c>), or <see langword="null"/> if not applicable.</value>
    [XmlRpcStructMember("UNIT", DefaultValue = "")]
    public string? Unit { get; set; }

    /// <summary>
    /// Gets or sets the display order of this parameter within its parameter set.
    /// </summary>
    /// <value>An integer that specifies the sort order for UI display.</value>
    [XmlRpcStructMember("TAB_ORDER")]
    public int TabOrder { get; set; }

    /// <summary>
    /// Gets or sets the UI control hint for rendering this parameter.
    /// </summary>
    /// <value>
    /// A string of the form <c>ControlName.VariableName:ControlIndex</c> that hints which UI control
    /// should be used to display the parameter value; or <see langword="null"/> if not specified.
    /// </value>
    [XmlRpcStructMember("CONTROL", DefaultValue = "")]
    public string? Control { get; set; }

    /// <summary>
    /// Gets or sets the list of symbolic names for each possible value of an <c>ENUM</c> parameter.
    /// </summary>
    /// <value>
    /// An ordered collection of strings where the index corresponds to the numeric enum value.
    /// An empty string at a given index means that value is not defined.
    /// Empty for non-enum parameters.
    /// </value>
    [XmlRpcStructMember("VALUE_LIST", DefaultValue = new string[0])]
    public IEnumerable<string> ValuesList { get; set; } = [];

    /// <summary>
    /// Gets or sets the collection of discrete special values with meaning outside the normal value range.
    /// </summary>
    /// <value>
    /// Each element is a dictionary with <c>ID</c> (string identifier) and <c>VALUE</c> (the special value).
    /// Applicable to <c>FLOAT</c> and <c>INTEGER</c> parameters only.
    /// </value>
    [XmlRpcStructMember("SPECIAL")]
    public IEnumerable<Dictionary<string, object>> SpecialValues { get; set; } = [];
}
