namespace TwinCAT.Ads
{
    using System;
    using System.Collections;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using TwinCAT.Ads.Internal;
    using TwinCAT.Ads.Tracing;
    using TwinCAT.TypeSystem;

    public class TcAdsSymbolInfoLoader : IEnumerable, ITypeBinderEvents
    {
        private TcAdsClient _adsClient;
        private bool _isEnumerating;
        private AdsParseSymbols _symbolParser;
        private TcAdsSymbolInfoCollection _symbols;
        private ReadOnlyTcAdsDataTypeCollection _dataTypes;
        private SymbolUploadInfo _symbolInfo;
        private const int SYMBOLUPLOAD_INITIAL_BLOCKSIZE = 0x10000;
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

        internal TcAdsSymbolInfoLoader(TcAdsClient adsClient, SymbolUploadInfo symbolInfo)
        {
            if (adsClient == null)
            {
                throw new ArgumentNullException("adsClient");
            }
            if (symbolInfo == null)
            {
                throw new ArgumentNullException("symbolInfo");
            }
            this._adsClient = adsClient;
            this._symbols = null;
            this._symbolInfo = symbolInfo;
        }

        private void _symbolParser_ResolveError(object sender, DataTypeNameEventArgs e)
        {
            if (this.TypeResolveError != null)
            {
                this.TypeResolveError(this, e);
            }
        }

        private void _symbolParser_TypesGenerated(object sender, DataTypeEventArgs e)
        {
            if (this.TypesGenerated != null)
            {
                this.TypesGenerated(this, e);
            }
        }

        public TcAdsSymbolInfo FindSymbol(string name)
        {
            if ((this._symbolParser != null) || (this.GetFirstSymbol(true) != null))
            {
                return this._symbolParser.GetSymbol(name);
            }
            return null;
        }

        public ReadOnlyTcAdsDataTypeCollection GetDataTypes(bool forceReload)
        {
            this.initializeUploadSymbols(forceReload);
            return this._dataTypes;
        }

        public virtual IEnumerator GetEnumerator()
        {
            this.GetFirstSymbol(true);
            this._isEnumerating = true;
            return new AdsSymbolEnumerator(this);
        }

        public TcAdsSymbolInfo GetFirstSymbol(bool forceReload)
        {
            TcAdsSymbolInfo symbol = null;
            if (forceReload || (this._symbolParser == null))
            {
                this.onUploadSymbols();
            }
            if (this._symbolParser != null)
            {
                symbol = this._symbolParser.GetSymbol(0);
            }
            return symbol;
        }

        public int GetSymbolCount(bool forceReload)
        {
            this.GetFirstSymbol(forceReload);
            return ((this._symbolParser == null) ? -1 : this._symbolParser.SymbolCount);
        }

        public TcAdsSymbolInfoCollection GetSymbols(bool forceReload)
        {
            this.initializeUploadSymbols(forceReload);
            return this._symbols;
        }

        private bool initializeUploadSymbols(bool forceReload)
        {
            if (forceReload || (this._symbolParser == null))
            {
                this._symbols = null;
                this._dataTypes = null;
                this._symbolInfo = null;
                this.onUploadSymbols();
            }
            if (this._symbolParser == null)
            {
                return false;
            }
            this._symbols = new TcAdsSymbolInfoCollection(this._symbolParser);
            this._dataTypes = this._symbolParser.GetDataTypes();
            return true;
        }

        private void onUploadSymbols()
        {
            using (new MethodTrace())
            {
                this._symbolParser = null;
                this._isEnumerating = false;
                try
                {
                    int timeout = this._adsClient.Timeout;
                    this._adsClient.Timeout = 0x7530;
                    try
                    {
                        if (this._symbolInfo == null)
                        {
                            this._symbolInfo = this.readUploadInfo();
                        }
                        bool containsBaseTypes = this._symbolInfo.ContainsBaseTypes;
                        Module.Trace.TraceInformation(this._symbolInfo.Dump());
                        AdsStream symbolStream = new AdsStream(this._symbolInfo.SymbolsBlockSize);
                        this.readSymbols(symbolStream, this._symbolInfo, 0x10000);
                        Module.Trace.TraceInformation("Symbols uploaded successfully!");
                        AdsStream datatypeStream = new AdsStream(this._symbolInfo.DataTypesBlockSize);
                        this.readDataTypes(datatypeStream, this._symbolInfo, 0x10000);
                        Module.Trace.TraceInformation("DataTypes uploaded successfully!");
                        this._symbolParser = new AdsParseSymbols(this._symbolInfo.TargetPointerSize, this._symbolInfo.ContainsBaseTypes, this._symbolInfo.StringEncoding);
                        this._symbolParser.TypeResolveError += new EventHandler<DataTypeNameEventArgs>(this._symbolParser_ResolveError);
                        this._symbolParser.TypesGenerated += new EventHandler<DataTypeEventArgs>(this._symbolParser_TypesGenerated);
                        this._symbolParser.Parse(symbolStream, datatypeStream, this._adsClient);
                    }
                    finally
                    {
                        this._adsClient.Timeout = timeout;
                    }
                }
                catch (Exception exception)
                {
                    Module.Trace.TraceError(exception);
                    throw;
                }
            }
        }

        private unsafe void readDataTypes(AdsStream datatypeStream, SymbolUploadInfo info, int initialBlockSize)
        {
            int num = initialBlockSize;
            if (info.DataTypeCount > 0)
            {
                if (info.DataTypesBlockSize < num)
                {
                    this._adsClient.Read(0xf00e, 0, datatypeStream);
                }
                else
                {
                    uint num2 = 0;
                    int num3 = 0;
                    int num4 = 0;
                    int num5 = 0;
                    BinaryReader reader = new BinaryReader(datatypeStream);
                    while (true)
                    {
                        int dataRead = 0;
                        while (true)
                        {
                            byte[] buffer;
                            int length = ((num3 + num) <= datatypeStream.Length) ? num : (((int) datatypeStream.Length) - num3);
                            if (((buffer = datatypeStream.GetBuffer()) == null) || (buffer.Length == 0))
                            {
                                numRef = null;
                            }
                            else
                            {
                                numRef = buffer;
                            }
                            AdsErrorCode adsErrorCode = this._adsClient.RawInterface.Read(0xf00e, 0x80000000 | num2, length, (void*) (numRef + num3), false, out dataRead);
                            fixed (byte* numRef = null)
                            {
                                if (adsErrorCode != AdsErrorCode.NoError)
                                {
                                    if (adsErrorCode == AdsErrorCode.DeviceInvalidSize)
                                    {
                                        num *= 4;
                                    }
                                    else
                                    {
                                        TcAdsDllWrapper.ThrowAdsException(adsErrorCode);
                                    }
                                }
                                if (adsErrorCode != AdsErrorCode.NoError)
                                {
                                    continue;
                                }
                                num = initialBlockSize;
                                num4 = num3;
                                num5 = 0;
                                datatypeStream.Position = num3;
                                num5 = reader.ReadInt32();
                                while (true)
                                {
                                    if ((num5 > 0) && (num2 < info.DataTypeCount))
                                    {
                                        num4 += num5;
                                        datatypeStream.Position = num4;
                                        num2++;
                                        if ((num4 < (num3 + num)) && (num4 < datatypeStream.Length))
                                        {
                                            num5 = reader.ReadInt32();
                                            continue;
                                        }
                                    }
                                    num3 = num4;
                                    if ((num2 >= info.DataTypeCount) || (num3 >= datatypeStream.Length))
                                    {
                                        datatypeStream.Position = 0L;
                                    }
                                    break;
                                }
                                break;
                            }
                        }
                    }
                }
            }
        }

        private unsafe void readSymbols(AdsStream symbolStream, SymbolUploadInfo info, int initialBlockSize)
        {
            int num = initialBlockSize;
            if (info.SymbolsBlockSize < num)
            {
                this._adsClient.Read(0xf00b, 0, symbolStream);
            }
            else
            {
                uint num2 = 0;
                int num3 = 0;
                int num4 = 0;
                int num5 = 0;
                int length = 0;
                BinaryReader reader = new BinaryReader(symbolStream);
                while (true)
                {
                    int dataRead = 0;
                    while (true)
                    {
                        byte[] buffer;
                        length = ((num3 + num) <= symbolStream.Length) ? num : (((int) symbolStream.Length) - num3);
                        if (((buffer = symbolStream.GetBuffer()) == null) || (buffer.Length == 0))
                        {
                            numRef = null;
                        }
                        else
                        {
                            numRef = buffer;
                        }
                        AdsErrorCode adsErrorCode = this._adsClient.RawInterface.Read(0xf00b, 0x80000000 | num2, length, (void*) (numRef + num3), false, out dataRead);
                        fixed (byte* numRef = null)
                        {
                            if (adsErrorCode != AdsErrorCode.NoError)
                            {
                                if (adsErrorCode == AdsErrorCode.DeviceInvalidSize)
                                {
                                    num *= 4;
                                }
                                else
                                {
                                    TcAdsDllWrapper.ThrowAdsException(adsErrorCode);
                                }
                            }
                            if (adsErrorCode == AdsErrorCode.NoError)
                            {
                                num = initialBlockSize;
                                num4 = num3;
                                num5 = 0;
                                symbolStream.Position = num3;
                                num5 = reader.ReadInt32();
                                while (true)
                                {
                                    if ((num5 > 0) && (num2 < info.SymbolCount))
                                    {
                                        num4 += num5;
                                        symbolStream.Position = num4;
                                        num2++;
                                        if ((num4 < (num3 + num)) && (num4 < symbolStream.Length))
                                        {
                                            num5 = reader.ReadInt32();
                                            continue;
                                        }
                                    }
                                    num3 = num4;
                                    if ((num2 < info.SymbolCount) && (num3 < symbolStream.Length))
                                    {
                                        break;
                                    }
                                    symbolStream.Position = 0L;
                                    return;
                                }
                                break;
                            }
                        }
                    }
                }
            }
        }

        private SymbolUploadInfo readUploadInfo()
        {
            AdsStream dataStream = new AdsStream(0x40);
            try
            {
                int version = SymbolUploadInfo.CalcVersion(this._adsClient.Read(0xf00f, 0, dataStream));
                dataStream.Position = 0L;
                return new SymbolUploadInfo(new BinaryReader(dataStream), version);
            }
            catch (AdsErrorException exception1)
            {
                if (exception1.ErrorCode != AdsErrorCode.DeviceServiceNotSupported)
                {
                    throw;
                }
                int version = SymbolUploadInfo.CalcVersion(this._adsClient.Read(0xf00c, 0, dataStream));
                dataStream.Position = 0L;
                return new SymbolUploadInfo(new BinaryReader(dataStream), version);
            }
        }

        internal int PlatformPointerSize =>
            ((this._symbolParser == null) ? 0 : this._symbolParser.PlatformPointerSize);

        internal class AdsSymbolEnumerator : IEnumerator
        {
            private TcAdsSymbolInfo curSymbol;
            private TcAdsSymbolInfoLoader symbolLoader;
            private bool isValid;
            private AdsGetDynamicSymbolType nextNavType;
            private bool _dereference;

            public AdsSymbolEnumerator(TcAdsSymbolInfoLoader symbolLoader)
            {
                this.symbolLoader = symbolLoader;
                this.isValid = true;
                this.nextNavType = AdsGetDynamicSymbolType.Child;
            }

            private void CheckValid()
            {
                if (!this.isValid || !this.symbolLoader._isEnumerating)
                {
                    throw new InvalidOperationException();
                }
            }

            public virtual bool MoveNext()
            {
                this.CheckValid();
                if (this.curSymbol == null)
                {
                    this.curSymbol = this.symbolLoader.GetFirstSymbol(false);
                    this.nextNavType = AdsGetDynamicSymbolType.Child;
                    if (this.curSymbol == null)
                    {
                        this.isValid = false;
                    }
                    return this.isValid;
                }
                TcAdsSymbolInfo nextSymbol = null;
                if (this.symbolLoader._symbolParser != null)
                {
                    switch (this.nextNavType)
                    {
                        case AdsGetDynamicSymbolType.Sibling:
                            nextSymbol = this.curSymbol.NextSymbol;
                            break;

                        case AdsGetDynamicSymbolType.Child:
                            nextSymbol = this.curSymbol.GetFirstSubSymbol(this._dereference);
                            break;

                        case AdsGetDynamicSymbolType.Parent:
                            this.curSymbol = this.curSymbol.Parent;
                            nextSymbol = this.curSymbol.NextSymbol;
                            break;

                        default:
                            break;
                    }
                    if (nextSymbol != null)
                    {
                        AdsGetDynamicSymbolType nextNavType = this.nextNavType;
                        if ((nextNavType == AdsGetDynamicSymbolType.Sibling) || (nextNavType == AdsGetDynamicSymbolType.Parent))
                        {
                            this.nextNavType = AdsGetDynamicSymbolType.Child;
                        }
                    }
                    else
                    {
                        switch (this.nextNavType)
                        {
                            case AdsGetDynamicSymbolType.Sibling:
                            case AdsGetDynamicSymbolType.Parent:
                                this.nextNavType = AdsGetDynamicSymbolType.Parent;
                                break;

                            case AdsGetDynamicSymbolType.Child:
                                this.nextNavType = AdsGetDynamicSymbolType.Sibling;
                                break;

                            default:
                                break;
                        }
                        if ((this.nextNavType != AdsGetDynamicSymbolType.Parent) || (this.curSymbol.Parent != null))
                        {
                            return this.MoveNext();
                        }
                    }
                }
                if (nextSymbol == null)
                {
                    this.isValid = false;
                }
                this.curSymbol = nextSymbol;
                return this.isValid;
            }

            public virtual void Reset()
            {
                if (!this.symbolLoader._isEnumerating)
                {
                    throw new InvalidOperationException();
                }
                this.curSymbol = null;
                this.isValid = true;
            }

            public object Current
            {
                get
                {
                    this.CheckValid();
                    return this.curSymbol;
                }
            }
        }
    }
}

