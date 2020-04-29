namespace TwinCAT.ValueAccess
{
    using System;
    using System.ComponentModel;
    using System.Runtime.InteropServices;
    using TwinCAT.TypeSystem;

    [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
    public interface IAccessorDynamicValue : IAccessorValue, IAccessorRawValue
    {
        int TryWriteValue(DynamicValue value, out DateTime utcWriteTime);
    }
}

