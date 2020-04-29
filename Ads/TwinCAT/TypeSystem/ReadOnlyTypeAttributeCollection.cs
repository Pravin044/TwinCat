namespace TwinCAT.TypeSystem
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Reflection;
    using System.Runtime.InteropServices;

    public class ReadOnlyTypeAttributeCollection : ReadOnlyCollection<ITypeAttribute>
    {
        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        public ReadOnlyTypeAttributeCollection(TypeAttributeCollection coll) : base(coll)
        {
        }

        public bool Contains(string name) => 
            ((TypeAttributeCollection) base.Items).Contains(name);

        public bool TryGetAttribute(string name, out ITypeAttribute attribute) => 
            ((TypeAttributeCollection) base.Items).TryGetAttribute(name, out attribute);

        public bool TryGetValue(string name, out string value) => 
            ((TypeAttributeCollection) base.Items).TryGetValue(name, out value);

        public string this[string name] =>
            ((TypeAttributeCollection) base.Items)[name];
    }
}

