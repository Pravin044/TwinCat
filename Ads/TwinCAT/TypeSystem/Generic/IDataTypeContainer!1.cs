namespace TwinCAT.TypeSystem.Generic
{
    using System;
    using System.Reflection;
    using System.Runtime.InteropServices;

    public interface IDataTypeContainer<T> where T: IDataType
    {
        bool ContainsType(string name);
        bool TryGetType(string name, out T type);

        T this[string name] { get; }
    }
}

