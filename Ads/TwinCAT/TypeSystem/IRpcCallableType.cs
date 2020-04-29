namespace TwinCAT.TypeSystem
{
    public interface IRpcCallableType
    {
        ReadOnlyRpcMethodCollection RpcMethods { get; }
    }
}

