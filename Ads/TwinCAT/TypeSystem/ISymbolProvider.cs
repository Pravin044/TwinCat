namespace TwinCAT.TypeSystem
{
    using System;
    using TwinCAT.TypeSystem.Generic;

    public interface ISymbolProvider
    {
        ReadOnlyNamespaceCollection Namespaces { get; }

        string RootNamespaceName { get; }

        INamespace<IDataType> RootNamespace { get; }

        ReadOnlySymbolCollection Symbols { get; }

        ReadOnlyDataTypeCollection DataTypes { get; }
    }
}

