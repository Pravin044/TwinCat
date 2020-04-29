namespace TwinCAT.TypeSystem
{
    using System;
    using System.ComponentModel;
    using System.Runtime.InteropServices;

    [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
    public interface IRpcMarshaller
    {
        int GetInMarshallingSize(IRpcMethod method, object[] parameterValues);
        int GetOutMarshallingSize(IRpcMethod method, object[] outValues);
        int MarshallParameters(IRpcMethod method, object[] parameterValues, byte[] buffer, int offset);
        int UnmarshalOutParameters(IRpcMethod method, byte[] data, int offset, out object[] values);
        int UnmarshalReturnValue(IRpcMethod method, IDataType returnType, byte[] buffer, int offset, out object returnValue);
        int UnmarshalRpcMethod(IRpcMethod method, object[] parameterValues, byte[] data, out object returnValue);
    }
}

