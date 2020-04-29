namespace TwinCAT.TypeSystem
{
    using System;

    [Flags]
    public enum SymbolAccessRights
    {
        None = 0,
        Read = 1,
        Write = 2,
        MethodInvoke = 4,
        ReadWrite = 3,
        All = 7
    }
}

