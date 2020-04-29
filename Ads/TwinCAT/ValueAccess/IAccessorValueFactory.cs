namespace TwinCAT.ValueAccess
{
    using System;
    using System.ComponentModel;
    using TwinCAT.TypeSystem;

    [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
    public interface IAccessorValueFactory
    {
        object CreatePrimitiveValue(ISymbol symbol, byte[] rawData, int offset);
        object CreateValue(ISymbol symbol, byte[] rawData, int offset, DateTime utcTime);
        object CreateValue(ISymbol symbol, byte[] rawData, int offset, IValue parent);
    }
}

