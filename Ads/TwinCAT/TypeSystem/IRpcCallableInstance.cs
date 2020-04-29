namespace TwinCAT.TypeSystem
{
    using System;
    using System.Runtime.InteropServices;

    public interface IRpcCallableInstance
    {
        object InvokeRpcMethod(string methodName, object[] parameters);
        int TryInvokeRpcMethod(string methodName, object[] args, out object result);
        int TryInvokeRpcMethod(IRpcMethod method, object[] args, out object result);

        ReadOnlyRpcMethodCollection RpcMethods { get; }
    }
}

