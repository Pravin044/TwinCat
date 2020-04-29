namespace TwinCAT.Ads.TypeSystem
{
    using System;
    using System.ComponentModel;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Text;
    using TwinCAT;
    using TwinCAT.Ads;
    using TwinCAT.Ads.Internal;
    using TwinCAT.PlcOpen;
    using TwinCAT.TypeSystem;
    using TwinCAT.TypeSystem.Generic;
    using TwinCAT.ValueAccess;

    public sealed class AdsSymbolLoader : IAdsSymbolLoader, ISymbolLoader, ISymbolProvider, ITypeBinderEvents, IInternalSymbolProvider, IDisposable, IDynamicSymbolLoader
    {
        private IAdsConnection _connection;
        private ISymbolFactoryServices _symbolFactorServices;
        private bool _disposed;
        private SymbolLoaderSettings _settings = SymbolLoaderSettings.Default;
        private NamespaceCollection _namespaces;
        private SymbolUploadInfo _symbolUploadInfo;
        private const uint ADSIGRP_SYM_UPLOADINFO2 = 0xf00f;
        private const uint ADSIGRP_SYM_UPLOADINFO = 0xf00c;
        private const uint ADSIGRP_SYM_UPLOAD = 0xf00b;
        private const uint ADSIGRP_SYM_DT_UPLOAD = 0xf00e;
        internal static TimeSpan DEFAULT_TIMEOUT = TimeSpan.FromMilliseconds(30000.0);
        private TimeSpan _timeout = DEFAULT_TIMEOUT;
        private DataTypeCollection<IDataType> _buildInTypes;
        private string _rootNamespace = string.Empty;
        private SymbolCollection<ISymbol> _symbols;
        private NotificationSettings _notificationSettings = NotificationSettings.Default;

        public event EventHandler<DataTypeNameEventArgs> TypeResolveError
        {
            add
            {
                this.Binder.TypeResolveError += value;
            }
            remove
            {
                this.Binder.TypeResolveError -= value;
            }
        }

        public event EventHandler<DataTypeEventArgs> TypesGenerated
        {
            add
            {
                this.Binder.TypesGenerated += value;
            }
            remove
            {
                this.Binder.TypesGenerated -= value;
            }
        }

        internal AdsSymbolLoader(IAdsConnection connection, SymbolLoaderSettings settings, IAccessorRawValue accessor, ISession session, SymbolUploadInfo symbolsInfo)
        {
            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }
            if (accessor == null)
            {
                throw new ArgumentNullException("accessor");
            }
            if (symbolsInfo == null)
            {
                throw new ArgumentNullException("symbolsInfo");
            }
            ISymbolFactory symbolFactory = null;
            this._connection = connection;
            this._symbolUploadInfo = symbolsInfo;
            symbolFactory = (settings.SymbolsLoadMode != SymbolsLoadMode.DynamicTree) ? ((ISymbolFactory) new TwinCAT.Ads.TypeSystem.SymbolFactory(settings.NonCachedArrayElements)) : ((ISymbolFactory) new DynamicSymbolFactory(new TwinCAT.Ads.TypeSystem.SymbolFactory(settings.NonCachedArrayElements), settings.NonCachedArrayElements));
            this._settings = settings;
            AdsBinder binder = new AdsBinder(this._connection.Address, this, symbolFactory, this.UseVirtualInstances);
            this._symbolFactorServices = new SymbolFactoryServices(binder, symbolFactory, accessor, session);
            symbolFactory.Initialize(this._symbolFactorServices);
            this._rootNamespace = this._connection.Address.ToString();
            this._namespaces = new NamespaceCollection();
        }

        private void alignBaseTypes(IBinder binder)
        {
        }

        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        public static DataTypeCollection<IDataType> CreateBuildInTypes()
        {
            DataTypeCollection<IDataType> types = new DataTypeCollection<IDataType>();
            DataType item = new PrimitiveType("TOD", AdsDatatypeId.ADST_BIGTYPE, 4, PrimitiveTypeFlags.Time, typeof(TOD));
            DataType type2 = new PrimitiveType("DT", AdsDatatypeId.ADST_BIGTYPE, 4, PrimitiveTypeFlags.MaskDateTime, typeof(DT));
            DataType type3 = new PrimitiveType("UDINT", AdsDatatypeId.ADST_UINT32, 4, PrimitiveTypeFlags.Unsigned, typeof(uint));
            types.Add(new BitMappingType("BIT", 1, typeof(bool)));
            types.Add(new BitMappingType("BIT2", 2, typeof(byte)));
            types.Add(new BitMappingType("BIT3", 3, typeof(byte)));
            types.Add(new BitMappingType("BIT4", 4, typeof(byte)));
            types.Add(new BitMappingType("BIT5", 5, typeof(byte)));
            types.Add(new BitMappingType("BIT6", 6, typeof(byte)));
            types.Add(new BitMappingType("BIT7", 7, typeof(byte)));
            types.Add(new BitMappingType("BIT8", 8, typeof(byte)));
            types.Add(new PrimitiveType("VOID", AdsDatatypeId.ADST_VOID, 0, PrimitiveTypeFlags.None, typeof(void)));
            PointerType type4 = new PointerType("POINTER TO VOID", "VOID");
            types.Add(type4);
            AliasType type5 = new AliasType("PVOID", type4);
            types.Add(type5);
            types.Add(new PrimitiveType("SINT", AdsDatatypeId.ADST_INT8, 1, PrimitiveTypeFlags.Numeric, typeof(sbyte)));
            types.Add(new PrimitiveType("USINT", AdsDatatypeId.ADST_UINT8, 1, PrimitiveTypeFlags.MaskNumericUnsigned, typeof(byte)));
            types.Add(new PrimitiveType("BYTE", AdsDatatypeId.ADST_UINT8, 1, PrimitiveTypeFlags.Unsigned | PrimitiveTypeFlags.System, typeof(byte)));
            types.Add(new PrimitiveType("UINT8", AdsDatatypeId.ADST_UINT8, 1, PrimitiveTypeFlags.MaskNumericUnsigned, typeof(byte)));
            types.Add(new PrimitiveType("INT", AdsDatatypeId.ADST_INT16, 2, PrimitiveTypeFlags.Numeric, typeof(short)));
            types.Add(new PrimitiveType("INT16", AdsDatatypeId.ADST_INT16, 2, PrimitiveTypeFlags.Numeric, typeof(short)));
            types.Add(new PrimitiveType("UINT", AdsDatatypeId.ADST_UINT16, 2, PrimitiveTypeFlags.MaskNumericUnsigned, typeof(ushort)));
            types.Add(new PrimitiveType("WORD", AdsDatatypeId.ADST_UINT16, 2, PrimitiveTypeFlags.Unsigned | PrimitiveTypeFlags.System, typeof(ushort)));
            types.Add(new PrimitiveType("UINT16", AdsDatatypeId.ADST_UINT16, 2, PrimitiveTypeFlags.MaskNumericUnsigned, typeof(ushort)));
            types.Add(new PrimitiveType("DINT", AdsDatatypeId.ADST_INT32, 4, PrimitiveTypeFlags.Numeric, typeof(int)));
            types.Add(new PrimitiveType("INT32", AdsDatatypeId.ADST_INT32, 4, PrimitiveTypeFlags.Numeric, typeof(int)));
            types.Add(type3);
            types.Add(new PrimitiveType("UINT32", AdsDatatypeId.ADST_UINT32, 4, PrimitiveTypeFlags.Numeric, typeof(uint)));
            types.Add(new PrimitiveType("DWORD", AdsDatatypeId.ADST_UINT32, 4, PrimitiveTypeFlags.MaskNumericUnsigned, typeof(uint)));
            types.Add(new PrimitiveType("REAL", AdsDatatypeId.ADST_REAL32, 4, PrimitiveTypeFlags.Numeric | PrimitiveTypeFlags.Float, typeof(float)));
            types.Add(new PrimitiveType("FLOAT", AdsDatatypeId.ADST_REAL32, 4, PrimitiveTypeFlags.Numeric | PrimitiveTypeFlags.Float, typeof(float)));
            types.Add(new PrimitiveType("LREAL", AdsDatatypeId.ADST_REAL64, 8, PrimitiveTypeFlags.Numeric | PrimitiveTypeFlags.Float, typeof(double)));
            types.Add(new PrimitiveType("DOUBLE", AdsDatatypeId.ADST_REAL64, 8, PrimitiveTypeFlags.Numeric | PrimitiveTypeFlags.Float, typeof(double)));
            types.Add(new PrimitiveType("BOOL", AdsDatatypeId.ADST_BIT, 1, PrimitiveTypeFlags.System, typeof(bool)));
            types.Add(new PrimitiveType("TIME", AdsDatatypeId.ADST_BIGTYPE, 4, PrimitiveTypeFlags.Time, typeof(TIME)));
            types.Add(item);
            types.Add(new PrimitiveType("DATE", AdsDatatypeId.ADST_BIGTYPE, 4, PrimitiveTypeFlags.Date, typeof(DATE)));
            types.Add(type2);
            types.Add(new PrimitiveType("LTIME", AdsDatatypeId.ADST_BIGTYPE, 8, PrimitiveTypeFlags.System, typeof(LTIME)));
            types.Add(new AliasType("DATE_AND_TIME", type2));
            types.Add(new AliasType("TIME_OF_DAY", item));
            types.Add(new PrimitiveType("LINT", AdsDatatypeId.ADST_INT64, 8, PrimitiveTypeFlags.Numeric, typeof(long)));
            types.Add(new PrimitiveType("ULINT", AdsDatatypeId.ADST_UINT64, 8, PrimitiveTypeFlags.MaskNumericUnsigned, typeof(ulong)));
            types.Add(new PrimitiveType("LWORD", AdsDatatypeId.ADST_UINT64, 8, PrimitiveTypeFlags.Numeric, typeof(ulong)));
            types.Add(new AliasType("OTCID", type3));
            types.Add(new AliasType("UXINT", type5));
            return types;
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
                ((IDisposable) this.Accessor).Dispose();
            }
        }

        private void expandDataType(IDataType type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            DataTypeCategory category = type.Category;
            if (category > DataTypeCategory.Array)
            {
                if (category == DataTypeCategory.Pointer)
                {
                    DataType referencedType = (DataType) ((PointerType) type).ReferencedType;
                    if (referencedType != null)
                    {
                        this.expandDataType(referencedType);
                    }
                }
                else if (category == DataTypeCategory.Reference)
                {
                    DataType referencedType = (DataType) ((ReferenceType) type).ReferencedType;
                    if (referencedType != null)
                    {
                        this.expandDataType(referencedType);
                    }
                }
            }
            else if (category != DataTypeCategory.Alias)
            {
                if (category == DataTypeCategory.Array)
                {
                    DataType elementType = (DataType) ((ArrayType) type).ElementType;
                    if (elementType != null)
                    {
                        this.expandDataType(elementType);
                    }
                }
            }
            else
            {
                AliasType type2 = (AliasType) type;
                DataType baseType = (DataType) type2.BaseType;
                if (baseType != null)
                {
                    this.expandDataType(baseType);
                }
                if ((type2.Size <= 0) && (baseType != null))
                {
                    type2.SetSize(baseType.Size, baseType.ManagedType);
                }
            }
        }

        private void expandDataTypes()
        {
            DataTypeCollection<IDataType> types = this._namespaces.AllTypesInternal.Clone();
            int count = types.Count;
            foreach (DataType type in types)
            {
                this.expandDataType(type);
            }
            int num2 = this._namespaces.AllTypesInternal.Count;
            object[] args = new object[] { num2 - count };
            Module.Trace.TraceInformation("{0} datatypes expanded!", args);
        }

        ~AdsSymbolLoader()
        {
            this.Dispose(false);
        }

        private void loadData()
        {
            if (this._namespaces.Count == 0)
            {
                this.loadTypes(this._timeout);
            }
            if (this._symbols == null)
            {
                this.loadSymbols(this._timeout);
            }
        }

        private unsafe void loadSymbols(TimeSpan timeout)
        {
            switch (this._settings.SymbolsLoadMode)
            {
                case SymbolsLoadMode.Flat:
                    this._symbols = new SymbolCollection(InstanceCollectionMode.Path);
                    break;

                case SymbolsLoadMode.VirtualTree:
                case SymbolsLoadMode.DynamicTree:
                    this._symbols = new SymbolCollection(InstanceCollectionMode.PathHierarchy);
                    break;

                default:
                    throw new NotSupportedException();
            }
            int num = this._connection.Timeout;
            if (this.UploadInfo.SymbolsBlockSize > 0)
            {
                AdsStream input = new AdsStream(this.UploadInfo.SymbolsBlockSize);
                using (BinaryReader reader = new BinaryReader(input))
                {
                    this._connection.Timeout = (int) timeout.TotalMilliseconds;
                    try
                    {
                        int num2 = 0x10000;
                        if (this.UploadInfo.SymbolsBlockSize >= num2)
                        {
                            uint num3 = 0;
                            int num4 = 0;
                            int num5 = 0;
                            int num6 = 0;
                            int length = 0;
                            while (true)
                            {
                                int dataRead = 0;
                                while (true)
                                {
                                    AdsErrorCode code;
                                    ref byte pinned numRef;
                                    length = ((num4 + num2) <= input.Length) ? num2 : (((int) input.Length) - num4);
                                    try
                                    {
                                        byte[] buffer;
                                        if (((buffer = input.GetBuffer()) == null) || (buffer.Length == 0))
                                        {
                                            numRef = null;
                                        }
                                        else
                                        {
                                            numRef = buffer;
                                        }
                                        code = this._connection.RawInterface.Read(0xf00b, 0x80000000 | num3, length, (void*) (numRef + num4), false, out dataRead);
                                    }
                                    finally
                                    {
                                        numRef = null;
                                    }
                                    if (code != AdsErrorCode.NoError)
                                    {
                                        if (code == AdsErrorCode.DeviceInvalidSize)
                                        {
                                            num2 *= 4;
                                        }
                                        else
                                        {
                                            TcAdsDllWrapper.ThrowAdsException(code);
                                        }
                                    }
                                    if (code == AdsErrorCode.NoError)
                                    {
                                        num2 = 0x10000;
                                        num5 = num4;
                                        num6 = 0;
                                        input.Position = num4;
                                        num6 = reader.ReadInt32();
                                        while (true)
                                        {
                                            if ((num6 > 0) && (num3 < this.UploadInfo.SymbolCount))
                                            {
                                                num5 += num6;
                                                input.Position = num5;
                                                num3++;
                                                if ((num5 < (num4 + num2)) && (num5 < input.Length))
                                                {
                                                    num6 = reader.ReadInt32();
                                                    continue;
                                                }
                                            }
                                            num4 = num5;
                                            if ((num3 < this.UploadInfo.SymbolCount) && (num4 < input.Length))
                                            {
                                                break;
                                            }
                                            break;
                                        }
                                        break;
                                    }
                                }
                            }
                        }
                        else
                        {
                            this._connection.Read(0xf00b, 0, input);
                        }
                        SymbolParser.ParseSymbols(input, this.UploadInfo.StringEncoding, this._symbolFactorServices);
                    }
                    catch (Exception exception)
                    {
                        this._symbols = null;
                        Module.Trace.TraceError(exception);
                        throw;
                    }
                    finally
                    {
                        this._connection.Timeout = num;
                    }
                }
            }
        }

        private unsafe void loadTypes(TimeSpan timeout)
        {
            this._namespaces.Clear();
            Namespace item = new Namespace(this.RootNamespaceName);
            this._namespaces.Add(item);
            int targetPointerSize = this.UploadInfo.TargetPointerSize;
            if (targetPointerSize > 0)
            {
                this.SetPlatformPointerSize(targetPointerSize);
            }
            try
            {
                this._buildInTypes = CreateBuildInTypes();
                bool containsBaseTypes = this.UploadInfo.ContainsBaseTypes;
                object[] args = new object[] { containsBaseTypes };
                Module.Trace.TraceInformation("BaseTypes in DataType Stream: {0}", args);
                this.Binder.RegisterTypes(this._buildInTypes);
                this.Binder.OnTypesGenerated(this._buildInTypes);
                int num2 = 0x10000;
                int num3 = this._connection.Timeout;
                if (this.UploadInfo.DataTypesBlockSize > 0)
                {
                    AdsStream input = new AdsStream(this.UploadInfo.DataTypesBlockSize);
                    using (BinaryReader reader = new BinaryReader(input))
                    {
                        if (this.UploadInfo.DataTypeCount > 0)
                        {
                            this._connection.Timeout = (int) timeout.TotalMilliseconds;
                            try
                            {
                                uint num4;
                                int num5;
                                int num6;
                                int num7;
                                if (this.UploadInfo.DataTypesBlockSize >= num2)
                                {
                                    num4 = 0;
                                    num5 = 0;
                                    num6 = 0;
                                    num7 = 0;
                                }
                                else
                                {
                                    this._connection.Read(0xf00e, 0, input);
                                    goto TR_0007;
                                }
                                while (true)
                                {
                                    int dataRead = 0;
                                    while (true)
                                    {
                                        AdsErrorCode code;
                                        ref byte pinned numRef;
                                        int length = ((num5 + num2) <= input.Length) ? num2 : (((int) input.Length) - num5);
                                        try
                                        {
                                            byte[] buffer;
                                            if (((buffer = input.GetBuffer()) == null) || (buffer.Length == 0))
                                            {
                                                numRef = null;
                                            }
                                            else
                                            {
                                                numRef = buffer;
                                            }
                                            code = this._connection.RawInterface.Read(0xf00e, 0x80000000 | num4, length, (void*) (numRef + num5), false, out dataRead);
                                        }
                                        finally
                                        {
                                            numRef = null;
                                        }
                                        if (code != AdsErrorCode.NoError)
                                        {
                                            if (code == AdsErrorCode.DeviceInvalidSize)
                                            {
                                                num2 *= 4;
                                            }
                                            else
                                            {
                                                TcAdsDllWrapper.ThrowAdsException(code);
                                            }
                                        }
                                        if (code == AdsErrorCode.NoError)
                                        {
                                            num2 = 0x10000;
                                            num6 = num5;
                                            num7 = 0;
                                            input.Position = num5;
                                            num7 = reader.ReadInt32();
                                            while (true)
                                            {
                                                if ((num7 > 0) && (num4 < this.UploadInfo.DataTypeCount))
                                                {
                                                    num6 += num7;
                                                    input.Position = num6;
                                                    num4++;
                                                    if ((num6 < (num5 + num2)) && (num6 < input.Length))
                                                    {
                                                        num7 = reader.ReadInt32();
                                                        continue;
                                                    }
                                                }
                                                num5 = num6;
                                                if ((num4 >= this.UploadInfo.DataTypeCount) || (num5 >= input.Length))
                                                {
                                                    input.Position = 0L;
                                                }
                                                else
                                                {
                                                    continue;
                                                }
                                                break;
                                            }
                                            break;
                                        }
                                    }
                                    break;
                                }
                            }
                            finally
                            {
                                this._connection.Timeout = num3;
                            }
                        }
                    TR_0007:
                        input.Position = 0L;
                        SymbolParser.ParseTypes(input, this.UploadInfo.StringEncoding, this.Binder, containsBaseTypes, this._buildInTypes);
                    }
                }
                this.expandDataTypes();
                if (this.UploadInfo.ContainsBaseTypes)
                {
                    Module.Trace.TraceInformation("Aligning Base types");
                }
            }
            catch (Exception exception)
            {
                this._namespaces.Clear();
                this._buildInTypes = null;
                Module.Trace.TraceError(exception);
                throw;
            }
        }

        internal static AdsErrorCode loadUploadInfo(IAdsConnection connection, TimeSpan timeout, out SymbolUploadInfo uploadInfo)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("client");
            }
            AdsErrorCode noError = AdsErrorCode.NoError;
            uploadInfo = null;
            int num = connection.Timeout;
            connection.Timeout = (int) timeout.TotalMilliseconds;
            try
            {
                AdsStream input = new AdsStream(0x40);
                using (BinaryReader reader = new BinaryReader(input))
                {
                    int readBytes = 0;
                    noError = connection.TryRead(0xf00f, 0, input, 0, (int) input.Length, out readBytes);
                    if (noError == AdsErrorCode.NoError)
                    {
                        input.Position = 0L;
                        uploadInfo = new SymbolUploadInfo(reader, SymbolUploadInfo.CalcVersion(readBytes));
                    }
                    else if ((noError == AdsErrorCode.DeviceServiceNotSupported) || (noError == AdsErrorCode.DeviceSymbolNotFound))
                    {
                        noError = connection.TryRead(0xf00c, 0, input, 0, (int) input.Length, out readBytes);
                        if (noError == AdsErrorCode.NoError)
                        {
                            input.Position = 0L;
                            uploadInfo = new SymbolUploadInfo(reader, SymbolUploadInfo.CalcVersion(readBytes));
                        }
                    }
                }
            }
            finally
            {
                connection.Timeout = num;
            }
            return noError;
        }

        public void Reset()
        {
            this._symbolUploadInfo = null;
            this._symbols = null;
            this._namespaces = null;
        }

        internal void SetPlatformPointerSize(int sz)
        {
            this.Binder.SetPlatformPointerSize(sz);
            Type type = null;
            if (sz == 4)
            {
                type = typeof(uint);
            }
            else if (sz == 8)
            {
                type = typeof(ulong);
            }
            object[] args = new object[] { sz };
            Module.Trace.TraceInformation("Platform pointer size -> {0} bytes", args);
        }

        private TwinCAT.TypeSystem.Binder Binder =>
            ((TwinCAT.TypeSystem.Binder) this._symbolFactorServices.Binder);

        private IAccessorRawValue Accessor =>
            ((ISymbolFactoryValueServices) this._symbolFactorServices).ValueAccessor;

        private ISymbolFactory SymbolFactory =>
            this._symbolFactorServices.SymbolFactory;

        public ISymbolLoaderSettings Settings =>
            this._settings;

        public SymbolUploadInfo UploadInfo
        {
            get
            {
                if (this._symbolUploadInfo == null)
                {
                    AdsErrorCode code = loadUploadInfo(this._connection, this._timeout, out this._symbolUploadInfo);
                    if (code == AdsErrorCode.NoError)
                    {
                        if (this._symbolUploadInfo.TargetPointerSize > 0)
                        {
                            this.SetPlatformPointerSize(this._symbolUploadInfo.TargetPointerSize);
                        }
                        Module.Trace.TraceInformation(this._symbolUploadInfo.Dump());
                    }
                    else if ((code == AdsErrorCode.DeviceSymbolNotFound) || (code == AdsErrorCode.DeviceServiceNotSupported))
                    {
                        this._symbolUploadInfo = new SymbolUploadInfo();
                    }
                    else
                    {
                        object[] args = new object[] { code };
                        Module.Trace.TraceError("Could not upload symbols info!", args);
                        this._symbolUploadInfo = null;
                    }
                }
                return this._symbolUploadInfo;
            }
        }

        public int DataTypeCount
        {
            get
            {
                SymbolUploadInfo uploadInfo = this.UploadInfo;
                return ((uploadInfo == null) ? 0 : uploadInfo.DataTypeCount);
            }
        }

        public int SymbolCount =>
            ((this.UploadInfo == null) ? 0 : this.UploadInfo.SymbolCount);

        public int MaxDynamicSymbolCount
        {
            get
            {
                SymbolUploadInfo uploadInfo = this.UploadInfo;
                return ((uploadInfo == null) ? 0 : uploadInfo.MaxDynamicSymbolCount);
            }
        }

        public int UsedDynamicSymbolCount
        {
            get
            {
                SymbolUploadInfo uploadInfo = this.UploadInfo;
                return ((uploadInfo == null) ? 0 : uploadInfo.MaxDynamicSymbolCount);
            }
        }

        public Encoding StringEncoding
        {
            get
            {
                SymbolUploadInfo uploadInfo = this.UploadInfo;
                return ((uploadInfo == null) ? Encoding.Default : uploadInfo.StringEncoding);
            }
        }

        public ReadOnlyDataTypeCollection<IDataType> BuildInTypes
        {
            get
            {
                if (this._buildInTypes == null)
                {
                    this.loadTypes(this._timeout);
                }
                return new ReadOnlyDataTypeCollection<IDataType>(this._buildInTypes);
            }
        }

        public AmsAddress ImageBaseAddress =>
            this._connection.Address;

        public string RootNamespaceName =>
            this._rootNamespace;

        public ReadOnlySymbolCollection Symbols
        {
            get
            {
                this.loadData();
                return new ReadOnlySymbolCollection(this._symbols);
            }
        }

        SymbolCollection<ISymbol> IInternalSymbolProvider.SymbolsInternal =>
            this._symbols;

        public ReadOnlyNamespaceCollection Namespaces
        {
            get
            {
                if (this._namespaces.Count == 0)
                {
                    this.loadTypes(this._timeout);
                }
                return this._namespaces.AsReadOnly();
            }
        }

        NamespaceCollection<INamespace<IDataType>, IDataType> IInternalSymbolProvider.NamespacesInternal =>
            this._namespaces;

        public INamespace<IDataType> RootNamespace
        {
            get
            {
                if (this._namespaces.Count == 0)
                {
                    this.loadTypes(this._timeout);
                }
                return ((this._namespaces.Count <= 0) ? null : this._namespaces[0]);
            }
        }

        public ReadOnlyDataTypeCollection DataTypes
        {
            get
            {
                this.loadData();
                return new ReadOnlyDataTypeCollection(this._namespaces.AllTypesInternal);
            }
        }

        DataTypeCollection<IDataType> IInternalSymbolProvider.DataTypesInternal =>
            ((INamespaceInternal<IDataType>) this.RootNamespace).DataTypesInternal;

        internal bool UseVirtualInstances
        {
            get
            {
                switch (this._settings.SymbolsLoadMode)
                {
                    case SymbolsLoadMode.VirtualTree:
                    case SymbolsLoadMode.DynamicTree:
                        return true;
                }
                return false;
            }
        }

        public DynamicSymbolsContainer SymbolsDynamic
        {
            get
            {
                if (this._settings.SymbolsLoadMode != SymbolsLoadMode.DynamicTree)
                {
                    return null;
                }
                this.loadData();
                return new DynamicSymbolsContainer(((IInternalSymbolProvider) this).SymbolsInternal);
            }
        }

        public INotificationSettings DefaultNotificationSettings
        {
            get => 
                this._notificationSettings;
            set => 
                (this._notificationSettings = (NotificationSettings) value);
        }

        internal ISymbolFactoryServices FactoryServices =>
            this._symbolFactorServices;

        private IInternalSymbolProvider ITypeBinderProvider =>
            this;

        public IInternalSymbolProvider Provider =>
            this;
    }
}

