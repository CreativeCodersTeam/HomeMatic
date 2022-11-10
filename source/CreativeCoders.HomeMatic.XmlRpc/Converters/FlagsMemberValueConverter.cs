using System;
using CreativeCoders.Net.XmlRpc.Definition;
using CreativeCoders.Net.XmlRpc.Model;
using CreativeCoders.Net.XmlRpc.Model.Values;

namespace CreativeCoders.HomeMatic.XmlRpc.Converters;

public class FlagsMemberValueConverter<T> : IXmlRpcMemberValueConverter
{
    public object ConvertFromValue(XmlRpcValue xmlRpcValue)
    {
        if (xmlRpcValue is IntegerValue intValue)
        {
            return (T) intValue.Data;
        }

        return xmlRpcValue.Data;
    }

    public XmlRpcValue ConvertFromObject(object value)
    {
        throw new NotImplementedException();
    }
}