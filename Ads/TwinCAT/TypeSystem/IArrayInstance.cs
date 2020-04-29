namespace TwinCAT.TypeSystem
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.InteropServices;

    public interface IArrayInstance : ISymbol, IAttributedInstance, IInstance, IBitSize
    {
        bool TryGetElement(IList<int[]> jaggedIndices, out ISymbol symbol);
        bool TryGetElement(int[] indices, out ISymbol symbol);

        ReadOnlySymbolCollection Elements { get; }

        ISymbol this[int[] indices] { get; }

        ReadOnlyDimensionCollection Dimensions { get; }

        IDataType ElementType { get; }
    }
}

