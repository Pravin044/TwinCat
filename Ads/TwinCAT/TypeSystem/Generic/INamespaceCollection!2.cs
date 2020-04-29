namespace TwinCAT.TypeSystem.Generic
{
    using System;
    using System.Reflection;
    using System.Runtime.InteropServices;

    public interface INamespaceCollection<N, T> where N: INamespace<T> where T: IDataType
    {
        bool ContainsNamespace(string namespaceName);
        bool TryGetNamespace(string namespaceName, out N nspace);
        bool TryGetType(string typeName, out T dataType);
        bool TryGetTypeByFullName(string fullName, out T dataType);

        N this[string namespaceName] { get; }

        ReadOnlyDataTypeCollection<T> AllTypes { get; }
    }
}

