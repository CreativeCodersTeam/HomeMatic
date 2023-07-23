using System;
using System.Collections.Generic;
using CreativeCoders.HomeMatic.Core.Parameters;
using CreativeCoders.Net.XmlRpc.Definition;
using CreativeCoders.Net.XmlRpc.Model;
using CreativeCoders.Net.XmlRpc.Model.Values;
using JetBrains.Annotations;

namespace CreativeCoders.HomeMatic.XmlRpc.Converters
{
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

        public XmlRpcValue ConvertFromObject(object value)
        {
            throw new NotImplementedException();
        }
    }
}