namespace TwinCAT.Ads.TypeSystem
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using TwinCAT.Ads;
    using TwinCAT.Ads.Internal;
    using TwinCAT.TypeSystem;

    internal class ArrayInstance : Symbol, IArrayInstance, ISymbol, IAttributedInstance, IInstance, IBitSize
    {
        internal ArrayInstance(Member member, ISymbol parent) : base(member, parent)
        {
            base.category = DataTypeCategory.Array;
        }

        internal ArrayInstance(int[] currentIndex, bool oversample, ISymbol parent) : base(currentIndex, oversample, parent)
        {
            base.category = DataTypeCategory.Array;
        }

        internal ArrayInstance(AdsSymbolEntry entry, IArrayType type, ISymbol parent, ISymbolFactoryServices factoryServices) : base(entry, type, parent, factoryServices)
        {
            base.category = DataTypeCategory.Array;
        }

        internal ArrayInstance(ISymbol parent, IArrayType type, string instanceName, int fieldOffset) : base(parent, type, instanceName, fieldOffset, ((ISymbolFactoryServicesProvider) parent).FactoryServices)
        {
            base.category = DataTypeCategory.Array;
        }

        internal override ISymbolCollection OnCreateSubSymbols(ISymbol parentInstance)
        {
            ISymbolCollection symbols = null;
            ArrayType dataType = (ArrayType) base.DataType;
            ISymbolFactory symbolFactory = this.FactoryServices.SymbolFactory;
            try
            {
                symbols = symbolFactory.CreateArrayElementInstances(parentInstance, dataType);
            }
            catch (Exception exception)
            {
                TwinCAT.Ads.Module.Trace.TraceError(exception);
                throw;
            }
            return symbols;
        }

        protected virtual ReadOnlySymbolCollection OnGetElements() => 
            base.SubSymbols;

        internal override int OnGetSubSymbolCount(ISymbol parentSymbol) => 
            ((IArrayType) base.DataType).Dimensions.ElementCount;

        public bool TryGetElement(IList<int[]> jaggedIndices, out ISymbol symbol)
        {
            if (jaggedIndices == null)
            {
                throw new ArgumentNullException("jaggedIndices");
            }
            if (((IArrayType) base.DataType).JaggedLevel < jaggedIndices.Count)
            {
                throw new ArgumentOutOfRangeException("jaggedIndices");
            }
            bool flag = true;
            IArrayInstance instance = this;
            ISymbol symbol2 = null;
            symbol = null;
            int num = 0;
            while (true)
            {
                if (num < jaggedIndices.Count)
                {
                    flag &= instance.TryGetElement(jaggedIndices[num], out symbol2);
                    if (flag)
                    {
                        instance = symbol2 as IArrayInstance;
                        num++;
                        continue;
                    }
                }
                if (flag)
                {
                    symbol = symbol2;
                }
                return flag;
            }
        }

        public bool TryGetElement(int[] indices, out ISymbol symbol)
        {
            if (indices == null)
            {
                throw new ArgumentNullException("indices");
            }
            ArrayType dataType = (ArrayType) base.DataType;
            if (!ArrayType.AreIndicesValid(indices, dataType, false))
            {
                symbol = null;
                return false;
            }
            int elementPosition = dataType.GetElementPosition(indices);
            symbol = base.SubSymbols[elementPosition];
            return true;
        }

        public ReadOnlySymbolCollection Elements =>
            this.OnGetElements();

        public ISymbol this[int[] indices]
        {
            get
            {
                ISymbol symbol = null;
                if (!this.TryGetElement(indices, out symbol))
                {
                    throw new ArgumentOutOfRangeException("indices");
                }
                return symbol;
            }
        }

        public int JaggedLevel =>
            ((IArrayType) base.DataType).JaggedLevel;

        public override bool IsContainerType =>
            true;

        public override bool IsPrimitiveType
        {
            get
            {
                IDataType dataType = base.DataType;
                return ((dataType != null) && dataType.IsPrimitive);
            }
        }

        public ReadOnlyDimensionCollection Dimensions =>
            ((IArrayType) base.DataType).Dimensions;

        public IDataType ElementType =>
            ((IArrayType) base.DataType).ElementType;

        public bool IsOversampled =>
            ((ArrayType) base.DataType).IsOversampled;
    }
}

