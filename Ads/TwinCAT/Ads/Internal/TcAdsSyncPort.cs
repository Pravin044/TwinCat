namespace TwinCAT.Ads.Internal
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Text;
    using TwinCAT.Ads;
    using TwinCAT.Ads.Tracing;
    using TwinCAT.PlcOpen;

    internal class TcAdsSyncPort : TcAdsDllWrapper, ISyncMessageReceiver, ITcAdsRaw, ITcAdsRawPrimitives, ITcAdsRawAny, IAdsErrorInjector
    {
        private bool _bClientCycle;
        private bool _bSynchronize;
        private bool _bLocal;
        private int _id = ++s_id;
        private SyncWindow _routerSyncWindow;
        private NotificationMngt _notificationMngt;
        private SymbolTable _symbolTable;
        private int _adsStateHandle;
        private int _symbolVersionHandle;
        protected int _port;
        protected TcLocalSystem _localSystem;
        protected INotificationReceiver _iNoteReceiver;
        private static int s_id;

        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TcAdsSyncPort(AmsAddress addr, TcLocalSystem localSystem, INotificationReceiver iNoteReceiver, bool clientCycle, bool synchronize)
        {
            object[] args = new object[] { this._id, addr };
            using (new MethodTrace("Id: {0:d}, Address = {1}", args))
            {
                this._localSystem = localSystem;
                this._iNoteReceiver = iNoteReceiver;
                if (!base.IsLocalNetId(addr.netId, localSystem.NetId))
                {
                    base.address = addr.Clone();
                }
                else
                {
                    base.address = new AmsAddress(localSystem.NetId, addr.port);
                    this._bLocal = true;
                }
                this._bSynchronize = synchronize;
                this._bClientCycle = clientCycle;
                this._adsStateHandle = 0;
                this._symbolVersionHandle = 0;
                this._symbolTable = new SymbolTable(this);
                if (synchronize)
                {
                    this._routerSyncWindow = new SyncWindow(this);
                }
            }
        }

        public int AddDeviceNotification(string variableName, AdsStream data, int offset, int length, int transMode, int cycleTime, int maxDelay, object userData)
        {
            uint handle = 0;
            AdsErrorCode adsErrorCode = this.TryAddDeviceNotification(variableName, data, offset, length, transMode, cycleTime, maxDelay, userData, out handle);
            if (adsErrorCode != AdsErrorCode.NoError)
            {
                ThrowAdsException(adsErrorCode);
            }
            return (int) handle;
        }

        public int AddDeviceNotification(uint indexGroup, uint indexOffset, AdsStream data, int offset, int length, int transMode, int cycleTime, int maxDelay, object userData)
        {
            AdsErrorCode code;
            if (this._notificationMngt == null)
            {
                this._notificationMngt = this.CreateNotificationMngt();
            }
            int num = this._notificationMngt.AddNotification(indexGroup, indexOffset, data, offset, length, transMode, cycleTime, maxDelay, userData, out code);
            if (code != AdsErrorCode.NoError)
            {
                ThrowAdsException(code);
            }
            return num;
        }

        private AdsErrorCode AddStateChangedNotification()
        {
            AdsErrorCode code;
            if (this._notificationMngt == null)
            {
                this._notificationMngt = this.CreateNotificationMngt();
            }
            this._adsStateHandle = this._notificationMngt.AddNotification(0xf100, 0, new AdsStream(4), 0, 4, 4, 0, 0, null, out code);
            return code;
        }

        private AdsErrorCode AddSymbolVersionChangedNotification()
        {
            AdsErrorCode code;
            if (this._notificationMngt == null)
            {
                this._notificationMngt = this.CreateNotificationMngt();
            }
            this._symbolVersionHandle = this._notificationMngt.AddNotification(0xf008, 0, new AdsStream(1), 0, 1, 4, 0, 0, null, out code);
            return code;
        }

        protected override AdsErrorCode AdsPortClose(bool throwAdsException) => 
            AdsErrorCode.NoError;

        protected override int AdsPortOpen() => 
            this._localSystem.Port;

        public void Connect()
        {
            AdsErrorCode adsErrorCode = this.OnOpenPort();
            if (adsErrorCode != AdsErrorCode.NoError)
            {
                ThrowAdsException(adsErrorCode);
            }
        }

        private NotificationMngt CreateNotificationMngt() => 
            (!this._bClientCycle ? ((NotificationMngt) new ServerCycleNotificationMngt(this, this._symbolTable, this._bSynchronize)) : ((NotificationMngt) new ClientCycleNotificationMngt(this, this._symbolTable, this._bSynchronize)));

        public void DelDeviceNotification(int notificationHandle)
        {
            if (this._notificationMngt == null)
            {
                ThrowAdsException(AdsErrorCode.ClientError);
            }
            AdsErrorCode adsErrorCode = this.TryDeleteDeviceNotification((uint) notificationHandle);
            if (adsErrorCode != AdsErrorCode.NoError)
            {
                ThrowAdsException(adsErrorCode);
            }
        }

        public void Disconnect()
        {
            AdsErrorCode adsErrorCode = this.OnClosePort();
            if (adsErrorCode != AdsErrorCode.NoError)
            {
                ThrowAdsException(adsErrorCode);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (!base._disposed)
            {
                Module.Trace.TraceVerbose($"TcAdsSyncPort.Dispose({this._id:d}); disposing={disposing.ToString()}");
                if (disposing)
                {
                    if (this._notificationMngt != null)
                    {
                        this._notificationMngt.Dispose();
                        this._notificationMngt = null;
                    }
                    if (this._routerSyncWindow != null)
                    {
                        this._routerSyncWindow.Dispose();
                        this._routerSyncWindow = null;
                    }
                    if (this._symbolTable != null)
                    {
                        this._symbolTable.Dispose();
                        this._symbolTable = null;
                    }
                }
                AdsErrorCode code = this.OnClosePort();
                base.Dispose(disposing);
            }
        }

        ~TcAdsSyncPort()
        {
            Module.Trace.TraceVerbose($"ID: ({this._id:d})");
            this.Dispose(false);
        }

        public AdsErrorCode InjectError(AdsErrorCode error, bool throwAdsException)
        {
            if (throwAdsException)
            {
                ThrowAdsException(error);
            }
            return error;
        }

        public void OnAdsStateChanged(AdsStateChangedEventArgs eventArgs)
        {
            this._iNoteReceiver.OnAdsStateChanged(eventArgs);
        }

        protected virtual AdsErrorCode OnClosePort() => 
            this.AdsPortClose(false);

        public void OnNotificationError(Exception e)
        {
            this._iNoteReceiver.OnNotificationError(e);
        }

        protected virtual AdsErrorCode OnOpenPort()
        {
            this._port = this.AdsPortOpen();
            return ((this._port != 0) ? AdsErrorCode.NoError : AdsErrorCode.ClientPortNotOpen);
        }

        public void OnRouterNotification(AmsRouterState state)
        {
            if (!base._disposed)
            {
                if (!this._bSynchronize)
                {
                    this.OnSyncRouterNotification(state);
                }
                else if (this._routerSyncWindow != null)
                {
                    this._routerSyncWindow.PostRouterNotification(state);
                }
            }
        }

        public void OnSymbolVersionChanged(AdsSymbolVersionChangedEventArgs eventArgs)
        {
            this._iNoteReceiver.OnSymbolVersionChanged(eventArgs);
        }

        public void OnSyncNotification(int handle, long timeStamp, int length, NotificationEntry entry, bool bError)
        {
            if (handle == this._symbolVersionHandle)
            {
                if (!bError)
                {
                    this.OnSymbolVersionChanged(new AdsSymbolVersionChangedEventArgs(entry.data.GetBuffer()[0]));
                }
            }
            else if (handle != this._adsStateHandle)
            {
                if (bError)
                {
                    this._iNoteReceiver.OnNotificationError(handle, timeStamp);
                }
                else
                {
                    this._iNoteReceiver.OnNotification(handle, timeStamp, length, entry);
                }
            }
            else if (!bError)
            {
                StateInfo state = new StateInfo();
                BinaryReader reader = new BinaryReader(entry.data) {
                    BaseStream = { Position = 0L }
                };
                state.AdsState = (AdsState) reader.ReadInt16();
                state.DeviceState = reader.ReadInt16();
                this.OnAdsStateChanged(new AdsStateChangedEventArgs(state));
            }
        }

        public void OnSyncRouterNotification(AmsRouterState state)
        {
            if (this._iNoteReceiver != null)
            {
                this._iNoteReceiver.OnRouterNotification(state);
            }
        }

        public AdsErrorCode Read(int variableHandle, int offset, int length, byte[] data, bool throwAdsException, out int dataRead)
        {
            AdsErrorCode code;
            uint serverHandle = this._symbolTable.GetServerHandle(variableHandle, out code);
            if (code == AdsErrorCode.NoError)
            {
                code = base.Read(0xf005, serverHandle, offset, length, data, throwAdsException, out dataRead);
            }
            else
            {
                dataRead = 0;
                if (throwAdsException)
                {
                    ThrowAdsException(code);
                }
            }
            return code;
        }

        public AdsErrorCode ReadAny(int variableHandle, Type type, bool throwAdsException, out object value)
        {
            AdsErrorCode code;
            uint serverHandle = this._symbolTable.GetServerHandle(variableHandle, out code);
            if (code == AdsErrorCode.NoError)
            {
                code = this.ReadAny(0xf005, serverHandle, type, throwAdsException, out value);
            }
            else
            {
                value = null;
                if (throwAdsException)
                {
                    ThrowAdsException(code);
                }
            }
            return code;
        }

        public AdsErrorCode ReadAny(int variableHandle, Type type, int[] args, bool throwAdsException, out object value)
        {
            AdsErrorCode code;
            uint serverHandle = this._symbolTable.GetServerHandle(variableHandle, out code);
            if (code == AdsErrorCode.NoError)
            {
                code = this.ReadAny(0xf005, serverHandle, type, args, throwAdsException, out value);
            }
            else
            {
                value = null;
                if (throwAdsException)
                {
                    ThrowAdsException(code);
                }
            }
            return code;
        }

        public AdsErrorCode ReadAny(uint indexGroup, uint indexOffset, Type type, bool throwAdsException, out object value)
        {
            AdsErrorCode code;
            if (!type.IsPrimitive)
            {
                if (type == typeof(string))
                {
                    throw new ArgumentException("Use overload ReadAnyString(uint indexGroup, uint indexOffset,  Type type, int characters) for strings.", "type");
                }
                if (type.IsArray)
                {
                    throw new ArgumentException("Use overload ReadAny(uint indexGroup, uint indexOffset,  Type type, int[] args) for arrays.", "type");
                }
                if (type == typeof(TimeSpan))
                {
                    uint milliseconds = base.ReadUInt32(indexGroup, indexOffset, throwAdsException, out code);
                    value = PlcOpenTimeConverter.MillisecondsToTimeSpan(milliseconds);
                }
                else if (type == typeof(DateTime))
                {
                    uint dateValue = base.ReadUInt32(indexGroup, indexOffset, throwAdsException, out code);
                    value = PlcOpenDateConverterBase.ToDateTime(dateValue);
                }
                else if (type == typeof(TIME))
                {
                    uint timeValue = base.ReadUInt32(indexGroup, indexOffset, throwAdsException, out code);
                    value = new TIME(timeValue);
                }
                else if (type == typeof(LTIME))
                {
                    ulong timeValue = base.ReadUInt64(indexGroup, indexOffset, throwAdsException, out code);
                    value = new LTIME(timeValue);
                }
                else if (type == typeof(TOD))
                {
                    uint time = base.ReadUInt32(indexGroup, indexOffset, throwAdsException, out code);
                    value = new TOD(time);
                }
                else if (type == typeof(DATE))
                {
                    uint dateValue = base.ReadUInt32(indexGroup, indexOffset, throwAdsException, out code);
                    value = new DATE(dateValue);
                }
                else if (type != typeof(DT))
                {
                    value = base.ReadStruct(indexGroup, indexOffset, type, throwAdsException, out code);
                }
                else
                {
                    uint dateValue = base.ReadUInt32(indexGroup, indexOffset, throwAdsException, out code);
                    value = new DT(dateValue);
                }
            }
            else if (type == typeof(bool))
            {
                value = base.ReadBoolean(indexGroup, indexOffset, throwAdsException, out code);
            }
            else if (type == typeof(int))
            {
                value = base.ReadInt32(indexGroup, indexOffset, throwAdsException, out code);
            }
            else if (type == typeof(short))
            {
                value = base.ReadInt16(indexGroup, indexOffset, throwAdsException, out code);
            }
            else if (type == typeof(byte))
            {
                value = base.ReadUInt8(indexGroup, indexOffset, throwAdsException, out code);
            }
            else if (type == typeof(float))
            {
                value = base.ReadReal32(indexGroup, indexOffset, throwAdsException, out code);
            }
            else if (type == typeof(double))
            {
                value = base.ReadReal64(indexGroup, indexOffset, throwAdsException, out code);
            }
            else if (type == typeof(long))
            {
                value = base.ReadInt64(indexGroup, indexOffset, throwAdsException, out code);
            }
            else if (type == typeof(uint))
            {
                value = base.ReadUInt32(indexGroup, indexOffset, throwAdsException, out code);
            }
            else if (type == typeof(ushort))
            {
                value = base.ReadUInt16(indexGroup, indexOffset, throwAdsException, out code);
            }
            else if (type == typeof(ulong))
            {
                value = base.ReadUInt64(indexGroup, indexOffset, throwAdsException, out code);
            }
            else
            {
                if (type != typeof(sbyte))
                {
                    throw new ArgumentException("Unable to marshal type.", "type");
                }
                value = base.ReadInt8(indexGroup, indexOffset, throwAdsException, out code);
            }
            return code;
        }

        public AdsErrorCode ReadAny(uint indexGroup, uint indexOffset, Type type, int[] args, bool throwAdsException, out object value)
        {
            AdsErrorCode code;
            if (args == null)
            {
                return this.ReadAny(indexGroup, indexOffset, type, throwAdsException, out value);
            }
            if (!type.IsArray)
            {
                if (type != typeof(string))
                {
                    throw new ArgumentException($"Unable to marshal type '{type}'!", "type");
                }
                value = base.ReadString(indexGroup, indexOffset, args[0], base.Encoding, throwAdsException, out code);
                return code;
            }
            else
            {
                Type elementType = type.GetElementType();
                if (!elementType.IsPrimitive)
                {
                    if (type != typeof(string[]))
                    {
                        if (elementType.IsArray)
                        {
                            goto TR_001D;
                        }
                        else if (type.GetArrayRank() != 1)
                        {
                            goto TR_001D;
                        }
                        else
                        {
                            value = base.ReadArrayOfStruct(indexGroup, indexOffset, elementType, args[0], throwAdsException, out code);
                        }
                    }
                    else
                    {
                        if (args.Length < 2)
                        {
                            throw new ArgumentException($"Invalid additional arguments for type '{type}'!", "type");
                        }
                        value = base.ReadArrayOfString(indexGroup, indexOffset, args[0], args[1], throwAdsException, out code);
                    }
                }
                else if (elementType == typeof(bool))
                {
                    value = base.ReadArrayOfBoolean(indexGroup, indexOffset, args, throwAdsException, out code);
                }
                else if (elementType == typeof(int))
                {
                    value = base.ReadArrayOfInt32(indexGroup, indexOffset, args, throwAdsException, out code);
                }
                else if (elementType == typeof(short))
                {
                    value = base.ReadArrayOfInt16(indexGroup, indexOffset, args, throwAdsException, out code);
                }
                else if (elementType == typeof(byte))
                {
                    value = base.ReadArrayOfUInt8(indexGroup, indexOffset, args, throwAdsException, out code);
                }
                else if (elementType == typeof(float))
                {
                    value = base.ReadArrayOfReal32(indexGroup, indexOffset, args, throwAdsException, out code);
                }
                else if (elementType == typeof(double))
                {
                    value = base.ReadArrayOfReal64(indexGroup, indexOffset, args, throwAdsException, out code);
                }
                else if (elementType == typeof(long))
                {
                    value = base.ReadArrayOfInt64(indexGroup, indexOffset, args, throwAdsException, out code);
                }
                else if (elementType == typeof(uint))
                {
                    value = base.ReadArrayOfUInt32(indexGroup, indexOffset, args, throwAdsException, out code);
                }
                else if (elementType == typeof(ushort))
                {
                    value = base.ReadArrayOfUInt16(indexGroup, indexOffset, args, throwAdsException, out code);
                }
                else if (type == typeof(ulong))
                {
                    value = base.ReadArrayOfUInt64(indexGroup, indexOffset, args, throwAdsException, out code);
                }
                else
                {
                    if (elementType != typeof(sbyte))
                    {
                        throw new ArgumentException($"Unable to marshal type '{type}'!", "type");
                    }
                    value = base.ReadArrayOfInt8(indexGroup, indexOffset, args, throwAdsException, out code);
                }
                return code;
            }
        TR_001D:
            throw new ArgumentException($"Unable to marshal type '{type}'!", "type");
        }

        public string ReadString(int variableHandle, int len, Encoding encoding, bool throwAdsException, out AdsErrorCode errorCode)
        {
            string str;
            uint serverHandle = this._symbolTable.GetServerHandle(variableHandle, out errorCode);
            if (errorCode == AdsErrorCode.NoError)
            {
                errorCode = this.ReadString(0xf005, serverHandle, len, encoding, throwAdsException, out str);
            }
            else
            {
                str = null;
                if (throwAdsException)
                {
                    ThrowAdsException(errorCode);
                }
            }
            return str;
        }

        public AdsErrorCode ReadString(uint indexGroup, uint indexOffset, int len, Encoding encoding, bool throwAdsException, out string value)
        {
            AdsErrorCode code;
            value = base.ReadString(indexGroup, indexOffset, len, encoding, throwAdsException, out code);
            return code;
        }

        public AdsErrorCode ReadWrite(int variableHandle, int rdOffset, int rdLength, byte[] rdData, int wrOffset, int wrLength, byte[] wrData, bool throwAdsException, out int dataRead)
        {
            AdsErrorCode code;
            uint serverHandle = this._symbolTable.GetServerHandle(variableHandle, out code);
            if (code == AdsErrorCode.NoError)
            {
                code = base.ReadWrite(0xf005, serverHandle, rdOffset, rdLength, rdData, wrOffset, wrLength, wrData, throwAdsException, out dataRead);
            }
            else
            {
                dataRead = 0;
                if (throwAdsException)
                {
                    ThrowAdsException(code);
                }
            }
            return code;
        }

        public void RegisterStateChangedNotification()
        {
            if (this._adsStateHandle == 0)
            {
                AdsErrorCode adsErrorCode = this.AddStateChangedNotification();
                if (adsErrorCode != AdsErrorCode.NoError)
                {
                    ThrowAdsException(adsErrorCode);
                }
            }
        }

        public void RegisterSymbolVersionChangedNotification()
        {
            if (this._symbolVersionHandle == 0)
            {
                AdsErrorCode adsErrorCode = this.AddSymbolVersionChangedNotification();
                if (adsErrorCode != AdsErrorCode.NoError)
                {
                    ThrowAdsException(adsErrorCode);
                }
            }
        }

        public AdsErrorCode TryAddDeviceNotification(string variableName, AdsStream data, int offset, int length, int transMode, int cycleTime, int maxDelay, object userData, out uint handle)
        {
            AdsErrorCode code;
            if (this._notificationMngt == null)
            {
                this._notificationMngt = this.CreateNotificationMngt();
            }
            handle = (uint) this._notificationMngt.AddNotification(variableName, data, offset, length, transMode, cycleTime, maxDelay, userData, out code);
            return code;
        }

        public AdsErrorCode TryCreateVariableHandle(string variableName, bool throwAdsException, out int handle)
        {
            handle = 0;
            AdsErrorCode adsErrorCode = this._symbolTable.TryCreateVariableHandle(variableName, out handle);
            if ((adsErrorCode != AdsErrorCode.NoError) & throwAdsException)
            {
                ThrowAdsException(adsErrorCode);
            }
            return adsErrorCode;
        }

        public AdsErrorCode TryDeleteDeviceNotification(uint notificationHandle)
        {
            AdsErrorCode code;
            if (this._notificationMngt == null)
            {
                ThrowAdsException(AdsErrorCode.ClientError);
            }
            this._notificationMngt.DeleteNotification((int) notificationHandle, out code);
            return code;
        }

        public AdsErrorCode TryDeleteVariableHandle(int variableHandle, bool throwAdsException)
        {
            AdsErrorCode adsErrorCode = this._symbolTable.TryDeleteVariableHandle(variableHandle);
            if ((adsErrorCode != AdsErrorCode.NoError) & throwAdsException)
            {
                ThrowAdsException(adsErrorCode);
            }
            return adsErrorCode;
        }

        public void UnregisterStateChangedNotification()
        {
            if (this._adsStateHandle != 0)
            {
                if (this._notificationMngt != null)
                {
                    AdsErrorCode code;
                    this._notificationMngt.DeleteNotification(this._adsStateHandle, out code);
                }
                this._adsStateHandle = 0;
            }
        }

        public void UnregisterSymbolVersionChangedNotification()
        {
            if (this._symbolVersionHandle != 0)
            {
                if (this._notificationMngt != null)
                {
                    AdsErrorCode code;
                    this._notificationMngt.DeleteNotification(this._symbolVersionHandle, out code);
                }
                this._symbolVersionHandle = 0;
            }
        }

        public AdsErrorCode Write(int variableHandle, int offset, int length, byte[] data, bool throwAdsException)
        {
            AdsErrorCode code;
            uint serverHandle = this._symbolTable.GetServerHandle(variableHandle, out code);
            if (code == AdsErrorCode.NoError)
            {
                code = base.Write(0xf005, serverHandle, offset, length, data, throwAdsException);
            }
            else if (throwAdsException)
            {
                ThrowAdsException(code);
            }
            return code;
        }

        public AdsErrorCode WriteAny(int variableHandle, object value, bool throwAdsException)
        {
            AdsErrorCode code;
            uint serverHandle = this._symbolTable.GetServerHandle(variableHandle, out code);
            if (code == AdsErrorCode.NoError)
            {
                code = this.WriteAny(0xf005, serverHandle, value, throwAdsException);
            }
            else if (throwAdsException)
            {
                ThrowAdsException(code);
            }
            return code;
        }

        public AdsErrorCode WriteAny(int variableHandle, object value, int[] args, bool throwAdsException)
        {
            AdsErrorCode code;
            uint serverHandle = this._symbolTable.GetServerHandle(variableHandle, out code);
            if (code == AdsErrorCode.NoError)
            {
                code = this.WriteAny(0xf005, serverHandle, value, args, throwAdsException);
            }
            else if (throwAdsException)
            {
                ThrowAdsException(code);
            }
            return code;
        }

        public AdsErrorCode WriteAny(uint indexGroup, uint indexOffset, object value, bool throwAdsException)
        {
            Type type = value.GetType();
            if (type.IsPrimitive)
            {
                if (type == typeof(bool))
                {
                    return base.Write(indexGroup, indexOffset, (bool) value, throwAdsException);
                }
                if (type == typeof(int))
                {
                    return base.Write(indexGroup, indexOffset, (int) value, throwAdsException);
                }
                if (type == typeof(short))
                {
                    return base.Write(indexGroup, indexOffset, (short) value, throwAdsException);
                }
                if (type == typeof(byte))
                {
                    return base.Write(indexGroup, indexOffset, (byte) value, throwAdsException);
                }
                if (type == typeof(float))
                {
                    return base.Write(indexGroup, indexOffset, (float) value, throwAdsException);
                }
                if (type == typeof(double))
                {
                    return base.Write(indexGroup, indexOffset, (double) value, throwAdsException);
                }
                if (type == typeof(long))
                {
                    return base.Write(indexGroup, indexOffset, (long) value, throwAdsException);
                }
                if (type == typeof(uint))
                {
                    return base.Write(indexGroup, indexOffset, (uint) value, throwAdsException);
                }
                if (type == typeof(ushort))
                {
                    return base.Write(indexGroup, indexOffset, (ushort) value, throwAdsException);
                }
                if (type == typeof(ulong))
                {
                    return base.Write(indexGroup, indexOffset, (ulong) value, throwAdsException);
                }
                if (type != typeof(sbyte))
                {
                    throw new ArgumentException("Unable to marshal object.", "value");
                }
                return base.Write(indexGroup, indexOffset, (sbyte) value, throwAdsException);
            }
            if (!type.IsArray)
            {
                if (type == typeof(string))
                {
                    string str = (string) value;
                    throw new ArgumentException("Use overload WriteAnyString(uint indexGroup, uint indexOffset, string value, int characters)) for strings.", "value");
                }
                if (type == typeof(DateTime))
                {
                    uint num = PlcOpenDateConverterBase.ToTicks((DateTime) value);
                    return base.Write(indexGroup, indexOffset, num, throwAdsException);
                }
                if (type != typeof(TimeSpan))
                {
                    return ((type != typeof(TIME)) ? ((type != typeof(LTIME)) ? ((type != typeof(TOD)) ? ((type != typeof(DATE)) ? ((type != typeof(DT)) ? base.Write(indexGroup, indexOffset, value, throwAdsException) : base.Write(indexGroup, indexOffset, ((DT) value).Ticks, throwAdsException)) : base.Write(indexGroup, indexOffset, ((DATE) value).Ticks, throwAdsException)) : base.Write(indexGroup, indexOffset, ((TOD) value).Ticks, throwAdsException)) : base.Write(indexGroup, indexOffset, ((LTIME) value).Ticks, throwAdsException)) : base.Write(indexGroup, indexOffset, ((TIME) value).Ticks, throwAdsException));
                }
                uint val = PlcOpenTimeConverter.ToMilliseconds((TimeSpan) value);
                return base.Write(indexGroup, indexOffset, val, throwAdsException);
            }
            Type elementType = type.GetElementType();
            if (!elementType.IsPrimitive)
            {
                if (type == typeof(string[]))
                {
                    throw new ArgumentException("Use overload WriteString(uint indexGroup, uint indexOffset, string value, int characters)) for strings.", "value");
                }
                if (elementType.IsArray || (type.GetArrayRank() != 1))
                {
                    throw new ArgumentException("Unable to marshal object.", "value");
                }
                return base.WriteArrayOfStruct(indexGroup, indexOffset, value, throwAdsException);
            }
            if (elementType == typeof(bool))
            {
                return base.WriteArrayOfBoolean(indexGroup, indexOffset, (Array) value, throwAdsException);
            }
            if (elementType == typeof(int))
            {
                return base.WriteArrayOfInt32(indexGroup, indexOffset, (Array) value, throwAdsException);
            }
            if (elementType == typeof(short))
            {
                return base.WriteArrayOfInt16(indexGroup, indexOffset, (Array) value, throwAdsException);
            }
            if (elementType == typeof(byte))
            {
                return base.WriteArrayOfUInt8(indexGroup, indexOffset, (Array) value, throwAdsException);
            }
            if (elementType == typeof(float))
            {
                return base.WriteArrayOfReal32(indexGroup, indexOffset, (Array) value, throwAdsException);
            }
            if (elementType == typeof(double))
            {
                return base.WriteArrayOfReal64(indexGroup, indexOffset, (Array) value, throwAdsException);
            }
            if (elementType == typeof(long))
            {
                return base.WriteArrayOfInt64(indexGroup, indexOffset, (Array) value, throwAdsException);
            }
            if (elementType == typeof(uint))
            {
                return base.WriteArrayOfUInt32(indexGroup, indexOffset, (Array) value, throwAdsException);
            }
            if (elementType == typeof(ushort))
            {
                return base.WriteArrayOfUInt16(indexGroup, indexOffset, (Array) value, throwAdsException);
            }
            if (elementType != typeof(sbyte))
            {
                throw new ArgumentException("Unable to marshal object.", "value");
            }
            return base.WriteArrayOfInt8(indexGroup, indexOffset, (Array) value, throwAdsException);
        }

        public AdsErrorCode WriteAny(uint indexGroup, uint indexOffset, object value, int[] args, bool throwAdsException)
        {
            if (args == null)
            {
                return this.WriteAny(indexGroup, indexOffset, value, throwAdsException);
            }
            Type type = value.GetType();
            if (type == typeof(string))
            {
                return base.Write(indexGroup, indexOffset, (string) value, args[0], throwAdsException);
            }
            if (type != typeof(string[]))
            {
                throw new ArgumentException("Unable to marshal object.", "value");
            }
            return base.WriteArrayOfString(indexGroup, indexOffset, (string[]) value, args[0], throwAdsException);
        }

        public AdsErrorCode WriteString(int variableHandle, string str, int characters, Encoding encoding, bool throwAdsException)
        {
            AdsErrorCode code;
            uint serverHandle = this._symbolTable.GetServerHandle(variableHandle, out code);
            if (code == AdsErrorCode.NoError)
            {
                code = this.WriteString(0xf005, serverHandle, str, characters, encoding, throwAdsException);
            }
            else if (throwAdsException)
            {
                ThrowAdsException(code);
            }
            return code;
        }

        public AdsErrorCode WriteString(uint indexGroup, uint indexOffset, string str, int characters, Encoding encoding, bool throwAdsException) => 
            base.Write(indexGroup, indexOffset, str, characters, throwAdsException);

        public int Id =>
            this._id;

        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        public bool ClientCycle
        {
            get => 
                this._bClientCycle;
            set
            {
                if ((this._notificationMngt != null) && this._notificationMngt.IsActive)
                {
                    throw new AdsException("Cannot change ClientCycle value while notifications are active.");
                }
                if (value != this._bClientCycle)
                {
                    this._bClientCycle = value;
                    if (this._notificationMngt != null)
                    {
                        this._notificationMngt.Dispose();
                        this._notificationMngt = this.CreateNotificationMngt();
                    }
                }
            }
        }

        public bool Synchronize
        {
            get => 
                this._bSynchronize;
            set
            {
                if (value != this._bSynchronize)
                {
                    if ((this._notificationMngt != null) && this._notificationMngt.IsActive)
                    {
                        throw new AdsException("Cannot change synchronization mode while notifications are active.");
                    }
                    this._bSynchronize = value;
                    if (this._notificationMngt != null)
                    {
                        this._notificationMngt.Dispose();
                        this._notificationMngt = this.CreateNotificationMngt();
                    }
                    if (this._bSynchronize)
                    {
                        this._routerSyncWindow = new SyncWindow(this);
                    }
                    else
                    {
                        this._routerSyncWindow.Dispose();
                        this._routerSyncWindow = null;
                    }
                }
            }
        }

        public int Port =>
            this._port;

        public AmsNetId NetId =>
            this._localSystem.NetId;

        public bool IsLocal =>
            this._bLocal;
    }
}

