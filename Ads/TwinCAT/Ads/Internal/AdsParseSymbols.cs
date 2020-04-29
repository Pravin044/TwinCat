namespace TwinCAT.Ads.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using TwinCAT.Ads;
    using TwinCAT.Ads.TypeSystem;
    using TwinCAT.TypeSystem;

    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
    internal class AdsParseSymbols : IDataTypeResolver, ITypeBinderEvents
    {
        private ReadOnlyTcAdsDataTypeCollection _defaultTypes;
        private SymbolEntryCollection _symbolTable;
        internal TcAdsClient _adsClient;
        private const uint TCOMOBJ_MIN_OID = 0x100000;
        private SubSymbolFactory _subSymbolFactory;
        private Encoding _symbolEncoding = Encoding.Default;
        private bool _streamIncludesBuildInTypes = true;
        private TcAdsDataTypeCollection _dataTypes;
        [CompilerGenerated]
        private EventHandler<DataTypeEventArgs> TypesGenerated;
        [CompilerGenerated]
        private EventHandler<DataTypeNameEventArgs> TypeResolveError;
        private int _platformPointerSize;

        public event EventHandler<DataTypeNameEventArgs> TypeResolveError
        {
            [CompilerGenerated] add
            {
                EventHandler<DataTypeNameEventArgs> typeResolveError = this.TypeResolveError;
                while (true)
                {
                    EventHandler<DataTypeNameEventArgs> a = typeResolveError;
                    EventHandler<DataTypeNameEventArgs> handler3 = (EventHandler<DataTypeNameEventArgs>) Delegate.Combine(a, value);
                    typeResolveError = Interlocked.CompareExchange<EventHandler<DataTypeNameEventArgs>>(ref this.TypeResolveError, handler3, a);
                    if (ReferenceEquals(typeResolveError, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                EventHandler<DataTypeNameEventArgs> typeResolveError = this.TypeResolveError;
                while (true)
                {
                    EventHandler<DataTypeNameEventArgs> source = typeResolveError;
                    EventHandler<DataTypeNameEventArgs> handler3 = (EventHandler<DataTypeNameEventArgs>) Delegate.Remove(source, value);
                    typeResolveError = Interlocked.CompareExchange<EventHandler<DataTypeNameEventArgs>>(ref this.TypeResolveError, handler3, source);
                    if (ReferenceEquals(typeResolveError, source))
                    {
                        return;
                    }
                }
            }
        }

        public event EventHandler<DataTypeEventArgs> TypesGenerated
        {
            [CompilerGenerated] add
            {
                EventHandler<DataTypeEventArgs> typesGenerated = this.TypesGenerated;
                while (true)
                {
                    EventHandler<DataTypeEventArgs> a = typesGenerated;
                    EventHandler<DataTypeEventArgs> handler3 = (EventHandler<DataTypeEventArgs>) Delegate.Combine(a, value);
                    typesGenerated = Interlocked.CompareExchange<EventHandler<DataTypeEventArgs>>(ref this.TypesGenerated, handler3, a);
                    if (ReferenceEquals(typesGenerated, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                EventHandler<DataTypeEventArgs> typesGenerated = this.TypesGenerated;
                while (true)
                {
                    EventHandler<DataTypeEventArgs> source = typesGenerated;
                    EventHandler<DataTypeEventArgs> handler3 = (EventHandler<DataTypeEventArgs>) Delegate.Remove(source, value);
                    typesGenerated = Interlocked.CompareExchange<EventHandler<DataTypeEventArgs>>(ref this.TypesGenerated, handler3, source);
                    if (ReferenceEquals(typesGenerated, source))
                    {
                        return;
                    }
                }
            }
        }

        public AdsParseSymbols(int platformPointerSize, bool streamIncludesBuildInTypes, Encoding stringEncoding)
        {
            if (stringEncoding == null)
            {
                throw new ArgumentNullException("stringEncoding");
            }
            this._platformPointerSize = platformPointerSize;
            this._symbolEncoding = stringEncoding;
            this._streamIncludesBuildInTypes = streamIncludesBuildInTypes;
        }

        private TcAdsDataTypeCollection collectDataTypes(int count, uint[] dataTypeOffsets, AdsBinaryReader dataTypeReader)
        {
            TcAdsDataTypeCollection types = new TcAdsDataTypeCollection();
            types.AddRange(this._defaultTypes);
            AdsDataTypeEntry[] entryArray = new AdsDataTypeEntry[count];
            for (int i = 0; i < count; i++)
            {
                entryArray[i] = this.GetDataTypeEntry(i, dataTypeOffsets, dataTypeReader);
            }
            for (int j = 0; j < count; j++)
            {
                if (!(this._streamIncludesBuildInTypes && this._defaultTypes.Contains(entryArray[j].entryName)))
                {
                    TcAdsDataType item = new TcAdsDataType(entryArray[j], this);
                    if (item.IsPointer || item.IsReference)
                    {
                        this.SetPlatformPointerSize(item.Size);
                        Type managedType = typeof(ulong);
                        if (item.Size == 4)
                        {
                            managedType = typeof(uint);
                        }
                        for (int k = 0; k < types.Count; k++)
                        {
                            ITcAdsDataType dataType = types[k];
                            if (dataType.Size <= 0)
                            {
                                if (dataType.Category == DataTypeCategory.Alias)
                                {
                                    ITcAdsDataType type4 = types.ResolveType(dataType, DataTypeResolveStrategy.Alias);
                                    if ((type4 != null) && (type4.Category == DataTypeCategory.Pointer))
                                    {
                                        ((TcAdsDataType) dataType).SetSize(item.Size, managedType);
                                    }
                                }
                                if (dataType.Category == DataTypeCategory.Pointer)
                                {
                                    ((TcAdsDataType) dataType).SetSize(item.Size, managedType);
                                }
                            }
                        }
                    }
                    types.Add(item);
                }
            }
            return types;
        }

        private int countDataTypes(AdsBinaryReader dataTypeReader)
        {
            int num = 0;
            AdsStream baseStream = (AdsStream) dataTypeReader.BaseStream;
            for (uint i = 0; i < baseStream.Length; i += dataTypeReader.ReadUInt32())
            {
                baseStream.Position = i;
                num++;
            }
            return num;
        }

        private int countSymbols(AdsBinaryReader symbolReader)
        {
            int num = 0;
            uint num2 = 0;
            AdsStream baseStream = (AdsStream) symbolReader.BaseStream;
            while (num2 < baseStream.Length)
            {
                baseStream.Position = num2;
                num++;
                num2 += symbolReader.ReadUInt32();
            }
            return num;
        }

        private TcAdsSymbolInfo CreateSymbolInfo(string symbolName)
        {
            TcAdsSymbolInfo info = null;
            IList<AdsSymbolEntry> entry = null;
            if (this._symbolTable.TryGetSymbol(symbolName, out entry))
            {
                int index = this._symbolTable.IndexOf(entry[0]);
                info = this.CreateSymbolInfo(entry[0], index);
            }
            return info;
        }

        private TcAdsSymbolInfo CreateSymbolInfo(AdsSymbolEntry symbolEntry, int index)
        {
            ITcAdsDataType typeByName = this.GetTypeByName(symbolEntry.type);
            if (typeByName == null)
            {
                typeByName = (ITcAdsDataType) this.ResolveDataType(symbolEntry.type);
            }
            return new TcAdsSymbolInfo(this, null, index, symbolEntry, (TcAdsDataType) typeByName);
        }

        private void expandDataType(ITcAdsDataType cloneType)
        {
            ITcAdsDataType baseType = cloneType.BaseType;
            if (baseType != null)
            {
                this.expandDataType(baseType);
            }
        }

        private void expandDataTypes()
        {
            int count = this._dataTypes.Count;
            foreach (TcAdsDataType type in this._dataTypes.Clone())
            {
                this.expandDataType(type);
            }
            int num2 = this._dataTypes.Count;
            object[] args = new object[] { num2 - count };
            Module.Trace.TraceInformation("{0} datatypes expanded!", args);
        }

        private Dictionary<string, int> fillDataTypeTables(AdsBinaryReader dataTypeReader, int dataTypeCount, out uint[] dataTypeEntryOffsets)
        {
            int index = 0;
            AdsStream baseStream = (AdsStream) dataTypeReader.BaseStream;
            dataTypeEntryOffsets = new uint[dataTypeCount];
            Dictionary<string, int> dictionary = new Dictionary<string, int>(dataTypeCount, StringComparer.OrdinalIgnoreCase);
            index = 0;
            for (uint i = 0; i < baseStream.Length; i += dataTypeReader.ReadUInt32())
            {
                dataTypeEntryOffsets[index] = i;
                baseStream.Position = (long) ((ulong) (i + 0x20));
                ushort num3 = dataTypeReader.ReadUInt16();
                baseStream.Position = (long) ((ulong) (i + 0x2a));
                string str = dataTypeReader.ReadPlcString(num3 + 1, this._symbolEncoding);
                index++;
                dictionary[str] = index;
                baseStream.Position = i;
            }
            return dictionary;
        }

        private SymbolEntryCollection fillSymbolTables(AdsBinaryReader symbolReader, int symbolCount, out uint[] symbolEntryOffsets)
        {
            symbolEntryOffsets = new uint[symbolCount];
            SymbolEntryCollection entrys = new SymbolEntryCollection(symbolCount);
            uint num = 0;
            AdsStream baseStream = (AdsStream) symbolReader.BaseStream;
            baseStream.Position = 0L;
            while (num < baseStream.Length)
            {
                AdsSymbolEntry item = new AdsSymbolEntry((long) num, this._symbolEncoding, symbolReader);
                num += item.entryLength;
                try
                {
                    entrys.Add(item);
                }
                catch (Exception exception)
                {
                    Module.Trace.TraceError($"Cannot add symbol '{item.name}'. {exception.Message}", exception);
                }
            }
            return entrys;
        }

        private AdsDataTypeEntry GetDataTypeEntry(int index, uint[] dataTypeEntryOffsets, AdsBinaryReader dataTypeReader)
        {
            if ((index < 0) | (index >= dataTypeEntryOffsets.Length))
            {
                throw new ArgumentOutOfRangeException("index");
            }
            dataTypeReader.BaseStream.Position = dataTypeEntryOffsets[index];
            AdsDataTypeEntry entry = new AdsDataTypeEntry();
            entry.Read(-1L, this._symbolEncoding, dataTypeReader);
            return entry;
        }

        private AdsDatatypeId GetDataTypeId(string type)
        {
            ITcAdsDataType typeByName = this.GetTypeByName(type);
            if (typeByName != null)
            {
                return typeByName.DataTypeId;
            }
            ITcAdsDataType type3 = null;
            if ((this._defaultTypes != null) && this._defaultTypes.TryGetDataType(type, out type3))
            {
                return type3.DataTypeId;
            }
            int length = -1;
            bool isUnicode = false;
            return (!DataTypeStringParser.TryParseString(type, out length, out isUnicode) ? AdsDatatypeId.ADST_BIGTYPE : (!isUnicode ? AdsDatatypeId.ADST_STRING : AdsDatatypeId.ADST_WSTRING));
        }

        internal ReadOnlyTcAdsDataTypeCollection GetDataTypes() => 
            this._dataTypes.AsReadOnly();

        private int GetSizeByName(string type)
        {
            ITcAdsDataType typeByName = this.GetTypeByName(type);
            if (typeByName != null)
            {
                return typeByName.Size;
            }
            ITcAdsDataType type3 = null;
            if ((this._defaultTypes != null) && this._defaultTypes.TryGetDataType(type, out type3))
            {
                return type3.Size;
            }
            int length = -1;
            bool isUnicode = false;
            if (DataTypeStringParser.TryParseString(type, out length, out isUnicode))
            {
                int num2 = 1;
                if (isUnicode)
                {
                    num2 = 2;
                }
                length = (length + 1) * num2;
            }
            return length;
        }

        internal TcAdsSymbolInfo GetSubSymbol(TcAdsSymbolInfo parent, int subIndex, bool dereference)
        {
            if (parent == null)
            {
                throw new ArgumentNullException("parent");
            }
            TcAdsSymbolInfo info = null;
            try
            {
                ITcAdsDataType dataType = parent.DataType;
                ITcAdsDataType type2 = null;
                if (dataType != null)
                {
                    type2 = (ITcAdsDataType) dataType.ResolveType(DataTypeResolveStrategy.AliasReference);
                }
                if (type2 != null)
                {
                    int count = parent.SubSymbols.Count;
                    if (((type2 != null) && (type2.SubItems.Count > 0)) && (subIndex < type2.SubItems.Count))
                    {
                        info = this._subSymbolFactory.CreateSubSymbol(parent, subIndex);
                    }
                    else if ((type2 != null) && ((type2.Dimensions.Count > 0) || DataTypeStringParser.IsArray(type2.Name)))
                    {
                        info = this._subSymbolFactory.CreateArrayElement(parent, (TcAdsDataType) type2, subIndex);
                    }
                    else if ((type2 != null) && type2.IsReference)
                    {
                        this.SetPlatformPointerSize(type2.Size);
                    }
                    else if ((subIndex == 0) && type2.IsPointer)
                    {
                        this.SetPlatformPointerSize(type2.Size);
                        if (dereference)
                        {
                            string str;
                            DataTypeStringParser.TryParsePointer(type2.Name, out str);
                            TcAdsDataType referencedType = (TcAdsDataType) this.ResolveDataType(str);
                            if ((referencedType != null) & dereference)
                            {
                                bool flag = (referencedType.Flags & AdsDataTypeFlags.AnySizeArray) == AdsDataTypeFlags.AnySizeArray;
                                if ((referencedType.Size > 0) | flag)
                                {
                                    info = this._subSymbolFactory.CreatePointerSymbol(parent, referencedType);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Module.Trace.TraceError($"ParentSymbol: {parent.Name}", exception);
                throw;
            }
            return info;
        }

        private TcAdsSymbolInfo GetSubSymbol(TcAdsSymbolInfo symbol, string symbolName, int pos)
        {
            TcAdsSymbolInfo parent = null;
            if (symbol.DataType == null)
            {
                symbol.dataType = this.GetTypeByName(symbol.typeName);
            }
            if (symbol.DataType != null)
            {
                ITcAdsDataType type = symbol.ResolveType(DataTypeResolveStrategy.AliasReference);
                if (type.SubItems.Count > 0)
                {
                    string str = symbolName;
                    char[] anyOf = new char[] { '.', '[' };
                    int length = str.IndexOfAny(anyOf, pos + 1);
                    if (length != -1)
                    {
                        str = str.Substring(0, length);
                    }
                    bool flag = str[str.Length - 1] == '^';
                    if (flag)
                    {
                        str = str.Substring(0, str.Length - 1);
                    }
                    for (int i = 0; i < type.SubItems.Count; i++)
                    {
                        parent = this.GetSubSymbol(symbol, i, true);
                        if (parent.instancePath.Equals(str, StringComparison.OrdinalIgnoreCase))
                        {
                            if ((parent != null) & flag)
                            {
                                parent = this.GetSubSymbol(parent, 0, true);
                            }
                            if (length != -1)
                            {
                                parent = this.GetSubSymbol(parent, symbolName, length);
                            }
                            return parent;
                        }
                    }
                    return null;
                }
                if (type.Dimensions.Count > 0)
                {
                    SymbolParser.ArrayIndexType type2;
                    IList<int[]> jaggedIndices = null;
                    int index = symbolName.IndexOf(']', pos + 1);
                    if (index == -1)
                    {
                        return null;
                    }
                    int length = symbol.instancePath.Length;
                    if (!SymbolParser.TryParseIndices(symbolName.Substring(length, (index + 1) - length), out jaggedIndices, out type2))
                    {
                        goto TR_0013;
                    }
                    else if (jaggedIndices.Count <= 1)
                    {
                        ReadOnlyDimensionCollection dimensions = type.Dimensions;
                        bool flag3 = type2 == SymbolParser.ArrayIndexType.Oversample;
                        if (ArrayIndexConverter.TryCheckIndices(jaggedIndices, type))
                        {
                            int subIndex = -1;
                            if (ArrayIndexConverter.TryGetSubIndex(jaggedIndices[0], dimensions.LowerBounds, dimensions.UpperBounds, type2 == SymbolParser.ArrayIndexType.Oversample, out subIndex))
                            {
                                parent = this.GetSubSymbol(symbol, subIndex, true);
                                if ((parent != null) && (symbolName.Length > (index + 1)))
                                {
                                    parent = this.GetSubSymbol(parent, symbolName, index + 1);
                                }
                            }
                        }
                    }
                    else
                    {
                        goto TR_0013;
                    }
                }
            }
            return parent;
        TR_0013:
            return null;
        }

        public int GetSubSymbolCount(TcAdsSymbolInfo parent)
        {
            int num2;
            if (parent == null)
            {
                throw new ArgumentNullException("parent");
            }
            int count = 0;
            try
            {
                ITcAdsDataType type = parent.ResolveType(DataTypeResolveStrategy.AliasReference);
                string referencedType = null;
                if (!parent.TryGetReference(out referencedType))
                {
                    if (!parent.TryGetPointerRef(out referencedType))
                    {
                        if (((type != null) && (type.SubItems != null)) && (type.SubItems.Count > 0))
                        {
                            count = type.SubItems.Count;
                        }
                        else if (((type != null) && (type.Dimensions != null)) && (type.Dimensions.Count > 0))
                        {
                            ReadOnlyDimensionCollection dimensions = type.Dimensions;
                            if (dimensions != null)
                            {
                                count = 1;
                                int num3 = type.Dimensions.Count - 1;
                                while (true)
                                {
                                    if (num3 < 0)
                                    {
                                        if (parent.IsOversamplingArray)
                                        {
                                            count++;
                                        }
                                        break;
                                    }
                                    count *= dimensions[num3].ElementCount;
                                    num3--;
                                }
                            }
                            else
                            {
                                return 0;
                            }
                        }
                    }
                    else
                    {
                        this.SetPlatformPointerSize(parent.Size);
                        ITcAdsDataType typeByName = this.GetTypeByName(referencedType);
                        if (typeByName != null)
                        {
                            count = ((parent.typeEntryFlags & AdsDataTypeFlags.AnySizeArray) != AdsDataTypeFlags.AnySizeArray) ? ((typeByName.Size > 0) ? 1 : 0) : 1;
                        }
                        else
                        {
                            object[] args = new object[] { referencedType };
                            Module.Trace.TraceWarning("Cannot get referenced type '{0}'!", args);
                        }
                    }
                }
                else
                {
                    this.SetPlatformPointerSize(parent.Size);
                    ITcAdsDataType typeByName = this.GetTypeByName(referencedType);
                    count = (typeByName == null) ? 0 : typeByName.SubItems.Count;
                }
                return count;
            }
            catch (Exception exception)
            {
                Module.Trace.TraceError($"SymbolParent: {parent.ToString()}", exception);
                throw;
            }
            return num2;
        }

        public TcAdsSymbolInfo GetSymbol(int symbolIndex)
        {
            AdsSymbolEntry entry = null;
            return (!this._symbolTable.TryGetSymbol(symbolIndex, out entry) ? null : this.CreateSymbolInfo(entry, symbolIndex));
        }

        public TcAdsSymbolInfo GetSymbol(string symbolName)
        {
            TcAdsSymbolInfo symbol = null;
            int index = symbolName.IndexOf('.');
            string str = null;
            do
            {
                char[] anyOf = new char[] { '.', '[' };
                int length = symbolName.IndexOfAny(anyOf, index + 1);
                str = (length == -1) ? symbolName : symbolName.Substring(0, length);
                symbol = this.CreateSymbolInfo(str);
                if ((symbol != null) && (length != -1))
                {
                    symbol = this.GetSubSymbol(symbol, symbolName, length);
                    break;
                }
                index = symbolName.IndexOf('.', index + 1);
            }
            while (index >= 0);
            if (symbol == null)
            {
                symbol = this.CreateSymbolInfo(symbolName);
            }
            return symbol;
        }

        private AdsSymbolEntry GetSymbolEntry(int symbolIndex, uint[] symbolEntryOffsets, AdsBinaryReader symbolReader)
        {
            if ((symbolIndex < 0) || (symbolIndex >= this.SymbolCount))
            {
                return null;
            }
            symbolReader.BaseStream.Position = symbolEntryOffsets[symbolIndex];
            return new AdsSymbolEntry(-1L, this._symbolEncoding, symbolReader);
        }

        private ITcAdsDataType GetTypeByName(string type)
        {
            if (string.IsNullOrEmpty(type))
            {
                throw new ArgumentException();
            }
            ITcAdsDataType ret = null;
            this._dataTypes.TryGetDataType(type, out ret);
            return ret;
        }

        private void OnTypeCreated(IDataType dataType)
        {
            if (this.TypesGenerated != null)
            {
                DataTypeCollection types = new DataTypeCollection();
                types.Add(dataType);
                this.TypesGenerated(this, new DataTypeEventArgs(types));
            }
        }

        private void OnTypeResolveError(string name)
        {
            if (this.TypeResolveError != null)
            {
                this.TypeResolveError(this, new DataTypeNameEventArgs(name));
            }
        }

        private void OnTypesCreated(IEnumerable<IDataType> dataTypes)
        {
            if (this.TypesGenerated != null)
            {
                this.TypesGenerated(this, new DataTypeEventArgs(dataTypes));
            }
        }

        internal void Parse(AdsStream symbolStream, AdsStream dataTypeStream, TcAdsClient adsClient)
        {
            this._subSymbolFactory = new SubSymbolFactory(this);
            this._defaultTypes = DataTypeInfoTable.GetDefaultTypes(this);
            this.OnTypesCreated(this._defaultTypes);
            object[] args = new object[] { adsClient.Address, symbolStream.Length, dataTypeStream.Length };
            Module.Trace.TraceInformation("Client: {0}, SymbolStream: {1} bytes, DataTypeStream: {2} bytes", args);
            this._adsClient = adsClient;
            using (AdsBinaryReader reader = new AdsBinaryReader(symbolStream))
            {
                using (AdsBinaryReader reader2 = new AdsBinaryReader(dataTypeStream))
                {
                    uint[] numArray;
                    uint[] numArray2;
                    int symbolCount = 0;
                    symbolCount = this.countSymbols(reader);
                    object[] objArray2 = new object[] { symbolCount };
                    Module.Trace.TraceInformation("SymbolCount: {0}", objArray2);
                    this._symbolTable = this.fillSymbolTables(reader, symbolCount, out numArray);
                    int dataTypeCount = this.countDataTypes(reader2);
                    object[] objArray3 = new object[] { symbolCount };
                    Module.Trace.TraceInformation("DataTypeCount: {0}", objArray3);
                    Dictionary<string, int> dictionary = this.fillDataTypeTables(reader2, dataTypeCount, out numArray2);
                    this._dataTypes = this.collectDataTypes(dataTypeCount, numArray2, reader2);
                    this.expandDataTypes();
                }
            }
        }

        internal IDataType ResolveDataType(string name)
        {
            IDataType ret = null;
            this.TryResolveType(name, out ret);
            return ret;
        }

        private void SetPlatformPointerSize(int size)
        {
            if (((size != 0) && (size != 4)) && (size != 8))
            {
                throw new ArgumentOutOfRangeException("size");
            }
            if (this._platformPointerSize == 0)
            {
                this._platformPointerSize = size;
            }
        }

        public bool TryResolveType(string name, out IDataType ret)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("typeName");
            }
            ITcAdsDataType type = null;
            if ((this._dataTypes != null) && !this._dataTypes.TryGetDataType(name, out type))
            {
                int length = 0;
                bool isUnicode = false;
                AdsDatatypeArrayInfo[] dims = null;
                if (DataTypeStringParser.TryParseString(name, out length, out isUnicode))
                {
                    AdsDatatypeId dataType = isUnicode ? AdsDatatypeId.ADST_WSTRING : AdsDatatypeId.ADST_STRING;
                    type = new TcAdsDataType(name, dataType, isUnicode ? ((uint) ((length + 1) * 2)) : ((uint) (length + 1)), AdsDataTypeFlags.DataType, DataTypeCategory.String, typeof(string), this);
                }
                else
                {
                    string str;
                    if (DataTypeStringParser.TryParsePointer(name, out str))
                    {
                        ITcAdsDataType type2 = (ITcAdsDataType) this.ResolveDataType(str);
                        type = new TcAdsDataType(name, AdsDatatypeId.ADST_BIGTYPE, (uint) this.PlatformPointerSize, AdsDataTypeFlags.DataType, DataTypeCategory.Pointer, null, this);
                    }
                    else if (DataTypeStringParser.TryParseReference(name, out str))
                    {
                        ITcAdsDataType type3 = (ITcAdsDataType) this.ResolveDataType(str);
                        type = new TcAdsDataType(name, AdsDatatypeId.ADST_BIGTYPE, (uint) this.PlatformPointerSize, AdsDataTypeFlags.DataType, DataTypeCategory.Reference, null, this);
                    }
                    else if (DataTypeStringParser.TryParseArray(name, out dims, out str))
                    {
                        ITcAdsDataType type4 = (ITcAdsDataType) this.ResolveDataType(str);
                        if (type4 != null)
                        {
                            type = new TcAdsDataType(name, str, (uint) type4.Size, dims, this);
                        }
                    }
                }
                if (type == null)
                {
                    this.OnTypeResolveError(name);
                }
                else
                {
                    this._dataTypes.Add(type);
                    this.OnTypeCreated(type);
                }
            }
            ret = type;
            return (ret != null);
        }

        internal ReadOnlyTcAdsDataTypeCollection DefaultTypes =>
            this._defaultTypes;

        public int SymbolCount =>
            this._symbolTable.Count;

        public int PlatformPointerSize =>
            this._platformPointerSize;

        private class SubSymbolFactory
        {
            private AdsParseSymbols _parser;

            internal SubSymbolFactory(AdsParseSymbols parser)
            {
                this._parser = parser;
            }

            private void calcArrayElementIndexGroupIndexOffset(int subIndex, TcAdsSymbolInfo arrayInstance, int elementSize, out uint indexGroup, out uint indexOffset)
            {
                TcAdsDataType dataType = (TcAdsDataType) arrayInstance.DataType;
                bool isBitType = arrayInstance.IsBitType;
                if ((dataType.Flags & (AdsDataTypeFlags.None | AdsDataTypeFlags.PropItem)) == (AdsDataTypeFlags.None | AdsDataTypeFlags.PropItem))
                {
                    indexGroup = 0xf017;
                    indexOffset = dataType.TypeHashValue;
                }
                else if (isBitType)
                {
                    this.calcBitAccessAddress(arrayInstance, 0, out indexGroup, out indexOffset);
                }
                else if (arrayInstance.IsReference || arrayInstance.IsDereferencedReference)
                {
                    indexGroup = 0xf016;
                    indexOffset = 0;
                }
                else
                {
                    indexGroup = (uint) arrayInstance.IndexGroup;
                    indexOffset = ((uint) arrayInstance.IndexOffset) + ((uint) (elementSize * subIndex));
                }
            }

            private void calcBitAccessAddress(TcAdsSymbolInfo parent, int subEntryOffset, out uint indexGroup, out uint indexOffset)
            {
                if (parent == null)
                {
                    throw new ArgumentNullException("parent");
                }
                if (subEntryOffset < 0)
                {
                    throw new ArgumentOutOfRangeException("subEntryOffset");
                }
                if (parent.IsStatic)
                {
                    indexGroup = 0xf019;
                    indexOffset = 0;
                }
                else
                {
                    indexGroup = parent.indexGroup;
                    indexOffset = parent.indexOffset + ((uint) subEntryOffset);
                }
                uint num = indexGroup;
                if (num > 0x4030)
                {
                    if (num > 0xf014)
                    {
                        if (num == 0xf016)
                        {
                            indexGroup = 0xf01b;
                            indexOffset = 0;
                            return;
                        }
                        if ((num != 0xf020) && (num != 0xf030))
                        {
                            goto TR_0004;
                        }
                    }
                    else if (num != 0x4040)
                    {
                        if (num == 0xf014)
                        {
                            indexGroup = 0xf01a;
                            indexOffset = 0;
                            return;
                        }
                        goto TR_0004;
                    }
                    goto TR_0005;
                }
                else if (num > 0x4010)
                {
                    if ((num == 0x4020) || (num == 0x4030))
                    {
                        goto TR_0005;
                    }
                }
                else if ((num == 0x4000) || (num == 0x4010))
                {
                    goto TR_0005;
                }
            TR_0004:
                if (indexGroup > 0x100000)
                {
                    uint num2 = (uint) ((parent.indexOffset & 0x3f000000) >> 0x18);
                    uint num3 = ((parent.indexOffset & 0xffffff) * 8) + ((uint) subEntryOffset);
                    indexOffset = (0xc0000000 | (0x3f000000 & (num2 << 0x18))) | (0xffffff & num3);
                }
                return;
            TR_0005:
                indexGroup = parent.indexGroup + 1;
                indexOffset = (parent.indexOffset * 8) + ((uint) subEntryOffset);
            }

            private int calcElementBaseSize(TcAdsSymbolInfo parentArrayInstance)
            {
                TcAdsDataType type = (TcAdsDataType) parentArrayInstance.DataType.ResolveType(DataTypeResolveStrategy.AliasReference);
                int size = type.Size;
                bool isBitType = parentArrayInstance.IsBitType;
                ReadOnlyDimensionCollection dimensions = type.Dimensions;
                for (int i = type.Dimensions.Count - 1; i >= 0; i--)
                {
                    if (isBitType)
                    {
                        size = 1;
                    }
                    else if (dimensions[i].ElementCount > 0)
                    {
                        size /= dimensions[i].ElementCount;
                    }
                }
                return size;
            }

            private void calcFieldAddress(TcAdsSymbolInfo parent, TcAdsSymbolInfo subSymbol, TcAdsSubItem subEntry, out uint indexGroup, out uint indexOffset)
            {
                if (parent == null)
                {
                    throw new ArgumentNullException("parent");
                }
                if (subSymbol == null)
                {
                    throw new ArgumentNullException("subSymbol");
                }
                int subEntryOffset = 0;
                if (subEntry != null)
                {
                    subEntryOffset = subEntry.Offset;
                }
                if (subSymbol.IsStatic)
                {
                    indexGroup = 0xf019;
                    indexOffset = 0;
                }
                else if (subSymbol.IsBitType && !parent.IsBitType)
                {
                    this.calcBitAccessAddress(parent, subEntryOffset, out indexGroup, out indexOffset);
                }
                else if ((subEntry != null) && ((subEntry.Flags & (AdsDataTypeFlags.None | AdsDataTypeFlags.PropItem)) > AdsDataTypeFlags.None))
                {
                    indexGroup = 0xf017;
                    indexOffset = subEntry.TypeHashValue;
                }
                else if (subSymbol.IsDereferencedPointer)
                {
                    if (subSymbol.IsBitType)
                    {
                        indexGroup = 0xf01a;
                        indexOffset = 0;
                    }
                    else
                    {
                        indexGroup = 0xf014;
                        indexOffset = 0;
                    }
                }
                else if ((subSymbol.Category != DataTypeCategory.Reference) && !subSymbol.IsDereferencedReference)
                {
                    indexGroup = parent.indexGroup;
                    indexOffset = parent.indexOffset + ((uint) subEntryOffset);
                }
                else if (subSymbol.IsBitType)
                {
                    indexGroup = 0xf01b;
                    indexOffset = 0;
                }
                else
                {
                    indexGroup = 0xf016;
                    indexOffset = 0;
                }
            }

            internal TcAdsSymbolInfo CreateArrayElement(TcAdsSymbolInfo parentArrayInstance, TcAdsDataType parentArrayType, int subIndex)
            {
                if (parentArrayInstance == null)
                {
                    throw new ArgumentNullException("arrayInstance");
                }
                if (parentArrayType == null)
                {
                    throw new ArgumentNullException("parentArrayType");
                }
                TcAdsSymbolInfo info = null;
                ITcAdsDataType baseType = parentArrayType.BaseType;
                if (baseType == null)
                {
                    DataTypeException innerException = new DataTypeException("Base type of Array '{0}' not defined!", parentArrayType);
                    throw new AdsSymbolException("Cannot create array element!", parentArrayInstance, innerException);
                }
                bool isBitType = parentArrayInstance.IsBitType;
                ReadOnlyDimensionCollection dimensions = parentArrayType.Dimensions;
                int elementSize = this.calcElementBaseSize(parentArrayInstance);
                if (subIndex < parentArrayType.Dimensions.ElementCount)
                {
                    info = new TcAdsSymbolInfo(this._parser, parentArrayInstance, subIndex) {
                        size = (uint) elementSize,
                        typeEntryFlags = this.createElementTypeEntryFlags(parentArrayInstance, (TcAdsDataType) baseType),
                        flags = this.createElementSymbolFlags(parentArrayInstance, (TcAdsDataType) baseType),
                        dataTypeId = (baseType == null) ? AdsDatatypeId.ADST_BIGTYPE : baseType.DataTypeId
                    };
                    if (baseType != null)
                    {
                        info.typeName = baseType.Name;
                        info.arrayInfo = baseType.Dimensions.ToArray();
                    }
                    else
                    {
                        info.typeName = parentArrayType.BaseTypeName;
                        info.arrayInfo = null;
                    }
                    info.comment = parentArrayInstance.Comment;
                    this.calcArrayElementIndexGroupIndexOffset(subIndex, parentArrayInstance, elementSize, out info.indexGroup, out info.indexOffset);
                    string str = ArrayIndexConverter.SubIndexToIndexString(parentArrayType.Dimensions.LowerBounds, parentArrayType.Dimensions.UpperBounds, subIndex);
                    info.instancePath = parentArrayInstance.instancePath + str;
                    info.shortName = parentArrayInstance.shortName + str;
                    info.attributes = parentArrayInstance.attributes;
                }
                if ((subIndex == parentArrayType.Dimensions.ElementCount) && ((parentArrayType.Flags & (AdsDataTypeFlags.None | AdsDataTypeFlags.Oversample)) != AdsDataTypeFlags.None))
                {
                    info = new TcAdsSymbolInfo(this._parser, parentArrayInstance, subIndex) {
                        size = (uint) elementSize,
                        typeEntryFlags = this.createElementTypeEntryFlags(parentArrayInstance, (TcAdsDataType) baseType),
                        flags = this.createElementSymbolFlags(parentArrayInstance, (TcAdsDataType) baseType)
                    };
                    if (info.typeEntryFlags.HasFlag(AdsDataTypeFlags.None | AdsDataTypeFlags.Static))
                    {
                        info.indexGroup = 0xf019;
                        info.indexOffset = 0;
                    }
                    else
                    {
                        info.indexGroup = parentArrayInstance.indexGroup;
                        info.indexOffset = parentArrayInstance.indexOffset;
                    }
                    if (baseType != null)
                    {
                        info.dataTypeId = baseType.DataTypeId;
                        info.typeName = baseType.Name;
                    }
                    else
                    {
                        info.dataTypeId = AdsDatatypeId.ADST_BIGTYPE;
                        info.typeName = parentArrayType.BaseTypeName;
                    }
                    info.comment = parentArrayInstance.Comment;
                    string str2 = ArrayIndexConverter.OversamplingSubElementToString(subIndex);
                    info.instancePath = parentArrayInstance.instancePath + str2;
                    info.shortName = parentArrayInstance.shortName + str2;
                    if ((parentArrayType.Flags & (AdsDataTypeFlags.None | AdsDataTypeFlags.PropItem)) == (AdsDataTypeFlags.None | AdsDataTypeFlags.PropItem))
                    {
                        info.indexGroup = 0xf017;
                        info.indexOffset = parentArrayType.TypeHashValue;
                    }
                    info.attributes = parentArrayInstance.attributes;
                }
                return info;
            }

            private AdsSymbolFlags createElementSymbolFlags(TcAdsSymbolInfo arrayInstance, TcAdsDataType elementType)
            {
                AdsSymbolFlags none = AdsSymbolFlags.None;
                if (elementType != null)
                {
                    none = DataTypeFlagConverter.Convert(elementType.Flags);
                }
                none = (none & (AdsSymbolFlags.Attributes | AdsSymbolFlags.BitValue | AdsSymbolFlags.ExtendedFlags | AdsSymbolFlags.InitOnReset | AdsSymbolFlags.ItfMethodAccess | AdsSymbolFlags.MethodDeref | AdsSymbolFlags.Persistent | AdsSymbolFlags.ReadOnly | AdsSymbolFlags.ReferenceTo | AdsSymbolFlags.Static | AdsSymbolFlags.TComInterfacePtr | AdsSymbolFlags.TypeGuid)) | (AdsSymbolFlags.ContextMask & arrayInstance.flags);
                if (arrayInstance.IsBitType)
                {
                    none |= AdsSymbolFlags.BitValue;
                }
                if (arrayInstance.IsPersistent)
                {
                    none |= AdsSymbolFlags.None | AdsSymbolFlags.Persistent;
                }
                return none;
            }

            private AdsDataTypeFlags createElementTypeEntryFlags(TcAdsSymbolInfo parentArrayInstance, TcAdsDataType elementType) => 
                ((elementType == null) ? AdsDataTypeFlags.None : elementType.Flags);

            internal TcAdsSymbolInfo CreatePointerSymbol(TcAdsSymbolInfo parent, TcAdsDataType referencedType)
            {
                if (parent == null)
                {
                    throw new ArgumentNullException("parent");
                }
                if (referencedType == null)
                {
                    throw new ArgumentNullException("referencedType");
                }
                return new TcAdsSymbolInfo(this._parser, parent, 0) { 
                    indexGroup = !referencedType.IsBitType ? 0xf014 : 0xf01a,
                    indexOffset = 0,
                    size = (uint) referencedType.Size,
                    typeName = referencedType.Name,
                    dataTypeId = referencedType.DataTypeId,
                    comment = string.Empty,
                    instancePath = parent.instancePath + "^",
                    shortName = parent.shortName + "^",
                    typeEntryFlags = parent.typeEntryFlags,
                    flags = parent.flags,
                    arrayInfo = parent.arrayInfo
                };
            }

            internal TcAdsSymbolInfo CreateReferenceSymbol(TcAdsSymbolInfo parent, int subIndex)
            {
                if (parent == null)
                {
                    throw new ArgumentNullException("parent");
                }
                TcAdsSymbolInfo subSymbol = null;
                if (subIndex == 0)
                {
                    TcAdsDataType dataType = (TcAdsDataType) parent.DataType;
                    string referencedType = string.Empty;
                    if (DataTypeStringParser.TryParseReference(parent.DataType.Name, out referencedType))
                    {
                        TcAdsDataType typeByName = (TcAdsDataType) this._parser.GetTypeByName(referencedType);
                        if (typeByName != null)
                        {
                            subSymbol = new TcAdsSymbolInfo(this._parser, parent, subIndex, typeByName);
                            this.calcFieldAddress(parent, subSymbol, null, out subSymbol.indexGroup, out subSymbol.indexOffset);
                        }
                        else
                        {
                            object[] args = new object[] { referencedType };
                            Module.Trace.TraceWarning("Cannot create Reference Symbol. Dereferenced Type '{0}' not found!", args);
                        }
                    }
                }
                return subSymbol;
            }

            internal TcAdsSymbolInfo CreateSubSymbol(TcAdsSymbolInfo parent, int subIndex)
            {
                if (parent == null)
                {
                    throw new ArgumentNullException("parent");
                }
                ITcAdsDataType dataType = parent.DataType;
                if (dataType == null)
                {
                    throw new ArgumentException("DataType not specified", "parent");
                }
                if (((subIndex < 0) || (dataType.SubItems == null)) || (subIndex >= dataType.SubItems.Count))
                {
                    throw new ArgumentOutOfRangeException("subIndex");
                }
                TcAdsSymbolInfo subSymbol = null;
                if (parent.IsPointer && (subIndex > 0))
                {
                    throw new ArgumentOutOfRangeException("subIndex");
                }
                TcAdsSubItem typeSubEntry = (TcAdsSubItem) dataType.SubItems[subIndex];
                TcAdsDataType typeByName = (TcAdsDataType) this._parser.GetTypeByName(typeSubEntry.Name);
                if (typeSubEntry != null)
                {
                    if (typeByName != null)
                    {
                        typeSubEntry.AlignSubItemToType(typeByName);
                    }
                    subSymbol = new TcAdsSymbolInfo(this._parser, parent, subIndex) {
                        size = (uint) typeSubEntry.Size,
                        dataTypeId = typeSubEntry.DataTypeId,
                        typeEntryFlags = this.createTypeEntryFlags(parent, typeSubEntry),
                        flags = this.createSubSymbolFlags(parent, typeSubEntry),
                        shortName = typeSubEntry.SubItemName
                    };
                    subSymbol.instancePath = $"{parent.instancePath}.{subSymbol.shortName}";
                    subSymbol.typeName = typeSubEntry.Name;
                    subSymbol.comment = typeSubEntry.Comment;
                    subSymbol.arrayInfo = typeSubEntry.ArrayInfo;
                    subSymbol.attributes = typeSubEntry.Attributes;
                    this.calcFieldAddress(parent, subSymbol, typeSubEntry, out subSymbol.indexGroup, out subSymbol.indexOffset);
                }
                return subSymbol;
            }

            private AdsSymbolFlags createSubSymbolFlags(TcAdsSymbolInfo parent, TcAdsSubItem typeSubEntry) => 
                (((DataTypeFlagConverter.Convert(typeSubEntry.Flags) & (AdsSymbolFlags.Attributes | AdsSymbolFlags.BitValue | AdsSymbolFlags.ExtendedFlags | AdsSymbolFlags.InitOnReset | AdsSymbolFlags.ItfMethodAccess | AdsSymbolFlags.MethodDeref | AdsSymbolFlags.Persistent | AdsSymbolFlags.ReadOnly | AdsSymbolFlags.ReferenceTo | AdsSymbolFlags.Static | AdsSymbolFlags.TComInterfacePtr | AdsSymbolFlags.TypeGuid)) | (AdsSymbolFlags.ContextMask & parent.flags)) | ((AdsSymbolFlags.None | AdsSymbolFlags.Persistent) & parent.flags));

            private AdsDataTypeFlags createTypeEntryFlags(TcAdsSymbolInfo parent, TcAdsSubItem typeSubEntry) => 
                (typeSubEntry.Flags | ((AdsDataTypeFlags.None | AdsDataTypeFlags.PropItem | AdsDataTypeFlags.ReferenceTo) & parent.typeEntryFlags));
        }
    }
}

