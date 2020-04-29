namespace TwinCAT.TypeSystem
{
    using System;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using TwinCAT.Ads.Internal;

    public class EnumValue<T> : IEnumValue where T: IConvertible
    {
        private string _name;
        private T _value;
        private int _size;

        internal EnumValue(AdsEnumInfoEntry entry)
        {
            this._name = entry.name;
            this._size = entry.value.Length;
            Type type = typeof(T);
            if (type == typeof(byte))
            {
                this._value = (T) Convert.ChangeType(entry.value[0], typeof(byte), CultureInfo.InvariantCulture);
            }
            else if (type == typeof(sbyte))
            {
                this._value = (T) Convert.ChangeType((sbyte) entry.value[0], typeof(sbyte), CultureInfo.InvariantCulture);
            }
            else if (type == typeof(short))
            {
                short num = BitConverter.ToInt16(entry.value, 0);
                this._value = (T) Convert.ChangeType(num, typeof(T), CultureInfo.InvariantCulture);
            }
            else if (type == typeof(ushort))
            {
                ushort num2 = BitConverter.ToUInt16(entry.value, 0);
                this._value = (T) Convert.ChangeType(num2, typeof(T), CultureInfo.InvariantCulture);
            }
            else if (type == typeof(int))
            {
                int num3 = BitConverter.ToInt32(entry.value, 0);
                this._value = (T) Convert.ChangeType(num3, typeof(T), CultureInfo.InvariantCulture);
            }
            else if (type == typeof(uint))
            {
                uint num4 = BitConverter.ToUInt32(entry.value, 0);
                this._value = (T) Convert.ChangeType(num4, typeof(T), CultureInfo.InvariantCulture);
            }
            else if (type == typeof(long))
            {
                long num5 = BitConverter.ToInt64(entry.value, 0);
                this._value = (T) Convert.ChangeType(num5, typeof(T), CultureInfo.InvariantCulture);
            }
            else
            {
                if (type != typeof(ulong))
                {
                    throw new ArgumentException("Wrong Enum base type.");
                }
                ulong num6 = BitConverter.ToUInt64(entry.value, 0);
                this._value = (T) Convert.ChangeType(num6, typeof(T), CultureInfo.InvariantCulture);
            }
        }

        internal EnumValue(IEnumType<T> enumType, T value)
        {
            this._size = enumType.Size;
            this._value = value;
            this._name = enumType.ToString(this._value);
        }

        public static EnumValue<T> Parse(IEnumType<T> type, string str)
        {
            EnumValue<T> value2 = null;
            if (!EnumValue<T>.TryParse(type, str, out value2))
            {
                throw new FormatException($"Cannot parse '{str}' to EnumValue of type '{type.Name}'!");
            }
            return value2;
        }

        public override string ToString() => 
            this.Name;

        public static bool TryParse(IEnumType<T> type, string str, out EnumValue<T> value)
        {
            T local;
            bool flag = false;
            value = null;
            if (type.EnumValues.TryParse(str, out local))
            {
                value = new EnumValue<T>(type, local);
                flag = true;
            }
            return flag;
        }

        public string Name =>
            this._name;

        public T Primitive =>
            this._value;

        object IEnumValue.Primitive =>
            this.Primitive;

        public byte[] RawValue
        {
            get
            {
                byte[] bytes = null;
                Type type = typeof(T);
                if (type == typeof(byte))
                {
                    bytes = new byte[] { this.Primitive.ToByte(null) };
                }
                else if (type == typeof(sbyte))
                {
                    bytes = BitConverter.GetBytes((short) this.Primitive.ToSByte(null));
                }
                else if (type == typeof(short))
                {
                    bytes = BitConverter.GetBytes(this.Primitive.ToInt16(null));
                }
                else if (type == typeof(ushort))
                {
                    bytes = BitConverter.GetBytes(this.Primitive.ToUInt16(null));
                }
                else if (type == typeof(int))
                {
                    bytes = BitConverter.GetBytes(this.Primitive.ToInt32(null));
                }
                else if (type == typeof(uint))
                {
                    bytes = BitConverter.GetBytes(this.Primitive.ToUInt32(null));
                }
                else if (type == typeof(long))
                {
                    bytes = BitConverter.GetBytes(this.Primitive.ToInt64(null));
                }
                else
                {
                    if (type != typeof(ulong))
                    {
                        throw new NotSupportedException("Base type of enum is not allowed!");
                    }
                    bytes = BitConverter.GetBytes(this.Primitive.ToUInt64(null));
                }
                return bytes;
            }
        }

        public Type ManagedBaseType =>
            typeof(T);

        public int Size =>
            this._size;
    }
}

