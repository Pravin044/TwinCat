namespace TwinCAT.TypeSystem.Generic
{
    using System;

    public interface ISymbolProvider<N, T, S> where N: INamespace<T> where T: IDataType where S: ISymbol
    {
        ReadOnlyNamespaceCollection<N, T> Namespaces { get; }

        string RootNamespaceName { get; }

        N RootNamespace { get; }

        ReadOnlySymbolCollection<S> Symbols { get; }

        ReadOnlyDataTypeCollection<T> DataTypes { get; }
    }
}

