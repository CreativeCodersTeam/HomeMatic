using System.Collections.Generic;
using CreativeCoders.HomeMatic.Core.Parameters;
using JetBrains.Annotations;

namespace CreativeCoders.HomeMatic.Api.Parameters
{
    [UsedImplicitly]
    public class CcuParameterInfo : ICcuParameterInfo
    {
        public string Id { get; set; }
        
        public object DefaultValue { get; set; }
        
        public object MinValue { get; set; }
        
        public object MaxValue { get; set; }
        
        public string Type { get; set; }
        
        public ParameterDataType DataType { get; set; }
        
        public string Unit { get; set; }
        
        public int TabOrder { get; set; }
        
        public string Control { get; set; }
        
        public IEnumerable<string> ValuesList { get; set; }
        
        public IEnumerable<Dictionary<string, object>> SpecialValues { get; set; }
    }
}