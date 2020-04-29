namespace TwinCAT.Ads.TypeSystem
{
    using System;
    using TwinCAT.Ads.Internal;
    using TwinCAT.TypeSystem;
    using TwinCAT.TypeSystem.Generic;

    internal sealed class UnionInstance : Symbol, IUnionInstance, ISymbol, IAttributedInstance, IInstance, IBitSize
    {
        internal UnionInstance(Member member, ISymbol parent) : base(member, parent)
        {
            base.category = DataTypeCategory.Union;
        }

        internal UnionInstance(int[] currentIndex, bool oversample, ISymbol parent) : base(currentIndex, oversample, parent)
        {
            base.category = DataTypeCategory.Union;
        }

        internal UnionInstance(string instanceName, string instancePath, ISymbolFactoryServices factoryServices) : base(instanceName, instancePath, factoryServices)
        {
            base.category = DataTypeCategory.Union;
        }

        internal UnionInstance(string instanceName, ISymbol parent, ISymbolFactoryServices factoryServices) : base(instanceName, parent, factoryServices)
        {
            base.category = DataTypeCategory.Union;
        }

        internal UnionInstance(AdsSymbolEntry entry, IUnionType type, ISymbol parent, ISymbolFactoryServices factoryServices) : base(entry, type, parent, factoryServices)
        {
            base.category = DataTypeCategory.Union;
        }

        internal UnionInstance(ISymbol parent, IUnionType type, string instanceName, int fieldOffset) : base(parent, type, instanceName, fieldOffset, ((ISymbolFactoryServicesProvider) parent).FactoryServices)
        {
            base.category = DataTypeCategory.Union;
        }

        internal override ISymbolCollection OnCreateSubSymbols(ISymbol parentInstance)
        {
            ISymbolCollection symbols = null;
            try
            {
                UnionType dataType = (UnionType) base.DataType;
                symbols = (dataType == null) ? new SymbolCollection(InstanceCollectionMode.Names) : this.FactoryServices.SymbolFactory.CreateFieldInstances(this, dataType);
            }
            catch (Exception exception1)
            {
                Console.WriteLine(exception1);
                throw;
            }
            return symbols;
        }

        internal override int OnGetSubSymbolCount(ISymbol parentSymbol) => 
            ((IUnionType) base.DataType).Fields.Count;

        public ReadOnlySymbolCollection FieldInstances =>
            base.SubSymbols;

        public override bool IsPrimitiveType =>
            false;

        public override bool IsContainerType =>
            true;
    }
}

