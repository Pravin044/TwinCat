namespace TwinCAT.Ads.TypeSystem
{
    using System;
    using TwinCAT.Ads;
    using TwinCAT.Ads.Internal;
    using TwinCAT.TypeSystem;
    using TwinCAT.TypeSystem.Generic;

    internal class StructInstance : Symbol, IStructInstance, ISymbol, IAttributedInstance, IInstance, IBitSize
    {
        internal StructInstance(Member member, ISymbol parent) : base(member, parent)
        {
            base.category = DataTypeCategory.Struct;
        }

        internal StructInstance(int[] currentIndex, bool oversample, ISymbol parent) : base(currentIndex, oversample, parent)
        {
            base.category = DataTypeCategory.Struct;
        }

        protected StructInstance(string instanceName, string instancePath, ISymbolFactoryServices factoryServices) : base(instanceName, instancePath, factoryServices)
        {
            base.category = DataTypeCategory.Struct;
        }

        protected StructInstance(string instanceName, ISymbol parent, int fieldOffset, ISymbolFactoryServices factoryServices) : base(instanceName, parent, factoryServices)
        {
            base.category = DataTypeCategory.Struct;
        }

        internal StructInstance(AdsSymbolEntry entry, IStructType type, ISymbol parent, ISymbolFactoryServices factoryServices) : base(entry, type, parent, factoryServices)
        {
            base.category = DataTypeCategory.Struct;
        }

        internal StructInstance(ISymbol parent, IStructType type, string instanceName, int fieldOffset) : base(parent, type, instanceName, fieldOffset, ((ISymbolFactoryServicesProvider) parent).FactoryServices)
        {
            base.category = DataTypeCategory.Struct;
        }

        internal override ISymbolCollection OnCreateSubSymbols(ISymbol parentInstance)
        {
            ISymbolCollection symbols = null;
            try
            {
                StructType dataType = (StructType) base.DataType;
                symbols = (dataType == null) ? new SymbolCollection(InstanceCollectionMode.Names) : this.FactoryServices.SymbolFactory.CreateFieldInstances(this, dataType);
            }
            catch (Exception exception)
            {
                Module.Trace.TraceError(exception);
            }
            return symbols;
        }

        internal override int OnGetSubSymbolCount(ISymbol parentSymbol) => 
            ((IStructType) base.DataType).AllMembers.Count;

        public ReadOnlySymbolCollection MemberInstances =>
            base.SubSymbols;

        public override bool IsPrimitiveType =>
            false;

        public override bool IsContainerType =>
            true;

        public bool HasRpcMethods
        {
            get
            {
                StructType dataType = (StructType) base.DataType;
                return ((dataType != null) && dataType.HasRpcMethods);
            }
        }
    }
}

