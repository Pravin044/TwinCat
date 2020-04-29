namespace TwinCAT.TypeSystem.Generic
{
    using System;

    [Flags]
    public enum SymbolIterationMask
    {
        None = 0,
        Structures = 1,
        Arrays = 2,
        Unions = 4,
        Pointer = 8,
        References = 0x10,
        All = 0x1f
    }
}

