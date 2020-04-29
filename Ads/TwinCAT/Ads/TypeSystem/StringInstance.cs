namespace TwinCAT.Ads.TypeSystem
{
    using System;
    using System.Text;
    using TwinCAT.Ads.Internal;
    using TwinCAT.TypeSystem;

    internal sealed class StringInstance : Symbol, IStringInstance, ISymbol, IAttributedInstance, IInstance, IBitSize
    {
        internal StringInstance(Member member, ISymbol parent) : base(member, parent)
        {
            base.category = DataTypeCategory.String;
        }

        internal StringInstance(int[] currentIndex, bool oversample, ISymbol parent) : base(currentIndex, oversample, parent)
        {
            base.category = DataTypeCategory.String;
        }

        internal StringInstance(AdsSymbolEntry entry, IStringType type, ISymbol parent, ISymbolFactoryServices factoryServices) : base(entry, type, parent, factoryServices)
        {
            base.category = DataTypeCategory.String;
        }

        internal StringInstance(ISymbol parent, IStringType type, string instanceName, int fieldOffset) : base(parent, type, instanceName, fieldOffset, ((ISymbolFactoryServicesProvider) parent).FactoryServices)
        {
            base.category = DataTypeCategory.String;
        }

        internal override int OnGetSubSymbolCount(ISymbol parentSymbol) => 
            0;

        public System.Text.Encoding Encoding =>
            ((IStringType) base.DataType).Encoding;

        public bool IsFixedLength =>
            ((IStringType) base.DataType).IsFixedLength;
    }
}

