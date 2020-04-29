namespace TwinCAT.TypeSystem.Generic
{
    using System;
    using System.Collections.ObjectModel;
    using System.Reflection;
    using System.Runtime.InteropServices;

    public class ReadOnlyNamespaceCollection<N, T> : ReadOnlyCollection<N>, INamespaceCollection<N, T> where N: INamespace<T> where T: IDataType
    {
        public ReadOnlyNamespaceCollection(NamespaceCollection<N, T> coll) : base(coll)
        {
        }

        public bool ContainsNamespace(string name) => 
            ((NamespaceCollection<N, T>) base.Items).ContainsNamespace(name);

        public bool TryGetNamespace(string name, out N nspace) => 
            ((NamespaceCollection<N, T>) base.Items).TryGetNamespace(name, out nspace);

        public bool TryGetType(string typeName, out T dataType) => 
            ((NamespaceCollection<N, T>) base.Items).TryGetType(typeName, out dataType);

        public bool TryGetTypeByFullName(string fullname, out T dataType) => 
            ((NamespaceCollection<N, T>) base.Items).TryGetTypeByFullName(fullname, out dataType);

        public N this[string name] =>
            ((NamespaceCollection<N, T>) base.Items)[name];

        public ReadOnlyDataTypeCollection<T> AllTypes =>
            ((NamespaceCollection<N, T>) base.Items).AllTypes;
    }
}

