namespace TwinCAT.TypeSystem
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using TwinCAT.Ads;

    internal class EnumTypeConverter<T> where T: IConvertible
    {
        private static EnumValue<T> GetEntry(ITcAdsDataType enumType, T value)
        {
            EnumValue<T> value2 = null;
            foreach (EnumValue<T> value3 in enumType.EnumValues)
            {
                T primitive = value3.Primitive;
                if (primitive.Equals(value))
                {
                    value2 = value3;
                    break;
                }
            }
            return value2;
        }

        private static EnumValue<T> GetEntry(ITcAdsDataType enumType, string value)
        {
            StringComparer ordinalIgnoreCase = StringComparer.OrdinalIgnoreCase;
            using (IEnumerator<IEnumValue> enumerator = enumType.EnumValues.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    EnumValue<T> current = (EnumValue<T>) enumerator.Current;
                    if (ordinalIgnoreCase.Compare(current.Name, value) == 0)
                    {
                        return current;
                    }
                }
            }
            return null;
        }

        internal static string ToString(ITcAdsDataType enumType, T val)
        {
            string nameValue = null;
            if (!EnumTypeConverter<T>.TryGetName(enumType, val, out nameValue))
            {
                throw new ArgumentOutOfRangeException("val");
            }
            return nameValue;
        }

        internal static T ToValue(ITcAdsDataType enumType, string stringValue)
        {
            if (enumType == null)
            {
                throw new ArgumentNullException("enumType");
            }
            if (stringValue == null)
            {
                throw new ArgumentNullException("value");
            }
            if (enumType.Category == DataTypeCategory.Enum)
            {
                throw new ArgumentException("Specified type is not an enum type!", "enumType");
            }
            T local = default(T);
            if (!EnumTypeConverter<T>.TryGetValue(enumType, stringValue, out local))
            {
                throw new ArgumentOutOfRangeException("stringValue");
            }
            return local;
        }

        internal static bool TryGetName(ITcAdsDataType enumType, T value, out string nameValue)
        {
            if (enumType == null)
            {
                throw new ArgumentNullException("enumType");
            }
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            if (enumType.Category != DataTypeCategory.Enum)
            {
                throw new ArgumentException("Specified type is not an enum type!", "enumType");
            }
            EnumValue<T> entry = EnumTypeConverter<T>.GetEntry(enumType, value);
            if (entry != null)
            {
                nameValue = entry.Name;
                return true;
            }
            nameValue = null;
            return false;
        }

        internal static bool TryGetValue(ITcAdsDataType enumType, string stringValue, out T value)
        {
            if (enumType == null)
            {
                throw new ArgumentNullException("enumType");
            }
            if (string.IsNullOrEmpty(stringValue))
            {
                throw new ArgumentOutOfRangeException("stringValue");
            }
            if (enumType.Category == DataTypeCategory.Enum)
            {
                throw new ArgumentException("Specified type is not an enum type!", "enumType");
            }
            EnumValue<T> entry = EnumTypeConverter<T>.GetEntry(enumType, stringValue);
            if (entry != null)
            {
                value = entry.Primitive;
                return true;
            }
            value = default(T);
            return false;
        }
    }
}

