namespace TwinCAT.TypeSystem
{
    using System;

    [Flags]
    public enum MethodParamFlags : uint
    {
        In = 1,
        Out = 2,
        ByReference = 4,
        MaskIn = 5,
        MaskOut = 6
    }
}

