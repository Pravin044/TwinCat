namespace TwinCAT.ValueAccess
{
    using System;
    using System.ComponentModel;
    using System.Runtime.InteropServices;
    using TwinCAT.TypeSystem;

    [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
    internal interface IAccessorValueAny
    {
        int TryReadAnyValue(ISymbol symbol, Type valueType, out object value, out DateTime utcReadTime);
        int TryUpdateAnyValue(ISymbol symbol, ref object valueObject, out DateTime utcReadTime);
        int TryWriteAnyValue(ISymbol symbol, object valueObject, out DateTime utcReadTime);
    }
}

