namespace TwinCAT.TypeSystem
{
    using System;

    public enum DataTypeCategory
    {
        Unknown = 0,
        None = 0,
        Primitive = 1,
        Alias = 2,
        Enum = 3,
        Array = 4,
        Struct = 5,
        FunctionBlock = 6,
        Program = 7,
        Function = 8,
        SubRange = 9,
        String = 10,
        Bitset = 12,
        Pointer = 13,
        Union = 14,
        Reference = 15,
        Interface = 0x10
    }
}

