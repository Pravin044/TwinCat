namespace TwinCAT.TypeSystem
{
    using System;
    using System.Runtime.InteropServices;

    public interface IEnumType : IAliasType, IDataType, IBitSize
    {
        bool Contains(string name);
        string[] GetNames();
        object[] GetValues();
        object Parse(string name);
        string ToString(object val);
        bool TryParse(string name, out object value);

        ReadOnlyEnumValueCollection EnumValues { get; }
    }
}

