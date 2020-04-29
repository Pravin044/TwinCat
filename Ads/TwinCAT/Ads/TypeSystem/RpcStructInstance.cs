namespace TwinCAT.Ads.TypeSystem
{
    using System;
    using System.Runtime.InteropServices;
    using TwinCAT.Ads;
    using TwinCAT.Ads.Internal;
    using TwinCAT.TypeSystem;
    using TwinCAT.ValueAccess;

    internal sealed class RpcStructInstance : StructInstance, IRpcStructInstance, IStructInstance, ISymbol, IAttributedInstance, IInstance, IBitSize, IRpcCallableInstance
    {
        internal RpcStructInstance(Member typedMember, ISymbol parent) : base(typedMember, parent)
        {
        }

        internal RpcStructInstance(int[] currentIndex, bool oversample, ISymbol arrayInstance) : base(currentIndex, oversample, arrayInstance)
        {
        }

        internal RpcStructInstance(AdsSymbolEntry entry, RpcStructType structRpcCallable, ISymbol parent, ISymbolFactoryServices services) : base(entry, structRpcCallable, parent, services)
        {
        }

        internal RpcStructInstance(ISymbol parent, StructType type, string instanceName, int fieldOffset) : base(parent, type, instanceName, fieldOffset)
        {
            base.category = DataTypeCategory.Struct;
        }

        public object InvokeRpcMethod(string methodName, object[] parameters)
        {
            if (string.IsNullOrEmpty(methodName))
            {
                throw new ArgumentOutOfRangeException("methodName");
            }
            if (((IRpcCallableType) base.DataType).RpcMethods[methodName].Parameters.Count != parameters.Length)
            {
                throw new ArgumentException("The parameters are not matching the method prototype.", "parameters");
            }
            object result = null;
            int num = this.TryInvokeRpcMethod(methodName, parameters, out result);
            if (num != 0)
            {
                throw new SymbolException($"Invoke RPC Method failed with error code '{num}'", this);
            }
            return result;
        }

        public int TryInvokeRpcMethod(string methodName, object[] args, out object result)
        {
            IRpcMethod method = null;
            if (!((RpcStructType) base.DataType).RpcMethods.TryGetMethod(methodName, out method))
            {
                throw new ArgumentOutOfRangeException("methodName");
            }
            return this.TryInvokeRpcMethod(method, args, out result);
        }

        public int TryInvokeRpcMethod(IRpcMethod method, object[] args, out object result)
        {
            DateTime time;
            base.EnsureRights(SymbolAccessRights.MethodInvoke);
            return ((IAccessorRpc) base.ValueAccessor).TryInvokeRpcMethod(this, method, args, out result, out time);
        }

        public ReadOnlyRpcMethodCollection RpcMethods =>
            ((RpcStructType) base.DataType).RpcMethods;
    }
}

