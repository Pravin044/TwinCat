namespace TwinCAT.Ads.TypeSystem
{
    using System;
    using System.ComponentModel;
    using System.Runtime.InteropServices;
    using TwinCAT.Ads;
    using TwinCAT.Ads.Internal;
    using TwinCAT.TypeSystem;

    public sealed class EnumType<T> : DataType, IEnumType<T>, IAliasType, IDataType, IBitSize, IEnumType where T: IConvertible
    {
        private AdsDatatypeId _baseTypeId;
        private string _baseTypeName;
        private IDataType _baseType;
        private EnumValueCollection<T> _fields;

        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        public EnumType(AdsDataTypeEntry entry) : base(DataTypeCategory.Enum, entry)
        {
            int num1;
            Type type = typeof(T);
            if (((type == typeof(byte)) || ((type == typeof(sbyte)) || ((type == typeof(short)) || ((type == typeof(ushort)) || ((type == typeof(int)) || (type == typeof(uint))))))) || (type == typeof(long)))
            {
                num1 = 1;
            }
            else
            {
                num1 = (int) (type == typeof(ulong));
            }
            if (num1 == 0)
            {
                throw new AdsException($"'{type}' is not a valid base type for an enumeration!");
            }
            base.dotnetType = type;
            this._baseTypeId = entry.baseTypeId;
            this._baseTypeName = entry.typeName;
            this._fields = new EnumValueCollection<T>(entry.enums);
        }

        public bool Contains(string name)
        {
            T local = default(T);
            return this.TryParse(name, out local);
        }

        public string[] GetNames() => 
            this._fields.GetNames();

        public T[] GetValues() => 
            this._fields.GetValues();

        public T Parse(string strValue) => 
            this._fields.Parse(strValue);

        public string ToString(T val)
        {
            EnumValue<T> value2;
            return (!this._fields.TryGetInfo(val, out value2) ? val.ToString() : value2.Name);
        }

        public string ToString(object val)
        {
            IConvertible convertible = (IConvertible) val;
            object obj2 = Convert.ChangeType(val, typeof(T), null);
            return this.ToString((T) obj2);
        }

        public bool TryParse(string strValue, out T value) => 
            this._fields.TryParse(strValue, out value);

        object[] IEnumType.GetValues()
        {
            object[] array = new object[this._fields.Count];
            this._fields.GetValues().CopyTo(array, 0);
            return array;
        }

        object IEnumType.Parse(string name) => 
            this.Parse(name);

        bool IEnumType.TryParse(string name, out object value)
        {
            T local = default(T);
            value = null;
            bool flag = this.TryParse(name, out local);
            if (flag)
            {
                value = local;
            }
            return flag;
        }

        public string BaseTypeName =>
            this._baseTypeName;

        public IDataType BaseType
        {
            get
            {
                if (this._baseType == null)
                {
                    base.resolver.TryResolveType(this._baseTypeName, out this._baseType);
                }
                return this._baseType;
            }
        }

        ReadOnlyEnumValueCollection IEnumType.EnumValues =>
            ((EnumValueCollection) this._fields).AsReadOnly();

        public ReadOnlyEnumValueCollection<T> EnumValues =>
            this._fields.AsReadOnly();
    }
}

