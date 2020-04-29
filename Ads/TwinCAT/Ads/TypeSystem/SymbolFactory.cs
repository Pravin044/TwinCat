namespace TwinCAT.Ads.TypeSystem
{
    using System;
    using System.ComponentModel;
    using TwinCAT.Ads;
    using TwinCAT.Ads.Internal;
    using TwinCAT.TypeSystem;

    [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
    public class SymbolFactory : SymbolFactoryBase, ISymbolFactoryOversampled
    {
        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        public SymbolFactory(bool nonCachedArrayElements) : base(nonCachedArrayElements)
        {
        }

        private ISymbol createArrayElement(int[] currentIndex, bool oversample, ISymbol parent, IArrayType arrayType)
        {
            if (parent == null)
            {
                throw new ArgumentNullException("parent");
            }
            DataType elementType = (DataType) arrayType.ElementType;
            return this.createArrayElement(currentIndex, oversample, parent, (ArrayType) arrayType, elementType);
        }

        private Symbol createArrayElement(int[] currentIndex, bool oversample, ISymbol parent, ArrayType arrType, DataType elementType)
        {
            bool flag = true;
            DataTypeCategory unknown = DataTypeCategory.Unknown;
            if (elementType != null)
            {
                unknown = elementType.Category;
            }
            Symbol symbol = null;
            if (unknown == DataTypeCategory.Bitset)
            {
                symbol = new Symbol(currentIndex, oversample, parent);
            }
            else if (unknown == DataTypeCategory.Primitive)
            {
                symbol = new Symbol(currentIndex, oversample, parent);
            }
            else if (unknown == DataTypeCategory.Array)
            {
                symbol = !arrType.IsOversampled ? new ArrayInstance(currentIndex, false, parent) : new OversamplingArrayInstance(currentIndex, oversample, parent);
            }
            else if (unknown == DataTypeCategory.Struct)
            {
                symbol = !((IStructType) elementType).HasRpcMethods ? new StructInstance(currentIndex, oversample, parent) : new RpcStructInstance(currentIndex, oversample, parent);
            }
            else if (unknown == DataTypeCategory.Union)
            {
                symbol = new UnionInstance(currentIndex, oversample, parent);
            }
            else if (unknown == DataTypeCategory.Reference)
            {
                symbol = new ReferenceInstance(currentIndex, oversample, parent);
            }
            else if (unknown == DataTypeCategory.Pointer)
            {
                symbol = new PointerInstance(currentIndex, oversample, parent);
            }
            else if (unknown == DataTypeCategory.Union)
            {
                symbol = new Symbol(currentIndex, oversample, parent);
            }
            else if (unknown == DataTypeCategory.Enum)
            {
                symbol = new Symbol(currentIndex, oversample, parent);
            }
            else if (unknown == DataTypeCategory.Alias)
            {
                AliasType type3 = (AliasType) elementType;
                symbol = new AliasInstance(currentIndex, oversample, parent);
            }
            else if (unknown != DataTypeCategory.String)
            {
                symbol = new Symbol(currentIndex, oversample, parent);
            }
            else
            {
                IStringType type4 = (IStringType) elementType;
                symbol = new StringInstance(currentIndex, oversample, parent);
            }
            if (flag)
            {
                symbol.SetComment(parent.Comment);
            }
            return symbol;
        }

        public ISymbol CreateOversamplingElement(ISymbol parent)
        {
            if (parent == null)
            {
                throw new ArgumentNullException("parent");
            }
            ArrayType arrayType = (ArrayType) ((DataType) parent.DataType).ResolveType(DataTypeResolveStrategy.AliasReference);
            int[] currentIndex = new int[arrayType.Dimensions.Count];
            for (int i = 0; i < arrayType.Dimensions.Count; i++)
            {
                currentIndex[i] = arrayType.Dimensions[i].ElementCount;
            }
            return this.createArrayElement(currentIndex, true, parent, arrayType);
        }

        protected override IAliasInstance OnCreateAlias(ISymbolInfo entry, IAliasType aliasType, ISymbol parent) => 
            new AliasInstance((AdsSymbolEntry) entry, aliasType, parent, base.services);

        protected override ISymbol OnCreateArrayElement(int[] currentIndex, ISymbol parent, IArrayType arrayType) => 
            this.createArrayElement(currentIndex, false, parent, arrayType);

        protected override IArrayInstance OnCreateArrayInstance(ISymbolInfo entry, IArrayType type, ISymbol parent)
        {
            ArrayType type2 = (ArrayType) type;
            return (!type2.IsOversampled ? new ArrayInstance((AdsSymbolEntry) entry, type2, parent, base.services) : new OversamplingArrayInstance((AdsSymbolEntry) entry, type2, parent, base.services));
        }

        protected override ISymbol OnCreateFieldInstance(IField field, ISymbol parent)
        {
            if (field == null)
            {
                throw new ArgumentNullException("member");
            }
            if (parent == null)
            {
                throw new ArgumentNullException("parent");
            }
            Member member = (Member) field;
            Symbol symbol = null;
            if (member.Category == DataTypeCategory.Unknown)
            {
                object[] args = new object[] { parent.InstancePath, member.InstanceName };
                Module.Trace.TraceWarning("Category of member '{0}.{1}' is Unknown", args);
                symbol = new Symbol(member, parent);
            }
            else if (field.DataType.Category == DataTypeCategory.Array)
            {
                symbol = !((ArrayType) member.DataType).IsOversampled ? new ArrayInstance(member, parent) : new OversamplingArrayInstance(member, parent);
            }
            else if (member.Category == DataTypeCategory.Struct)
            {
                symbol = !((IStructType) member.DataType).HasRpcMethods ? new StructInstance(member, parent) : new RpcStructInstance(member, parent);
            }
            else if (member.Category == DataTypeCategory.Union)
            {
                symbol = new UnionInstance(member, parent);
            }
            else if (member.Category == DataTypeCategory.Reference)
            {
                symbol = new ReferenceInstance(member, parent);
            }
            else if (member.Category == DataTypeCategory.Pointer)
            {
                symbol = new PointerInstance(member, parent);
            }
            else if (member.Category == DataTypeCategory.Alias)
            {
                DataType type4 = (DataType) ((AliasType) member.DataType).ResolveType(DataTypeResolveStrategy.Alias);
                symbol = new AliasInstance(member, parent);
            }
            else if (member.Category == DataTypeCategory.String)
            {
                symbol = new StringInstance(member, parent);
            }
            else
            {
                if (member.Category == DataTypeCategory.Union)
                {
                    object[] args = new object[] { parent.InstancePath, member.InstanceName };
                    Module.Trace.TraceWarning("Category of member '{0}.{1}' is Union. This is not supported yet!", args);
                }
                symbol = new Symbol(member, parent);
            }
            return symbol;
        }

        protected override IPointerInstance OnCreatePointerInstance(ISymbolInfo entry, IPointerType pointerType, ISymbol parent) => 
            new PointerInstance((AdsSymbolEntry) entry, (PointerType) pointerType, parent, base.services);

        protected override ISymbol OnCreatePrimitive(ISymbolInfo entry, IDataType dataType, ISymbol parent) => 
            ((dataType != null) ? new Symbol((AdsSymbolEntry) entry, (DataType) dataType, parent, base.services) : new Symbol((AdsSymbolEntry) entry, parent, base.services));

        protected override ISymbol OnCreateReference(IPointerType type, ISymbol parent)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            if (parent == null)
            {
                throw new ArgumentNullException("parent");
            }
            int fieldOffset = 0;
            ISymbol symbol2 = null;
            DataType referencedType = (DataType) type.ReferencedType;
            bool flag = (referencedType.Flags & AdsDataTypeFlags.AnySizeArray) == AdsDataTypeFlags.AnySizeArray;
            bool flag2 = (((Symbol) parent).MemberFlags & AdsDataTypeFlags.AnySizeArray) == AdsDataTypeFlags.AnySizeArray;
            if ((referencedType == null) || ((referencedType.Size == 0) && !flag2))
            {
                return null;
            }
            string instanceName = parent.InstanceName;
            bool isBitType = type.ReferencedType.IsBitType;
            if (((DataType) parent.DataType).ResolveType(DataTypeResolveStrategy.AliasReference).IsPointer)
            {
                instanceName = instanceName + "^";
            }
            if (referencedType.Category == DataTypeCategory.Struct)
            {
                StructType type3 = (StructType) referencedType;
                symbol2 = !type3.HasRpcMethods ? new StructInstance(parent, type3, instanceName, fieldOffset) : new RpcStructInstance(parent, type3, instanceName, fieldOffset);
            }
            else if (referencedType.Category == DataTypeCategory.Array)
            {
                symbol2 = !flag2 ? new ArrayInstance(parent, (ArrayType) referencedType, instanceName, fieldOffset) : new AnySizeArrayInstance(parent, (ArrayType) referencedType, instanceName, fieldOffset);
            }
            else if (referencedType.Category == DataTypeCategory.Reference)
            {
                symbol2 = new ReferenceInstance(parent, (ReferenceType) referencedType, instanceName, fieldOffset);
            }
            else if (referencedType.Category == DataTypeCategory.Pointer)
            {
                symbol2 = new PointerInstance(parent, (PointerType) referencedType, instanceName, fieldOffset);
            }
            else if (referencedType.Category == DataTypeCategory.Alias)
            {
                AliasType type4 = (AliasType) referencedType;
                DataType type5 = (DataType) type4.ResolveType(DataTypeResolveStrategy.Alias);
                symbol2 = new AliasInstance(parent, type4, instanceName, fieldOffset);
            }
            else if (referencedType.Category == DataTypeCategory.Union)
            {
                symbol2 = new UnionInstance(parent, (UnionType) referencedType, instanceName, fieldOffset);
            }
            else if (referencedType.Category == DataTypeCategory.String)
            {
                symbol2 = new StringInstance(parent, (IStringType) referencedType, instanceName, fieldOffset);
            }
            else
            {
                if (referencedType.Category == DataTypeCategory.Union)
                {
                    Module.Trace.TraceError("Unions not supported yet!");
                }
                symbol2 = new Symbol(parent, referencedType, instanceName, fieldOffset, ((ISymbolFactoryServicesProvider) parent).FactoryServices);
            }
            return symbol2;
        }

        protected override IReferenceInstance OnCreateReferenceInstance(ISymbolInfo entry, IReferenceType referenceType, ISymbol parent) => 
            new ReferenceInstance((AdsSymbolEntry) entry, (ReferenceType) referenceType, parent, base.services);

        protected override ISymbol OnCreateString(ISymbolInfo entry, IStringType stringType, ISymbol parent) => 
            new StringInstance((AdsSymbolEntry) entry, stringType, parent, base.services);

        protected override IStructInstance OnCreateStruct(ISymbolInfo entry, IStructType structType, ISymbol parent) => 
            (!structType.HasRpcMethods ? new StructInstance((AdsSymbolEntry) entry, (StructType) structType, parent, base.services) : new RpcStructInstance((AdsSymbolEntry) entry, (RpcStructType) structType, parent, base.services));

        protected override IUnionInstance OnCreateUnion(ISymbolInfo entry, IUnionType unionType, ISymbol parent) => 
            new UnionInstance((AdsSymbolEntry) entry, unionType, parent, base.services);

        protected override ISymbol OnCreateVirtualStruct(string instanceName, string instancePath, ISymbol parent) => 
            new VirtualStructInstance(instanceName, instancePath, parent, base.services);
    }
}

