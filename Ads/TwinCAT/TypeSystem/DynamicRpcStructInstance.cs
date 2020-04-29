namespace TwinCAT.TypeSystem
{
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Runtime.InteropServices;

    public sealed class DynamicRpcStructInstance : DynamicStructInstance, IRpcStructInstance, IStructInstance, ISymbol, IAttributedInstance, IInstance, IBitSize, IRpcCallableInstance
    {
        internal DynamicRpcStructInstance(IRpcStructInstance structInstance) : base(structInstance)
        {
        }

        public override IEnumerable<string> GetDynamicMemberNames()
        {
            List<string> list = new List<string>(base.GetDynamicMemberNames());
            foreach (IRpcMethod method in ((IRpcCallableType) base.DataType).RpcMethods)
            {
                list.Add(method.Name);
            }
            return list;
        }

        public object InvokeRpcMethod(string methodName, object[] parameters) => 
            ((IRpcCallableInstance) base.symbol).InvokeRpcMethod(methodName, parameters);

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            IRpcMethod method = null;
            if (!((IRpcCallableType) ((IRpcStructInstance) base.symbol).DataType).RpcMethods.TryGetMethod(binder.Name, out method))
            {
                return base.TryGetMember(binder, out result);
            }
            result = method;
            return true;
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            IRpcStructInstance symbol = (IRpcStructInstance) base.symbol;
            IRpcMethod method = null;
            if (!((IRpcCallableType) symbol.DataType).RpcMethods.TryGetMethod(binder.Name, out method))
            {
                return base.TryInvokeMember(binder, args, ref result);
            }
            result = symbol.InvokeRpcMethod(binder.Name, args);
            return true;
        }

        public int TryInvokeRpcMethod(string methodName, object[] args, out object result)
        {
            IRpcStructInstance symbol = (IRpcStructInstance) base.symbol;
            IRpcCallableType dataType = (IRpcCallableType) symbol.DataType;
            return symbol.TryInvokeRpcMethod(methodName, args, out result);
        }

        public int TryInvokeRpcMethod(IRpcMethod method, object[] args, out object result)
        {
            IRpcStructInstance symbol = (IRpcStructInstance) base.symbol;
            IRpcCallableType dataType = (IRpcCallableType) symbol.DataType;
            return symbol.TryInvokeRpcMethod(method, args, out result);
        }

        public override bool TrySetMember(SetMemberBinder binder, object value) => 
            base.TrySetMember(binder, value);

        public ReadOnlyRpcMethodCollection RpcMethods
        {
            get
            {
                IRpcStructInstance symbol = (IRpcStructInstance) base.symbol;
                IRpcCallableType dataType = (IRpcCallableType) symbol.DataType;
                return symbol.RpcMethods;
            }
        }
    }
}

