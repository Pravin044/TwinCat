namespace TwinCAT.Ads.Internal
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using TwinCAT.Ads;
    using TwinCAT.PlcOpen;
    using TwinCAT.TypeSystem;

    internal class DataTypeInfoTable : IDataTypeResolver, ITypeBinderEvents, IDataTypeInfoTable
    {
        private TcAdsDataTypeCollection _dataTypes = new TcAdsDataTypeCollection();
        private IAdsConnection _adsClient;
        private static TcAdsDataType[] _defaultTypes;
        private int _targetPointerSize;
        private Encoding _encoding;
        [CompilerGenerated]
        private EventHandler<DataTypeEventArgs> TypesGenerated;
        [CompilerGenerated]
        private EventHandler<DataTypeNameEventArgs> TypeResolveError;

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

        static DataTypeInfoTable()
        {
            TcAdsDataType[] typeArray1 = new TcAdsDataType[0x2a];
            typeArray1[0] = new TcAdsDataType("VOID", AdsDatatypeId.ADST_VOID, 0, AdsDataTypeFlags.DataType, DataTypeCategory.Primitive, null, null);
            typeArray1[1] = new TcAdsDataType("POINTER TO VOID", AdsDatatypeId.ADST_VOID, 0, AdsDataTypeFlags.DataType, DataTypeCategory.Pointer, null, null);
            typeArray1[2] = new TcAdsDataType("PVOID", AdsDatatypeId.ADST_BIGTYPE, 0, AdsDataTypeFlags.DataType, DataTypeCategory.Alias, "POINTER TO VOID", null, null);
            typeArray1[3] = new TcAdsDataType("BIT", AdsDatatypeId.ADST_BIT, 1, AdsDataTypeFlags.BitValues | AdsDataTypeFlags.DataType, DataTypeCategory.Primitive, typeof(bool), null);
            typeArray1[4] = new TcAdsDataType("BIT2", AdsDatatypeId.ADST_BIT, 2, AdsDataTypeFlags.BitValues | AdsDataTypeFlags.DataType, DataTypeCategory.Primitive, typeof(byte), null);
            typeArray1[5] = new TcAdsDataType("BIT3", AdsDatatypeId.ADST_BIT, 3, AdsDataTypeFlags.BitValues | AdsDataTypeFlags.DataType, DataTypeCategory.Primitive, typeof(byte), null);
            typeArray1[6] = new TcAdsDataType("BIT4", AdsDatatypeId.ADST_BIT, 4, AdsDataTypeFlags.BitValues | AdsDataTypeFlags.DataType, DataTypeCategory.Primitive, typeof(byte), null);
            typeArray1[7] = new TcAdsDataType("BIT5", AdsDatatypeId.ADST_BIT, 5, AdsDataTypeFlags.BitValues | AdsDataTypeFlags.DataType, DataTypeCategory.Primitive, typeof(byte), null);
            typeArray1[8] = new TcAdsDataType("BIT6", AdsDatatypeId.ADST_BIT, 6, AdsDataTypeFlags.BitValues | AdsDataTypeFlags.DataType, DataTypeCategory.Primitive, typeof(byte), null);
            typeArray1[9] = new TcAdsDataType("BIT7", AdsDatatypeId.ADST_BIT, 7, AdsDataTypeFlags.BitValues | AdsDataTypeFlags.DataType, DataTypeCategory.Primitive, typeof(byte), null);
            typeArray1[10] = new TcAdsDataType("BIT8", AdsDatatypeId.ADST_BIT, 8, AdsDataTypeFlags.BitValues | AdsDataTypeFlags.DataType, DataTypeCategory.Primitive, typeof(byte), null);
            typeArray1[11] = new TcAdsDataType("SINT", AdsDatatypeId.ADST_INT8, 1, DataTypeCategory.Primitive, typeof(sbyte));
            typeArray1[12] = new TcAdsDataType("USINT", AdsDatatypeId.ADST_UINT8, 1, DataTypeCategory.Primitive, typeof(byte));
            typeArray1[13] = new TcAdsDataType("BYTE", AdsDatatypeId.ADST_UINT8, 1, DataTypeCategory.Primitive, typeof(byte));
            typeArray1[14] = new TcAdsDataType("UINT8", AdsDatatypeId.ADST_UINT8, 1, DataTypeCategory.Primitive, typeof(byte));
            typeArray1[15] = new TcAdsDataType("INT", AdsDatatypeId.ADST_INT16, 2, DataTypeCategory.Primitive, typeof(short));
            typeArray1[0x10] = new TcAdsDataType("INT16", AdsDatatypeId.ADST_INT16, 2, DataTypeCategory.Primitive, typeof(short));
            typeArray1[0x11] = new TcAdsDataType("UINT", AdsDatatypeId.ADST_UINT16, 2, DataTypeCategory.Primitive, typeof(ushort));
            typeArray1[0x12] = new TcAdsDataType("WORD", AdsDatatypeId.ADST_UINT16, 2, DataTypeCategory.Primitive, typeof(ushort));
            typeArray1[0x13] = new TcAdsDataType("UINT16", AdsDatatypeId.ADST_UINT16, 2, DataTypeCategory.Primitive, typeof(ushort));
            typeArray1[20] = new TcAdsDataType("DINT", AdsDatatypeId.ADST_INT32, 4, DataTypeCategory.Primitive, typeof(int));
            typeArray1[0x15] = new TcAdsDataType("INT32", AdsDatatypeId.ADST_INT32, 4, DataTypeCategory.Primitive, typeof(int));
            typeArray1[0x16] = new TcAdsDataType("UDINT", AdsDatatypeId.ADST_UINT32, 4, DataTypeCategory.Primitive, typeof(uint));
            typeArray1[0x17] = new TcAdsDataType("UINT32", AdsDatatypeId.ADST_UINT32, 4, DataTypeCategory.Primitive, typeof(int));
            typeArray1[0x18] = new TcAdsDataType("UXINT", AdsDatatypeId.ADST_BIGTYPE, 0, AdsDataTypeFlags.DataType, DataTypeCategory.Alias, "PVOID", null, null);
            typeArray1[0x19] = new TcAdsDataType("DWORD", AdsDatatypeId.ADST_UINT32, 4, DataTypeCategory.Primitive, typeof(uint));
            typeArray1[0x1a] = new TcAdsDataType("REAL", AdsDatatypeId.ADST_REAL32, 4, DataTypeCategory.Primitive, typeof(float));
            typeArray1[0x1b] = new TcAdsDataType("FLOAT", AdsDatatypeId.ADST_REAL32, 4, DataTypeCategory.Primitive, typeof(float));
            typeArray1[0x1c] = new TcAdsDataType("LREAL", AdsDatatypeId.ADST_REAL64, 8, DataTypeCategory.Primitive, typeof(double));
            typeArray1[0x1d] = new TcAdsDataType("DOUBLE", AdsDatatypeId.ADST_REAL64, 8, DataTypeCategory.Primitive, typeof(double));
            typeArray1[30] = new TcAdsDataType("BOOL", AdsDatatypeId.ADST_BIT, 1, DataTypeCategory.Primitive, typeof(bool));
            typeArray1[0x1f] = new TcAdsDataType("TIME", AdsDatatypeId.ADST_BIGTYPE, 4, DataTypeCategory.Primitive, typeof(TIME));
            typeArray1[0x20] = new TcAdsDataType("TOD", AdsDatatypeId.ADST_BIGTYPE, 4, DataTypeCategory.Primitive, typeof(TOD));
            typeArray1[0x21] = new TcAdsDataType("DATE", AdsDatatypeId.ADST_BIGTYPE, 4, DataTypeCategory.Primitive, typeof(DATE));
            typeArray1[0x22] = new TcAdsDataType("DT", AdsDatatypeId.ADST_BIGTYPE, 4, DataTypeCategory.Primitive, typeof(DT));
            typeArray1[0x23] = new TcAdsDataType("LTIME", AdsDatatypeId.ADST_BIGTYPE, 8, DataTypeCategory.Primitive, typeof(LTIME));
            typeArray1[0x24] = new TcAdsDataType("DATE_AND_TIME", AdsDatatypeId.ADST_BIGTYPE, 4, DataTypeCategory.Alias, "DT", typeof(DT));
            typeArray1[0x25] = new TcAdsDataType("TIME_OF_DAY", AdsDatatypeId.ADST_BIGTYPE, 4, DataTypeCategory.Alias, "TOD", typeof(TOD));
            typeArray1[0x26] = new TcAdsDataType("LINT", AdsDatatypeId.ADST_INT64, 8, DataTypeCategory.Primitive, typeof(long));
            typeArray1[0x27] = new TcAdsDataType("ULINT", AdsDatatypeId.ADST_UINT64, 8, DataTypeCategory.Primitive, typeof(ulong));
            typeArray1[40] = new TcAdsDataType("LWORD", AdsDatatypeId.ADST_UINT64, 8, DataTypeCategory.Primitive, typeof(ulong));
            typeArray1[0x29] = new TcAdsDataType("OTCID", AdsDatatypeId.ADST_BIGTYPE, 4, DataTypeCategory.Alias, "UDINT", typeof(uint));
            _defaultTypes = typeArray1;
        }

        public DataTypeInfoTable(IAdsConnection adsClient, Encoding encoding, int targetPointerSize)
        {
            this._adsClient = adsClient;
            this._encoding = encoding;
            this._targetPointerSize = targetPointerSize;
            this.Clear();
        }

        public void Clear()
        {
            DataTypeInfoTable table = this;
            lock (table)
            {
                this._dataTypes.Clear();
                this._dataTypes.AddRange(GetDefaultTypes(this));
            }
        }

        internal static ReadOnlyTcAdsDataTypeCollection GetDefaultTypes(IDataTypeResolver resolver)
        {
            TcAdsDataTypeCollection types = new TcAdsDataTypeCollection(_defaultTypes);
            foreach (TcAdsDataType type in types)
            {
                type.SetResolver(resolver);
            }
            return types.AsReadOnly();
        }

        private void OnResolveError(string typeName)
        {
            if (this.TypeResolveError != null)
            {
                this.TypeResolveError(this, new DataTypeNameEventArgs(typeName));
            }
        }

        private void OnTypeGenerated(IDataType type)
        {
            if (this.TypesGenerated != null)
            {
                DataTypeCollection types = new DataTypeCollection();
                types.Add(type);
                this.TypesGenerated(this, new DataTypeEventArgs(types));
            }
        }

        private void OnTypesGenerated(DataTypeCollection types)
        {
            if (this.TypesGenerated != null)
            {
                this.TypesGenerated(this, new DataTypeEventArgs(types));
            }
        }

        internal IDataType ResolveDataType(string name)
        {
            IDataType type = null;
            this.TryResolveType(name, out type);
            return type;
        }

        public AdsErrorCode TryLoadType(string name, bool lookup, out ITcAdsDataType type)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentOutOfRangeException("name");
            }
            if (lookup)
            {
                DataTypeInfoTable table = this;
                lock (table)
                {
                    if (this._dataTypes.TryGetDataType(name, out type))
                    {
                        return AdsErrorCode.NoError;
                    }
                }
            }
            int length = 0;
            bool isUnicode = false;
            if (DataTypeStringParser.TryParseString(name, out length, out isUnicode))
            {
                int byteCount = 0;
                AdsDatatypeId dataType = AdsDatatypeId.ADST_VOID;
                if (isUnicode)
                {
                    char[] chars = new char[] { 'a' };
                    byteCount = Encoding.Unicode.GetByteCount(chars);
                    dataType = AdsDatatypeId.ADST_WSTRING;
                }
                else
                {
                    char[] chars = new char[] { 'a' };
                    byteCount = Encoding.ASCII.GetByteCount(chars);
                    dataType = AdsDatatypeId.ADST_STRING;
                }
                type = new TcAdsDataType(name, dataType, (uint) ((length + 1) * byteCount), DataTypeCategory.String, typeof(string));
                DataTypeInfoTable table2 = this;
                lock (table2)
                {
                    this._dataTypes.Add(type);
                }
                this.OnTypeGenerated(type);
                return AdsErrorCode.NoError;
            }
            AdsStream stream = new AdsStream(name.Length + 1);
            using (AdsBinaryWriter writer = new AdsBinaryWriter(stream))
            {
                writer.WritePlcString(name, name.Length + 1, this._encoding);
                AdsStream rdDataStream = new AdsStream(0xffff);
                try
                {
                    int num3 = this._adsClient.ReadWrite(0xf011, 0, rdDataStream, stream);
                }
                catch (AdsErrorException exception1)
                {
                    if (exception1.ErrorCode != AdsErrorCode.DeviceSymbolNotFound)
                    {
                        throw;
                    }
                    type = null;
                }
                using (AdsBinaryReader reader = new AdsBinaryReader(rdDataStream))
                {
                    AdsDataTypeEntry entry = new AdsDataTypeEntry(true, this._encoding, reader);
                    type = new TcAdsDataType(entry, this);
                    DataTypeInfoTable table3 = this;
                    lock (table3)
                    {
                        this._dataTypes.Add(type);
                    }
                }
            }
            if (type != null)
            {
                return AdsErrorCode.NoError;
            }
            this.OnResolveError(name);
            return AdsErrorCode.DeviceSymbolNotFound;
        }

        public bool TryResolveType(string name, out IDataType type)
        {
            ITcAdsDataType type2 = null;
            if (this.TryLoadType(name, true, out type2) == AdsErrorCode.NoError)
            {
                type = type2;
                return true;
            }
            type = null;
            return false;
        }

        public ReadOnlyTcAdsDataTypeCollection DataTypes =>
            this._dataTypes.AsReadOnly();

        public int PlatformPointerSize =>
            this._targetPointerSize;
    }
}

