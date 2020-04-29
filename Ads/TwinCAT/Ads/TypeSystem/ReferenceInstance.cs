namespace TwinCAT.Ads.TypeSystem
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using TwinCAT.Ads;
    using TwinCAT.Ads.Internal;
    using TwinCAT.TypeSystem;
    using TwinCAT.TypeSystem.Generic;

    internal sealed class ReferenceInstance : Symbol, IReferenceInstanceAccess, IReferenceInstance, ISymbol, IAttributedInstance, IInstance, IBitSize
    {
        internal ReferenceInstance(Member member, ISymbol parent) : base(member, parent)
        {
            base.category = DataTypeCategory.Reference;
        }

        internal ReferenceInstance(int[] currentIndex, bool oversample, ISymbol parent) : base(currentIndex, oversample, parent)
        {
            base.category = DataTypeCategory.Reference;
        }

        internal ReferenceInstance(AdsSymbolEntry entry, IReferenceType type, ISymbol parent, ISymbolFactoryServices factoryServices) : base(entry, type, parent, factoryServices)
        {
            base.category = DataTypeCategory.Reference;
        }

        internal ReferenceInstance(ISymbol parent, IReferenceType type, string instanceName, int fieldOffset) : base(parent, type, instanceName, fieldOffset, ((ISymbolFactoryServicesProvider) parent).FactoryServices)
        {
            base.category = DataTypeCategory.Reference;
        }

        internal override ISymbolCollection OnCreateSubSymbols(ISymbol parentInstance)
        {
            ISymbolCollection symbols = base.OnCreateSubSymbols(parentInstance);
            try
            {
                DataType type2 = (DataType) ((ReferenceType) base.DataType).ResolveType(DataTypeResolveStrategy.AliasReference);
                if (type2 == null)
                {
                    object[] args = new object[] { parentInstance.InstancePath };
                    Module.Trace.TraceWarning("Cannot resolve reference type '{0}'!", args);
                }
                else if (type2.Size > 0)
                {
                    if (type2.Category == DataTypeCategory.Struct)
                    {
                        StructType parentType = (StructType) type2;
                        symbols = (parentType == null) ? new SymbolCollection(InstanceCollectionMode.Names) : this.FactoryServices.SymbolFactory.CreateFieldInstances(this, parentType);
                    }
                    else if (type2.Category == DataTypeCategory.Array)
                    {
                        ArrayType arrayType = (ArrayType) type2;
                        symbols = this.FactoryServices.SymbolFactory.CreateArrayElementInstances(parentInstance, arrayType);
                    }
                    else if (type2.Category == DataTypeCategory.Union)
                    {
                        UnionType parentType = (UnionType) type2;
                        symbols = (parentType == null) ? new SymbolCollection(InstanceCollectionMode.Names) : this.FactoryServices.SymbolFactory.CreateFieldInstances(this, parentType);
                    }
                    else if (type2.Category == DataTypeCategory.Pointer)
                    {
                        PointerType type = (PointerType) type2;
                        ISymbol item = this.FactoryServices.SymbolFactory.CreateReferenceInstance(type, this);
                        if ((item != null) && (type2.Size > 0))
                        {
                            symbols.Add(item);
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Module.Trace.TraceError(exception);
            }
            return symbols;
        }

        internal override int OnGetSubSymbolCount(ISymbol parentSymbol)
        {
            int count = 0;
            DataType type2 = (DataType) ((ReferenceType) base.DataType).ResolveType(DataTypeResolveStrategy.AliasReference);
            if ((type2 != null) && (type2.Size > 0))
            {
                if (type2.Category == DataTypeCategory.Struct)
                {
                    count = ((IStructType) type2).AllMembers.Count;
                }
                else if (type2.Category == DataTypeCategory.Array)
                {
                    count = ((IArrayType) type2).Dimensions.ElementCount;
                }
                else if (type2.Category == DataTypeCategory.Union)
                {
                    count = ((IUnionType) type2).Fields.Count;
                }
                else if (type2.Category == DataTypeCategory.Pointer)
                {
                    count = 1;
                }
            }
            return count;
        }

        bool IReferenceInstanceAccess.TryGetElement(IList<int[]> jaggedIndices, out ISymbol symbol)
        {
            if (jaggedIndices == null)
            {
                throw new ArgumentNullException("jaggedIndices");
            }
            if (this.ResolvedCategory != DataTypeCategory.Array)
            {
                throw new SymbolException($"Cannot dereference indices. The reference symbol '{this.InstancePath}' is not referencing an array type!", this);
            }
            if (((ArrayType) base.ResolveType(DataTypeResolveStrategy.AliasReference)).JaggedLevel != jaggedIndices.Count)
            {
                throw new ArgumentOutOfRangeException("jaggedIndices");
            }
            bool flag = true;
            ISymbol symbol2 = this;
            ISymbol symbol3 = null;
            symbol = null;
            for (int i = 0; i < jaggedIndices.Count; i++)
            {
                if (symbol2 is IReferenceInstanceAccess)
                {
                    flag &= ((IReferenceInstanceAccess) symbol2).TryGetElement(jaggedIndices[i], out symbol3);
                }
                else if (symbol2 is IArrayInstance)
                {
                    flag &= ((IArrayInstance) symbol2).TryGetElement(jaggedIndices[i], out symbol3);
                }
                if (!flag)
                {
                    break;
                }
                symbol2 = (IArrayInstance) symbol3;
            }
            if (flag)
            {
                symbol = symbol3;
            }
            return flag;
        }

        bool IReferenceInstanceAccess.TryGetElement(int[] indices, out ISymbol symbol)
        {
            if (indices == null)
            {
                throw new ArgumentNullException("indices");
            }
            if (this.ResolvedCategory != DataTypeCategory.Array)
            {
                throw new SymbolException($"Cannot dereference indices. The reference symbol '{this.InstancePath}' is not referencing an array type!", this);
            }
            ArrayType type = (ArrayType) base.ResolveType(DataTypeResolveStrategy.AliasReference);
            if (!ArrayType.AreIndicesValid(indices, type, false))
            {
                symbol = null;
                return false;
            }
            int elementPosition = type.GetElementPosition(indices);
            symbol = base.SubSymbols[elementPosition];
            return true;
        }

        public override bool IsPrimitiveType =>
            base.IsPrimitiveType;

        public override bool IsContainerType =>
            ((DataType) ((DataType) base.DataType).ResolveType(DataTypeResolveStrategy.AliasReference)).IsContainer;

        public DataTypeCategory ResolvedCategory
        {
            get
            {
                IDataType resolvedType = this.ResolvedType;
                return ((resolvedType == null) ? DataTypeCategory.Unknown : resolvedType.Category);
            }
        }

        public IDataType ReferencedType =>
            ((ReferenceType) base.DataType).ReferencedType;

        public IDataType ResolvedType
        {
            get
            {
                IDataType type = null;
                ReferenceType dataType = (ReferenceType) base.DataType;
                if (dataType != null)
                {
                    type = dataType.ResolveType(DataTypeResolveStrategy.AliasReference);
                }
                return type;
            }
        }

        public int ResolvedByteSize
        {
            get
            {
                IDataType resolvedType = this.ResolvedType;
                return ((resolvedType == null) ? 0 : resolvedType.ByteSize);
            }
        }
    }
}

