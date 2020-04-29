namespace TwinCAT.TypeSystem
{
    using System;
    using System.ComponentModel;

    [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
    public interface ISymbolInternal
    {
        ISymbolCollection CreateSubSymbols(ISymbol parent);

        ISymbolCollection SubSymbolsInternal { get; }

        bool SubSymbolsCreated { get; }
    }
}

