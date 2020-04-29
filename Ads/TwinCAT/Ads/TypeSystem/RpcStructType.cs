namespace TwinCAT.Ads.TypeSystem
{
    using System;
    using System.ComponentModel;
    using TwinCAT.Ads.Internal;
    using TwinCAT.TypeSystem;

    public sealed class RpcStructType : StructType, IRpcCallableType
    {
        private RpcMethodCollection _rpcMethods;

        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        internal RpcStructType(AdsDataTypeEntry entry) : base(entry)
        {
            this._rpcMethods = new RpcMethodCollection(entry.methods);
        }

        public ReadOnlyRpcMethodCollection RpcMethods =>
            this._rpcMethods.AsReadOnly();

        public override bool HasRpcMethods =>
            true;
    }
}

