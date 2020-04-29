namespace TwinCAT.ValueAccess
{
    using System;
    using System.ComponentModel;
    using System.Runtime.InteropServices;
    using TwinCAT.TypeSystem;

    [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
    public interface IAccessorRpc
    {
        int TryInvokeRpcMethod(IInstance instance, IRpcMethod method, object[] parameters, out object returnValue, out DateTime utcInvokeTime);
    }
}

