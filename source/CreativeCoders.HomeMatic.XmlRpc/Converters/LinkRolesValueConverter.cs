using System;
using System.Collections.Generic;
using CreativeCoders.Net.XmlRpc.Definition;
using CreativeCoders.Net.XmlRpc.Model;
using CreativeCoders.Net.XmlRpc.Model.Values;

namespace CreativeCoders.HomeMatic.XmlRpc.Converters
{
    public class LinkRolesValueConverter : IXmlRpcMemberValueConverter
    {
        public object ConvertFromValue(XmlRpcValue xmlRpcValue)
        {
            var text = xmlRpcValue.GetValue<string>();

            return text?.Split(new []{" "}, StringSplitOptions.RemoveEmptyEntries);
        }

        public XmlRpcValue ConvertFromObject(object value)
        {
            if (value is IEnumerable<string> textItems)
            {
                return new StringValue(string.Join(" ", textItems));
            }
            
            return new StringValue(string.Empty); 
        }
    }
}