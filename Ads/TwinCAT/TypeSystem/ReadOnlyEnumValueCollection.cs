namespace TwinCAT.TypeSystem
{
    using System;
    using System.Collections.ObjectModel;
    using System.Reflection;

    public class ReadOnlyEnumValueCollection : ReadOnlyCollection<IEnumValue>
    {
        public ReadOnlyEnumValueCollection(EnumValueCollection coll) : base(coll)
        {
        }

        public bool Contains(string value) => 
            ((EnumValueCollection) base.Items).Contains(value);

        public string[] GetNames() => 
            ((EnumValueCollection) base.Items).GetNames();

        public object[] GetValues() => 
            ((EnumValueCollection) base.Items).GetValues();

        public object Parse(string name) => 
            ((EnumValueCollection) base.Items).Parse(name);

        public bool TryParse(string strValue, object value) => 
            ((EnumValueCollection) base.Items).TryParse(strValue, out value);

        public IEnumValue this[string name] =>
            ((EnumValueCollection) base.Items)[name];
    }
}

