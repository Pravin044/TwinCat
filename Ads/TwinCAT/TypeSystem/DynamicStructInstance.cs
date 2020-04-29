namespace TwinCAT.TypeSystem
{
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq;
    using System.Runtime.InteropServices;

    public class DynamicStructInstance : DynamicSymbol, IStructInstance, ISymbol, IAttributedInstance, IInstance, IBitSize
    {
        protected Dictionary<string, ISymbol> normalizedDict;

        internal DynamicStructInstance(IStructInstance structInstance) : base((IValueSymbol) structInstance)
        {
            this.normalizedDict = new Dictionary<string, ISymbol>(StringComparer.OrdinalIgnoreCase);
            this.normalizedDict = createMemberDictionary(structInstance is IProcessImageAddress, structInstance.MemberInstances);
        }

        internal static Dictionary<string, ISymbol> createMemberDictionary(bool allowIGIOAccess, ReadOnlySymbolCollection memberInstances)
        {
            Dictionary<string, ISymbol> dictionary = new Dictionary<string, ISymbol>(StringComparer.OrdinalIgnoreCase);
            List<string> list = new List<string>();
            if (allowIGIOAccess)
            {
                list.Add("IndexGroup");
                list.Add("IndexOffset");
            }
            list.Add(DynamicPointerValue.s_pointerDeref);
            foreach (DynamicSymbol symbol in memberInstances)
            {
                string normalizedName = symbol.NormalizedName;
                if (Enumerable.Contains<string>(list, normalizedName, StringComparer.OrdinalIgnoreCase))
                {
                    normalizedName = normalizedName.Insert(0, "_");
                }
                dictionary.Add(normalizedName, symbol);
            }
            return dictionary;
        }

        public override IEnumerable<string> GetDynamicMemberNames() => 
            this.normalizedDict.Keys;

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            ISymbol symbol = null;
            if (!this.normalizedDict.TryGetValue(binder.Name, out symbol))
            {
                return base.TryGetMember(binder, out result);
            }
            result = symbol;
            return true;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value) => 
            base.TrySetMember(binder, value);

        public ReadOnlySymbolCollection MemberInstances =>
            base.SubSymbols;

        public bool HasRpcMethods =>
            ((IStructInstance) base.symbol).HasRpcMethods;
    }
}

