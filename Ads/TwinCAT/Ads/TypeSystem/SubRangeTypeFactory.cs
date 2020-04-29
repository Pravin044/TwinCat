namespace TwinCAT.Ads.TypeSystem
{
    using System;
    using TwinCAT.Ads;
    using TwinCAT.Ads.Internal;
    using TwinCAT.TypeSystem;

    internal static class SubRangeTypeFactory
    {
        private static ISubRangeType Create(string name, IDataType baseType)
        {
            ISubRangeType type = null;
            string str;
            Type managedType = ((IManagedMappableType) baseType).ManagedType;
            if (managedType == typeof(sbyte))
            {
                sbyte num;
                sbyte num2;
                DataTypeStringParser.TryParseSubRange<sbyte>(name, out str, out num, out num2);
                type = new SubRangeType<sbyte>(name, baseType.Name, 1, num, num2);
            }
            else if (managedType == typeof(byte))
            {
                byte num3;
                byte num4;
                DataTypeStringParser.TryParseSubRange<byte>(name, out str, out num3, out num4);
                type = new SubRangeType<byte>(name, baseType.Name, 1, num3, num4);
            }
            else if (managedType == typeof(short))
            {
                short num5;
                short num6;
                DataTypeStringParser.TryParseSubRange<short>(name, out str, out num5, out num6);
                type = new SubRangeType<short>(name, baseType.Name, 2, num5, num6);
            }
            else if (managedType == typeof(ushort))
            {
                ushort num7;
                ushort num8;
                DataTypeStringParser.TryParseSubRange<ushort>(name, out str, out num7, out num8);
                type = new SubRangeType<ushort>(name, baseType.Name, 2, num7, num8);
            }
            else if (managedType == typeof(int))
            {
                int num9;
                int num10;
                DataTypeStringParser.TryParseSubRange<int>(name, out str, out num9, out num10);
                type = new SubRangeType<int>(name, baseType.Name, 4, num9, num10);
            }
            else if (managedType == typeof(uint))
            {
                uint num11;
                uint num12;
                DataTypeStringParser.TryParseSubRange<uint>(name, out str, out num11, out num12);
                type = new SubRangeType<uint>(name, baseType.Name, 4, num11, num12);
            }
            else if (managedType == typeof(long))
            {
                long num13;
                long num14;
                DataTypeStringParser.TryParseSubRange<long>(name, out str, out num13, out num14);
                type = new SubRangeType<long>(name, baseType.Name, 8, num13, num14);
            }
            else
            {
                ulong num15;
                ulong num16;
                if (managedType != typeof(ulong))
                {
                    throw new AdsException($"Could not create range type '{name}'");
                }
                DataTypeStringParser.TryParseSubRange<ulong>(name, out str, out num15, out num16);
                type = new SubRangeType<ulong>(name, baseType.Name, 8, num15, num16);
            }
            return type;
        }

        internal static ISubRangeType Create(string name, IDataTypeResolver resolver)
        {
            string baseType = null;
            if (DataTypeStringParser.TryParseSubRange(name, out baseType))
            {
                IDataType type = null;
                if (resolver.TryResolveType(baseType, out type))
                {
                    Create(name, type);
                }
            }
            return null;
        }

        internal static ISubRangeType Create(AdsDataTypeEntry entry, IDataTypeResolver resolver)
        {
            ISubRangeType type = null;
            string baseType = null;
            if (DataTypeStringParser.TryParseSubRange(entry.entryName, out baseType))
            {
                IDataType type2 = null;
                if (resolver.TryResolveType(baseType, out type2))
                {
                    type = Create(entry.entryName, type2);
                }
            }
            return type;
        }
    }
}

