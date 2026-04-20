using CreativeCoders.HomeMatic.Core.Devices;
using CreativeCoders.HomeMatic.XmlRpc.Parameters;

namespace CreativeCoders.HomeMatic.Tests.Exporting;

internal sealed class ParamSetValuesBuilder
{
    private readonly List<ParamSetValueWithDescription> _values = [];

    public ParamSetValuesBuilder Add(string name, object value, string? descriptionId = null)
    {
        _values.Add(new ParamSetValueWithDescription
        {
            ParamSetValue = new ParamSetValue { Name = name, Value = value },
            Description = new CcuParameterDescription
            {
                Id = descriptionId,
                DefaultValue = null,
                MinValue = null,
                MaxValue = null,
                Type = null,
                DataType = ParameterDataType.Float,
                Unit = null,
                TabOrder = 0,
                Control = null,
                ValuesList = [],
                SpecialValues = []
            }
        });

        return this;
    }

    public IEnumerable<ParamSetValueWithDescription> Build()
    {
        return _values;
    }
}
