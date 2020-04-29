namespace TwinCAT.Ads.TypeSystem
{
    using System;
    using TwinCAT.Ads;
    using TwinCAT.Ads.Internal;
    using TwinCAT.TypeSystem;

    internal sealed class AliasInstance : Symbol, IAliasInstance, ISymbol, IAttributedInstance, IInstance, IBitSize
    {
        internal AliasInstance(Member member, ISymbol parent) : base(member, parent)
        {
            base.category = DataTypeCategory.Alias;
        }

        internal AliasInstance(int[] currentIndex, bool oversample, ISymbol parent) : base(currentIndex, oversample, parent)
        {
            base.category = DataTypeCategory.Alias;
        }

        internal AliasInstance(AdsSymbolEntry entry, IAliasType type, ISymbol parent, ISymbolFactoryServices factoryServices) : base(entry, type, parent, factoryServices)
        {
            base.category = DataTypeCategory.Alias;
        }

        internal AliasInstance(ISymbol parent, IAliasType type, string instanceName, int fieldOffset) : base(parent, type, instanceName, fieldOffset, ((ISymbolFactoryServicesProvider) parent).FactoryServices)
        {
            base.category = DataTypeCategory.Alias;
        }

        internal override ISymbolCollection OnCreateSubSymbols(ISymbol parentInstance)
        {
            IDataType type = ((DataType) base.DataType).ResolveType(DataTypeResolveStrategy.AliasReference);
            ISymbolCollection symbols = base.OnCreateSubSymbols(parentInstance);
            try
            {
                ISymbolFactory symbolFactory = this.FactoryServices.SymbolFactory;
                if (type != null)
                {
                    DataTypeCategory category = type.Category;
                    if (category <= DataTypeCategory.Struct)
                    {
                        if (category == DataTypeCategory.Array)
                        {
                            symbols = symbolFactory.CreateArrayElementInstances(parentInstance, (IArrayType) type);
                        }
                        else if (category == DataTypeCategory.Struct)
                        {
                            symbols = symbolFactory.CreateFieldInstances(parentInstance, (IStructType) type);
                        }
                    }
                    else if (category != DataTypeCategory.Pointer)
                    {
                        if (category == DataTypeCategory.Union)
                        {
                            symbols = symbolFactory.CreateFieldInstances(parentInstance, (IUnionType) type);
                        }
                    }
                    else
                    {
                        ISymbol item = symbolFactory.CreateReferenceInstance((IPointerType) type, parentInstance);
                        if (item != null)
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
            int elementCount = 0;
            IDataType type = ((DataType) base.DataType).ResolveType(DataTypeResolveStrategy.AliasReference);
            if (type != null)
            {
                DataTypeCategory category = type.Category;
                if (category <= DataTypeCategory.Struct)
                {
                    if (category == DataTypeCategory.Array)
                    {
                        elementCount = ((IArrayType) type).Dimensions.ElementCount;
                    }
                    else if (category == DataTypeCategory.Struct)
                    {
                        elementCount = ((IStructType) type).AllMembers.Count;
                    }
                }
                else if (category != DataTypeCategory.Pointer)
                {
                    if (category == DataTypeCategory.Union)
                    {
                        elementCount = ((IUnionType) type).Fields.Count;
                    }
                }
                else
                {
                    IDataType referencedType = ((IPointerType) type).ReferencedType;
                    if ((referencedType.Category != DataTypeCategory.Array) && (referencedType.Size == 0))
                    {
                        return 0;
                    }
                    elementCount = 1;
                }
            }
            return elementCount;
        }

        public override bool IsPrimitiveType =>
            ((DataType) ((DataType) base.DataType).ResolveType(DataTypeResolveStrategy.AliasReference)).IsPrimitive;

        public override bool IsContainerType =>
            ((DataType) ((DataType) base.DataType).ResolveType(DataTypeResolveStrategy.AliasReference)).IsContainer;
    }
}

