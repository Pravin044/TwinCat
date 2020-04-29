namespace TwinCAT.TypeSystem
{
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Runtime.InteropServices;

    public sealed class DynamicUnionInstance : DynamicSymbol, IUnionInstance, ISymbol, IAttributedInstance, IInstance, IBitSize
    {
        private Dictionary<string, ISymbol> normalizedDict;

        internal DynamicUnionInstance(IUnionInstance unionInstance) : base((IValueSymbol) unionInstance)
        {
            this.normalizedDict = new Dictionary<string, ISymbol>(StringComparer.OrdinalIgnoreCase);
            foreach (DynamicSymbol symbol in this.FieldInstances)
            {
                this.normalizedDict.Add(symbol.NormalizedName, symbol);
            }
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

        public ReadOnlySymbolCollection FieldInstances =>
            base.SubSymbols;
    }
}

