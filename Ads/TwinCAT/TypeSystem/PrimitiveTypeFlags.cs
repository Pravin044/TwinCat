namespace TwinCAT.TypeSystem
{
    using System;

    [Flags]
    public enum PrimitiveTypeFlags
    {
        None = 0,
        System = 1,
        Unsigned = 2,
        Bool = 4,
        Float = 8,
        Date = 0x10,
        Time = 0x20,
        Numeric = 0x40,
        Bitset = 0x80,
        MaskNumericUnsigned = 0x42,
        MaskDateTime = 0x30
    }
}

