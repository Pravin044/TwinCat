namespace TwinCAT.TypeSystem.Generic
{
    public interface IInternalSymbolProvider<N, T, S> where N: INamespace<T> where T: IDataType where S: class, ISymbol
    {
        NamespaceCollection<N, T> NamespacesInternal { get; }

        SymbolCollection<S> SymbolsInternal { get; }

        DataTypeCollection<T> DataTypesInternal { get; }
    }
}

