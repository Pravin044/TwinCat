namespace TwinCAT.Ads.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Text;
    using TwinCAT.Ads;
    using TwinCAT.Ads.TypeSystem;
    using TwinCAT.PlcOpen;
    using TwinCAT.TypeSystem;

    [DebuggerDisplay("Count = {_symbolTable.Count}")]
    internal class SymbolInfoTable : ISymbolInfoTable, IDisposable
    {
        private Dictionary<string, TcAdsSymbol> _symbolTable;
        private DataTypeInfoTable _datatypeTable;
        private IAdsConnection _adsClient;
        private Encoding _encoding;
        private bool _disposed;

        internal SymbolInfoTable(IAdsConnection adsClient, Encoding symbolEncoding, int targetPointerSize)
        {
            if (adsClient == null)
            {
                throw new ArgumentNullException("adsClient");
            }
            if (symbolEncoding == null)
            {
                throw new ArgumentNullException("symbolEncoding");
            }
            this._adsClient = adsClient;
            this._symbolTable = new Dictionary<string, TcAdsSymbol>(StringComparer.OrdinalIgnoreCase);
            this._datatypeTable = new DataTypeInfoTable(adsClient, symbolEncoding, targetPointerSize);
            this._encoding = symbolEncoding;
        }

        private static void checkArrayDimensions(Array array, AdsDatatypeArrayInfo[] arrayInfo, bool exact)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            if (arrayInfo == null)
            {
                throw new ArgumentNullException("arrayInfo");
            }
            int jagLevel = 0;
            Type baseElementType = null;
            int jaggedElementCount = 0;
            if (PrimitiveTypeConverter.TryJaggedArray(array, out jagLevel, out baseElementType, out jaggedElementCount))
            {
                if (new DimensionCollection(arrayInfo).ElementCount != jaggedElementCount)
                {
                    throw new ArgumentException("Cannot convert dataType of symbol to this type. Jagged array mismatching!");
                }
            }
            else
            {
                if (arrayInfo.Length != array.Rank)
                {
                    throw new ArgumentException($"Cannot convert data type of symbol to this type. Expected an array rank of {arrayInfo.Length}, actual rank is {array.Rank}", "type");
                }
                if (arrayInfo.Length == 1)
                {
                    if (exact && (arrayInfo[0].Elements != array.Length))
                    {
                        throw new ArgumentException($"Cannot convert data type of symbol to this type. Expected an array of length {arrayInfo[0].Elements}, actual length is {array.Length}", "type");
                    }
                    if (!exact && (arrayInfo[0].Elements < array.Length))
                    {
                        throw new ArgumentException($"Cannot convert data type of symbol to this type. Expected an array of length {arrayInfo[0].Elements}, actual length is {array.Length}", "type");
                    }
                }
                else if (arrayInfo.Length == 2)
                {
                    if (arrayInfo[0].Elements != array.GetLength(0))
                    {
                        throw new ArgumentException($"Cannot convert data type of symbol to this type. Expected an array of length {arrayInfo[0].Elements}, actual length is {array.GetLength(0)}", "type");
                    }
                    if (arrayInfo[1].Elements != array.GetLength(1))
                    {
                        throw new ArgumentException($"Cannot convert data type of symbol to this type. Expected an array of length {arrayInfo[1].Elements}, actual length is {array.GetLength(1)}", "type");
                    }
                }
                else
                {
                    if (arrayInfo.Length != 3)
                    {
                        throw new ArgumentException("Cannot convert ads array type of symbol to this type.", "type");
                    }
                    if (arrayInfo[0].Elements != array.GetLength(0))
                    {
                        throw new ArgumentException($"Cannot convert data type of symbol to this type. Expected an array of length {arrayInfo[0].Elements}, actual length is {array.GetLength(0)}", "type");
                    }
                    if (arrayInfo[1].Elements != array.GetLength(1))
                    {
                        throw new ArgumentException($"Cannot convert data type of symbol to this type. Expected an array of length {arrayInfo[1].Elements}, actual length is {array.GetLength(1)}", "type");
                    }
                    if (arrayInfo[2].Elements != array.GetLength(2))
                    {
                        throw new ArgumentException($"Cannot convert data type of symbol to this type. Expected an array of length {arrayInfo[2].Elements}, actual length is {array.GetLength(2)}", "type");
                    }
                }
            }
        }

        private void Cleanup()
        {
            this._datatypeTable.Clear();
            if (this._symbolTable.Count > 0)
            {
                uint[] numArray = new uint[this._symbolTable.Count];
                int index = 0;
                SymbolInfoTable table = this;
                lock (table)
                {
                    foreach (KeyValuePair<string, TcAdsSymbol> pair in this._symbolTable)
                    {
                        if (index < numArray.Length)
                        {
                            if (pair.Value.IndexGroup == 0xf005L)
                            {
                                index++;
                                numArray[index] = (uint) pair.Value.IndexOffset;
                                continue;
                            }
                            index++;
                            numArray[index] = 0;
                        }
                    }
                    this._symbolTable.Clear();
                }
                for (int i = 0; i < index; i++)
                {
                    this._adsClient.RawInterface.Write(0xf006, 0, numArray[i], false);
                }
            }
        }

        public void Dispose()
        {
            if (!this._disposed)
            {
                this.Dispose(true);
                this._disposed = true;
                GC.SuppressFinalize(this);
            }
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.Cleanup();
                this._adsClient = null;
            }
        }

        ~SymbolInfoTable()
        {
            this.Dispose(false);
        }

        public ITcAdsSymbol GetSymbol(string symbolPath, bool bLookup)
        {
            if (string.IsNullOrEmpty(symbolPath))
            {
                throw new ArgumentOutOfRangeException("name");
            }
            ITcAdsSymbol symbol = null;
            AdsErrorCode adsErrorCode = this.TryGetSymbol(symbolPath, bLookup, out symbol);
            if ((adsErrorCode != AdsErrorCode.NoError) && (adsErrorCode != AdsErrorCode.DeviceSymbolNotFound))
            {
                throw AdsErrorException.Create(adsErrorCode);
            }
            return symbol;
        }

        internal int InitializeArray(Type managedType, TcAdsDataType adsType, AdsBinaryReader reader, int readerOffset, int jagLevel, out object value)
        {
            int num2;
            if (managedType == null)
            {
                throw new ArgumentNullException("managedType");
            }
            if (adsType == null)
            {
                throw new ArgumentNullException("adsType");
            }
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }
            int num = readerOffset;
            if (!managedType.IsArray)
            {
                throw new ArgumentException($"Cannot convert datatype of symbol to this type. Expected an array, actual type is {managedType.ToString()}", "type");
            }
            Type elementType = managedType.GetElementType();
            AdsDatatypeArrayInfo[] arrayInfo = adsType.ArrayInfo;
            Type baseElementType = null;
            bool flag = PrimitiveTypeConverter.TryJaggedArray(managedType, out num2, out baseElementType);
            int arrayElementCount = AdsArrayDimensionsInfo.GetArrayElementCount(arrayInfo);
            int byteLength = adsType.ByteSize / arrayElementCount;
            if (flag)
            {
                if (arrayInfo.Length != 1)
                {
                    throw new ArgumentException($"Cannot convert datatype of symbol to this type. Expected an array rank of {arrayInfo.Length}, actual rank is {num2}", "type");
                }
            }
            else
            {
                int length = arrayInfo.Length;
                if (length != managedType.GetArrayRank())
                {
                    throw new ArgumentException($"Cannot convert datatype of symbol to this type. Expected an array rank of {length}, actual rank is {managedType.GetArrayRank()}", "type");
                }
            }
            AdsArrayDimensionsInfo info = new AdsArrayDimensionsInfo(arrayInfo);
            Array array = null;
            if (flag)
            {
                jagLevel++;
                array = Array.CreateInstance(elementType, info.DimensionElements[jagLevel]);
                int[] lowerBounds = new int[] { info.LowerBounds[jagLevel] };
                int[] upperBounds = new int[] { info.UpperBounds[jagLevel] };
                foreach (int[] numArray in new ArrayIndexIterator(lowerBounds, upperBounds, true))
                {
                    object obj2 = null;
                    readerOffset += this.InitializeArray(elementType, (TcAdsDataType) adsType.BaseType, reader, readerOffset, jagLevel, out obj2);
                    array.SetValue(obj2, numArray);
                }
            }
            else
            {
                DataTypeCategory category = adsType.BaseType.Category;
                array = Array.CreateInstance(elementType, info.DimensionElements);
                foreach (int[] numArray2 in new ArrayIndexIterator(info.LowerBounds, info.UpperBounds, true))
                {
                    object obj3 = null;
                    if (category == DataTypeCategory.Primitive)
                    {
                        if (elementType != ((TcAdsDataType) adsType.BaseType).ManagedType)
                        {
                            throw new ArgumentException("Cannot convert data type of symbol to this type.", "type");
                        }
                        readerOffset += this.InitializePrimitiveType(adsType.BaseType.Name, elementType, adsType.DataTypeId, byteLength, reader, readerOffset, out obj3);
                        array.SetValue(obj3, numArray2);
                        continue;
                    }
                    if (category == DataTypeCategory.Enum)
                    {
                        if (elementType != ((TcAdsDataType) adsType.BaseType).ManagedType)
                        {
                            throw new ArgumentException("Cannot convert data type of symbol to this type.", "type");
                        }
                        readerOffset += this.InitializeEnum(adsType.BaseType.Name, elementType, adsType.DataTypeId, byteLength, reader, readerOffset, out obj3);
                        array.SetValue(obj3, numArray2);
                        continue;
                    }
                    if (adsType.BaseType.Category == DataTypeCategory.Struct)
                    {
                        readerOffset += this.InitializeStruct(adsType.BaseType.SubItems, elementType, reader, readerOffset, out obj3);
                        array.SetValue(obj3, numArray2);
                        continue;
                    }
                    if (adsType.BaseType.Category == DataTypeCategory.Array)
                    {
                        readerOffset += this.InitializeArray(elementType, (TcAdsDataType) adsType.BaseType, reader, readerOffset, jagLevel, out obj3);
                        array.SetValue(obj3, numArray2);
                    }
                }
            }
            value = array;
            return (readerOffset - num);
        }

        internal int InitializeEnum(string typeName, Type managedType, AdsDatatypeId dataType, int size, AdsBinaryReader reader, int readerPosition, out object value)
        {
            AdsDatatypeId typeId = dataType;
            if (typeId == AdsDatatypeId.ADST_BIGTYPE)
            {
                PrimitiveTypeConverter.TryGetDataTypeId(managedType, out typeId);
            }
            return this.InitializePrimitiveType(typeName, managedType, typeId, size, reader, readerPosition, out value);
        }

        internal int InitializePointerType(Type managedType, AdsDatatypeId dataType, int byteLength, AdsBinaryReader reader, int readerOffset, out object value)
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }
            if ((byteLength != 4) && (byteLength != 8))
            {
                throw new ArgumentOutOfRangeException("byteLength");
            }
            reader.BaseStream.Position = readerOffset;
            byte[] buffer = reader.ReadBytes(byteLength);
            object sourceValue = null;
            if (byteLength == 4)
            {
                sourceValue = BitConverter.ToUInt32(buffer, 0);
            }
            else if (byteLength == 8)
            {
                sourceValue = BitConverter.ToUInt64(buffer, 0);
            }
            if ((managedType == null) || (managedType == sourceValue.GetType()))
            {
                value = sourceValue;
            }
            else
            {
                value = PrimitiveTypeConverter.Convert(sourceValue, managedType);
            }
            return 0;
        }

        internal int InitializePrimitiveType(string typeName, Type managedType, AdsDatatypeId dataType, int byteLength, AdsBinaryReader reader, int readerOffset, out object value)
        {
            if (typeName == null)
            {
                throw new ArgumentNullException("typeName");
            }
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }
            int num = 0;
            reader.BaseStream.Position = readerOffset;
            object sourceValue = null;
            switch (dataType)
            {
                case AdsDatatypeId.ADST_INT16:
                    sourceValue = reader.ReadInt16();
                    num = 2;
                    break;

                case AdsDatatypeId.ADST_INT32:
                    sourceValue = reader.ReadInt32();
                    num = 4;
                    break;

                case AdsDatatypeId.ADST_REAL32:
                    sourceValue = reader.ReadSingle();
                    num = 4;
                    break;

                case AdsDatatypeId.ADST_REAL64:
                    sourceValue = reader.ReadDouble();
                    num = 8;
                    break;

                default:
                    switch (dataType)
                    {
                        case AdsDatatypeId.ADST_INT8:
                            sourceValue = reader.ReadSByte();
                            num = 1;
                            break;

                        case AdsDatatypeId.ADST_UINT8:
                            sourceValue = reader.ReadByte();
                            num = 1;
                            break;

                        case AdsDatatypeId.ADST_UINT16:
                            sourceValue = reader.ReadUInt16();
                            num = 2;
                            break;

                        case AdsDatatypeId.ADST_UINT32:
                            sourceValue = reader.ReadUInt32();
                            num = 4;
                            break;

                        case AdsDatatypeId.ADST_INT64:
                            sourceValue = reader.ReadInt64();
                            num = 8;
                            break;

                        case AdsDatatypeId.ADST_UINT64:
                            sourceValue = reader.ReadUInt64();
                            num = 8;
                            break;

                        default:
                            switch (dataType)
                            {
                                case AdsDatatypeId.ADST_STRING:
                                    sourceValue = reader.ReadPlcAnsiString(byteLength);
                                    num = byteLength;
                                    break;

                                case AdsDatatypeId.ADST_WSTRING:
                                    sourceValue = reader.ReadPlcUnicodeString(byteLength);
                                    num = byteLength;
                                    break;

                                case AdsDatatypeId.ADST_BIT:
                                    sourceValue = reader.ReadByte() > 0;
                                    num = 1;
                                    break;

                                default:
                                    if ((typeName == "TOD") || (typeName == "TIME"))
                                    {
                                        sourceValue = reader.ReadPlcTIME();
                                        num = 4;
                                    }
                                    else
                                    {
                                        if ((typeName != "DT") && (typeName != "DATE"))
                                        {
                                            throw new ArgumentException("Unexpected datatype. Cannot convert datatype of symbol to this type.", "type");
                                        }
                                        sourceValue = reader.ReadPlcDATE();
                                        num = 4;
                                    }
                                    break;
                            }
                            break;
                    }
                    break;
            }
            if ((managedType == null) || (managedType == sourceValue.GetType()))
            {
                value = sourceValue;
            }
            else
            {
                value = PrimitiveTypeConverter.Convert(sourceValue, managedType);
            }
            return num;
        }

        internal int InitializeStruct(IList<ITcAdsSubItem> subItems, Type type, AdsBinaryReader reader, int readerOffset, out object value)
        {
            int num = 0;
            object obj2 = type.InvokeMember(null, BindingFlags.CreateInstance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, null, null);
            FieldInfo[] fields = type.GetFields();
            if (fields.Length != subItems.Count)
            {
                throw new ArgumentException($"Cannot convert data type of symbol to this type.Expected number of fields {subItems.Count}, actual number is {fields.Length}", "type");
            }
            for (int i = 0; i < fields.Length; i++)
            {
                TcAdsSubItem item = (TcAdsSubItem) subItems[i];
                if ((item.BaseType.Category == DataTypeCategory.Primitive) || (item.BaseType.Category == DataTypeCategory.String))
                {
                    if (fields[i].FieldType != ((TcAdsDataType) item.BaseType).ManagedType)
                    {
                        throw new ArgumentException("Cannot convert data type of symbol to this type.", "type");
                    }
                    object obj3 = null;
                    num += this.InitializePrimitiveType(item.BaseType.Name, fields[i].FieldType, item.DataTypeId, item.Size, reader, readerOffset + item.Offset, out obj3);
                    fields[i].SetValue(obj2, obj3);
                }
                else if (item.BaseType.Category == DataTypeCategory.Enum)
                {
                    if (fields[i].FieldType != ((TcAdsDataType) item.BaseType).ManagedType)
                    {
                        throw new ArgumentException("Cannot convert data type of symbol to this type.", "type");
                    }
                    object obj4 = null;
                    num += this.InitializeEnum(item.BaseType.Name, fields[i].FieldType, item.DataTypeId, item.Size, reader, readerOffset + item.Offset, out obj4);
                    fields[i].SetValue(obj2, obj4);
                }
                else if (item.BaseType.Category == DataTypeCategory.Struct)
                {
                    object obj5 = null;
                    num += this.InitializeStruct(item.BaseType.SubItems, fields[i].FieldType, reader, readerOffset + item.Offset, out obj5);
                    fields[i].SetValue(obj2, obj5);
                }
                else
                {
                    if (item.BaseType.Category != DataTypeCategory.Array)
                    {
                        throw new ArgumentException($"Cannot convert data type of symbol to this type. ADS Data type '{item.BaseType.Name}' of subitem '{item.SubItemName}' not supported.", "type");
                    }
                    object obj6 = null;
                    num += this.InitializeArray(fields[i].FieldType, (TcAdsDataType) item.BaseType, reader, readerOffset + item.Offset, -1, out obj6);
                    fields[i].SetValue(obj2, obj6);
                }
            }
            value = obj2;
            return num;
        }

        public object InvokeRpcMethod(ITcAdsSymbol symbol, IRpcMethod rpcMethod, object[] parameterValues)
        {
            if (symbol == null)
            {
                throw new ArgumentNullException("symbol");
            }
            if (rpcMethod == null)
            {
                throw new ArgumentNullException("rpcMethod");
            }
            if (parameterValues == null)
            {
                throw new ArgumentNullException("parameters");
            }
            if (rpcMethod.Parameters.Count != parameterValues.Length)
            {
                throw new ArgumentOutOfRangeException("parameters", $"Parameter mismatch! The number of expected parameters is '{rpcMethod.Parameters.Count}");
            }
            object returnValue = null;
            string variableName = $"{symbol.Name}#{rpcMethod.Name}";
            ITcAdsDataType type = null;
            if (!rpcMethod.IsVoid)
            {
                type = (ITcAdsDataType) this._datatypeTable.ResolveDataType(rpcMethod.ReturnType);
            }
            List<IDataType> list = new List<IDataType>();
            foreach (RpcMethodParameter parameter in rpcMethod.Parameters)
            {
                ITcAdsDataType item = (ITcAdsDataType) this._datatypeTable.ResolveDataType(parameter.TypeName);
                if (item == null)
                {
                    throw new AdsDatatypeNotSupportedException();
                }
                list.Add(item);
            }
            int wrLength = 0;
            SymbolAdsMarshaller marshaller = new SymbolAdsMarshaller(this._datatypeTable);
            object[] objArray = new object[0];
            wrLength = marshaller.GetInMarshallingSize(rpcMethod, objArray);
            int outMarshallingSize = marshaller.GetOutMarshallingSize(rpcMethod, objArray);
            int variableHandle = this._adsClient.CreateVariableHandle(variableName);
            try
            {
                byte[] buffer = new byte[wrLength];
                int num4 = marshaller.MarshallParameters((RpcMethod) rpcMethod, parameterValues, buffer, 0);
                using (AdsStream stream = new AdsStream(buffer))
                {
                    using (AdsStream stream2 = new AdsStream(outMarshallingSize))
                    {
                        int num5 = 0;
                        num5 = this._adsClient.ReadWrite(variableHandle, stream2, 0, outMarshallingSize, stream, 0, wrLength);
                        if (outMarshallingSize > 0)
                        {
                            int num6 = marshaller.UnmarshalRpcMethod(rpcMethod, parameterValues, stream2.GetBuffer(), out returnValue);
                        }
                    }
                }
            }
            finally
            {
                this._adsClient.DeleteVariableHandle(variableHandle);
            }
            return returnValue;
        }

        public object ReadSymbol(string symbolPath, Type managedType, bool bReloadInfo)
        {
            AdsErrorCode code2;
            if (bReloadInfo)
            {
                this.Cleanup();
            }
            TcAdsSymbol adsSymbol = (TcAdsSymbol) this.GetSymbol(symbolPath, true);
            if (adsSymbol == null)
            {
                TcAdsDllWrapper.ThrowAdsException(AdsErrorCode.DeviceSymbolNotFound);
            }
            TcAdsDataType type = (TcAdsDataType) this._datatypeTable.ResolveDataType(adsSymbol.TypeName);
            if (type == null)
            {
                throw new NotSupportedException("Type of symbol not supported");
            }
            if (adsSymbol.IndexGroup != 0xf005L)
            {
                AdsErrorCode adsErrorCode = this.updateSymbolHandle(adsSymbol);
                if (adsErrorCode == AdsErrorCode.ClientSyncTimeOut)
                {
                    TcAdsDllWrapper.ThrowAdsException(adsErrorCode);
                }
            }
            AdsStream stream = new AdsStream(adsSymbol.Size);
            bool flag = true;
            AdsBinaryReader reader = new AdsBinaryReader(stream);
            int num2 = 0;
            goto TR_001C;
        TR_000F:
            num2++;
        TR_001C:
            while (true)
            {
                int num3;
                if (!((num2 < 2) & flag))
                {
                    object obj2 = null;
                    int num = 0;
                    TcAdsDataType adsType = (TcAdsDataType) type.ResolveType(DataTypeResolveStrategy.AliasReference);
                    if (adsType.IsEnum)
                    {
                        num = this.InitializeEnum(adsType.Name, managedType, adsSymbol.DataTypeId, adsSymbol.Size, reader, 0, out obj2);
                        return obj2;
                    }
                    if (adsType.IsArray)
                    {
                        num = this.InitializeArray(managedType, adsType, reader, 0, -1, out obj2);
                        return obj2;
                    }
                    if (adsType.IsStruct)
                    {
                        num = this.InitializeStruct(adsType.SubItems, managedType, reader, 0, out obj2);
                        return obj2;
                    }
                    if (adsType.IsPointer)
                    {
                        num = this.InitializePointerType(managedType, adsSymbol.DataTypeId, adsSymbol.Size, reader, 0, out obj2);
                        return obj2;
                    }
                    if (adsType.IsPrimitive)
                    {
                        num = this.InitializePrimitiveType(adsType.Name, managedType, adsSymbol.DataTypeId, adsSymbol.Size, reader, 0, out obj2);
                        return obj2;
                    }
                    if (!adsType.IsPointer)
                    {
                        throw new NotSupportedException("Type of symbol not supported");
                    }
                    num = this.InitializePointerType(managedType, adsSymbol.DataTypeId, adsSymbol.Size, reader, 0, out obj2);
                    return obj2;
                }
                flag = false;
                code2 = this._adsClient.RawInterface.Read((uint) adsSymbol.IndexGroup, (uint) adsSymbol.IndexOffset, 0, (int) stream.Length, stream.GetBuffer(), false, out num3);
                if (code2 > AdsErrorCode.DeviceInvalidOffset)
                {
                    if ((code2 != AdsErrorCode.DeviceSymbolNotFound) && (code2 != AdsErrorCode.DeviceSymbolVersionInvalid))
                    {
                        break;
                    }
                }
                else if (code2 == AdsErrorCode.NoError)
                {
                    goto TR_000F;
                }
                else if (code2 != AdsErrorCode.DeviceInvalidOffset)
                {
                    break;
                }
                if (adsSymbol.IndexGroup == 0xf005L)
                {
                    uint num4;
                    this._adsClient.RawInterface.Write(0xf006, 0, (uint) adsSymbol.IndexOffset, false);
                    if (this._adsClient.RawInterface.ReadWrite(0xf003, 0, symbolPath, false, out num4) == AdsErrorCode.NoError)
                    {
                        adsSymbol.IndexGroup = 0xf005L;
                        adsSymbol.IndexOffset = num4;
                        flag = true;
                    }
                }
                goto TR_000F;
            }
            TcAdsDllWrapper.ThrowAdsException(code2);
            goto TR_000F;
        }

        public AdsErrorCode TryGetDataType(ITcAdsSymbol symbol, bool bLookup, out ITcAdsDataType dataType) => 
            this._datatypeTable.TryLoadType(((ITcAdsSymbol5) symbol).TypeName, bLookup, out dataType);

        public AdsErrorCode TryGetSymbol(string symbolPath, bool bLookup, out ITcAdsSymbol symbol)
        {
            if (string.IsNullOrEmpty(symbolPath))
            {
                throw new ArgumentOutOfRangeException("name");
            }
            symbol = null;
            SymbolInfoTable table = this;
            lock (table)
            {
                if (bLookup)
                {
                    TcAdsSymbol symbol2 = null;
                    if (this._symbolTable.TryGetValue(symbolPath, out symbol2))
                    {
                        symbol = symbol2;
                        return AdsErrorCode.NoError;
                    }
                }
            }
            AdsErrorCode deviceSymbolNotFound = AdsErrorCode.DeviceSymbolNotFound;
            AdsStream stream = new AdsStream(symbolPath.Length + 1);
            using (AdsBinaryWriter writer = new AdsBinaryWriter(stream))
            {
                writer.WritePlcAnsiString(symbolPath, symbolPath.Length + 1);
                AdsStream rdDataStream = new AdsStream(0xffff);
                int readBytes = 0;
                deviceSymbolNotFound = this._adsClient.TryReadWrite(0xf009, 0, rdDataStream, 0, (int) rdDataStream.Length, stream, 0, (int) stream.Length, out readBytes);
                if (deviceSymbolNotFound == AdsErrorCode.NoError)
                {
                    using (AdsBinaryReader reader = new AdsBinaryReader(rdDataStream))
                    {
                        AdsSymbolEntry symbolEntry = new AdsSymbolEntry(-1L, this._encoding, reader);
                        bool flag2 = true;
                        flag2 = StringComparer.OrdinalIgnoreCase.Compare(symbolPath, symbolEntry.name) == 0;
                        symbol = new TcAdsSymbol(symbolEntry, (TcAdsDataType) this._datatypeTable.ResolveDataType(symbolEntry.type));
                        SymbolInfoTable table2 = this;
                        lock (table2)
                        {
                            this._symbolTable[symbolPath] = (TcAdsSymbol) symbol;
                            if (!flag2 && !this._symbolTable.ContainsKey(symbol.Name))
                            {
                                this._symbolTable[symbol.Name] = (TcAdsSymbol) symbol;
                                string message = $"InstancePath Ambiguity '{symbolPath}' and '{symbol.Name}'!";
                                TwinCAT.Ads.Module.Trace.TraceWarning(message);
                            }
                        }
                    }
                }
            }
            return deviceSymbolNotFound;
        }

        private AdsErrorCode updateSymbolHandle(TcAdsSymbol adsSymbol)
        {
            AdsErrorCode noError = AdsErrorCode.NoError;
            if (adsSymbol.IndexGroup != 0xf005L)
            {
                uint num;
                noError = this._adsClient.RawInterface.ReadWrite(0xf003, 0, adsSymbol.Name, false, out num);
                if (noError == AdsErrorCode.NoError)
                {
                    adsSymbol.IndexGroup = 0xf005L;
                    adsSymbol.IndexOffset = num;
                }
                else if (noError != AdsErrorCode.ClientSyncTimeOut)
                {
                }
            }
            return noError;
        }

        internal void WriteArray(object value, ITcAdsDataType arrayType, AdsBinaryWriter writer, int writerOffset)
        {
            Array array = value as Array;
            if (array == null)
            {
                throw new ArgumentException($"Cannot convert data type of symbol to this type. Expected an array, actual type is {value.GetType()}", "type");
            }
            AdsDatatypeArrayInfo[] arrayInfo = ((TcAdsDataType) arrayType).ArrayInfo;
            checkArrayDimensions(array, arrayInfo, false);
            AdsArrayDimensionsInfo info = new AdsArrayDimensionsInfo(arrayInfo);
            int elements = info.Elements;
            int byteSize = arrayType.ByteSize / elements;
            DataTypeCategory category = arrayType.BaseType.Category;
            int num3 = 0;
            foreach (int[] numArray in new ArrayIndexIterator(info.LowerBounds, info.UpperBounds, true))
            {
                if ((array.Rank == 1) && (num3 >= array.Length))
                {
                    break;
                }
                if (category == DataTypeCategory.Primitive)
                {
                    this.WritePrimitiveValue(arrayType.BaseType.Name, array.GetValue(numArray), ((TcAdsDataType) arrayType.BaseType).ManagedType, arrayType.DataTypeId, byteSize, writer, writerOffset);
                }
                else if (category == DataTypeCategory.Enum)
                {
                    this.WriteEnumValue(arrayType.BaseType.Name, array.GetValue(numArray), (TcAdsDataType) arrayType, writer, writerOffset);
                }
                else if (category == DataTypeCategory.Struct)
                {
                    this.WriteStruct(array.GetValue(numArray), arrayType.BaseType.SubItems, writer, writerOffset);
                }
                else
                {
                    if (category != DataTypeCategory.Array)
                    {
                        throw new ArgumentException("Cannot convert ads array type of symbol to this type.", "type");
                    }
                    this.WriteArray(array.GetValue(numArray), arrayType.BaseType, writer, writerOffset);
                }
                writerOffset += byteSize;
                num3++;
            }
        }

        internal void WriteEnumValue(string symbolPath, object value, TcAdsDataType type, AdsBinaryWriter writer, int writerOffset)
        {
            object obj2 = value;
            if (value is string)
            {
                obj2 = EnumValueConverter.ToValue(type, (string) value);
            }
            this.WritePrimitiveValue(symbolPath, obj2, type.ManagedType, type.DataTypeId, type.Size, writer, writerOffset);
        }

        internal void WritePointerValue(string symbolPath, object value, AdsDatatypeId dataType, int byteSize, AdsBinaryWriter writer, int writerOffset)
        {
            if (string.IsNullOrEmpty(symbolPath))
            {
                throw new ArgumentNullException("symbolPath");
            }
            if ((byteSize < 0) || ((byteSize != 4) && (byteSize != 8)))
            {
                throw new ArgumentOutOfRangeException("byteSize");
            }
            if (writer == null)
            {
                throw new ArgumentNullException("writer");
            }
            if (writerOffset < 0)
            {
                throw new ArgumentOutOfRangeException("writerOffset");
            }
            Type targetType = null;
            if (byteSize == 4)
            {
                targetType = typeof(uint);
            }
            else if (byteSize == 8)
            {
                targetType = typeof(ulong);
            }
            if (targetType != value.GetType())
            {
                try
                {
                    object obj1 = PrimitiveTypeConverter.Convert(value, targetType);
                    value = obj1;
                }
                catch (MarshalException exception)
                {
                    throw new ArgumentException($"Cannot convert value type '{value.GetType()}' to symbol type '{targetType}'!", "value", exception);
                }
            }
            writer.BaseStream.Position = writerOffset;
            if (byteSize == 4)
            {
                writer.Write((uint) value);
            }
            else if (byteSize == 8)
            {
                writer.Write((ulong) value);
            }
        }

        internal void WritePrimitiveValue(string symbolPath, object value, Type managedType, AdsDatatypeId dataType, int byteSize, AdsBinaryWriter writer, int writerOffset)
        {
            if (string.IsNullOrEmpty(symbolPath))
            {
                throw new ArgumentNullException("symbolPath");
            }
            if (managedType == null)
            {
                throw new ArgumentNullException("managedType");
            }
            if (byteSize < 0)
            {
                throw new ArgumentOutOfRangeException("byteSize");
            }
            if (writer == null)
            {
                throw new ArgumentNullException("writer");
            }
            if (writerOffset < 0)
            {
                throw new ArgumentOutOfRangeException("writerOffset");
            }
            Type type = value.GetType();
            bool flag2 = PrimitiveTypeConverter.IsPlcOpenType(value.GetType());
            if (PrimitiveTypeConverter.IsPlcOpenType(managedType))
            {
                Type tp = null;
                if (PrimitiveTypeConverter.TryGetManagedType(dataType, out tp))
                {
                    managedType = tp;
                }
                else if (managedType == typeof(LTIME))
                {
                    managedType = typeof(ulong);
                    dataType = AdsDatatypeId.ADST_UINT64;
                }
            }
            if (managedType != type)
            {
                try
                {
                    object obj1 = PrimitiveTypeConverter.Convert(value, managedType);
                    value = obj1;
                }
                catch (MarshalException exception)
                {
                    throw new ArgumentException($"Cannot convert value type '{value.GetType()}' to symbol type '{managedType}'!", "value", exception);
                }
            }
            writer.BaseStream.Position = writerOffset;
            switch (dataType)
            {
                case AdsDatatypeId.ADST_INT16:
                    writer.Write((short) value);
                    return;

                case AdsDatatypeId.ADST_INT32:
                    writer.Write((int) value);
                    return;

                case AdsDatatypeId.ADST_REAL32:
                    writer.Write((float) value);
                    return;

                case AdsDatatypeId.ADST_REAL64:
                    writer.Write((double) value);
                    return;
            }
            switch (dataType)
            {
                case AdsDatatypeId.ADST_INT8:
                    writer.Write((sbyte) value);
                    return;

                case AdsDatatypeId.ADST_UINT8:
                    writer.Write((byte) value);
                    return;

                case AdsDatatypeId.ADST_UINT16:
                    writer.Write((ushort) value);
                    return;

                case AdsDatatypeId.ADST_UINT32:
                    writer.Write((uint) value);
                    return;

                case AdsDatatypeId.ADST_INT64:
                    writer.Write((long) value);
                    return;

                case AdsDatatypeId.ADST_UINT64:
                    writer.Write((ulong) value);
                    return;
            }
            switch (dataType)
            {
                case AdsDatatypeId.ADST_STRING:
                {
                    int byteCount = Encoding.Default.GetByteCount("a");
                    long position = writer.BaseStream.Position;
                    int length = ((string) value).Length;
                    if (((length + 1) * byteCount) > byteSize)
                    {
                        throw AdsErrorException.Create(AdsErrorCode.DeviceInvalidSize);
                    }
                    writer.WritePlcAnsiString((string) value, length);
                    writer.BaseStream.Position = position + byteSize;
                    return;
                }
                case AdsDatatypeId.ADST_WSTRING:
                {
                    int byteCount = Encoding.Unicode.GetByteCount("a");
                    long position = writer.BaseStream.Position;
                    int length = ((string) value).Length;
                    if (((length + 1) * byteCount) > byteSize)
                    {
                        throw AdsErrorException.Create(AdsErrorCode.DeviceInvalidSize);
                    }
                    writer.WritePlcUnicodeString((string) value, length);
                    writer.BaseStream.Position = position + byteSize;
                    return;
                }
                case AdsDatatypeId.ADST_BIT:
                    if ((bool) value)
                    {
                        writer.Write((byte) 1);
                        return;
                    }
                    writer.Write((byte) 0);
                    return;
            }
            throw new ArgumentException("Unexpected datatype. Cannot convert datatype of symbol to this type.", "type");
        }

        internal void WriteStruct(object value, IList<ITcAdsSubItem> subItems, AdsBinaryWriter writer, int writerOffset)
        {
            FieldInfo[] fields = value.GetType().GetFields();
            if (fields.Length != subItems.Count)
            {
                throw new ArgumentException($"Cannot convert data type of symbol to this type.Expected number of fields {subItems.Count}, actual number is {fields.Length}", "type");
            }
            for (int i = 0; i < fields.Length; i++)
            {
                ITcAdsSubItem item = subItems[i];
                writerOffset += item.Offset;
                object obj2 = fields[i].GetValue(value);
                if (((TcAdsDataType) item.BaseType).IsPrimitive)
                {
                    this.WritePrimitiveValue(item.BaseType.Name, obj2, ((TcAdsDataType) item.BaseType).ManagedType, item.DataTypeId, item.Size, writer, writerOffset);
                }
                else if (item.BaseType.Category == DataTypeCategory.Enum)
                {
                    this.WriteEnumValue(item.BaseType.Name, obj2, (TcAdsDataType) item, writer, writerOffset);
                }
                else if (item.BaseType.Category == DataTypeCategory.Struct)
                {
                    this.WriteStruct(obj2, item.BaseType.SubItems, writer, writerOffset);
                }
                else
                {
                    if (item.BaseType.Category != DataTypeCategory.Array)
                    {
                        throw new ArgumentException($"Cannot convert data type of symbol to this type. ADS Data type '{item.BaseType.Name}' of subitem '{item.SubItemName}' not supported.", "type");
                    }
                    this.WriteArray(obj2, (TcAdsDataType) item.BaseType, writer, writerOffset);
                }
            }
        }

        public void WriteSymbol(string name, object value, bool bReloadInfo)
        {
            AdsErrorCode code2;
            if (bReloadInfo)
            {
                this.Cleanup();
            }
            TcAdsSymbol adsSymbol = (TcAdsSymbol) this.GetSymbol(name, true);
            if (adsSymbol == null)
            {
                TcAdsDllWrapper.ThrowAdsException(AdsErrorCode.DeviceSymbolNotFound);
            }
            TcAdsDataType type = (TcAdsDataType) ((TcAdsDataType) this._datatypeTable.ResolveDataType(adsSymbol.TypeName)).ResolveType(DataTypeResolveStrategy.AliasReference);
            if (type == null)
            {
                throw new NotSupportedException("Type of symbol not supported");
            }
            if (adsSymbol.IndexGroup != 0xf005L)
            {
                AdsErrorCode adsErrorCode = this.updateSymbolHandle(adsSymbol);
                if (adsErrorCode == AdsErrorCode.ClientSyncTimeOut)
                {
                    TcAdsDllWrapper.ThrowAdsException(adsErrorCode);
                }
            }
            AdsStream stream = new AdsStream(adsSymbol.Size);
            AdsBinaryWriter writer = new AdsBinaryWriter(stream);
            if (type.IsEnum)
            {
                this.WriteEnumValue(adsSymbol.Name, value, type, writer, 0);
            }
            else if (type.IsArray)
            {
                this.WriteArray(value, type, writer, 0);
            }
            else if (type.IsStruct)
            {
                this.WriteStruct(value, type.SubItems, writer, 0);
            }
            else if (type.IsPointer)
            {
                this.WritePointerValue(adsSymbol.Name, value, adsSymbol.DataTypeId, adsSymbol.Size, writer, 0);
            }
            else
            {
                if (!type.IsPrimitive)
                {
                    throw new NotSupportedException("Type of symbol not supported");
                }
                this.WritePrimitiveValue(adsSymbol.Name, value, type.ManagedType, adsSymbol.DataTypeId, adsSymbol.Size, writer, 0);
            }
            bool flag = true;
            int num = 0;
            goto TR_000F;
        TR_0002:
            num++;
        TR_000F:
            while (true)
            {
                if (!((num < 2) & flag))
                {
                    return;
                }
                flag = false;
                code2 = this._adsClient.RawInterface.Write((uint) adsSymbol.IndexGroup, (uint) adsSymbol.IndexOffset, 0, (int) stream.Length, stream.GetBuffer(), false);
                if (code2 > AdsErrorCode.DeviceInvalidOffset)
                {
                    if ((code2 != AdsErrorCode.DeviceSymbolNotFound) && (code2 != AdsErrorCode.DeviceSymbolVersionInvalid))
                    {
                        break;
                    }
                }
                else if (code2 == AdsErrorCode.NoError)
                {
                    goto TR_0002;
                }
                else if (code2 != AdsErrorCode.DeviceInvalidOffset)
                {
                    break;
                }
                if (adsSymbol.IndexGroup == 0xf005L)
                {
                    uint num2;
                    this._adsClient.RawInterface.Write(0xf006, 0, (uint) adsSymbol.IndexOffset, false);
                    if (this._adsClient.RawInterface.ReadWrite(0xf003, 0, name, false, out num2) == AdsErrorCode.NoError)
                    {
                        adsSymbol.IndexGroup = 0xf005L;
                        adsSymbol.IndexOffset = num2;
                        flag = true;
                    }
                }
                goto TR_0002;
            }
            TcAdsDllWrapper.ThrowAdsException(code2);
            goto TR_0002;
        }
    }
}

