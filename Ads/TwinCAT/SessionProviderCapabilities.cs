namespace TwinCAT
{
    using System;

    [Flags]
    public enum SessionProviderCapabilities
    {
        DataTypeSupport = 1,
        SymbolBrowsing = 2,
        ValueRead = 4,
        ValueWrite = 8,
        ValueNotifications = 0x10,
        None = 0,
        Mask_All = 0x1f
    }
}

