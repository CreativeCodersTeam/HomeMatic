using System.Collections.Generic;
using CreativeCoders.HomeMatic.Core.Parameters;
using JetBrains.Annotations;

namespace CreativeCoders.HomeMatic.Api.Core.Parameters
{
    [PublicAPI]
    public interface ICcuParameterInfo
    {
        string Id { get; }
        
        object DefaultValue { get; }
        
        object MinValue { get; }
        
        object MaxValue { get; }
        
        string Type { get; }
        
        ParameterDataType DataType { get; }
        
        string Unit { get; }
        
        int TabOrder { get; }
        
        string Control { get; }
        
        IEnumerable<string> ValuesList { get; }
        
        IEnumerable<Dictionary<string, object>> SpecialValues { get; }
    }
}