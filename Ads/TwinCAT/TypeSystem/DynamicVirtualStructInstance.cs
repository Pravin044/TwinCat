namespace TwinCAT.TypeSystem
{
    using System;

    public sealed class DynamicVirtualStructInstance : DynamicStructInstance, IVirtualStructInstance, IStructInstance, ISymbol, IAttributedInstance, IInstance, IBitSize
    {
        internal DynamicVirtualStructInstance(IVirtualStructInstance structInstance) : base(structInstance)
        {
        }

        public bool AddMember(ISymbol memberInstance, IVirtualStructInstance parent)
        {
            bool flag = ((IVirtualStructInstance) base.symbol).AddMember(memberInstance, parent);
            string instanceName = memberInstance.InstanceName;
            if (flag)
            {
                IDynamicSymbol symbol = memberInstance as IDynamicSymbol;
                if (symbol != null)
                {
                    instanceName = symbol.NormalizedName;
                }
                base.normalizedDict.Add(instanceName, memberInstance);
            }
            return flag;
        }

        protected override object OnReadAnyValue(Type managedType) => 
            base.OnReadAnyValue(managedType);
    }
}

