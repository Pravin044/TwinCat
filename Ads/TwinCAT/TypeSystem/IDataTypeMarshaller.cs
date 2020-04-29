namespace TwinCAT.TypeSystem
{
    using System;
    using System.Runtime.InteropServices;
    using System.Text;

    public interface IDataTypeMarshaller
    {
        byte[] Marshal(IDataType type, Encoding encoding, object value);
        int Marshal(IDataType type, Encoding encoding, object value, byte[] bytes, int offset);
        int MarshalSize(IDataType type, Encoding encoding, object value);
        bool TryGetManagedType(IDataType type, out Type managed);
        int Unmarshal(IDataType type, Encoding encoding, byte[] data, int offset, out object value);
    }
}

