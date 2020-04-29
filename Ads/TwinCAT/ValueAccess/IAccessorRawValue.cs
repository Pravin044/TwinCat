namespace TwinCAT.ValueAccess
{
    using System;
    using System.ComponentModel;
    using System.Runtime.InteropServices;
    using TwinCAT.TypeSystem;

    [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
    public interface IAccessorRawValue
    {
        int TryReadArrayElementValue(ISymbol arrayInstance, int[] indices, out byte[] value, out DateTime utcReadTime);
        int TryReadValue(ISymbol symbolInstance, out byte[] value, out DateTime utcReadTime);
        int TryWriteArrayElementValue(ISymbol arrayInstance, int[] indices, byte[] value, int offset, out DateTime utcWriteTime);
        int TryWriteValue(ISymbol symbolInstance, byte[] value, int offset, out DateTime utcWriteTime);

        IAccessorValueFactory ValueFactory { get; }
    }
}

