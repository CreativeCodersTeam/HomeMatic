using System;
using JetBrains.Annotations;

namespace CreativeCoders.HomeMatic.Core.Parameters;

[PublicAPI]
[Flags]
public enum ParameterFlags
{
    None = 0,
    Visible = 1,
    Internal = 2,
    Transform = 4,
    Service = 8,
    Sticky = 16
}