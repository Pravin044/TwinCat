namespace TwinCAT.TypeSystem
{
    using System;
    using TwinCAT.Ads;

    internal static class EnumValueConverter
    {
        public static string ToString(ITcAdsDataType dataType, object value)
        {
            AdsDatatypeId dataTypeId = dataType.BaseType.DataTypeId;
            if (dataTypeId == AdsDatatypeId.ADST_INT16)
            {
                return EnumTypeConverter<short>.ToString(dataType, (short) value);
            }
            if (dataTypeId == AdsDatatypeId.ADST_INT32)
            {
                return EnumTypeConverter<int>.ToString(dataType, (int) value);
            }
            switch (dataTypeId)
            {
                case AdsDatatypeId.ADST_INT8:
                    return EnumTypeConverter<sbyte>.ToString(dataType, (sbyte) value);

                case AdsDatatypeId.ADST_UINT8:
                    return EnumTypeConverter<byte>.ToString(dataType, (byte) value);

                case AdsDatatypeId.ADST_UINT16:
                    return EnumTypeConverter<ushort>.ToString(dataType, (ushort) value);

                case AdsDatatypeId.ADST_UINT32:
                    return EnumTypeConverter<uint>.ToString(dataType, (uint) value);

                case AdsDatatypeId.ADST_INT64:
                    return EnumTypeConverter<long>.ToString(dataType, (long) value);

                case AdsDatatypeId.ADST_UINT64:
                    return EnumTypeConverter<ulong>.ToString(dataType, (ulong) value);
            }
            throw new ArgumentException("Wrong data type!");
        }

        public static object ToValue(ITcAdsDataType dataType, string value)
        {
            AdsDatatypeId dataTypeId = dataType.BaseType.DataTypeId;
            if (dataTypeId == AdsDatatypeId.ADST_INT16)
            {
                return EnumTypeConverter<short>.ToValue(dataType, value);
            }
            if (dataTypeId == AdsDatatypeId.ADST_INT32)
            {
                return EnumTypeConverter<int>.ToValue(dataType, value);
            }
            switch (dataTypeId)
            {
                case AdsDatatypeId.ADST_INT8:
                    return EnumTypeConverter<sbyte>.ToValue(dataType, value);

                case AdsDatatypeId.ADST_UINT8:
                    return EnumTypeConverter<byte>.ToValue(dataType, value);

                case AdsDatatypeId.ADST_UINT16:
                    return EnumTypeConverter<ushort>.ToValue(dataType, value);

                case AdsDatatypeId.ADST_UINT32:
                    return EnumTypeConverter<uint>.ToValue(dataType, value);

                case AdsDatatypeId.ADST_INT64:
                    return EnumTypeConverter<long>.ToValue(dataType, value);

                case AdsDatatypeId.ADST_UINT64:
                    return EnumTypeConverter<ulong>.ToValue(dataType, value);
            }
            throw new ArgumentException("Wrong data type!");
        }
    }
}

