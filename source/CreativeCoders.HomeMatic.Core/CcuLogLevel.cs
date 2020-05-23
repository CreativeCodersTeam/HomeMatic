using JetBrains.Annotations;

namespace CreativeCoders.HomeMatic.Core
{
    [PublicAPI]
    public enum CcuLogLevel
    {
        All = 0,
        Debug = 1,
        Info = 2,
        Notice = 3,
        Warning = 4,
        Error = 5,
        FatalError = 6
    }
}