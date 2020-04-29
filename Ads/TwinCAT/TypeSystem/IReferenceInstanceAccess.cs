namespace TwinCAT.TypeSystem
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.InteropServices;

    [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
    public interface IReferenceInstanceAccess : IReferenceInstance, ISymbol, IAttributedInstance, IInstance, IBitSize
    {
        bool TryGetElement(IList<int[]> jaggedIndices, out ISymbol symbol);
        bool TryGetElement(int[] indices, out ISymbol symbol);
    }
}

