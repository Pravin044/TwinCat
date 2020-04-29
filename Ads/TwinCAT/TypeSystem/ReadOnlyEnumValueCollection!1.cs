namespace TwinCAT.TypeSystem
{
    using System;
    using System.Collections.ObjectModel;
    using System.Runtime.InteropServices;

    public class ReadOnlyEnumValueCollection<T> : ReadOnlyCollection<EnumValue<T>> where T: IConvertible
    {
        public ReadOnlyEnumValueCollection(EnumValueCollection<T> coll) : base(coll)
        {
        }

        public bool Contains(string value) => 
            ((EnumValueCollection<T>) base.Items).Contains(value);

        public string[] GetNames() => 
            ((EnumValueCollection<T>) base.Items).GetNames();

        public T[] GetValues() => 
            ((EnumValueCollection<T>) base.Items).GetValues();

        public T Parse(string name) => 
            ((EnumValueCollection<T>) base.Items).Parse(name);

        public bool TryParse(string strValue, out T value) => 
            ((EnumValueCollection<T>) base.Items).TryParse(strValue, out value);
    }
}

