namespace TwinCAT.TypeSystem
{
    using System;
    using TwinCAT.Ads;
    using TwinCAT.Ads.Internal;

    internal static class EnumValueFactory
    {
        internal static IEnumValue Create(AdsDatatypeId baseTypeId, AdsEnumInfoEntry enumInfo)
        {
            if (enumInfo == null)
            {
                throw new ArgumentNullException("entry");
            }
            if (baseTypeId == AdsDatatypeId.ADST_INT16)
            {
                return new EnumValue<short>(enumInfo);
            }
            if (baseTypeId == AdsDatatypeId.ADST_INT32)
            {
                return new EnumValue<int>(enumInfo);
            }
            switch (baseTypeId)
            {
                case AdsDatatypeId.ADST_INT8:
                    return new EnumValue<sbyte>(enumInfo);

                case AdsDatatypeId.ADST_UINT8:
                    return new EnumValue<byte>(enumInfo);

                case AdsDatatypeId.ADST_UINT16:
                    return new EnumValue<ushort>(enumInfo);

                case AdsDatatypeId.ADST_UINT32:
                    return new EnumValue<uint>(enumInfo);

                case AdsDatatypeId.ADST_INT64:
                    return new EnumValue<long>(enumInfo);

                case AdsDatatypeId.ADST_UINT64:
                    return new EnumValue<ulong>(enumInfo);
            }
            throw new ArgumentOutOfRangeException("baseTypeId");
        }

        internal static IEnumValue Create(IEnumType enumType, object value)
        {
            Type managedType = ((IManagedMappableType) enumType).ManagedType;
            if (managedType == typeof(byte))
            {
                return new EnumValue<byte>((IEnumType<byte>) enumType, (byte) value);
            }
            if (managedType == typeof(sbyte))
            {
                return new EnumValue<sbyte>((IEnumType<sbyte>) enumType, (sbyte) value);
            }
            if (managedType == typeof(short))
            {
                return new EnumValue<short>((IEnumType<short>) enumType, (short) value);
            }
            if (managedType == typeof(ushort))
            {
                return new EnumValue<ushort>((IEnumType<ushort>) enumType, (ushort) value);
            }
            if (managedType == typeof(int))
            {
                return new EnumValue<int>((IEnumType<int>) enumType, (int) value);
            }
            if (managedType == typeof(uint))
            {
                return new EnumValue<uint>((IEnumType<uint>) enumType, (uint) value);
            }
            if (managedType == typeof(long))
            {
                return new EnumValue<long>((IEnumType<long>) enumType, (long) value);
            }
            if (managedType != typeof(ulong))
            {
                throw new NotSupportedException();
            }
            return new EnumValue<ulong>((IEnumType<ulong>) enumType, (ulong) value);
        }

        internal static IEnumValue Create(IEnumType enumType, byte[] bytes, int offset)
        {
            Type managedType = ((IManagedMappableType) enumType).ManagedType;
            object obj2 = null;
            if (managedType == typeof(byte))
            {
                obj2 = bytes[offset];
            }
            else if (managedType == typeof(sbyte))
            {
                obj2 = (sbyte) bytes[offset];
            }
            else if (managedType == typeof(short))
            {
                obj2 = BitConverter.ToInt16(bytes, offset);
            }
            else if (managedType == typeof(ushort))
            {
                obj2 = BitConverter.ToUInt16(bytes, offset);
            }
            else if (managedType == typeof(int))
            {
                obj2 = BitConverter.ToInt32(bytes, offset);
            }
            else if (managedType == typeof(uint))
            {
                obj2 = BitConverter.ToUInt32(bytes, offset);
            }
            else if (managedType == typeof(long))
            {
                obj2 = BitConverter.ToInt64(bytes, offset);
            }
            else
            {
                if (managedType != typeof(ulong))
                {
                    throw new ArgumentException("Wrong Enum base type.");
                }
                obj2 = BitConverter.ToUInt64(bytes, offset);
            }
            return Create(enumType, obj2);
        }
    }
}

