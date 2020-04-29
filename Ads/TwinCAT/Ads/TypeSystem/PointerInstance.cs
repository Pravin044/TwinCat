namespace TwinCAT.Ads.TypeSystem
{
    using System;
    using TwinCAT.Ads;
    using TwinCAT.Ads.Internal;
    using TwinCAT.TypeSystem;

    internal sealed class PointerInstance : Symbol, IPointerInstance, ISymbol, IAttributedInstance, IInstance, IBitSize
    {
        internal PointerInstance(Member member, ISymbol parent) : base(member, parent)
        {
            base.category = DataTypeCategory.Pointer;
        }

        internal PointerInstance(int[] currentIndex, bool oversample, ISymbol parent) : base(currentIndex, oversample, parent)
        {
            base.category = DataTypeCategory.Pointer;
        }

        internal PointerInstance(AdsSymbolEntry entry, IPointerType type, ISymbol parent, ISymbolFactoryServices factoryServices) : base(entry, type, parent, factoryServices)
        {
            base.category = DataTypeCategory.Pointer;
        }

        internal PointerInstance(ISymbol parent, PointerType type, string instanceName, int fieldOffset) : base(parent, type, instanceName, fieldOffset, ((ISymbolFactoryServicesProvider) parent).FactoryServices)
        {
            base.category = DataTypeCategory.Pointer;
        }

        internal override ISymbolCollection OnCreateSubSymbols(ISymbol parentInstance)
        {
            ISymbolCollection symbols = base.OnCreateSubSymbols(parentInstance);
            try
            {
                ISymbol item = this.FactoryServices.SymbolFactory.CreateReferenceInstance((IPointerType) base.DataType, parentInstance);
                if ((item != null) && (this.IsAnySizeArray || (item.Size > 0)))
                {
                    symbols.Add(item);
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
            IDataType referencedType = ((IPointerType) base.DataType).ReferencedType;
            if (this.IsAnySizeArray || (referencedType.Size > 0))
            {
                return 1;
            }
            return 0;
        }

        public override bool IsPrimitiveType =>
            base.IsPrimitiveType;

        public override bool IsContainerType =>
            true;

        internal bool IsAnySizeArray =>
            ((base.MemberFlags & AdsDataTypeFlags.AnySizeArray) == AdsDataTypeFlags.AnySizeArray);

        public ISymbol Reference =>
            base.SubSymbols[0];
    }
}

