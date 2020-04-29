namespace TwinCAT.TypeSystem
{
    using System;
    using System.Runtime.InteropServices;

    public interface ISymbolMarshaller
    {
        byte[] Marshal(IAttributedInstance symbol, object value);
        int Marshal(IAttributedInstance symbol, object value, byte[] bytes, int offset);
        int MarshalSize(IAttributedInstance symbol, object value);
        bool TryGetManagedType(IAttributedInstance type, out Type managed);
        bool TryGetManagedType(IDataType type, out Type managed);
        int Unmarshal(IAttributedInstance type, byte[] data, int offset, out object value);
    }
}

