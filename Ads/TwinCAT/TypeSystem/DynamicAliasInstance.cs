namespace TwinCAT.TypeSystem
{
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Runtime.InteropServices;

    public sealed class DynamicAliasInstance : DynamicSymbol, IAliasInstance, ISymbol, IAttributedInstance, IInstance, IBitSize
    {
        private Dictionary<string, ISymbol> normalizedDict;
        private IDataType resolvedAlias;

        internal DynamicAliasInstance(IAliasInstance aliasInstance) : base((IValueSymbol) aliasInstance)
        {
            IAliasType dataType = (IAliasType) aliasInstance.DataType;
            IResolvableType type2 = dataType as IResolvableType;
            this.resolvedAlias = (type2 == null) ? dataType : type2.ResolveType(DataTypeResolveStrategy.AliasReference);
            if (this.resolvedAlias.Category == DataTypeCategory.Struct)
            {
                this.normalizedDict = new Dictionary<string, ISymbol>(StringComparer.OrdinalIgnoreCase);
                foreach (DynamicSymbol symbol in base.SubSymbols)
                {
                    this.normalizedDict.Add(symbol.NormalizedName, symbol);
                }
            }
        }

        public override IEnumerable<string> GetDynamicMemberNames() => 
            ((this.resolvedAlias.Category != DataTypeCategory.Struct) ? base.GetDynamicMemberNames() : this.normalizedDict.Keys);

        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
        {
            if (this.resolvedAlias.Category != DataTypeCategory.Array)
            {
                return base.TryGetIndex(binder, indexes, ref result);
            }
            IArrayType resolvedAlias = (IArrayType) this.resolvedAlias;
            return DynamicArrayInstance.TryGetIndex(this, resolvedAlias, binder, indexes, out result);
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if (this.resolvedAlias.Category == DataTypeCategory.Struct)
            {
                ISymbol symbol = null;
                if (this.normalizedDict.TryGetValue(binder.Name, out symbol))
                {
                    result = symbol;
                    return true;
                }
            }
            return base.TryGetMember(binder, out result);
        }

        public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value) => 
            base.TrySetIndex(binder, indexes, value);

        public override bool TrySetMember(SetMemberBinder binder, object value) => 
            base.TrySetMember(binder, value);
    }
}

