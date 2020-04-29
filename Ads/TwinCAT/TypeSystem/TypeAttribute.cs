namespace TwinCAT.TypeSystem
{
    using System;
    using System.ComponentModel;

    public class TypeAttribute : ITypeAttribute
    {
        private string _name;
        private string _value;

        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        internal TypeAttribute(ITypeAttribute att)
        {
            this._name = att.Name;
            this._value = att.Value;
        }

        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        internal TypeAttribute(string name, string value)
        {
            this._name = name;
            this._value = value;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (base.GetType() != obj.GetType())
            {
                return false;
            }
            TypeAttribute attribute = (TypeAttribute) obj;
            return (StringComparer.OrdinalIgnoreCase.Compare(this.Name, attribute.Name) == 0);
        }

        public override int GetHashCode() => 
            ((0x5b * 10) + StringComparer.OrdinalIgnoreCase.GetHashCode(this._name));

        public static bool operator ==(TypeAttribute o1, TypeAttribute o2) => 
            (!Equals(o1, null) ? o1.Equals(o2) : Equals(o2, null));

        public static bool operator !=(TypeAttribute o1, TypeAttribute o2) => 
            !(o1 == o2);

        public string Name =>
            this._name;

        public string Value =>
            this._value;
    }
}

