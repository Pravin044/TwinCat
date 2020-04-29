namespace TwinCAT.TypeSystem.Generic
{
    using System;
    using System.Collections.ObjectModel;
    using System.Reflection;
    using System.Runtime.InteropServices;

    public class ReadOnlyDataTypeCollection<T> : ReadOnlyCollection<T>, IDataTypeContainer<T> where T: IDataType
    {
        public ReadOnlyDataTypeCollection(DataTypeCollection<T> coll) : base(coll)
        {
        }

        public ReadOnlyDataTypeCollection(ReadOnlyDataTypeCollection<T> coll) : base(coll.Items)
        {
        }

        public bool ContainsType(string name) => 
            ((DataTypeCollection<T>) base.Items).ContainsType(name);

        public bool TryGetType(string name, out T type) => 
            ((DataTypeCollection<T>) base.Items).TryGetType(name, out type);

        public T this[string name] =>
            ((DataTypeCollection<T>) base.Items)[name];
    }
}

