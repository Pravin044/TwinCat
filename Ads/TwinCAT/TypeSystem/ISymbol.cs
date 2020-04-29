namespace TwinCAT.TypeSystem
{
    using System;

    public interface ISymbol : IAttributedInstance, IInstance, IBitSize
    {
        DataTypeCategory Category { get; }

        ISymbol Parent { get; }

        ReadOnlySymbolCollection SubSymbols { get; }

        bool IsContainerType { get; }

        bool IsPrimitiveType { get; }

        bool IsPersistent { get; }

        bool IsReadOnly { get; }

        bool IsRecursive { get; }
    }
}

