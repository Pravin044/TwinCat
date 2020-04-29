namespace TwinCAT.TypeSystem
{
    using System;

    public interface IDynamicSymbol : ISymbol, IAttributedInstance, IInstance, IBitSize
    {
        IValueSymbol Unwrap();

        string NormalizedName { get; }
    }
}

