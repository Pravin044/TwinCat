namespace TwinCAT.TypeSystem
{
    using System;
    using System.ComponentModel;

    [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
    public interface IManagedMappableType
    {
        Type ManagedType { get; }
    }
}

