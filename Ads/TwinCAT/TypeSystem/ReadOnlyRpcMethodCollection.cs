namespace TwinCAT.TypeSystem
{
    using System;
    using System.Collections.ObjectModel;
    using System.Reflection;
    using System.Runtime.InteropServices;

    public class ReadOnlyRpcMethodCollection : ReadOnlyCollection<IRpcMethod>
    {
        internal ReadOnlyRpcMethodCollection(RpcMethodCollection coll) : base(coll)
        {
        }

        public bool Contains(string methodName) => 
            ((RpcMethodCollection) base.Items).Contains(methodName);

        public bool TryGetMethod(int vTableIndex, out IRpcMethod method) => 
            ((RpcMethodCollection) base.Items).TryGetMethod(vTableIndex, out method);

        public bool TryGetMethod(string methodName, out IRpcMethod method) => 
            ((RpcMethodCollection) base.Items).TryGetMethod(methodName, out method);

        internal static ReadOnlyRpcMethodCollection Empty =>
            new ReadOnlyRpcMethodCollection(RpcMethodCollection.Empty);

        public IRpcMethod this[string methodName] =>
            ((RpcMethodCollection) base.Items)[methodName];
    }
}

