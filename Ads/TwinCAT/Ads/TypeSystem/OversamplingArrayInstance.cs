namespace TwinCAT.Ads.TypeSystem
{
    using System;
    using System.Runtime.InteropServices;
    using TwinCAT.Ads;
    using TwinCAT.Ads.Internal;
    using TwinCAT.TypeSystem;

    internal sealed class OversamplingArrayInstance : ArrayInstance, IOversamplingArrayInstance, IArrayInstance, ISymbol, IAttributedInstance, IInstance, IBitSize
    {
        internal OversamplingArrayInstance(Member member, ISymbol parent) : base(member, parent)
        {
            base.category = DataTypeCategory.Array;
        }

        internal OversamplingArrayInstance(int[] currentIndex, bool oversample, ISymbol parent) : base(currentIndex, oversample, parent)
        {
            base.category = DataTypeCategory.Array;
        }

        internal OversamplingArrayInstance(AdsSymbolEntry entry, IArrayType type, ISymbol parent, ISymbolFactoryServices factoryServices) : base(entry, type, parent, factoryServices)
        {
            base.category = DataTypeCategory.Array;
        }

        internal OversamplingArrayInstance(ISymbol parent, IArrayType type, string instanceName, int fieldOffset) : base(parent, type, instanceName, fieldOffset)
        {
            base.category = DataTypeCategory.Array;
        }

        internal override ISymbolCollection OnCreateSubSymbols(ISymbol parentInstance)
        {
            ISymbolCollection symbols = base.OnCreateSubSymbols(parentInstance);
            try
            {
                ArrayType dataType = (ArrayType) base.DataType;
                ISymbolFactoryOversampled symbolFactory = (ISymbolFactoryOversampled) this.FactoryServices.SymbolFactory;
            }
            catch (Exception exception)
            {
                Module.Trace.TraceError(exception);
            }
            return symbols;
        }

        protected override ReadOnlySymbolCollection OnGetElements()
        {
            SymbolCollection symbols2 = new SymbolCollection(this.SubSymbolsInternal);
            symbols2.RemoveAt(symbols2.Count - 1);
            return symbols2.AsReadOnly();
        }

        internal override int OnGetSubSymbolCount(ISymbol parentSymbol) => 
            (base.OnGetSubSymbolCount(parentSymbol) + 1);

        public bool TryGetOversamplingElement(out ISymbol symbol)
        {
            if (!((ArrayType) base.DataType).IsOversampled)
            {
                symbol = null;
                return false;
            }
            ReadOnlySymbolCollection subSymbols = base.SubSymbols;
            symbol = subSymbols[subSymbols.Count - 1];
            return true;
        }

        public ISymbol OversamplingElement
        {
            get
            {
                ISymbol symbol = null;
                return (!this.TryGetOversamplingElement(out symbol) ? null : symbol);
            }
        }
    }
}

