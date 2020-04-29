namespace TwinCAT.TypeSystem
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Text;
    using TwinCAT.Ads;
    using TwinCAT.PlcOpen;

    [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
    public class PrimitiveTypeConverter
    {
        private static readonly Encoding s_DefaultEncoding = Encoding.Default;
        private readonly Encoding _encoding;
        private readonly PlcStringConverter _stringConverter;

        public PrimitiveTypeConverter(Encoding encoding) : this(encoding, StringConvertMode.ZeroTerminated)
        {
        }

        public PrimitiveTypeConverter(Encoding encoding, StringConvertMode stringConvertMode)
        {
            if (encoding == null)
            {
                throw new ArgumentNullException("encoding");
            }
            this._encoding = encoding;
            this._stringConverter = new PlcStringConverter(this._encoding, stringConvertMode);
        }

        public static bool CanConvert(object sourceValue, Type targetType)
        {
            object targetValue = null;
            return TryConvert(sourceValue, targetType, out targetValue);
        }

        public static bool CanMarshal(Type dataType)
        {
            int size = 0;
            return TryGetMarshalSize(dataType, out size);
        }

        public static bool CanMarshal(AdsDatatypeId typeId)
        {
            switch (typeId)
            {
                case AdsDatatypeId.ADST_INT16:
                case AdsDatatypeId.ADST_INT32:
                case AdsDatatypeId.ADST_REAL32:
                case AdsDatatypeId.ADST_REAL64:
                    break;

                default:
                    switch (typeId)
                    {
                        case AdsDatatypeId.ADST_INT8:
                        case AdsDatatypeId.ADST_UINT8:
                        case AdsDatatypeId.ADST_UINT16:
                        case AdsDatatypeId.ADST_UINT32:
                        case AdsDatatypeId.ADST_INT64:
                        case AdsDatatypeId.ADST_UINT64:
                            break;

                        default:
                            switch (typeId)
                            {
                                case AdsDatatypeId.ADST_STRING:
                                case AdsDatatypeId.ADST_WSTRING:
                                case AdsDatatypeId.ADST_BIT:
                                    break;

                                default:
                                    return false;
                            }
                            break;
                    }
                    break;
            }
            return true;
        }

        public static bool CanMarshal(DataTypeCategory category)
        {
            if ((category != DataTypeCategory.Primitive) && (category != DataTypeCategory.Enum))
            {
                switch (category)
                {
                    case DataTypeCategory.String:
                    case DataTypeCategory.Pointer:
                    case DataTypeCategory.Reference:
                    case DataTypeCategory.Interface:
                        break;

                    default:
                        return false;
                }
            }
            return true;
        }

        public static T Convert<T>(object sourceValue) => 
            ((T) Convert(sourceValue, typeof(T)));

        public static object Convert(object sourceValue, Type targetType)
        {
            if (targetType == null)
            {
                throw new ArgumentNullException("tp");
            }
            if (sourceValue == null)
            {
                return null;
            }
            Type type = sourceValue.GetType();
            if (type == targetType)
            {
                return sourceValue;
            }
            object targetValue = null;
            if (!TryConvert(sourceValue, targetType, out targetValue))
            {
                throw new MarshalException($"Cannot convert value '{type.Name}' to type '{targetType.Name}'!");
            }
            return targetValue;
        }

        private static int GetArrayLength(Type arrayType, int marshalSize)
        {
            Type elementType = arrayType.GetElementType();
            while (elementType.IsArray)
            {
                elementType = elementType.GetElementType();
            }
            int size = 0;
            if (!TryGetMarshalSize(elementType, out size))
            {
                return 0;
            }
            return (!IsMarshalledAsBitType(elementType) ? (marshalSize / size) : ((marshalSize / 1) * 8));
        }

        internal static PrimitiveTypeFlags GetPrimitiveFlags(AdsDatatypeId typeId)
        {
            PrimitiveTypeFlags none = PrimitiveTypeFlags.None;
            switch (typeId)
            {
                case AdsDatatypeId.ADST_INT16:
                case AdsDatatypeId.ADST_INT32:
                    goto TR_0001;

                case AdsDatatypeId.ADST_REAL32:
                case AdsDatatypeId.ADST_REAL64:
                    break;

                default:
                    switch (typeId)
                    {
                        case AdsDatatypeId.ADST_INT8:
                        case AdsDatatypeId.ADST_INT64:
                            goto TR_0001;

                        case AdsDatatypeId.ADST_UINT8:
                        case AdsDatatypeId.ADST_UINT16:
                        case AdsDatatypeId.ADST_UINT32:
                        case AdsDatatypeId.ADST_UINT64:
                            return PrimitiveTypeFlags.MaskNumericUnsigned;

                        case AdsDatatypeId.ADST_REAL80:
                            break;

                        case AdsDatatypeId.ADST_BIT:
                            return PrimitiveTypeFlags.Bool;

                        default:
                            return none;
                    }
                    break;
            }
            return (PrimitiveTypeFlags.Numeric | PrimitiveTypeFlags.Float);
        TR_0001:
            return PrimitiveTypeFlags.Numeric;
        }

        internal static PrimitiveTypeFlags GetPrimitiveFlags(ITcAdsDataType type) => 
            GetPrimitiveFlags(type.DataTypeId);

        public static bool IsContainerType(DataTypeCategory cat)
        {
            switch (cat)
            {
                case DataTypeCategory.Array:
                case DataTypeCategory.Struct:
                case DataTypeCategory.FunctionBlock:
                case DataTypeCategory.Program:
                case DataTypeCategory.Function:
                case DataTypeCategory.Pointer:
                case DataTypeCategory.Union:
                    return true;
            }
            return false;
        }

        public static bool IsMarshalledAsBitType(Type managedType) => 
            false;

        internal static bool IsPlcOpenType(Type tp) => 
            ((tp == typeof(TIME)) || ((tp == typeof(LTIME)) || ((tp == typeof(TOD)) || ((tp == typeof(DATE)) || (tp == typeof(DT))))));

        internal static bool IsPrimitive(AdsDatatypeId typeId)
        {
            if (typeId != AdsDatatypeId.ADST_VOID)
            {
                switch (typeId)
                {
                    case AdsDatatypeId.ADST_STRING:
                    case AdsDatatypeId.ADST_WSTRING:
                    case AdsDatatypeId.ADST_MAXTYPES:
                        break;

                    case AdsDatatypeId.ADST_REAL80:
                    case AdsDatatypeId.ADST_BIT:
                        return true;

                    default:
                        return (typeId != AdsDatatypeId.ADST_BIGTYPE);
                }
            }
            return false;
        }

        public static bool IsPrimitiveType(DataTypeCategory cat)
        {
            if ((cat != DataTypeCategory.Primitive) && (cat != DataTypeCategory.Enum))
            {
                switch (cat)
                {
                    case DataTypeCategory.SubRange:
                    case DataTypeCategory.String:
                    case DataTypeCategory.Pointer:
                        break;

                    default:
                        return false;
                }
            }
            return true;
        }

        public byte[] Marshal(object val)
        {
            byte[] data = null;
            if (!this.TryMarshal(val, out data))
            {
                throw new ArgumentOutOfRangeException($"Managed datatype '{val.GetType()}' not supported!", "tp");
            }
            return data;
        }

        public byte[] Marshal(IDataType type, object val)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            byte[] bytes = null;
            IManagedMappableType type2 = type as IManagedMappableType;
            if ((type2 != null) && (type2.ManagedType != null))
            {
                object targetValue = val;
                Type managedType = type2.ManagedType;
                if (managedType != val.GetType())
                {
                    if (type.Category == DataTypeCategory.Enum)
                    {
                        IEnumType type4 = (IEnumType) type;
                        if (val is string)
                        {
                            val = type4.Parse((string) val);
                        }
                    }
                    if (!TryConvert(val, managedType, out targetValue))
                    {
                        string message = $"Cannot convert Value '{val} ({val.GetType()})' to type '{managedType}'";
                        throw new ArgumentOutOfRangeException("value", val, message);
                    }
                }
                bytes = this.Marshal(targetValue);
            }
            else
            {
                DataTypeCategory category = type.Category;
                if (category != DataTypeCategory.Primitive)
                {
                    if (category == DataTypeCategory.Enum)
                    {
                        IEnumType type5 = (IEnumType) type;
                        object obj3 = null;
                        obj3 = !(val is IEnumValue) ? val : ((IEnumValue) val).Primitive;
                        bytes = this.Marshal(type5.BaseType, obj3);
                    }
                    else
                    {
                        switch (category)
                        {
                            case DataTypeCategory.Pointer:
                            case DataTypeCategory.Reference:
                            case DataTypeCategory.Interface:
                                if (type.ByteSize == 4)
                                {
                                    bytes = BitConverter.GetBytes(((IConvertible) val).ToUInt32(null));
                                }
                                else if (type.ByteSize == 8)
                                {
                                    bytes = BitConverter.GetBytes(((IConvertible) val).ToUInt64(null));
                                }
                                break;

                            default:
                                break;
                        }
                    }
                }
                else
                {
                    PrimitiveTypeFlags none = PrimitiveTypeFlags.None;
                    bool flag2 = false;
                    bool flag3 = false;
                    if (type is IPrimitiveType)
                    {
                        none = ((IPrimitiveType) type).PrimitiveFlags;
                    }
                    if (none != PrimitiveTypeFlags.None)
                    {
                        flag2 = (none & PrimitiveTypeFlags.Float) == PrimitiveTypeFlags.Float;
                        flag3 = (none & PrimitiveTypeFlags.Unsigned) == PrimitiveTypeFlags.Unsigned;
                    }
                    if (flag2)
                    {
                        if (type.ByteSize == 2)
                        {
                            bytes = BitConverter.GetBytes(((IConvertible) val).ToSingle(null));
                        }
                        else if (type.ByteSize == 4)
                        {
                            bytes = BitConverter.GetBytes(((IConvertible) val).ToDouble(null));
                        }
                    }
                    else if (type.ByteSize == 1)
                    {
                        if ((none & PrimitiveTypeFlags.Bool) == PrimitiveTypeFlags.Bool)
                        {
                            bytes = BitConverter.GetBytes(((IConvertible) val).ToBoolean(null));
                        }
                        else
                        {
                            bytes = new byte[] { (byte) val };
                        }
                    }
                    else if (type.ByteSize == 2)
                    {
                        bytes = !flag3 ? BitConverter.GetBytes(((IConvertible) val).ToInt16(null)) : BitConverter.GetBytes(((IConvertible) val).ToUInt16(null));
                    }
                    else if (type.ByteSize == 4)
                    {
                        bytes = !flag3 ? BitConverter.GetBytes(((IConvertible) val).ToInt32(null)) : BitConverter.GetBytes(((IConvertible) val).ToUInt32(null));
                    }
                    else if (type.ByteSize == 8)
                    {
                        bytes = !flag3 ? BitConverter.GetBytes(((IConvertible) val).ToInt64(null)) : BitConverter.GetBytes(((IConvertible) val).ToUInt64(null));
                    }
                }
            }
            if (bytes == null)
            {
                throw new DataTypeException("Cannot Convert to byte array!", type);
            }
            return bytes;
        }

        public static int Marshal(AdsDatatypeId typeId, object val, out byte[] data)
        {
            if (val == null)
            {
                throw new ArgumentNullException("data");
            }
            data = null;
            switch (typeId)
            {
                case AdsDatatypeId.ADST_VOID:
                case ((AdsDatatypeId) 1):
                    goto TR_0001;

                case AdsDatatypeId.ADST_INT16:
                    data = BitConverter.GetBytes(((IConvertible) val).ToInt16(null));
                    break;

                case AdsDatatypeId.ADST_INT32:
                    data = BitConverter.GetBytes(((IConvertible) val).ToUInt32(null));
                    break;

                case AdsDatatypeId.ADST_REAL32:
                    data = BitConverter.GetBytes(((IConvertible) val).ToSingle(null));
                    break;

                case AdsDatatypeId.ADST_REAL64:
                    data = BitConverter.GetBytes(((IConvertible) val).ToDouble(null));
                    break;

                default:
                    switch (typeId)
                    {
                        case AdsDatatypeId.ADST_INT8:
                            data = new byte[] { (byte) ((IConvertible) val).ToSByte(null) };
                            break;

                        case AdsDatatypeId.ADST_UINT8:
                            data = new byte[] { ((IConvertible) val).ToByte(null) };
                            break;

                        case AdsDatatypeId.ADST_UINT16:
                            data = BitConverter.GetBytes(((IConvertible) val).ToUInt16(null));
                            break;

                        case AdsDatatypeId.ADST_UINT32:
                            data = BitConverter.GetBytes(((IConvertible) val).ToUInt32(null));
                            break;

                        case AdsDatatypeId.ADST_INT64:
                            data = BitConverter.GetBytes(((IConvertible) val).ToInt64(null));
                            break;

                        case AdsDatatypeId.ADST_UINT64:
                            data = BitConverter.GetBytes(((IConvertible) val).ToUInt64(null));
                            break;

                        case (AdsDatatypeId.ADST_INT64 | AdsDatatypeId.ADST_INT16):
                        case (AdsDatatypeId.ADST_UINT64 | AdsDatatypeId.ADST_INT16):
                        case ((AdsDatatypeId) 0x18):
                        case ((AdsDatatypeId) 0x19):
                        case ((AdsDatatypeId) 0x1a):
                        case ((AdsDatatypeId) 0x1b):
                        case ((AdsDatatypeId) 0x1c):
                        case ((AdsDatatypeId) 0x1d):
                        case AdsDatatypeId.ADST_REAL80:
                        case AdsDatatypeId.ADST_MAXTYPES:
                            goto TR_0001;

                        case AdsDatatypeId.ADST_STRING:
                            data = PlcStringConverter.Default.Marshal((string) val);
                            break;

                        case AdsDatatypeId.ADST_WSTRING:
                            data = PlcStringConverter.Unicode.Marshal((string) val);
                            break;

                        case AdsDatatypeId.ADST_BIT:
                            data = BitConverter.GetBytes(((IConvertible) val).ToBoolean(null));
                            break;

                        default:
                            if (typeId != AdsDatatypeId.ADST_BIGTYPE)
                            {
                            }
                            goto TR_0001;
                    }
                    break;
            }
            return data.Length;
        TR_0001:
            throw new NotSupportedException();
        }

        public int MarshalSize(int strLen) => 
            this._stringConverter.MarshalSize(strLen);

        public int MarshalSize(object val)
        {
            int size = 0;
            TryGetMarshalSize(val, this._encoding, out size);
            return size;
        }

        public static int MarshalSize(Type dataType)
        {
            if (dataType == null)
            {
                throw new ArgumentNullException("dataType");
            }
            int size = 0;
            if (!TryGetMarshalSize(dataType, out size))
            {
                throw new ArgumentOutOfRangeException($"Managed datatype '{dataType}' not supported!", "dataType");
            }
            return size;
        }

        internal static int MarshalSize(AdsDatatypeId typeId)
        {
            switch (typeId)
            {
                case AdsDatatypeId.ADST_VOID:
                case ((AdsDatatypeId) 1):
                    goto TR_0000;

                case AdsDatatypeId.ADST_INT16:
                    goto TR_0003;

                case AdsDatatypeId.ADST_INT32:
                case AdsDatatypeId.ADST_REAL32:
                    goto TR_0004;

                case AdsDatatypeId.ADST_REAL64:
                    break;

                default:
                    switch (typeId)
                    {
                        case AdsDatatypeId.ADST_INT8:
                        case AdsDatatypeId.ADST_UINT8:
                        case AdsDatatypeId.ADST_BIT:
                            return 1;

                        case AdsDatatypeId.ADST_UINT16:
                            goto TR_0003;

                        case AdsDatatypeId.ADST_UINT32:
                            goto TR_0004;

                        case AdsDatatypeId.ADST_INT64:
                        case AdsDatatypeId.ADST_UINT64:
                            goto TR_0005;

                        case (AdsDatatypeId.ADST_INT64 | AdsDatatypeId.ADST_INT16):
                        case (AdsDatatypeId.ADST_UINT64 | AdsDatatypeId.ADST_INT16):
                        case ((AdsDatatypeId) 0x18):
                        case ((AdsDatatypeId) 0x19):
                        case ((AdsDatatypeId) 0x1a):
                        case ((AdsDatatypeId) 0x1b):
                        case ((AdsDatatypeId) 0x1c):
                        case ((AdsDatatypeId) 0x1d):
                        case AdsDatatypeId.ADST_STRING:
                        case AdsDatatypeId.ADST_WSTRING:
                        case AdsDatatypeId.ADST_REAL80:
                        case AdsDatatypeId.ADST_MAXTYPES:
                            break;

                        default:
                            if (typeId != AdsDatatypeId.ADST_BIGTYPE)
                            {
                            }
                            break;
                    }
                    goto TR_0000;
            }
            goto TR_0005;
        TR_0000:
            throw new ArgumentOutOfRangeException("typeId");
        TR_0003:
            return 2;
        TR_0004:
            return 4;
        TR_0005:
            return 8;
        }

        private static bool TryCastArray(Array sourceValue, Type targetType, out object targetValue)
        {
            if (sourceValue == null)
            {
                throw new ArgumentNullException("sourceValue");
            }
            if (targetType == null)
            {
                throw new ArgumentNullException("targetValue");
            }
            if (!targetType.IsArray)
            {
                throw new ArgumentOutOfRangeException("targetType");
            }
            Type type = sourceValue.GetType();
            Array array = sourceValue;
            Type elementType = targetType.GetElementType();
            if (type.GetElementType() == elementType)
            {
                targetValue = sourceValue;
                return true;
            }
            int arrayRank = type.GetArrayRank();
            int[] lengths = new int[arrayRank];
            int[] numArray2 = new int[arrayRank];
            for (int i = 0; i < arrayRank; i++)
            {
                lengths[i] = array.GetLength(i);
                numArray2[i] = array.GetLowerBound(i);
            }
            targetValue = Array.CreateInstance(elementType, lengths);
            for (int j = 0; j < array.Length; j++)
            {
                object obj2 = array.GetValue(j);
                object obj3 = null;
                if (!TryConvert(obj2, elementType, out obj3))
                {
                    targetValue = null;
                    return false;
                }
                ((Array) targetValue).SetValue(obj3, j);
            }
            return true;
        }

        public static bool TryConvert(object sourceValue, Type targetType, out object targetValue)
        {
            bool flag = true;
            if (sourceValue.GetType() == targetType)
            {
                flag = true;
                targetValue = sourceValue;
                return flag;
            }
            if (sourceValue is IConvertible)
            {
                try
                {
                    if (targetType == typeof(bool))
                    {
                        targetValue = ((IConvertible) sourceValue).ToBoolean(null);
                    }
                    else if (targetType == typeof(int))
                    {
                        targetValue = ((IConvertible) sourceValue).ToInt32(null);
                    }
                    else if (targetType == typeof(short))
                    {
                        targetValue = ((IConvertible) sourceValue).ToInt16(null);
                    }
                    else if (targetType == typeof(byte))
                    {
                        targetValue = ((IConvertible) sourceValue).ToByte(null);
                    }
                    else if (targetType == typeof(float))
                    {
                        targetValue = ((IConvertible) sourceValue).ToSingle(null);
                    }
                    else if (targetType == typeof(double))
                    {
                        targetValue = ((IConvertible) sourceValue).ToDouble(null);
                    }
                    else if (targetType == typeof(long))
                    {
                        targetValue = ((IConvertible) sourceValue).ToInt64(null);
                    }
                    else if (targetType == typeof(ulong))
                    {
                        targetValue = ((IConvertible) sourceValue).ToUInt64(null);
                    }
                    else if (targetType == typeof(uint))
                    {
                        targetValue = ((IConvertible) sourceValue).ToUInt32(null);
                    }
                    else if (targetType == typeof(ushort))
                    {
                        targetValue = ((IConvertible) sourceValue).ToUInt16(null);
                    }
                    else if (targetType == typeof(sbyte))
                    {
                        targetValue = (byte) ((IConvertible) sourceValue).ToSByte(null);
                    }
                    else if (targetType == typeof(string))
                    {
                        targetValue = sourceValue.ToString();
                    }
                    else if (targetType == typeof(DATE))
                    {
                        DATE timeOfDay = null;
                        if (PlcOpenDateConverter.TryConvert(sourceValue, out timeOfDay))
                        {
                            targetValue = timeOfDay;
                        }
                        else
                        {
                            targetValue = null;
                            flag = false;
                        }
                    }
                    else if (targetType == typeof(TIME))
                    {
                        TIME time = null;
                        if (PlcOpenTimeConverter.TryConvert(sourceValue, out time))
                        {
                            targetValue = time;
                        }
                        else
                        {
                            targetValue = null;
                            flag = false;
                        }
                    }
                    else if (targetType == typeof(LTIME))
                    {
                        LTIME time = null;
                        if (PlcOpenTimeConverter.TryConvert(sourceValue, out time))
                        {
                            targetValue = time;
                        }
                        else
                        {
                            targetValue = null;
                            flag = false;
                        }
                    }
                    else if (targetType == typeof(DT))
                    {
                        DT timeOfDay = null;
                        if (PlcOpenDTConverter.TryConvert(sourceValue, out timeOfDay))
                        {
                            targetValue = timeOfDay;
                        }
                        else
                        {
                            targetValue = null;
                            flag = false;
                        }
                    }
                    else if (targetType != typeof(TOD))
                    {
                        targetValue = null;
                        flag = false;
                    }
                    else
                    {
                        TOD timeOfDay = null;
                        if (PlcOpenTODConverter.TryConvert(sourceValue, out timeOfDay))
                        {
                            targetValue = timeOfDay;
                        }
                        else
                        {
                            targetValue = null;
                            flag = false;
                        }
                    }
                }
                catch (FormatException)
                {
                    targetValue = null;
                    flag = false;
                }
            }
            else if (sourceValue.GetType() == typeof(BitArray))
            {
                targetValue = BitTypeConverter.ToNumeric(targetType, (BitArray) sourceValue);
            }
            else if (sourceValue.GetType().IsArray && targetType.IsArray)
            {
                flag = TryCastArray((Array) sourceValue, targetType, out targetValue);
            }
            else
            {
                switch (sourceValue)
                {
                    case (IEnumValue _):
                        flag = TryConvert(((IEnumValue) sourceValue).Primitive, targetType, out targetValue);
                        break;

                    case (TimeBase _):
                        flag = PlcOpenTimeConverter.TryConvert((TimeBase) sourceValue, targetType, out targetValue);
                        break;

                    case (DateBase _):
                        flag = PlcOpenDateConverter.TryConvert((DateBase) sourceValue, targetType, out targetValue);
                        break;

                    case (LTimeBase _):
                        flag = PlcOpenTimeConverter.TryConvert((LTimeBase) sourceValue, targetType, out targetValue);
                        break;

                    case (typeof(TIME)):
                        TIME time2;
                        flag = PlcOpenTimeConverter.TryConvert(sourceValue, out time2);
                        targetValue = time2;
                        break;

                    case (typeof(LTIME)):
                        LTIME ltime2;
                        flag = PlcOpenTimeConverter.TryConvert(sourceValue, out ltime2);
                        targetValue = ltime2;
                        break;

                    case (typeof(TOD)):
                        TOD tod2;
                        flag = PlcOpenTODConverter.TryConvert(sourceValue, out tod2);
                        targetValue = tod2;
                        break;

                    case (typeof(DATE)):
                        DATE date2;
                        flag = PlcOpenDateConverter.TryConvert(sourceValue, out date2);
                        targetValue = date2;
                        break;

                    default:
                        targetValue = null;
                        flag = false;
                        break;
                }
            }
            return flag;
        }

        public bool TryGetArrayMarshalSize(AnyTypeSpecifier anyType, out int size)
        {
            if (anyType == null)
            {
                throw new ArgumentNullException("anyType");
            }
            if (!anyType.Type.IsArray)
            {
                throw new ArgumentOutOfRangeException("arrayType");
            }
            size = 0;
            AnyTypeSpecifier elementType = anyType.ElementType;
            int num = 0;
            int num2 = 1;
            if (!this.TryGetMarshalSize(elementType, out num))
            {
                return false;
            }
            for (int i = 0; i < anyType.DimLengths.Count; i++)
            {
                IDimensionCollection dimensions = anyType.DimLengths[i];
                num2 *= dimensions.ElementCount;
            }
            size = num2 * num;
            return true;
        }

        internal static bool TryGetDataTypeId(Type tp, out AdsDatatypeId typeId)
        {
            if (tp == typeof(sbyte))
            {
                typeId = AdsDatatypeId.ADST_INT8;
                return true;
            }
            if (tp == typeof(byte))
            {
                typeId = AdsDatatypeId.ADST_UINT8;
                return true;
            }
            if (tp == typeof(short))
            {
                typeId = AdsDatatypeId.ADST_INT16;
                return true;
            }
            if (tp == typeof(ushort))
            {
                typeId = AdsDatatypeId.ADST_UINT16;
                return true;
            }
            if (tp == typeof(int))
            {
                typeId = AdsDatatypeId.ADST_INT32;
                return true;
            }
            if (tp == typeof(uint))
            {
                typeId = AdsDatatypeId.ADST_UINT32;
                return true;
            }
            if (tp == typeof(long))
            {
                typeId = AdsDatatypeId.ADST_INT64;
                return true;
            }
            if (tp == typeof(ulong))
            {
                typeId = AdsDatatypeId.ADST_UINT64;
                return true;
            }
            if (tp == typeof(float))
            {
                typeId = AdsDatatypeId.ADST_REAL32;
                return true;
            }
            if (tp == typeof(double))
            {
                typeId = AdsDatatypeId.ADST_REAL64;
                return true;
            }
            if (tp == typeof(bool))
            {
                typeId = AdsDatatypeId.ADST_BIT;
                return true;
            }
            typeId = AdsDatatypeId.ADST_BIGTYPE;
            return false;
        }

        internal static bool TryGetManagedType(AdsDatatypeId typeId, out Type tp)
        {
            tp = null;
            switch (typeId)
            {
                case AdsDatatypeId.ADST_INT16:
                    tp = typeof(short);
                    break;

                case AdsDatatypeId.ADST_INT32:
                    tp = typeof(int);
                    break;

                case AdsDatatypeId.ADST_REAL32:
                    tp = typeof(float);
                    break;

                case AdsDatatypeId.ADST_REAL64:
                    tp = typeof(double);
                    break;

                default:
                    switch (typeId)
                    {
                        case AdsDatatypeId.ADST_INT8:
                            tp = typeof(sbyte);
                            break;

                        case AdsDatatypeId.ADST_UINT8:
                            tp = typeof(byte);
                            break;

                        case AdsDatatypeId.ADST_UINT16:
                            tp = typeof(ushort);
                            break;

                        case AdsDatatypeId.ADST_UINT32:
                            tp = typeof(uint);
                            break;

                        case AdsDatatypeId.ADST_INT64:
                            tp = typeof(long);
                            break;

                        case AdsDatatypeId.ADST_UINT64:
                            tp = typeof(ulong);
                            break;

                        default:
                            switch (typeId)
                            {
                                case AdsDatatypeId.ADST_STRING:
                                case AdsDatatypeId.ADST_WSTRING:
                                    tp = typeof(string);
                                    break;

                                case AdsDatatypeId.ADST_BIT:
                                    tp = typeof(bool);
                                    break;

                                default:
                                    break;
                            }
                            break;
                    }
                    break;
            }
            return (tp != null);
        }

        internal static bool TryGetManagedType(ITcAdsDataType dataType, out Type managedType)
        {
            bool flag = false;
            Type managed = null;
            flag = TryGetManagedType((IDataType) dataType, out managed);
            if (!flag)
            {
                flag = (dataType.BaseType == null) ? TryGetManagedType(dataType.DataTypeId, out managed) : TryGetManagedType(dataType.BaseType, out managed);
                if (flag && (dataType.Category == DataTypeCategory.Array))
                {
                    int count = dataType.Dimensions.Count;
                    managed = (count <= 1) ? managed.MakeArrayType() : managed.MakeArrayType(count);
                }
            }
            managedType = managed;
            return flag;
        }

        internal static bool TryGetManagedType(IDataType type, out Type managed)
        {
            managed = null;
            IResolvableType type2 = type as IResolvableType;
            if (type2 != null)
            {
                type = type2.ResolveType(DataTypeResolveStrategy.AliasReference);
            }
            IManagedMappableType type3 = type as IManagedMappableType;
            managed = null;
            if (type3 != null)
            {
                managed = type3.ManagedType;
            }
            else
            {
                PrimitiveTypeFlags none = PrimitiveTypeFlags.None;
                DataTypeCategory category = type.Category;
                switch (category)
                {
                    case DataTypeCategory.Primitive:
                    {
                        bool flag = false;
                        bool flag2 = false;
                        bool flag3 = false;
                        if (type is IPrimitiveType)
                        {
                            none = ((IPrimitiveType) type).PrimitiveFlags;
                            flag3 = (none & PrimitiveTypeFlags.Bool) == PrimitiveTypeFlags.Bool;
                        }
                        if (none != PrimitiveTypeFlags.None)
                        {
                            flag = (none & PrimitiveTypeFlags.Float) == PrimitiveTypeFlags.Float;
                            flag2 = (none & PrimitiveTypeFlags.Unsigned) == PrimitiveTypeFlags.Unsigned;
                        }
                        if (flag)
                        {
                            if (type.ByteSize == 2)
                            {
                                managed = typeof(float);
                            }
                            else if (type.ByteSize == 4)
                            {
                                managed = typeof(double);
                            }
                        }
                        else if (type.ByteSize == 1)
                        {
                            managed = !flag3 ? (!flag2 ? typeof(sbyte) : typeof(byte)) : typeof(bool);
                        }
                        else if (type.ByteSize == 2)
                        {
                            managed = !flag2 ? typeof(short) : typeof(ushort);
                        }
                        else if (type.ByteSize == 4)
                        {
                            managed = !flag2 ? typeof(int) : typeof(uint);
                        }
                        else
                        {
                            if (type.ByteSize != 8)
                            {
                                throw new DataTypeException("Cannot unmarshal type '{0}'!", type);
                            }
                            managed = !flag2 ? typeof(long) : typeof(ulong);
                        }
                        break;
                    }
                    case DataTypeCategory.Alias:
                    case DataTypeCategory.Array:
                        break;

                    case DataTypeCategory.Enum:
                        return TryGetManagedType((type as IEnumType).BaseType, out managed);

                    default:
                        switch (category)
                        {
                            case DataTypeCategory.String:
                                managed = typeof(string);
                                break;

                            case DataTypeCategory.Pointer:
                            case DataTypeCategory.Reference:
                            case DataTypeCategory.Interface:
                                if (type.ByteSize == 4)
                                {
                                    managed = typeof(uint);
                                }
                                else if (type.ByteSize == 8)
                                {
                                    managed = typeof(ulong);
                                }
                                break;

                            default:
                                break;
                        }
                        break;
                }
            }
            return (managed != null);
        }

        public bool TryGetMarshalSize(object managedValue, out int size)
        {
            bool flag = false;
            if (managedValue is string)
            {
                string str = (string) managedValue;
                flag = this.TryGetStringMarshalSize(str.Length, out size);
            }
            else if (!(managedValue is Array))
            {
                flag = TryGetMarshalSize(managedValue, this._encoding, out size);
            }
            else
            {
                AnyTypeSpecifier anyType = new AnyTypeSpecifier(managedValue);
                flag = this.TryGetArrayMarshalSize(anyType, out size);
            }
            return flag;
        }

        public static bool TryGetMarshalSize(Type dataType, out int size)
        {
            if (dataType == null)
            {
                throw new ArgumentNullException("dataType");
            }
            size = 0;
            if (dataType == typeof(bool))
            {
                size = 1;
            }
            else if (dataType == typeof(int))
            {
                size = 4;
            }
            else if (dataType == typeof(short))
            {
                size = 2;
            }
            else if (dataType == typeof(byte))
            {
                size = 1;
            }
            else if (dataType == typeof(float))
            {
                size = 4;
            }
            else if (dataType == typeof(double))
            {
                size = 8;
            }
            else if (dataType == typeof(long))
            {
                size = 8;
            }
            else if (dataType == typeof(ulong))
            {
                size = 8;
            }
            else if (dataType == typeof(uint))
            {
                size = 4;
            }
            else if (dataType == typeof(ushort))
            {
                size = 2;
            }
            else if (dataType == typeof(sbyte))
            {
                size = 1;
            }
            else if (dataType == typeof(TimeSpan))
            {
                size = 4;
            }
            else if (dataType == typeof(DateTime))
            {
                size = 4;
            }
            else if (dataType == typeof(DT))
            {
                size = 4;
            }
            else if (dataType == typeof(DATE))
            {
                size = 4;
            }
            else if (dataType == typeof(TIME))
            {
                size = 4;
            }
            else if (dataType == typeof(LTIME))
            {
                size = 8;
            }
            else if (dataType == typeof(TOD))
            {
                size = 4;
            }
            else if (dataType != typeof(string))
            {
                bool isArray = dataType.IsArray;
            }
            return (size > 0);
        }

        public bool TryGetMarshalSize(AnyTypeSpecifier anyType, out int size)
        {
            if (anyType == null)
            {
                throw new ArgumentNullException("anyType");
            }
            return ((anyType.Category != DataTypeCategory.Array) ? ((anyType.Category != DataTypeCategory.String) ? TryGetMarshalSize(anyType.Type, out size) : this.TryGetStringMarshalSize(anyType.StrLen, out size)) : this.TryGetArrayMarshalSize(anyType, out size));
        }

        public static bool TryGetMarshalSize(object managedValue, Encoding encoding, out int size)
        {
            if (managedValue == null)
            {
                throw new ArgumentNullException("val");
            }
            Type managedType = null;
            managedType = !(managedValue is Type) ? managedValue.GetType() : ((Type) managedValue);
            size = 0;
            if (managedType == typeof(BitArray))
            {
                BitArray array = (BitArray) managedValue;
                size = array.Count / 8;
            }
            else
            {
                if (managedType.IsArray)
                {
                    Array array2 = (Array) managedValue;
                    Type elementType = managedType.GetElementType();
                    int length = array2.Length;
                    Array array3 = array2;
                    while (elementType.IsArray)
                    {
                        int num3 = 0;
                        int index = 0;
                        while (true)
                        {
                            if (index >= array3.Length)
                            {
                                elementType = elementType.GetElementType();
                                length *= num3;
                                break;
                            }
                            Array array4 = (Array) array3.GetValue(index);
                            num3 = (num3 < array4.Length) ? array4.Length : num3;
                            index++;
                        }
                    }
                    int num2 = MarshalSize(elementType);
                    size = !IsMarshalledAsBitType(managedType) ? (length * num2) : ((length / 8) + (((length % 8) > 0) ? 1 : 0));
                    return true;
                }
                if (!(managedValue is string))
                {
                    return TryGetMarshalSize(managedType, out size);
                }
                size = PlcStringConverter.MarshalSize((string) managedValue, encoding, StringConvertMode.ZeroTerminated);
            }
            return (size > 0);
        }

        public bool TryGetStringMarshalSize(int strLen, out int size)
        {
            size = 0;
            size = this._stringConverter.MarshalSize(strLen);
            return (size != 0);
        }

        public static bool TryJaggedArray(Type jaggedArray, out int jagLevel, out Type baseElementType)
        {
            if (jaggedArray == null)
            {
                throw new ArgumentNullException("managedType");
            }
            bool flag = false;
            jagLevel = 0;
            baseElementType = null;
            if (jaggedArray.IsArray)
            {
                Type elementType = jaggedArray.GetElementType();
                if ((elementType != null) && elementType.IsArray)
                {
                    int num = 0;
                    Type type2 = jaggedArray;
                    while (true)
                    {
                        if ((type2 == null) || !type2.IsArray)
                        {
                            jagLevel = num;
                            baseElementType = type2;
                            flag = true;
                            break;
                        }
                        num++;
                        type2 = type2.GetElementType();
                    }
                }
            }
            return flag;
        }

        public static bool TryJaggedArray(Array array, out int jagLevel, out Type baseElementType, out int jaggedElementCount)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            Type jaggedArray = array.GetType();
            bool flag = TryJaggedArray(jaggedArray, out jagLevel, out baseElementType);
            jaggedElementCount = 0;
            if (flag)
            {
                int rank = array.Rank;
                ArrayIndexIterator iterator = new ArrayIndexIterator(array);
                int num2 = 1;
                int length = array.Length;
                if (jaggedArray.GetElementType().IsArray)
                {
                    foreach (int[] numArray in iterator)
                    {
                        int num4;
                        Type type2;
                        int num5;
                        Array array2 = (Array) array.GetValue(numArray);
                        if (TryJaggedArray(array2, out num4, out type2, out num5))
                        {
                            num2 = Math.Max(num5, num2);
                        }
                    }
                }
                jaggedElementCount = num2 * array.Length;
            }
            return flag;
        }

        public bool TryMarshal(object val, out byte[] data) => 
            this.TryMarshal(val, -1, out data);

        internal bool TryMarshal(object val, int marshalSize2, out byte[] data)
        {
            if (val == null)
            {
                throw new ArgumentNullException("val");
            }
            Type type = val.GetType();
            data = null;
            if (type != typeof(bool))
            {
                if (type != typeof(int))
                {
                    if (type != typeof(short))
                    {
                        if (type != typeof(byte))
                        {
                            if (type != typeof(float))
                            {
                                if (type != typeof(double))
                                {
                                    if (type != typeof(long))
                                    {
                                        if (type != typeof(ulong))
                                        {
                                            if (type != typeof(uint))
                                            {
                                                if (type != typeof(ushort))
                                                {
                                                    if (type != typeof(sbyte))
                                                    {
                                                        if (type != typeof(TimeSpan))
                                                        {
                                                            if (type != typeof(DateTime))
                                                            {
                                                                if (type != typeof(DT))
                                                                {
                                                                    if (type != typeof(DATE))
                                                                    {
                                                                        if (type != typeof(TIME))
                                                                        {
                                                                            if (type != typeof(LTIME))
                                                                            {
                                                                                if (type != typeof(TOD))
                                                                                {
                                                                                    if (type != typeof(BitArray))
                                                                                    {
                                                                                        if (type.IsArray)
                                                                                        {
                                                                                            Array array2 = (Array) val;
                                                                                            int size = 0;
                                                                                            int marshalled = 0;
                                                                                            if (!this.TryGetMarshalSize(val, out size))
                                                                                            {
                                                                                                return false;
                                                                                            }
                                                                                            data = new byte[size];
                                                                                            using (MemoryStream stream = new MemoryStream(data))
                                                                                            {
                                                                                                this.TryMarshalArray(array2, stream, size, out marshalled);
                                                                                                goto TR_0001;
                                                                                            }
                                                                                        }
                                                                                        if (type == typeof(string))
                                                                                        {
                                                                                            data = this._stringConverter.Marshal((string) val, marshalSize2);
                                                                                        }
                                                                                    }
                                                                                    else
                                                                                    {
                                                                                        BitArray array = (BitArray) val;
                                                                                        int num = array.Count / 8;
                                                                                        if ((array.Count % 8) > 0)
                                                                                        {
                                                                                            num++;
                                                                                        }
                                                                                        data = new byte[num];
                                                                                        array.CopyTo(data, 0);
                                                                                    }
                                                                                }
                                                                                else
                                                                                {
                                                                                    data = PlcOpenTODConverter.GetBytes((TOD) val);
                                                                                }
                                                                            }
                                                                            else
                                                                            {
                                                                                data = PlcOpenTimeConverter.GetBytes((LTIME) val);
                                                                            }
                                                                        }
                                                                        else
                                                                        {
                                                                            data = PlcOpenTimeConverter.GetBytes((TIME) val);
                                                                        }
                                                                    }
                                                                    else
                                                                    {
                                                                        data = PlcOpenDateConverter.GetBytes((DATE) val);
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    data = PlcOpenDTConverter.GetBytes((DT) val);
                                                                }
                                                            }
                                                            else
                                                            {
                                                                data = PlcOpenDateConverterBase.GetBytes((DateTime) val);
                                                            }
                                                        }
                                                        else
                                                        {
                                                            TIME time = new TIME((TimeSpan) val);
                                                            data = PlcOpenTimeConverter.GetBytes(time);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        data = new byte[] { (byte) ((IConvertible) val).ToSByte(null) };
                                                    }
                                                }
                                                else
                                                {
                                                    data = BitConverter.GetBytes(((IConvertible) val).ToUInt16(null));
                                                }
                                            }
                                            else
                                            {
                                                data = BitConverter.GetBytes(((IConvertible) val).ToUInt32(null));
                                            }
                                        }
                                        else
                                        {
                                            data = BitConverter.GetBytes(((IConvertible) val).ToUInt64(null));
                                        }
                                    }
                                    else
                                    {
                                        data = BitConverter.GetBytes(((IConvertible) val).ToInt64(null));
                                    }
                                }
                                else
                                {
                                    data = BitConverter.GetBytes(((IConvertible) val).ToDouble(null));
                                }
                            }
                            else
                            {
                                data = BitConverter.GetBytes(((IConvertible) val).ToSingle(null));
                            }
                        }
                        else
                        {
                            data = new byte[] { ((IConvertible) val).ToByte(null) };
                        }
                    }
                    else
                    {
                        data = BitConverter.GetBytes(((IConvertible) val).ToInt16(null));
                    }
                }
                else
                {
                    data = BitConverter.GetBytes(((IConvertible) val).ToInt32(null));
                }
            }
            else
            {
                data = BitConverter.GetBytes(((IConvertible) val).ToBoolean(null));
            }
        TR_0001:
            return (data != null);
        }

        internal bool TryMarshalArray(object value, Stream stream, int marshalSize2, out int marshalled)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            Type type = value.GetType();
            if (!type.IsArray)
            {
                throw new ArgumentOutOfRangeException("value", "No ArrayType");
            }
            marshalled = 0;
            if (type != typeof(byte[]))
            {
                if (type != typeof(char[]))
                {
                    if (!type.IsArray)
                    {
                        return false;
                    }
                    else
                    {
                        Array array = (Array) value;
                        int length = array.Length;
                        Type elementType = value.GetType().GetElementType();
                        if (!IsMarshalledAsBitType(value.GetType()))
                        {
                            int size = 0;
                            if (!TryGetMarshalSize(elementType, out size))
                            {
                                size = marshalSize2 / length;
                            }
                            foreach (int[] numArray in new ArrayIndexIterator(array))
                            {
                                byte[] data = null;
                                object val = array.GetValue(numArray);
                                bool flag2 = this.TryMarshal(val, out data);
                                marshalled += data.Length;
                                stream.Write(data, 0, data.Length);
                            }
                        }
                        else
                        {
                            byte[] buffer3 = new byte[marshalSize2];
                            new BitArray((bool[]) value).CopyTo(buffer3, 0);
                            stream.Write(buffer3, 0, buffer3.Length);
                            marshalled += buffer3.Length;
                        }
                    }
                }
                else
                {
                    byte[] bytes = this._encoding.GetBytes((char[]) value);
                    if (bytes.Length != marshalSize2)
                    {
                        throw new ArgumentOutOfRangeException("value");
                    }
                    stream.Write(bytes, 0, bytes.Length);
                    marshalled += bytes.Length;
                }
            }
            else
            {
                byte[] buffer = (byte[]) value;
                if (buffer.Length != marshalSize2)
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                stream.Write(buffer, 0, buffer.Length);
                marshalled += buffer.Length;
            }
            return true;
        }

        public int Unmarshal(ITcAdsDataType type, byte[] data, int offset, out object val)
        {
            Type type2;
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }
            if (!IsPrimitiveType(type.Category))
            {
                throw new DataTypeException("Type is not primitive!", type);
            }
            val = null;
            if (TryGetManagedType(type, out type2))
            {
                if (type.DataTypeId == AdsDatatypeId.ADST_WSTRING)
                {
                    string str;
                    PlcStringConverter.Unicode.Unmarshal(data, offset, data.Length - offset, out str);
                    val = str;
                }
                else if (type.DataTypeId != AdsDatatypeId.ADST_STRING)
                {
                    int num = this.UnmarshalPrimitive(type2, data, offset, type.ByteSize, out val);
                }
                else
                {
                    string str2;
                    PlcStringConverter.Default.Unmarshal(data, offset, data.Length - offset, out str2);
                    val = str2;
                }
            }
            if (val == null)
            {
                throw new DataTypeException("Cannot map to .NET Value!", type);
            }
            return type.ByteSize;
        }

        public int Unmarshal(byte[] data, int offset, int strLen, out string value)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }
            if ((offset < 0) || (offset >= data.Length))
            {
                throw new ArgumentOutOfRangeException("offset");
            }
            int byteCount = this._encoding.GetByteCount("a");
            int num2 = -1;
            if (strLen >= 0)
            {
                num2 = strLen * byteCount;
            }
            this._stringConverter.Unmarshal(data, offset, num2, out value);
            return num2;
        }

        public int Unmarshal<T>(byte[] data, int offset, int marshalSize, out T val)
        {
            object obj2 = null;
            val = default(T);
            int num = 0;
            if (!IsMarshalledAsBitType(typeof(T)))
            {
                num = this.UnmarshalPrimitive(typeof(T), data, offset, marshalSize, out obj2);
            }
            else
            {
                bool[] flagArray = null;
                num = this.UnmarshalBits(typeof(T), data, offset, marshalSize, out flagArray);
                obj2 = flagArray;
            }
            val = (T) obj2;
            return num;
        }

        public int Unmarshal<T>(AnyTypeSpecifier typeSpecifier, byte[] data, int offset, out T val)
        {
            int num;
            this.TryGetMarshalSize(typeSpecifier, out num);
            object obj2 = null;
            val = default(T);
            int num2 = 0;
            if (!IsMarshalledAsBitType(typeof(T)))
            {
                num2 = this.Unmarshal(typeSpecifier, data, offset, num, out obj2);
            }
            else
            {
                bool[] flagArray = null;
                num2 = this.UnmarshalBits(typeof(T), data, offset, num, out flagArray);
                obj2 = flagArray;
            }
            val = (T) obj2;
            return num2;
        }

        internal static int Unmarshal(AdsDatatypeId typeId, bool isBitType, byte[] data, int offset, out object val)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }
            if (offset < 0)
            {
                goto TR_0001;
            }
            else if (offset < data.Length)
            {
                if (data == null)
                {
                    throw new ArgumentNullException("data");
                }
                switch (typeId)
                {
                    case AdsDatatypeId.ADST_INT16:
                        val = BitConverter.ToInt16(data, offset);
                        break;

                    case AdsDatatypeId.ADST_INT32:
                        val = BitConverter.ToInt32(data, offset);
                        break;

                    case AdsDatatypeId.ADST_REAL32:
                        val = BitConverter.ToSingle(data, offset);
                        break;

                    case AdsDatatypeId.ADST_REAL64:
                        val = BitConverter.ToDouble(data, offset);
                        break;

                    default:
                        switch (typeId)
                        {
                            case AdsDatatypeId.ADST_INT8:
                                val = (sbyte) data[offset];
                                break;

                            case AdsDatatypeId.ADST_UINT8:
                                val = data[offset];
                                break;

                            case AdsDatatypeId.ADST_UINT16:
                                val = BitConverter.ToUInt16(data, offset);
                                break;

                            case AdsDatatypeId.ADST_UINT32:
                                val = BitConverter.ToUInt32(data, offset);
                                break;

                            case AdsDatatypeId.ADST_INT64:
                                val = BitConverter.ToInt64(data, offset);
                                break;

                            case AdsDatatypeId.ADST_UINT64:
                                val = BitConverter.ToUInt64(data, offset);
                                break;

                            case (AdsDatatypeId.ADST_INT64 | AdsDatatypeId.ADST_INT16):
                            case (AdsDatatypeId.ADST_UINT64 | AdsDatatypeId.ADST_INT16):
                            case ((AdsDatatypeId) 0x18):
                            case ((AdsDatatypeId) 0x19):
                            case ((AdsDatatypeId) 0x1a):
                            case ((AdsDatatypeId) 0x1b):
                            case ((AdsDatatypeId) 0x1c):
                            case ((AdsDatatypeId) 0x1d):
                            case AdsDatatypeId.ADST_STRING:
                            case AdsDatatypeId.ADST_WSTRING:
                            case AdsDatatypeId.ADST_REAL80:
                                goto TR_0003;

                            case AdsDatatypeId.ADST_BIT:
                                val = BitConverter.ToBoolean(data, offset);
                                break;

                            default:
                                if (typeId != AdsDatatypeId.ADST_BIGTYPE)
                                {
                                }
                                goto TR_0003;
                        }
                        break;
                }
                return MarshalSize(typeId);
            }
            else
            {
                goto TR_0001;
            }
            goto TR_0003;
        TR_0001:
            throw new ArgumentOutOfRangeException("offset");
        TR_0003:
            throw new ArgumentOutOfRangeException("tp");
        }

        public int Unmarshal(AnyTypeSpecifier typeSpecifier, byte[] data, int offset, int marshalSize, out object val)
        {
            bool bitSize = IsMarshalledAsBitType(typeSpecifier.Type);
            return this.Unmarshal(typeSpecifier, bitSize, data, offset, marshalSize, out val);
        }

        internal int Unmarshal(AnyTypeSpecifier typeSpec, bool bitSize, byte[] data, int offset, int marshalSize, out object val)
        {
            if (typeSpec == null)
            {
                throw new ArgumentNullException("typespec");
            }
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }
            if ((offset < 0) || (offset >= data.Length))
            {
                throw new ArgumentOutOfRangeException("offset");
            }
            if (marshalSize > (data.Length - offset))
            {
                throw new ArgumentOutOfRangeException("marshalSize");
            }
            if (marshalSize < 0)
            {
                marshalSize = data.Length - offset;
            }
            int num = 0;
            Type type = typeSpec.Type;
            if (type == typeof(bool))
            {
                val = BitConverter.ToBoolean(data, offset);
                num = 1;
            }
            else if (type == typeof(int))
            {
                val = BitConverter.ToInt32(data, offset);
                num = 4;
            }
            else if (type == typeof(short))
            {
                val = BitConverter.ToInt16(data, offset);
                num = 2;
            }
            else if (type == typeof(byte))
            {
                val = data[offset];
                num = 1;
            }
            else if (type == typeof(float))
            {
                val = BitConverter.ToSingle(data, offset);
                num = 4;
            }
            else if (type == typeof(double))
            {
                val = BitConverter.ToDouble(data, offset);
                num = 8;
            }
            else if (type == typeof(long))
            {
                val = BitConverter.ToInt64(data, offset);
                num = 8;
            }
            else if (type == typeof(uint))
            {
                val = BitConverter.ToUInt32(data, offset);
                num = 4;
            }
            else if (type == typeof(ulong))
            {
                val = BitConverter.ToUInt64(data, offset);
                num = 8;
            }
            else if (type == typeof(ushort))
            {
                val = BitConverter.ToUInt16(data, offset);
                num = 2;
            }
            else if (type == typeof(sbyte))
            {
                val = (sbyte) data[offset];
                num = 1;
            }
            else if (type == typeof(TimeSpan))
            {
                uint milliseconds = BitConverter.ToUInt32(data, offset);
                val = PlcOpenTimeConverter.MillisecondsToTimeSpan(milliseconds);
                num = 4;
            }
            else if (type == typeof(DateTime))
            {
                uint dateValue = BitConverter.ToUInt32(data, offset);
                val = PlcOpenDateConverterBase.ToDateTime(dateValue);
                num = 4;
            }
            else if (type == typeof(DT))
            {
                uint dateValue = BitConverter.ToUInt32(data, offset);
                val = new DT(dateValue);
                num = 4;
            }
            else if (type == typeof(DATE))
            {
                uint dateValue = BitConverter.ToUInt32(data, offset);
                val = new DATE(dateValue);
                num = 4;
            }
            else if (type == typeof(TIME))
            {
                uint timeValue = BitConverter.ToUInt32(data, offset);
                val = new TIME(timeValue);
                num = 4;
            }
            else if (type == typeof(LTIME))
            {
                ulong timeValue = BitConverter.ToUInt64(data, offset);
                val = new LTIME(timeValue);
                num = 8;
            }
            else if (type == typeof(TOD))
            {
                uint time = BitConverter.ToUInt32(data, offset);
                val = new TOD(time);
                num = 4;
            }
            else if (type.IsArray)
            {
                num = this.UnmarshalArray(typeSpec, data, offset, marshalSize, out val);
            }
            else
            {
                if (type != typeof(string))
                {
                    throw new AdsDatatypeNotSupportedException($"Cannot marshal managed type '{type}'");
                }
                string str = null;
                num = this._stringConverter.Unmarshal(data, offset, marshalSize, out str);
                val = str;
            }
            return num;
        }

        internal int UnmarshalArray(AnyTypeSpecifier typeSpec, byte[] data, int offset, int marshalSize, out object val)
        {
            if (typeSpec == null)
            {
                throw new ArgumentNullException("typeSpec");
            }
            int num = 0;
            bool flag = false;
            AnyTypeSpecifier elementType = typeSpec.ElementType;
            if (elementType == null)
            {
                throw new ArgumentOutOfRangeException("typeSpec");
            }
            int size = 0;
            if (!this.TryGetMarshalSize(elementType, out size) && (marshalSize > 0))
            {
                size = marshalSize / typeSpec.DimLengths[0].ElementCount;
            }
            if (marshalSize < 0)
            {
                marshalSize = data.Length - offset;
            }
            Array array = null;
            if (flag)
            {
                bool[] flagArray = null;
                num = this.UnmarshalBits(typeSpec.Type, data, offset, marshalSize, out flagArray);
                array = flagArray;
            }
            else
            {
                array = Array.CreateInstance(elementType.Type, typeSpec.DimLengths[0].GetDimensionLengths());
                int num3 = 0;
                int num4 = offset;
                PrimitiveTypeConverter defaultFixedLengthString = DefaultFixedLengthString;
                foreach (int[] numArray in new ArrayIndexIterator(typeSpec.DimLengths[0].LowerBounds, typeSpec.DimLengths[0].UpperBounds, true))
                {
                    object obj2 = null;
                    int num5 = 0;
                    num5 = defaultFixedLengthString.Unmarshal(typeSpec.ElementType, data, num4, size, out obj2);
                    num4 += num5;
                    num += num5;
                    array.SetValue(obj2, numArray);
                    num3++;
                }
            }
            val = array;
            return num;
        }

        public int UnmarshalBits(Type arrayType, byte[] data, int byteOffset, int marshalSize, out bool[] val)
        {
            if (arrayType == null)
            {
                throw new ArgumentNullException("arrayType");
            }
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }
            if (marshalSize <= 0)
            {
                throw new ArgumentOutOfRangeException("marshalSize");
            }
            Type elementType = arrayType.GetElementType();
            int num = 0;
            num = ((data.Length - byteOffset) / 1) * 8;
            val = new bool[num];
            byte[] destinationArray = new byte[marshalSize];
            Array.Copy(data, byteOffset, destinationArray, 0, marshalSize);
            new BitArray(destinationArray).CopyTo(val, 0);
            return marshalSize;
        }

        public int UnmarshalPrimitive(Type type, byte[] data, int offset, int marshalSize, out object val)
        {
            AnyTypeSpecifier typeSpecifier = null;
            if (!type.IsArray)
            {
                typeSpecifier = new AnyTypeSpecifier(type);
            }
            else
            {
                Type elementType = type.GetElementType();
                int num = MarshalSize(elementType);
                int elementCount = marshalSize / num;
                if (type.GetArrayRank() > 1)
                {
                    throw new MarshalException($"Cannot marshal type '{type.Name}'!");
                }
                if (elementType == typeof(byte))
                {
                    byte[] destinationArray = new byte[elementCount];
                    Array.Copy(data, offset, destinationArray, 0, elementCount * num);
                    val = destinationArray;
                    return (elementCount * num);
                }
                List<IDimensionCollection> dimLengths = new List<IDimensionCollection>();
                DimensionCollection item = new DimensionCollection {
                    new Dimension(0, elementCount)
                };
                dimLengths.Add(item);
                typeSpecifier = new AnyTypeSpecifier(type, dimLengths);
            }
            return this.Unmarshal(typeSpecifier, data, offset, marshalSize, out val);
        }

        public static PrimitiveTypeConverter Default =>
            new PrimitiveTypeConverter(s_DefaultEncoding);

        internal static PrimitiveTypeConverter DefaultFixedLengthString =>
            new PrimitiveTypeConverter(s_DefaultEncoding, StringConvertMode.FixedLengthZeroTerminated);

        public static PrimitiveTypeConverter Unicode =>
            new PrimitiveTypeConverter(Encoding.Unicode);
    }
}

