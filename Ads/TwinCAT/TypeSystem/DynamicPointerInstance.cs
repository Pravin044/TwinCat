namespace TwinCAT.TypeSystem
{
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Runtime.InteropServices;

    public sealed class DynamicPointerInstance : DynamicSymbol, IPointerInstance, ISymbol, IAttributedInstance, IInstance, IBitSize
    {
        internal DynamicPointerInstance(IPointerInstance pointerInstance) : base((IValueSymbol) pointerInstance)
        {
        }

        public override IEnumerable<string> GetDynamicMemberNames() => 
            new List<string>(base.GetDynamicMemberNames()) { DynamicPointerValue.s_pointerDeref };

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if (StringComparer.OrdinalIgnoreCase.Compare(binder.Name, DynamicPointerValue.s_pointerDeref) != 0)
            {
                return base.TryGetMember(binder, out result);
            }
            result = this.Reference;
            return true;
        }

        public ISymbol Reference =>
            base.SubSymbols[0];
    }
}

