namespace TwinCAT.TypeSystem
{
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using TwinCAT.Ads.TypeSystem;

    public class DynamicArrayInstance : DynamicSymbol, IArrayInstance, ISymbol, IAttributedInstance, IInstance, IBitSize
    {
        internal DynamicArrayInstance(IArrayInstance symbol) : base((IValueSymbol) symbol)
        {
        }

        public bool TryGetElement(IList<int[]> jaggedIndices, out ISymbol symbol) => 
            ((IArrayInstance) base.symbol).TryGetElement(jaggedIndices, out symbol);

        public bool TryGetElement(int[] indices, out ISymbol symbol)
        {
            IArrayType dataType = (IArrayType) base.DataType;
            if (!ArrayType.AreIndicesValid(indices, dataType, false))
            {
                symbol = null;
                return false;
            }
            int elementPosition = ArrayType.GetElementPosition(indices, dataType);
            symbol = base.SubSymbols[elementPosition];
            return true;
        }

        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
        {
            IArrayType dataType = (IArrayType) base.DataType;
            return TryGetIndex(this, dataType, binder, indexes, out result);
        }

        internal static bool TryGetIndex(DynamicSymbol symbol, IArrayType arrayType, GetIndexBinder binder, object[] indexes, out object result)
        {
            int[] indices = new int[indexes.GetLength(0)];
            for (int i = 0; i < indexes.GetLength(0); i++)
            {
                indices[i] = (int) indexes[i];
            }
            ArrayType.CheckIndices(indices, arrayType, false);
            result = symbol.SubSymbols[ArrayType.GetElementPosition(indices, arrayType)];
            return true;
        }

        public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value) => 
            base.TrySetIndex(binder, indexes, value);

        public ReadOnlySymbolCollection Elements =>
            ((IArrayInstance) base.symbol).Elements;

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

        public ReadOnlyDimensionCollection Dimensions =>
            ((IArrayInstance) base.symbol).Dimensions;

        public IDataType ElementType =>
            ((IArrayInstance) base.symbol).ElementType;
    }
}

