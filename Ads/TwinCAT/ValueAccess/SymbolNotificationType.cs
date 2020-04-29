namespace TwinCAT.ValueAccess
{
    using System;

    [Flags]
    public enum SymbolNotificationType
    {
        None,
        Value,
        RawValue,
        Both
    }
}

