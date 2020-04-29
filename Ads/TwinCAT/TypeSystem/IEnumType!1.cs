namespace TwinCAT.TypeSystem
{
    using System;
    using System.Runtime.InteropServices;

    public interface IEnumType<T> : IAliasType, IDataType, IBitSize where T: IConvertible
    {
        bool Contains(string name);
        string[] GetNames();
        T[] GetValues();
        T Parse(string name);
        string ToString(T val);
        bool TryParse(string name, out T value);

        ReadOnlyEnumValueCollection<T> EnumValues { get; }
    }
}

