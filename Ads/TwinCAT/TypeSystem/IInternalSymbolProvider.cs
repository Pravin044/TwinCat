namespace TwinCAT.TypeSystem
{
    using TwinCAT.TypeSystem.Generic;

    public interface IInternalSymbolProvider : ISymbolProvider
    {
        NamespaceCollection<INamespace<IDataType>, IDataType> NamespacesInternal { get; }

        SymbolCollection<ISymbol> SymbolsInternal { get; }

        DataTypeCollection<IDataType> DataTypesInternal { get; }
    }
}

