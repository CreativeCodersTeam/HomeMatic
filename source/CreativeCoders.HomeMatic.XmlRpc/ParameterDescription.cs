using System.Collections.Generic;
using CreativeCoders.HomeMatic.Core.Parameters;
using CreativeCoders.HomeMatic.XmlRpc.Converters;
using CreativeCoders.Net.XmlRpc.Definition;
using JetBrains.Annotations;

namespace CreativeCoders.HomeMatic.XmlRpc
{
    [PublicAPI]
    public class ParameterDescription
    {
        [XmlRpcStructMember("ID")]
        public string Id { get; set; }
        
        [XmlRpcStructMember("DEFAULT")]
        public object DefaultValue { get; set; }

        [XmlRpcStructMember("MIN")]
        public object MinValue { get; set; }

        [XmlRpcStructMember("MAX")]
        public object MaxValue { get; set; }

        [XmlRpcStructMember("TYPE")]
        public string Type { get; set; }

        [XmlRpcStructMember("TYPE", Converter = typeof(ParameterDataTypeValueConverter))]
        public ParameterDataType DataType { get; set; }

        [XmlRpcStructMember("UNIT", DefaultValue = "")]
        public string Unit { get; set; }

        [XmlRpcStructMember("TAB_ORDER")]
        public int TabOrder { get; set; }

        [XmlRpcStructMember("CONTROL", DefaultValue = "")]
        public string Control { get; set; }

        [XmlRpcStructMember("VALUE_LIST", DefaultValue = new string[0])]
        public IEnumerable<string> ValuesList { get; set; }

        [XmlRpcStructMember("SPECIAL")]
        public IEnumerable<Dictionary<string, object>> SpecialValues { get; set; } = new Dictionary<string, object>[0];
    }
}