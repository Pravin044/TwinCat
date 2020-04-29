namespace TwinCAT.Ads.Internal
{
    using System;
    using System.ComponentModel;

    [Flags, EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never), Browsable(false)]
    public enum AdsSymbolFlags : ushort
    {
        None = 0,
        Persistent = 1,
        BitValue = 2,
        ReferenceTo = 4,
        TypeGuid = 8,
        TComInterfacePtr = 0x10,
        ReadOnly = 0x20,
        ItfMethodAccess = 0x40,
        MethodDeref = 0x80,
        ContextMask = 0xf00,
        Attributes = 0x1000,
        Static = 0x2000,
        InitOnReset = 0x4000,
        ExtendedFlags = 0x8000
    }
}

