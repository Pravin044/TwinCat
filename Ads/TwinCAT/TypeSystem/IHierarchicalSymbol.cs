namespace TwinCAT.TypeSystem
{
    using System;
    using System.ComponentModel;

    [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
    public interface IHierarchicalSymbol : ISymbol, IAttributedInstance, IInstance, IBitSize
    {
        void SetParent(ISymbol symbol);
    }
}

