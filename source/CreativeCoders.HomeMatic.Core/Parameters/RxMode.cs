using System;
using JetBrains.Annotations;

namespace CreativeCoders.HomeMatic.Core.Parameters;

[PublicAPI]
[Flags]
public enum RxMode
{
    None = 0,
    Always = 1,
    Burst = 2,
    Config = 4,
    WakeUp = 8,
    LazyConfig = 16
}