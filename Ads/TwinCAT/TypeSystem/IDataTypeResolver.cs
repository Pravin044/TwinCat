namespace TwinCAT.TypeSystem
{
    using System;
    using System.ComponentModel;
    using System.Runtime.InteropServices;

    [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
    public interface IDataTypeResolver
    {
        bool TryResolveType(string name, out IDataType type);

        int PlatformPointerSize { get; }
    }
}

