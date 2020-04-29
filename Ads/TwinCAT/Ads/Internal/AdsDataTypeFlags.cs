namespace TwinCAT.Ads.Internal
{
    using System;
    using System.ComponentModel;

    [Flags, EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never), Browsable(false)]
    public enum AdsDataTypeFlags : uint
    {
        DataType = 1,
        DataItem = 2,
        ReferenceTo = 4,
        MethodDeref = 8,
        Oversample = 0x10,
        BitValues = 0x20,
        PropItem = 0x40,
        TypeGuid = 0x80,
        Persistent = 0x100,
        CopyMask = 0x200,
        TComInterfacePtr = 0x400,
        MethodInfos = 0x800,
        Attributes = 0x1000,
        EnumInfos = 0x2000,
        Aligned = 0x10000,
        Static = 0x20000,
        SpLevels = 0x40000,
        IgnorePersist = 0x80000,
        AnySizeArray = 0x100000,
        PersistantDatatype = 0x200000,
        InitOnResult = 0x400000,
        None = 0
    }
}

