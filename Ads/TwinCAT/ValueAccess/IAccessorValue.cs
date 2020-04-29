namespace TwinCAT.ValueAccess
{
    using System;
    using System.ComponentModel;
    using System.Runtime.InteropServices;
    using TwinCAT.TypeSystem;

    [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
    public interface IAccessorValue : IAccessorRawValue
    {
        object ReadValue(ISymbol symbol, out DateTime utcReadTime);
        int TryReadValue(ISymbol symbol, out object value, out DateTime utcReadTime);
        int TryWriteValue(ISymbol symbol, object value, out DateTime utcWriteTime);
        void WriteValue(ISymbol symbol, object value, out DateTime utcWriteTime);
    }
}

