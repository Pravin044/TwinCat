namespace TwinCAT.TypeSystem
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Text;
    using TwinCAT.Ads;
    using TwinCAT.Ads.TypeSystem;

    public class DynamicValueConverter : ISymbolMarshaller
    {
        private InstanceValueConverter _internalConverter = new InstanceValueConverter();
        private Dictionary<Type, List<IDataType>> _checkedTypesDict = new Dictionary<Type, List<IDataType>>();

        private void CheckType(IDataType type, Type targetType)
        {
            List<IDataType> list = null;
            if (this._checkedTypesDict.TryGetValue(targetType, out list) && list.Contains(type))
            {
                return;
            }
            switch (type.Category)
            {
                case DataTypeCategory.Primitive:
                case DataTypeCategory.Pointer:
                case DataTypeCategory.Reference:
                    if (type.ByteSize > PrimitiveTypeConverter.MarshalSize(targetType))
                    {
                        throw new MarshalException($"Source type '{type.Name}' is larger than target type '{targetType.Name}'!");
                    }
                    goto TR_0007;

                case DataTypeCategory.Alias:
                {
                    IAliasType type2 = (IAliasType) type;
                    try
                    {
                        this.CheckType(type2.BaseType, targetType);
                    }
                    catch (MarshalException exception)
                    {
                        throw new MarshalException($"Cannot Marshal Alias '{type2.Name}' !", exception);
                    }
                    goto TR_0007;
                }
                case DataTypeCategory.Enum:
                {
                    IEnumType type3 = (IEnumType) type;
                    if (!targetType.IsEnum)
                    {
                        IManagedMappableType baseType = type3.BaseType as IManagedMappableType;
                        bool flag = false;
                        if (baseType == null)
                        {
                            throw new MarshalException($"Type '{targetType.Name}' is not an enum type or enum base type!");
                        }
                        flag = baseType.ManagedType == targetType;
                    }
                    else
                    {
                        string[] names = type3.EnumValues.GetNames();
                        string[] strArray2 = Enum.GetNames(targetType);
                        if (names.Length > strArray2.Length)
                        {
                            throw new MarshalException($"Enum Types '{type.Name}' and '{targetType.Name}' are not compatible!");
                        }
                        StringComparer ordinalIgnoreCase = StringComparer.OrdinalIgnoreCase;
                        string[] strArray3 = names;
                        int index = 0;
                        while (index < strArray3.Length)
                        {
                            string x = strArray3[index];
                            bool flag2 = false;
                            string[] strArray4 = strArray2;
                            int num5 = 0;
                            while (true)
                            {
                                if (num5 < strArray4.Length)
                                {
                                    string y = strArray4[num5];
                                    if (ordinalIgnoreCase.Compare(x, y) != 0)
                                    {
                                        num5++;
                                        continue;
                                    }
                                    flag2 = true;
                                }
                                if (!flag2)
                                {
                                    throw new MarshalException($"Enum Types '{type.Name}' and '{targetType.Name}' are not compatible!");
                                }
                                index++;
                                break;
                            }
                        }
                    }
                    goto TR_0007;
                }
                case DataTypeCategory.Array:
                {
                    IArrayType type4 = (IArrayType) type;
                    if (!targetType.IsArray)
                    {
                        throw new MarshalException($"Type '{targetType.Name}' is not an array type!");
                    }
                    int arrayRank = targetType.GetArrayRank();
                    if (type4.Dimensions.Count != arrayRank)
                    {
                        throw new MarshalException($"Array Types '{type.Name}' and '{targetType.Name}' are not compatible!");
                    }
                    Type elementType = targetType.GetElementType();
                    try
                    {
                        this.CheckType(type4.ElementType, elementType);
                    }
                    catch (MarshalException exception2)
                    {
                        throw new MarshalException($"Cannot Marshal Elements of Array '{type4.Name}'!", exception2);
                    }
                    goto TR_0007;
                }
                case DataTypeCategory.Struct:
                    foreach (IMember member in ((IStructType) type).AllMembers)
                    {
                        PropertyInfo property = targetType.GetProperty(member.InstanceName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                        if (property != null)
                        {
                            this.CheckType(member.DataType, property.PropertyType);
                            continue;
                        }
                        FieldInfo field = targetType.GetField(member.InstanceName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                        if (field != null)
                        {
                            Type fieldType = field.FieldType;
                            try
                            {
                                this.CheckType(member.DataType, fieldType);
                            }
                            catch (MarshalException exception3)
                            {
                                IStructType type6;
                                throw new MarshalException($"Cannot Marshal Member '{member.InstanceName}' of Source Struct '{type6.Name}' to field '{field.Name}' of target struct '{targetType.Name}'!", exception3);
                            }
                        }
                    }
                    goto TR_0007;

                case DataTypeCategory.SubRange:
                {
                    ISubRangeType type9 = (ISubRangeType) type;
                    try
                    {
                        this.CheckType(type9.BaseType, targetType);
                    }
                    catch (MarshalException exception4)
                    {
                        throw new MarshalException($"Cannot Marshal Subrange '{type9.Name}'!", exception4);
                    }
                    goto TR_0007;
                }
                case DataTypeCategory.String:
                    break;

                case DataTypeCategory.Bitset:
                case DataTypeCategory.Union:
                    goto TR_0007;

                default:
                    throw new NotSupportedException();
            }
            if (targetType != typeof(string))
            {
                throw new MarshalException($"Type mismatch! Target Type '{type.Name}' is not a string (Marshalling AdsType '{targetType.Name}')!");
            }
        TR_0007:
            if (list == null)
            {
                list = new List<IDataType>();
                if (!this._checkedTypesDict.ContainsKey(targetType))
                {
                    this._checkedTypesDict.Add(targetType, list);
                }
            }
            list.Add(type);
        }

        private object createValue(IDataType sourceType, Type targetType)
        {
            Type type = targetType;
            object obj2 = null;
            if (sourceType.Category == DataTypeCategory.Array)
            {
                IArrayType arrayType = (IArrayType) sourceType;
                obj2 = Array.CreateInstance(targetType, arrayType.Dimensions.GetDimensionLengths());
                Array array = (Array) obj2;
                int[] numArray = new int[arrayType.Dimensions.Count];
                IDataType elementType = arrayType.ElementType;
                object obj3 = null;
                foreach (int[] numArray2 in new ArrayIndexIterator(arrayType, true))
                {
                    obj3 = this.createValue(elementType, type);
                    array.SetValue(obj3, numArray2);
                }
                return obj2;
            }
            return Activator.CreateInstance(targetType);
        }

        private void initializeInstanceValue(object instance, object member, object value)
        {
            if (member is FieldInfo)
            {
                FieldInfo info = (FieldInfo) member;
                object obj2 = this._internalConverter.TypeMarshaller.Convert(value, info.FieldType);
                info.SetValue(instance, obj2);
            }
            else if (member is PropertyInfo)
            {
                PropertyInfo info2 = (PropertyInfo) member;
                object obj3 = this._internalConverter.TypeMarshaller.Convert(value, info2.PropertyType);
                info2.SetValue(instance, obj3, new object[0]);
            }
            else
            {
                if (!(member is int[]))
                {
                    throw new NotSupportedException();
                }
                ((Array) instance).SetValue(value, (int[]) member);
            }
        }

        private void initializeInstanceValue(IDataType type, Encoding encoding, object targetInstance, Type targetType, object targetMember, byte[] data, int offset)
        {
            object obj2 = null;
            switch (type.Category)
            {
                case DataTypeCategory.Primitive:
                case DataTypeCategory.String:
                    this._internalConverter.TypeMarshaller.Unmarshal(type, encoding, data, offset, out obj2);
                    this.initializeInstanceValue(targetInstance, targetMember, obj2);
                    return;

                case DataTypeCategory.Alias:
                {
                    IAliasType type2 = (IAliasType) type;
                    this.initializeInstanceValue(type2.BaseType, encoding, targetInstance, targetType, targetMember, data, offset);
                    return;
                }
                case DataTypeCategory.Enum:
                {
                    IEnumType enumType = (IEnumType) type;
                    Type managedType = ((IManagedMappableType) enumType.BaseType).ManagedType;
                    IEnumValue value2 = EnumValueFactory.Create(enumType, data, offset);
                    if (!targetType.IsEnum)
                    {
                        throw new ArgumentException("Type is not an enum type or enum base type!", "type");
                    }
                    obj2 = Enum.Parse(targetType, value2.ToString(), true);
                    this.initializeInstanceValue(targetInstance, targetMember, obj2);
                    return;
                }
                case DataTypeCategory.Array:
                {
                    IArrayType type6 = (IArrayType) type;
                    int arrayRank = targetType.GetArrayRank();
                    Array array = (Array) targetInstance;
                    int[] numArray = new int[arrayRank];
                    int[] numArray2 = new int[arrayRank];
                    int[] lowerBounds = type6.Dimensions.LowerBounds;
                    int[] upperBounds = type6.Dimensions.UpperBounds;
                    for (int i = 0; i < arrayRank; i++)
                    {
                        numArray[i] = array.GetLowerBound(i);
                        numArray2[i] = array.GetUpperBound(i);
                    }
                    int position = 0;
                    while (position < type6.Dimensions.ElementCount)
                    {
                        int[] indicesOfPosition = ((ArrayType) type6).GetIndicesOfPosition(position);
                        int[] indices = new int[indicesOfPosition.Length];
                        int index = 0;
                        while (true)
                        {
                            if (index >= indicesOfPosition.Length)
                            {
                                object obj3 = array.GetValue(indices);
                                int elementOffset = ((ArrayType) type6).GetElementOffset(indicesOfPosition);
                                if (obj3 != null)
                                {
                                    this.initializeInstanceValue(type6.ElementType, encoding, obj3, obj3.GetType(), indices, data, elementOffset);
                                }
                                else
                                {
                                    TwinCAT.Ads.Module.Trace.TraceError("Failed to fill array element!");
                                }
                                position++;
                                break;
                            }
                            int num6 = numArray[index] - lowerBounds[index];
                            indices[index] = indicesOfPosition[index] + num6;
                            index++;
                        }
                    }
                    return;
                }
                case DataTypeCategory.Struct:
                    foreach (IMember member in ((IStructType) type).AllMembers)
                    {
                        PropertyInfo property = targetType.GetProperty(member.InstanceName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                        if (((property != null) && (property.GetGetMethod() != null)) && (property.GetSetMethod() != null))
                        {
                            this.initializeInstanceValue(member.DataType, encoding, targetInstance, targetType, property, data, offset + member.ByteOffset);
                            continue;
                        }
                        FieldInfo field = targetType.GetField(member.InstanceName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                        if (field != null)
                        {
                            this.initializeInstanceValue(member.DataType, encoding, targetInstance, targetType, field, data, offset + member.ByteOffset);
                        }
                        else
                        {
                            object[] args = new object[] { member.InstanceName, targetType.ToString() };
                            TwinCAT.Ads.Module.Trace.TraceWarning("Struct member '{0}' not found within {1}!", args);
                        }
                    }
                    break;

                case DataTypeCategory.SubRange:
                case DataTypeCategory.Bitset:
                    break;

                case DataTypeCategory.Pointer:
                case DataTypeCategory.Reference:
                {
                    int byteSize = ((IReferenceType) type).ByteSize;
                    return;
                }
                default:
                    throw new NotSupportedException();
            }
        }

        internal void InitializeInstanceValue(IAttributedInstance symbol, ref object targetInstance, byte[] data, int offset)
        {
            Encoding encoding = null;
            EncodingAttributeConverter.TryGetEncoding(symbol.Attributes, out encoding);
            this.InitializeInstanceValue(symbol.DataType, encoding, ref targetInstance, data, offset);
        }

        internal void InitializeInstanceValue(IDataType type, Encoding encoding, ref object targetInstance, byte[] data, int offset)
        {
            try
            {
                this.CheckType(type, targetInstance.GetType());
            }
            catch (MarshalException exception)
            {
                throw new MarshalException($"Cannot Type '{type.Name}' to type '{targetInstance.GetType()}'!", exception);
            }
            this.initializeInstanceValue(type, encoding, targetInstance, targetInstance.GetType(), null, data, offset);
        }

        public byte[] Marshal(IAttributedInstance symbol, object value)
        {
            byte[] bValue = new byte[symbol.ByteSize];
            this.CheckType(symbol.DataType, value.GetType());
            this.Marshal(symbol, value, bValue, 0);
            return bValue;
        }

        public int Marshal(IAttributedInstance symbol, object val, byte[] bValue, int offset)
        {
            Encoding encoding = null;
            EncodingAttributeConverter.TryGetEncoding(symbol.Attributes, out encoding);
            return this.Marshal(symbol.DataType, encoding, val, bValue, offset);
        }

        internal int Marshal(IDataType type, Encoding encoding, object val, byte[] bValue, int offset)
        {
            int num = 0;
            Type type2 = val.GetType();
            switch (type.Category)
            {
                case DataTypeCategory.Primitive:
                case DataTypeCategory.SubRange:
                case DataTypeCategory.String:
                    num = this._internalConverter.TypeMarshaller.Marshal(type, encoding, val, bValue, offset);
                    offset += num;
                    return num;

                case DataTypeCategory.Alias:
                {
                    IAliasType type3 = (IAliasType) type;
                    return this.Marshal(type3.BaseType, encoding, val, bValue, offset);
                }
                case DataTypeCategory.Enum:
                {
                    IEnumType enumType = (IEnumType) type;
                    if (!type2.IsEnum)
                    {
                        throw new ArgumentException("Type is not an enum type!", "type");
                    }
                    IEnumValue value2 = EnumValueFactory.Create(enumType, val);
                    Array.Copy(value2.RawValue, 0, bValue, offset, value2.RawValue.Length);
                    offset += value2.RawValue.Length;
                    return (num + value2.RawValue.Length);
                }
                case DataTypeCategory.Array:
                    break;

                case DataTypeCategory.Struct:
                    foreach (IMember member in ((IStructType) type).AllMembers)
                    {
                        PropertyInfo property = type2.GetProperty(member.InstanceName, BindingFlags.Public | BindingFlags.IgnoreCase);
                        if (((property != null) && (property.GetGetMethod() != null)) && (property.GetSetMethod() != null))
                        {
                            num += this.Marshal(member, property.GetValue(type2, new object[0]), bValue, offset);
                            continue;
                        }
                        FieldInfo field = type2.GetField(member.InstanceName, BindingFlags.Public | BindingFlags.IgnoreCase);
                        if (field == null)
                        {
                            throw new ArgumentException("Struct member not found!", "type");
                        }
                        num += this.Marshal(member, field.GetValue(type2), bValue, offset);
                    }
                    return num;

                default:
                    throw new NotSupportedException();
            }
            IArrayType type6 = (IArrayType) type;
            int arrayRank = type2.GetArrayRank();
            Array array = (Array) val;
            int[] numArray = new int[arrayRank];
            int[] numArray2 = new int[arrayRank];
            int[] numArray3 = new int[arrayRank];
            int[] numArray4 = new int[arrayRank];
            int index = 0;
            while (true)
            {
                if (index >= arrayRank)
                {
                    int position = 0;
                    while (position < type6.Dimensions.ElementCount)
                    {
                        int[] indicesOfPosition = ((ArrayType) type6).GetIndicesOfPosition(position);
                        int[] indices = new int[indicesOfPosition.Length];
                        int num6 = 0;
                        while (true)
                        {
                            if (num6 >= indicesOfPosition.Length)
                            {
                                object obj4 = array.GetValue(indices);
                                int elementOffset = ((ArrayType) type6).GetElementOffset(indices);
                                num += this.Marshal(type, encoding, obj4, bValue, offset + elementOffset);
                                position++;
                                break;
                            }
                            int num7 = numArray[num6] - numArray3[num6];
                            indices[num6] = indicesOfPosition[num6] + num7;
                            num6++;
                        }
                    }
                    break;
                }
                numArray[index] = array.GetLowerBound(index);
                numArray2[index] = array.GetUpperBound(index);
                numArray3[index] = ((Dimension) type6.Dimensions[index]).LowerBound;
                numArray4[index] = ((Dimension) type6.Dimensions[index]).UpperBound;
                index++;
            }
            return num;
        }

        public int MarshalSize(IAttributedInstance symbol, object value)
        {
            Type type = value.GetType();
            DataTypeCategory category = symbol.DataType.Category;
            if (category != DataTypeCategory.Primitive)
            {
                if (category == DataTypeCategory.Alias)
                {
                    return this._internalConverter.MarshalSize(symbol, value);
                }
                else if (category != DataTypeCategory.String)
                {
                    return symbol.ByteSize;
                }
            }
            return this._internalConverter.MarshalSize(symbol, value);
        }

        public bool TryGetManagedType(IAttributedInstance symbol, out Type managed) => 
            this._internalConverter.TryGetManagedType(symbol, out managed);

        public bool TryGetManagedType(IDataType type, out Type managed) => 
            this._internalConverter.TryGetManagedType(type, out managed);

        public int Unmarshal(IAttributedInstance symbol, byte[] data, int offset, out object value)
        {
            Type managed = null;
            if (!this.TryGetManagedType(symbol.DataType, out managed))
            {
                throw new NotSupportedException();
            }
            return this.Unmarshal(symbol, managed, data, offset, out value);
        }

        internal int Unmarshal(IAttributedInstance symbol, Type targetType, byte[] data, int offset, out object value)
        {
            if (symbol == null)
            {
                throw new ArgumentNullException("symbol");
            }
            if (targetType == null)
            {
                throw new ArgumentNullException("targetType");
            }
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }
            int byteSize = 0;
            if (targetType.IsValueType)
            {
                object obj2;
                byteSize = this._internalConverter.Unmarshal(symbol, data, offset, out obj2);
                value = PrimitiveTypeConverter.Convert(obj2, targetType);
            }
            else
            {
                value = this.createValue(symbol.DataType, targetType);
                this.InitializeInstanceValue(symbol, ref value, data, offset);
                byteSize = symbol.ByteSize;
            }
            return byteSize;
        }
    }
}

