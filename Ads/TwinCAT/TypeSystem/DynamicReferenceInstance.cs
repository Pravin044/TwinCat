namespace TwinCAT.TypeSystem
{
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Runtime.InteropServices;

    public sealed class DynamicReferenceInstance : DynamicSymbol, IReferenceInstanceAccess, IReferenceInstance, ISymbol, IAttributedInstance, IInstance, IBitSize
    {
        private IDataType resolvedReferenceType;
        private Dictionary<string, ISymbol> normalizedDict;

        internal DynamicReferenceInstance(IReferenceInstance refInstance) : base((IValueSymbol) refInstance)
        {
            this.normalizedDict = new Dictionary<string, ISymbol>(StringComparer.OrdinalIgnoreCase);
            IReferenceType dataType = (IReferenceType) refInstance.DataType;
            IResolvableType type2 = (IResolvableType) dataType;
            this.resolvedReferenceType = (type2 == null) ? dataType : type2.ResolveType(DataTypeResolveStrategy.AliasReference);
            if (this.resolvedReferenceType.Category == DataTypeCategory.Struct)
            {
                ReadOnlySymbolCollection subSymbols = ((IReferenceInstanceAccess) refInstance).SubSymbols;
                this.normalizedDict = DynamicStructInstance.createMemberDictionary(false, subSymbols);
            }
        }

        public override IEnumerable<string> GetDynamicMemberNames() => 
            this.normalizedDict.Keys;

        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
        {
            if (this.resolvedReferenceType.Category != DataTypeCategory.Array)
            {
                return base.TryGetIndex(binder, indexes, ref result);
            }
            IArrayType resolvedReferenceType = (IArrayType) this.resolvedReferenceType;
            return DynamicArrayInstance.TryGetIndex(this, resolvedReferenceType, binder, indexes, out result);
        }

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

        public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value) => 
            base.TrySetIndex(binder, indexes, value);

        public override bool TrySetMember(SetMemberBinder binder, object value) => 
            base.TrySetMember(binder, value);

        bool IReferenceInstanceAccess.TryGetElement(IList<int[]> jaggedIndices, out ISymbol symbol) => 
            ((IReferenceInstanceAccess) base.symbol).TryGetElement(jaggedIndices, out symbol);

        bool IReferenceInstanceAccess.TryGetElement(int[] indices, out ISymbol symbol) => 
            ((IReferenceInstanceAccess) base.symbol).TryGetElement(indices, out symbol);

        DataTypeCategory IReferenceInstance.ResolvedCategory =>
            ((IReferenceInstance) base.symbol).ResolvedCategory;

        int IReferenceInstance.ResolvedByteSize =>
            this.resolvedReferenceType.ByteSize;

        IDataType IReferenceInstance.ResolvedType =>
            this.resolvedReferenceType;

        IDataType IReferenceInstance.ReferencedType =>
            ((IReferenceInstance) base.symbol).ReferencedType;
    }
}

