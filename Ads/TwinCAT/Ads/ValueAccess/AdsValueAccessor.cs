namespace TwinCAT.Ads.ValueAccess
{
    using System;
    using System.Runtime.InteropServices;
    using TwinCAT.Ads;
    using TwinCAT.Ads.TypeSystem;
    using TwinCAT.TypeSystem;
    using TwinCAT.ValueAccess;

    internal class AdsValueAccessor : AdsValueAccessorBase, IDisposable
    {
        private DynamicValueConverter _converter;
        protected bool disposed;
        private object _syncNotification;
        private AmsAddress _address;
        private ValueAccessMode _accessMethod;
        private AdsNotificationCache _notificationTable;
        private AdsStream _notificationStream;

        internal AdsValueAccessor(IAdsConnection connection, ValueAccessMode accessMethod, IAccessorValueFactory valueFactory, NotificationSettings defaultSettings) : base(valueFactory, connection, defaultSettings)
        {
            this._converter = new DynamicValueConverter();
            this._syncNotification = new object();
            this._accessMethod = ValueAccessMode.IndexGroupOffsetPreferred;
            this._notificationTable = new AdsNotificationCache();
            this._notificationStream = new AdsStream();
            this._address = connection.Address;
            this._accessMethod = accessMethod;
            base._notificationSettings = defaultSettings;
            connection.AdsNotification += new AdsNotificationEventHandler(this.adsClient_AdsNotification);
            connection.AdsNotificationError += new AdsNotificationErrorEventHandler(this.adsClient_AdsNotificationError);
        }

        private void adsClient_AdsNotification(object sender, AdsNotificationEventArgs e)
        {
            int notificationHandle = e.NotificationHandle;
            ISymbol userData = (ISymbol) e.UserData;
            if (userData != null)
            {
                this.onAdsNotification(userData, e);
            }
        }

        private void adsClient_AdsNotificationError(object sender, AdsNotificationErrorEventArgs e)
        {
            Module.Trace.TraceError(e.Exception);
        }

        private ValueAccessMode calcAccessMethod(ISymbol symbol)
        {
            if ((this._accessMethod != ValueAccessMode.IndexGroupOffsetPreferred) || !symbol.IsReference)
            {
                return this.calcAccessMethodByAddress((IProcessImageAddress) symbol);
            }
            return ValueAccessMode.Symbolic;
        }

        private ValueAccessMode calcAccessMethodByAddress(IProcessImageAddress symbolAddress)
        {
            ValueAccessMode indexGroupOffset = this._accessMethod;
            switch (this._accessMethod)
            {
                case ValueAccessMode.IndexGroupOffset:
                    indexGroupOffset = ValueAccessMode.IndexGroupOffset;
                    break;

                case ValueAccessMode.Symbolic:
                    indexGroupOffset = ValueAccessMode.Symbolic;
                    break;

                case ValueAccessMode.IndexGroupOffsetPreferred:
                    switch (symbolAddress.IndexGroup)
                    {
                        case 0xf014:
                        case 0xf016:
                        case 0xf017:
                        case 0xf019:
                        case 0xf01a:
                        case 0xf01b:
                            indexGroupOffset = ValueAccessMode.Symbolic;
                            break;

                        default:
                            indexGroupOffset = ValueAccessMode.IndexGroupOffset;
                            break;
                    }
                    break;

                default:
                    throw new NotSupportedException($"'{this._accessMethod.ToString()}' not supported");
            }
            return indexGroupOffset;
        }

        public void Dispose()
        {
            if (!this.disposed)
            {
                this.Dispose(true);
                this.disposed = true;
                GC.SuppressFinalize(this);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                IAdsConnection connection = this.Connection;
                if (connection != null)
                {
                    connection.AdsNotification -= new AdsNotificationEventHandler(this.adsClient_AdsNotification);
                    connection.AdsNotificationError -= new AdsNotificationErrorEventHandler(this.adsClient_AdsNotificationError);
                }
                if (this._notificationStream != null)
                {
                    this._notificationStream.Close();
                }
                this._notificationStream = null;
            }
        }

        ~AdsValueAccessor()
        {
            this.Dispose(false);
        }

        private void onAdsNotification(ISymbol symbol, AdsNotificationEventArgs args)
        {
            byte[] rawValue = null;
            DateTime utcTwinCATTime = DateTime.FromFileTimeUtc(args.TimeStamp);
            DateTime utcNow = DateTime.UtcNow;
            DataType dataType = (DataType) symbol.DataType;
            object obj2 = this._syncNotification;
            lock (obj2)
            {
                int byteSize = symbol.ByteSize;
                byte[] destinationArray = new byte[byteSize];
                Array.Copy(args.DataStream.GetBuffer(), args.Offset, destinationArray, 0, byteSize);
                rawValue = destinationArray;
            }
            SymbolNotificationType notificationType = this._notificationTable.GetNotificationType(symbol);
            if ((notificationType & SymbolNotificationType.RawValue) == SymbolNotificationType.RawValue)
            {
                this.OnRawValueChanged(symbol, rawValue, utcTwinCATTime, utcNow);
            }
            if ((notificationType & SymbolNotificationType.Value) == SymbolNotificationType.Value)
            {
                this.OnValueChanged(symbol, rawValue, utcTwinCATTime, utcNow);
            }
        }

        public override void OnRegisterNotification(ISymbol symbol, SymbolNotificationType type, INotificationSettings settings)
        {
            if (symbol == null)
            {
                throw new ArgumentNullException("symbol");
            }
            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }
            NotificationSettings settings2 = null;
            if (!this._notificationTable.TryGetRegisteredNotificationSettings(symbol, out settings2))
            {
                this.RegisterNotification(symbol, type, (NotificationSettings) settings);
            }
            else if (settings2.CompareTo(settings) >= 0)
            {
                this.RegisterNotification(symbol, type, settings2);
            }
            else
            {
                this.UnregisterNotification(symbol, type);
                this.RegisterNotification(symbol, type, (NotificationSettings) settings);
            }
        }

        public override void OnUnregisterNotification(ISymbol symbol, SymbolNotificationType type)
        {
            if (symbol == null)
            {
                throw new ArgumentNullException("symbol");
            }
            this.UnregisterNotification(symbol, type);
        }

        private void RegisterNotification(ISymbol symbol, SymbolNotificationType type, NotificationSettings settings)
        {
            if (symbol == null)
            {
                throw new ArgumentNullException("symbol");
            }
            if (this.Connection == null)
            {
                throw new AdsException("Connection not established!");
            }
            int byteSize = symbol.ByteSize;
            int length = Math.Max(byteSize, this._notificationTable.GetLargestSymbolSize());
            this.resizeNotificationStream(length);
            if (!this._notificationTable.Contains(symbol))
            {
                IAdsSymbol symbol2 = (IAdsSymbol) Symbol.Unwrap(symbol);
                ValueAccessMode mode = this.calcAccessMethod(symbol2);
                int handle = 0;
                using (new AdsConnectionRestore(this))
                {
                    if (mode == ValueAccessMode.IndexGroupOffset)
                    {
                        handle = this.Connection.AddDeviceNotification(symbol2.IndexGroup, symbol2.IndexOffset, this._notificationStream, 0, byteSize, settings.NotificationMode, settings.CycleTime, settings.MaxDelay, symbol);
                    }
                    else
                    {
                        if (mode != ValueAccessMode.Symbolic)
                        {
                            throw new NotSupportedException("Value access mode not supported!");
                        }
                        handle = this.Connection.AddDeviceNotification(symbol.InstancePath, this._notificationStream, 0, byteSize, settings.NotificationMode, settings.CycleTime, settings.MaxDelay, symbol);
                    }
                    this._notificationTable.Add(symbol, handle, type, settings);
                    return;
                }
            }
            this._notificationTable.Update(symbol, type, settings);
        }

        private void resizeNotificationStream(int length)
        {
            long num = this._notificationStream.Length;
            long num2 = Math.Max(num, 0x400L);
            if (num < length)
            {
                while (true)
                {
                    if (num2 < length)
                    {
                        num2 *= 2L;
                        continue;
                    }
                    object obj2 = this._syncNotification;
                    lock (obj2)
                    {
                        this._notificationStream.SetLength(num2);
                        return;
                    }
                    break;
                }
            }
            if ((num > 0x400L) && (length < (num * 2L)))
            {
                num2 = num;
                while (true)
                {
                    num2 /= 2L;
                    if ((num2 <= 0x400L) || (length >= (num2 * 2L)))
                    {
                        object obj3 = this._syncNotification;
                        lock (obj3)
                        {
                            this._notificationStream.SetLength(num2);
                        }
                        break;
                    }
                }
            }
        }

        public override int TryInvokeRpcMethod(IInstance instance, IRpcMethod method, object[] parameters, out object returnValue, out DateTime invokeTime)
        {
            AdsErrorCode code;
            if (instance == null)
            {
                throw new ArgumentNullException("instance");
            }
            if (method == null)
            {
                throw new ArgumentNullException("method");
            }
            if (this.Connection == null)
            {
                throw new AdsException("Connection not established!");
            }
            string str = $"{instance.InstancePath}#{method.Name}";
            if (method.Parameters.Count != parameters.Length)
            {
                throw new ArgumentException($"Parameter set mismatching RpcMethod '{str}' prototype", "parameters");
            }
            invokeTime = DateTime.MinValue;
            IRpcCallableInstance instance2 = (IRpcCallableInstance) instance;
            IProcessImageAddress address = (IProcessImageAddress) instance;
            using (new AdsConnectionRestore(this))
            {
                code = this.Connection.TryInvokeRpcMethod(instance.InstancePath, method.Name, parameters, out returnValue);
                if (code == AdsErrorCode.NoError)
                {
                    invokeTime = DateTime.UtcNow;
                }
            }
            return (int) code;
        }

        public override int TryReadAnyValue(ISymbol symbol, Type valueType, out object value, out DateTime utcReadTime)
        {
            AdsErrorCode noError = AdsErrorCode.NoError;
            byte[] buffer = null;
            noError = (AdsErrorCode) this.TryReadValue(symbol, out buffer, out utcReadTime);
            value = null;
            if (noError == AdsErrorCode.NoError)
            {
                int num = this._converter.Unmarshal(symbol, valueType, buffer, 0, out value);
            }
            return (int) noError;
        }

        public override int TryReadArrayElementValue(ISymbol arraySymbol, int[] indices, out byte[] value, out DateTime readTime)
        {
            AdsErrorCode code;
            if (arraySymbol == null)
            {
                throw new ArgumentNullException("arraySymbol");
            }
            IArrayType dataType = arraySymbol.DataType as IArrayType;
            if (dataType == null)
            {
                throw new ArgumentException("Parameter 'arraySymbol' doesn't represent an array!", "arraySymbol");
            }
            ValueAccessMode mode = this.calcAccessMethod(arraySymbol);
            readTime = DateTime.MinValue;
            if ((indices == null) || (indices.Length == 0))
            {
                int byteSize = dataType.ByteSize;
                value = new byte[byteSize];
                int read = 0;
                if (mode != ValueAccessMode.IndexGroupOffset)
                {
                    code = this.TryReadSymbolic(arraySymbol, byteSize, value, out read);
                }
                else
                {
                    IAdsSymbol symbol2 = (IAdsSymbol) Symbol.Unwrap(arraySymbol);
                    code = this.Connection.RawInterface.Read(symbol2.IndexGroup, symbol2.IndexOffset, 0, byteSize, value, false, out read);
                }
            }
            else
            {
                ArrayType.CheckIndices(indices, dataType, false);
                int elementOffset = ArrayType.GetElementOffset(indices, dataType);
                int byteSize = dataType.ElementType.ByteSize;
                value = new byte[byteSize];
                int read = 0;
                if (mode != ValueAccessMode.IndexGroupOffset)
                {
                    code = this.TryReadSymbolic(arraySymbol, byteSize, value, out read);
                }
                else
                {
                    IAdsSymbol symbol = (IAdsSymbol) Symbol.Unwrap(arraySymbol);
                    code = this.Connection.RawInterface.Read(symbol.IndexGroup, symbol.IndexOffset + ((uint) elementOffset), 0, byteSize, value, false, out read);
                }
            }
            if (code == AdsErrorCode.NoError)
            {
                readTime = DateTime.UtcNow;
            }
            else
            {
                value = null;
            }
            return (int) code;
        }

        private AdsErrorCode TryReadSymbolic(ISymbol address, int numBytes, byte[] value, out int read)
        {
            if (address == null)
            {
                throw new ArgumentNullException("address");
            }
            if (this.Connection == null)
            {
                throw new AdsException("Connection not established!");
            }
            AdsErrorCode noError = AdsErrorCode.NoError;
            using (new AdsConnectionRestore(this))
            {
                int handle = 0;
                noError = this.Connection.RawInterface.TryCreateVariableHandle(address.InstancePath, false, out handle);
                read = 0;
                if (noError == AdsErrorCode.NoError)
                {
                    try
                    {
                        return this.Connection.RawInterface.Read(handle, 0, numBytes, value, false, out read);
                    }
                    finally
                    {
                        AdsErrorCode code2 = this.Connection.RawInterface.TryDeleteVariableHandle(handle, false);
                        if (noError == AdsErrorCode.NoError)
                        {
                            noError = code2;
                        }
                    }
                }
                return AdsErrorCode.DeviceSymbolNotFound;
            }
        }

        public override int TryReadValue(ISymbol symbol, out byte[] value, out DateTime utcReadTime)
        {
            if (symbol == null)
            {
                throw new ArgumentNullException("symbol");
            }
            if (this.Connection == null)
            {
                throw new AdsException("Connection not established!");
            }
            IAdsSymbol symbol2 = (IAdsSymbol) Symbol.Unwrap(symbol);
            IProcessImageAddress address = symbol2;
            ValueAccessMode mode = this.calcAccessMethod(symbol2);
            int byteSize = address.ByteSize;
            if (symbol.IsReference)
            {
                byteSize = ((DataType) symbol.DataType).ResolveType(DataTypeResolveStrategy.AliasReference).ByteSize;
            }
            value = new byte[byteSize];
            int read = 0;
            utcReadTime = DateTime.MinValue;
            AdsErrorCode noError = AdsErrorCode.NoError;
            using (new AdsConnectionRestore(this))
            {
                noError = (mode != ValueAccessMode.IndexGroupOffset) ? this.TryReadSymbolic(symbol, byteSize, value, out read) : this.Connection.RawInterface.Read(address.IndexGroup, address.IndexOffset, 0, byteSize, value, false, out read);
                if (noError == AdsErrorCode.NoError)
                {
                    utcReadTime = DateTime.UtcNow;
                }
                else
                {
                    value = null;
                }
            }
            return (int) noError;
        }

        public override int TryUpdateAnyValue(ISymbol symbol, ref object valueObject, out DateTime utcReadTime)
        {
            AdsErrorCode noError = AdsErrorCode.NoError;
            byte[] buffer = null;
            noError = (AdsErrorCode) this.TryReadValue(symbol, out buffer, out utcReadTime);
            if (noError == AdsErrorCode.NoError)
            {
                this._converter.InitializeInstanceValue(symbol, ref valueObject, buffer, 0);
            }
            return (int) noError;
        }

        public override int TryWriteAnyValue(ISymbol symbol, object valueObject, out DateTime utcWriteTime)
        {
            byte[] buffer = this._converter.Marshal(symbol, valueObject);
            return this.TryWriteValue(symbol, buffer, 0, out utcWriteTime);
        }

        public override int TryWriteArrayElementValue(ISymbol arraySymbol, int[] indices, byte[] value, int valOffset, out DateTime writeTime)
        {
            if (this.Connection == null)
            {
                throw new AdsException("Connection not established!");
            }
            if (arraySymbol == null)
            {
                throw new ArgumentNullException("arraySymbol");
            }
            AdsErrorCode noError = AdsErrorCode.NoError;
            IArrayType arrayType = null;
            IResolvableType dataType = arraySymbol.DataType as IResolvableType;
            arrayType = (dataType == null) ? (arraySymbol.DataType as IArrayType) : (dataType.ResolveType(DataTypeResolveStrategy.AliasReference) as IArrayType);
            if (arrayType == null)
            {
                throw new ArgumentException("Is not an array type", "arraySymbol");
            }
            ValueAccessMode mode = this.calcAccessMethod(arraySymbol);
            writeTime = DateTime.MinValue;
            if ((indices == null) || (indices.Length == 0))
            {
                int byteSize = arraySymbol.ByteSize;
                IAdsSymbol symbol2 = (IAdsSymbol) Symbol.Unwrap(arraySymbol);
                if (value.Length != byteSize)
                {
                    throw new ArgumentException("Value array size mismatch!", "value");
                }
                if (mode == ValueAccessMode.IndexGroupOffset)
                {
                    noError = this.Connection.RawInterface.Write(symbol2.IndexGroup, symbol2.IndexOffset, valOffset, byteSize, value, false);
                }
            }
            else
            {
                ArrayType.CheckIndices(indices, arrayType, false);
                int elementOffset = ArrayType.GetElementOffset(indices, arrayType);
                int byteSize = arrayType.ElementType.ByteSize;
                if (mode != ValueAccessMode.IndexGroupOffset)
                {
                    noError = this.TryWriteSymbolic(arraySymbol, valOffset, byteSize, value);
                }
                else
                {
                    IAdsSymbol symbol = (IAdsSymbol) Symbol.Unwrap(arraySymbol);
                    noError = this.Connection.RawInterface.Write(symbol.IndexGroup, symbol.IndexOffset + ((uint) elementOffset), valOffset, byteSize, value, false);
                }
            }
            if (noError != AdsErrorCode.NoError)
            {
                writeTime = DateTime.UtcNow;
            }
            return (int) noError;
        }

        private AdsErrorCode TryWriteSymbolic(ISymbol address, int offset, int byteSize, byte[] value)
        {
            if (address == null)
            {
                throw new ArgumentNullException("address");
            }
            if (this.Connection == null)
            {
                throw new AdsException("Connection not established!");
            }
            int handle = 0;
            using (new AdsConnectionRestore(this))
            {
                AdsErrorCode code = this.Connection.RawInterface.TryCreateVariableHandle(address.InstancePath, false, out handle);
                if (code == AdsErrorCode.NoError)
                {
                    try
                    {
                        return this.Connection.RawInterface.Write(handle, offset, byteSize, value, false);
                    }
                    finally
                    {
                        code = this.Connection.RawInterface.TryDeleteVariableHandle(handle, false);
                    }
                }
                return AdsErrorCode.DeviceSymbolNotFound;
            }
        }

        public override int TryWriteValue(ISymbol symbol, byte[] value, int offset, out DateTime utcWriteTime)
        {
            AdsErrorCode deviceInvalidSize;
            if (symbol == null)
            {
                throw new ArgumentNullException("symbol");
            }
            if (this.Connection == null)
            {
                throw new AdsException("Connection not established!");
            }
            int byteSize = 0;
            IAdsSymbol symbol2 = (IAdsSymbol) Symbol.Unwrap(symbol);
            ValueAccessMode mode = this.calcAccessMethod(symbol2);
            if (!symbol2.IsBitType)
            {
                byteSize = symbol2.Size;
            }
            else
            {
                byteSize = symbol2.Size / 8;
                if ((symbol2.Size % 8) > 0)
                {
                    byteSize++;
                }
            }
            utcWriteTime = DateTime.MinValue;
            if ((value.Length - offset) > byteSize)
            {
                deviceInvalidSize = AdsErrorCode.DeviceInvalidSize;
            }
            else
            {
                using (new AdsConnectionRestore(this))
                {
                    deviceInvalidSize = (mode != ValueAccessMode.IndexGroupOffset) ? this.TryWriteSymbolic(symbol2, offset, byteSize, value) : this.Connection.RawInterface.Write(symbol2.IndexGroup, symbol2.IndexOffset, offset, byteSize, value, false);
                    if (deviceInvalidSize == AdsErrorCode.NoError)
                    {
                        utcWriteTime = DateTime.UtcNow;
                    }
                }
            }
            return (int) deviceInvalidSize;
        }

        private bool UnregisterNotification(ISymbol symbol, SymbolNotificationType type)
        {
            if (symbol == null)
            {
                throw new ArgumentNullException("symbol");
            }
            if (this.Connection == null)
            {
                throw new AdsException("Connection not established!");
            }
            bool flag = false;
            int handle = 0;
            if (this._notificationTable.TryGetNotificationHandle(symbol, out handle))
            {
                flag = this._notificationTable.Remove(symbol, type);
                if (flag)
                {
                    AdsConnectionRestore restore = new AdsConnectionRestore(this);
                    try
                    {
                        this.Connection.DeleteDeviceNotification(handle);
                    }
                    catch (AdsErrorException exception)
                    {
                        Module.Trace.TraceError(exception);
                        flag = false;
                    }
                    finally
                    {
                        if (restore != null)
                        {
                            restore.Dispose();
                        }
                    }
                }
            }
            int largestSymbolSize = this._notificationTable.GetLargestSymbolSize();
            this.resizeNotificationStream(largestSymbolSize);
            return flag;
        }

        public IAdsConnection Connection =>
            (base.Connection as IAdsConnection);

        public ValueAccessMode AccessMethod
        {
            get => 
                this._accessMethod;
            set => 
                (this._accessMethod = value);
        }
    }
}

