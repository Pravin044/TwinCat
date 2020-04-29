namespace TwinCAT.Ads
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using TwinCAT;
    using TwinCAT.Ads.Internal;
    using TwinCAT.Ads.TypeSystem;

    [DebuggerDisplay("Id = {Id} : Address: {Address}, State: {ConnectionState}")]
    public sealed class AdsConnection : IAdsConnection, IConnection, IConnectionStateProvider, IAdsNotifications, IAdsSymbolicAccess, IAdsAnyAccess, IAdsHandleAccess, IAdsReadWriteAccess, IAdsStateControl, ITcAdsRpcInvoke, IAdsReadWriteTimeoutAccess, IAdsStateControlTimeout, IAdsSymbolLoaderFactory, IDisposable
    {
        private static int s_id;
        private AdsSession _session;
        private int _id = ++s_id;
        private TcAdsClient _client;
        private Action _beforeAccessDelegate;
        private Action _afterAccessDelegate;
        private bool _resurrecting;
        private CommunicationInterceptors _interceptors;
        private TwinCAT.ConnectionState _connectionState = TwinCAT.ConnectionState.Disconnected;
        [CompilerGenerated]
        private EventHandler<ConnectionStateChangedEventArgs> ConnectionStateChanged;
        [CompilerGenerated]
        private EventHandler<AdsStateChangedEventArgs2> DeviceStateChanged;
        private DateTime? _lostTime;
        private bool _disposed;
        private DateTime? _connectionEstablishTime;
        private DateTime? _connectionActiveSince;
        private int _resurrectingTryCount;
        private int _resurrections;
        private int _connectionLostCount;

        public event AdsNotificationEventHandler AdsNotification
        {
            add
            {
                if (this._disposed)
                {
                    throw new ObjectDisposedException(this.Name);
                }
                this._client.AdsNotification += value;
            }
            remove
            {
                if (this._client != null)
                {
                    this._client.AdsNotification -= value;
                }
            }
        }

        public event AdsNotificationErrorEventHandler AdsNotificationError
        {
            add
            {
                if (this._disposed)
                {
                    throw new ObjectDisposedException(this.Name);
                }
                this._client.AdsNotificationError += value;
            }
            remove
            {
                if (this._client != null)
                {
                    this._client.AdsNotificationError -= value;
                }
            }
        }

        public event AdsNotificationExEventHandler AdsNotificationEx
        {
            add
            {
                if (this._disposed)
                {
                    throw new ObjectDisposedException(this.Name);
                }
                this._client.AdsNotificationEx += value;
            }
            remove
            {
                if (this._client != null)
                {
                    this._client.AdsNotificationEx -= value;
                }
            }
        }

        public event AdsStateChangedEventHandler AdsStateChanged
        {
            add
            {
                if (this._disposed)
                {
                    throw new ObjectDisposedException(this.Name);
                }
                this._client.AdsStateChanged += value;
            }
            remove
            {
                if (this._client != null)
                {
                    this._client.AdsStateChanged -= value;
                }
            }
        }

        public event EventHandler AdsSymbolVersionChanged
        {
            add
            {
                if (this._disposed)
                {
                    throw new ObjectDisposedException(this.Name);
                }
                this._client.AdsSymbolVersionChanged += value;
            }
            remove
            {
                if (this._client != null)
                {
                    this._client.AdsSymbolVersionChanged -= value;
                }
            }
        }

        public event AmsRouterNotificationEventHandler AmsRouterNotification
        {
            add
            {
                if (this._disposed)
                {
                    throw new ObjectDisposedException(this.Name);
                }
                this._client.AmsRouterNotification += value;
            }
            remove
            {
                if (this._client != null)
                {
                    this._client.AmsRouterNotification -= value;
                }
            }
        }

        public event EventHandler<ConnectionStateChangedEventArgs> ConnectionStateChanged
        {
            [CompilerGenerated] add
            {
                EventHandler<ConnectionStateChangedEventArgs> connectionStateChanged = this.ConnectionStateChanged;
                while (true)
                {
                    EventHandler<ConnectionStateChangedEventArgs> a = connectionStateChanged;
                    EventHandler<ConnectionStateChangedEventArgs> handler3 = (EventHandler<ConnectionStateChangedEventArgs>) Delegate.Combine(a, value);
                    connectionStateChanged = Interlocked.CompareExchange<EventHandler<ConnectionStateChangedEventArgs>>(ref this.ConnectionStateChanged, handler3, a);
                    if (ReferenceEquals(connectionStateChanged, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                EventHandler<ConnectionStateChangedEventArgs> connectionStateChanged = this.ConnectionStateChanged;
                while (true)
                {
                    EventHandler<ConnectionStateChangedEventArgs> source = connectionStateChanged;
                    EventHandler<ConnectionStateChangedEventArgs> handler3 = (EventHandler<ConnectionStateChangedEventArgs>) Delegate.Remove(source, value);
                    connectionStateChanged = Interlocked.CompareExchange<EventHandler<ConnectionStateChangedEventArgs>>(ref this.ConnectionStateChanged, handler3, source);
                    if (ReferenceEquals(connectionStateChanged, source))
                    {
                        return;
                    }
                }
            }
        }

        internal event EventHandler<AdsStateChangedEventArgs2> DeviceStateChanged
        {
            [CompilerGenerated] add
            {
                EventHandler<AdsStateChangedEventArgs2> deviceStateChanged = this.DeviceStateChanged;
                while (true)
                {
                    EventHandler<AdsStateChangedEventArgs2> a = deviceStateChanged;
                    EventHandler<AdsStateChangedEventArgs2> handler3 = (EventHandler<AdsStateChangedEventArgs2>) Delegate.Combine(a, value);
                    deviceStateChanged = Interlocked.CompareExchange<EventHandler<AdsStateChangedEventArgs2>>(ref this.DeviceStateChanged, handler3, a);
                    if (ReferenceEquals(deviceStateChanged, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                EventHandler<AdsStateChangedEventArgs2> deviceStateChanged = this.DeviceStateChanged;
                while (true)
                {
                    EventHandler<AdsStateChangedEventArgs2> source = deviceStateChanged;
                    EventHandler<AdsStateChangedEventArgs2> handler3 = (EventHandler<AdsStateChangedEventArgs2>) Delegate.Remove(source, value);
                    deviceStateChanged = Interlocked.CompareExchange<EventHandler<AdsStateChangedEventArgs2>>(ref this.DeviceStateChanged, handler3, source);
                    if (ReferenceEquals(deviceStateChanged, source))
                    {
                        return;
                    }
                }
            }
        }

        internal AdsConnection(AdsSession session)
        {
            if (session == null)
            {
                throw new ArgumentNullException("session");
            }
            this._session = session;
            this._interceptors = this.createInterceptors();
            this._client = new TcAdsClient(session);
            this._client.SetCommunicationInterceptor(this._interceptors);
            this._client.Synchronize = session.Settings.Synchronized;
            this._client.Timeout = session.Settings.Timeout;
            this.createResurrectionHandler();
            ((ConnectionStateInterceptor) this.ConnectionObserver).AdsStateChanged += new EventHandler<AdsStateChangedEventArgs2>(this.ConnectionObserver_AdsStateChanged);
            this.ConnectionObserver.ConnectionStateChanged += new EventHandler<ConnectionStateChangedEventArgs>(this.ConnectionObserver_ConnectionStateChanged);
            this.OnConnect();
            this._connectionEstablishTime = new DateTime?(DateTime.UtcNow);
        }

        public int AddDeviceNotification(string variableName, AdsStream dataStream, AdsTransMode transMode, int cycleTime, int maxDelay, object userData)
        {
            int num;
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            this.BeforeAccess();
            try
            {
                num = this._client.AddDeviceNotification(variableName, dataStream, transMode, cycleTime, maxDelay, userData);
            }
            finally
            {
                this.AfterAccess();
            }
            return num;
        }

        public int AddDeviceNotification(uint indexGroup, uint indexOffset, AdsStream dataStream, AdsTransMode transMode, int cycleTime, int maxDelay, object userData)
        {
            int num;
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            this.BeforeAccess();
            try
            {
                num = this._client.AddDeviceNotification(indexGroup, indexOffset, dataStream, transMode, cycleTime, maxDelay, userData);
            }
            finally
            {
                this.AfterAccess();
            }
            return num;
        }

        public int AddDeviceNotification(string variableName, AdsStream dataStream, int offset, int length, AdsTransMode transMode, int cycleTime, int maxDelay, object userData)
        {
            int num;
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            this.BeforeAccess();
            try
            {
                num = this._client.AddDeviceNotification(variableName, dataStream, offset, length, transMode, cycleTime, maxDelay, userData);
            }
            finally
            {
                this.AfterAccess();
            }
            return num;
        }

        public int AddDeviceNotification(uint indexGroup, uint indexOffset, AdsStream dataStream, int offset, int length, AdsTransMode transMode, int cycleTime, int maxDelay, object userData)
        {
            int num;
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            this.BeforeAccess();
            try
            {
                num = this._client.AddDeviceNotification(indexGroup, indexOffset, dataStream, offset, length, transMode, cycleTime, maxDelay, userData);
            }
            finally
            {
                this.AfterAccess();
            }
            return num;
        }

        public int AddDeviceNotificationEx(string variableName, AdsTransMode transMode, int cycleTime, int maxDelay, object userData, Type type)
        {
            int num;
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            this.BeforeAccess();
            try
            {
                num = this._client.AddDeviceNotificationEx(variableName, transMode, cycleTime, maxDelay, userData, type);
            }
            finally
            {
                this.AfterAccess();
            }
            return num;
        }

        public int AddDeviceNotificationEx(string variableName, AdsTransMode transMode, int cycleTime, int maxDelay, object userData, Type type, int[] args)
        {
            int num;
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            this.BeforeAccess();
            try
            {
                num = this._client.AddDeviceNotificationEx(variableName, transMode, cycleTime, maxDelay, userData, type, args);
            }
            finally
            {
                this.AfterAccess();
            }
            return num;
        }

        public int AddDeviceNotificationEx(uint indexGroup, uint indexOffset, AdsTransMode transMode, int cycleTime, int maxDelay, object userData, Type type)
        {
            int num;
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            this.BeforeAccess();
            try
            {
                num = this._client.AddDeviceNotificationEx(indexGroup, indexOffset, transMode, cycleTime, maxDelay, userData, type);
            }
            finally
            {
                this.AfterAccess();
            }
            return num;
        }

        public int AddDeviceNotificationEx(uint indexGroup, uint indexOffset, AdsTransMode transMode, int cycleTime, int maxDelay, object userData, Type type, int[] args)
        {
            int num;
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            this.BeforeAccess();
            try
            {
                num = this._client.AddDeviceNotificationEx(indexGroup, indexOffset, transMode, cycleTime, maxDelay, userData, type, args);
            }
            finally
            {
                this.AfterAccess();
            }
            return num;
        }

        private void AfterAccess()
        {
            if (this._afterAccessDelegate != null)
            {
                this._afterAccessDelegate();
            }
        }

        private void BeforeAccess()
        {
            if (this._beforeAccessDelegate != null)
            {
                this._beforeAccessDelegate();
            }
        }

        public void Close()
        {
            this.Dispose();
        }

        public bool Connect()
        {
            if (this.IsConnected)
            {
                return false;
            }
            this.OnConnect();
            return true;
        }

        private void ConnectionObserver_AdsStateChanged(object sender, AdsStateChangedEventArgs2 e)
        {
            if (!this._disposed)
            {
                this.OnAdsStateChanged(e);
            }
        }

        private void ConnectionObserver_ConnectionStateChanged(object sender, ConnectionStateChangedEventArgs e)
        {
            if (!this._disposed)
            {
                this._connectionState = e.NewState;
                if (e.NewState == TwinCAT.ConnectionState.Connected)
                {
                    this.OnConnected();
                }
                else if (e.NewState == TwinCAT.ConnectionState.Disconnected)
                {
                    this.OnDisconnected();
                }
                else if (e.NewState == TwinCAT.ConnectionState.Lost)
                {
                    this.OnLost();
                }
                this.OnConnectionStatusChanged(e);
            }
        }

        private CommunicationInterceptors createInterceptors() => 
            ((CommunicationInterceptors) ((IInterceptionFactory) this._session).CreateInterceptor());

        private void createResurrectionHandler()
        {
            this._beforeAccessDelegate = delegate {
                if ((this._client != null) && (this._connectionState == TwinCAT.ConnectionState.Lost))
                {
                    TimeSpan? nullable1;
                    TimeSpan resurrectionTime = this._session.Settings.ResurrectionTime;
                    DateTime utcNow = DateTime.UtcNow;
                    DateTime? nullable2 = this._lostTime;
                    if (nullable2 != null)
                    {
                        nullable1 = new TimeSpan?(((TimeSpan) utcNow) - nullable2.GetValueOrDefault());
                    }
                    else
                    {
                        nullable1 = null;
                    }
                    TimeSpan? nullable = nullable1;
                    if ((nullable != null) ? ((Action) (resurrectionTime < nullable.GetValueOrDefault())) : ((Action) false))
                    {
                        this.OnResurrect();
                    }
                }
            };
            this._afterAccessDelegate = null;
        }

        public IAdsSymbolLoader CreateSymbolLoader(ISession session, SymbolLoaderSettings settings)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            IAdsSymbolLoader loader = null;
            this.BeforeAccess();
            try
            {
                loader = this._client.CreateSymbolLoader(this.Session, settings);
            }
            finally
            {
                this.AfterAccess();
            }
            return loader;
        }

        public int CreateVariableHandle(string variableName)
        {
            int num;
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            this.BeforeAccess();
            try
            {
                num = this._client.CreateVariableHandle(variableName);
            }
            finally
            {
                this.AfterAccess();
            }
            return num;
        }

        public void DeleteDeviceNotification(int notificationHandle)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            this.BeforeAccess();
            try
            {
                this._client.DeleteDeviceNotification(notificationHandle);
            }
            finally
            {
                this.AfterAccess();
            }
        }

        public void DeleteVariableHandle(int variableHandle)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            this.BeforeAccess();
            try
            {
                this._client.DeleteVariableHandle(variableHandle);
            }
            finally
            {
                this.AfterAccess();
            }
        }

        public bool Disconnect()
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            if (this._client == null)
            {
                return false;
            }
            bool flag = this._client.Disconnect();
            this._lostTime = new DateTime?(DateTime.UtcNow);
            return flag;
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (!this._disposed)
            {
                if (disposing)
                {
                    if (this._client != null)
                    {
                        this._client.Dispose();
                        this._client = null;
                    }
                    this._lostTime = new DateTime?(DateTime.UtcNow);
                }
                this._disposed = true;
            }
        }

        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never), Browsable(false)]
        public AdsErrorCode InjectError(AdsErrorCode error)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            return this.Client.InjectError(error);
        }

        public object InvokeRpcMethod(string symbolPath, int methodId, object[] parameters)
        {
            object obj2;
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            this.BeforeAccess();
            try
            {
                obj2 = this._client.InvokeRpcMethod(symbolPath, methodId, parameters);
            }
            finally
            {
                this.AfterAccess();
            }
            return obj2;
        }

        public object InvokeRpcMethod(string symbolPath, string methodName, object[] parameters)
        {
            object obj2;
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            this.BeforeAccess();
            try
            {
                obj2 = this._client.InvokeRpcMethod(symbolPath, methodName, parameters);
            }
            finally
            {
                this.AfterAccess();
            }
            return obj2;
        }

        public object InvokeRpcMethod(ITcAdsSymbol symbol, int methodId, object[] parameters)
        {
            object obj2;
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            this.BeforeAccess();
            try
            {
                obj2 = this._client.InvokeRpcMethod(symbol, methodId, parameters);
            }
            finally
            {
                this.AfterAccess();
            }
            return obj2;
        }

        public object InvokeRpcMethod(ITcAdsSymbol symbol, string methodName, object[] parameters)
        {
            object obj2;
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            this.BeforeAccess();
            try
            {
                obj2 = this._client.InvokeRpcMethod(symbol, methodName, parameters);
            }
            finally
            {
                this.AfterAccess();
            }
            return obj2;
        }

        private void OnAdsStateChanged(AdsStateChangedEventArgs2 e)
        {
            if (!this._disposed && (this.DeviceStateChanged != null))
            {
                this.DeviceStateChanged(this, e);
            }
        }

        private void OnConnect()
        {
            this._client.Connect(this._session.Address);
            this._connectionActiveSince = new DateTime?(DateTime.UtcNow);
            this._lostTime = null;
        }

        private void OnConnected()
        {
            Module.Trace.TraceInformation("Connection established");
            this._lostTime = null;
        }

        private void OnConnectionStatusChanged(ConnectionStateChangedEventArgs args)
        {
            if (!this._disposed && (this.ConnectionStateChanged != null))
            {
                SessionConnectionStateChangedEventArgs e = new SessionConnectionStateChangedEventArgs(args.Reason, args.NewState, args.OldState, this.Session, this, args.Exception);
                this.ConnectionStateChanged(this, e);
            }
        }

        private void OnDisconnected()
        {
            Module.Trace.TraceInformation("Connection closed");
            this._lostTime = null;
            this._connectionActiveSince = null;
            this._connectionEstablishTime = null;
        }

        private void OnLost()
        {
            Module.TraceSession.TraceInformation("Connection lost");
            this._lostTime = new DateTime?(DateTime.UtcNow);
            this._connectionActiveSince = null;
            this._connectionLostCount++;
        }

        private void OnResurrect()
        {
            Module.Trace.TraceInformation("Resurrect");
            AdsException error = null;
            bool flag = this.TryResurrect(out error);
        }

        public int Read(int variableHandle, AdsStream dataStream)
        {
            int num;
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            this.BeforeAccess();
            try
            {
                num = this._client.Read(variableHandle, dataStream);
            }
            finally
            {
                this.AfterAccess();
            }
            return num;
        }

        public int Read(uint indexGroup, uint indexOffset, AdsStream dataStream)
        {
            int num;
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            this.BeforeAccess();
            try
            {
                num = this._client.Read(indexGroup, indexOffset, dataStream);
            }
            finally
            {
                this.AfterAccess();
            }
            return num;
        }

        public int Read(int variableHandle, AdsStream dataStream, int offset, int length)
        {
            int num;
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            this.BeforeAccess();
            try
            {
                num = this._client.Read(variableHandle, dataStream, offset, length);
            }
            finally
            {
                this.AfterAccess();
            }
            return num;
        }

        public int Read(uint indexGroup, uint indexOffset, AdsStream dataStream, int offset, int length)
        {
            int num;
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            this.BeforeAccess();
            try
            {
                num = this._client.Read(indexGroup, indexOffset, dataStream, offset, length);
            }
            finally
            {
                this.AfterAccess();
            }
            return num;
        }

        public int Read(uint indexGroup, uint indexOffset, byte[] readBuffer, int offset, int length)
        {
            int num;
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            this.BeforeAccess();
            try
            {
                num = this._client.Read(indexGroup, indexOffset, readBuffer, offset, length);
            }
            finally
            {
                this.AfterAccess();
            }
            return num;
        }

        public int Read(uint indexGroup, uint indexOffset, AdsStream dataStream, int offset, int length, int timeout)
        {
            using (new AdsTimeoutSetter(this, timeout))
            {
                return this.Read(indexGroup, indexOffset, dataStream, offset, length);
            }
        }

        public int Read(uint indexGroup, uint indexOffset, byte[] readBuffer, int offset, int length, int timeout)
        {
            using (new AdsTimeoutSetter(this, timeout))
            {
                return this.Read(indexGroup, indexOffset, readBuffer, offset, length);
            }
        }

        public object ReadAny(int variableHandle, Type type)
        {
            object obj2;
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            this.BeforeAccess();
            try
            {
                obj2 = this._client.ReadAny(variableHandle, type);
            }
            finally
            {
                this.AfterAccess();
            }
            return obj2;
        }

        public object ReadAny(int variableHandle, Type type, int[] args)
        {
            object obj2;
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            this.BeforeAccess();
            try
            {
                obj2 = this._client.ReadAny(variableHandle, type, args);
            }
            finally
            {
                this.AfterAccess();
            }
            return obj2;
        }

        public object ReadAny(uint indexGroup, uint indexOffset, Type type)
        {
            object obj2;
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            this.BeforeAccess();
            try
            {
                obj2 = this._client.ReadAny(indexGroup, indexOffset, type);
            }
            finally
            {
                this.AfterAccess();
            }
            return obj2;
        }

        public object ReadAny(uint indexGroup, uint indexOffset, Type type, int[] args)
        {
            object obj2;
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            this.BeforeAccess();
            try
            {
                obj2 = this._client.ReadAny(indexGroup, indexOffset, type, args);
            }
            finally
            {
                this.AfterAccess();
            }
            return obj2;
        }

        public object ReadAny(uint indexGroup, uint indexOffset, Type type, int[] args, int timeout)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            using (new AdsTimeoutSetter(this, timeout))
            {
                return this.ReadAny(indexGroup, indexOffset, type, args);
            }
        }

        public string ReadAnyString(int variableHandle, int len, Encoding encoding)
        {
            string str;
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            this.BeforeAccess();
            try
            {
                str = this._client.ReadAnyString(variableHandle, len, encoding);
            }
            finally
            {
                this.AfterAccess();
            }
            return str;
        }

        public string ReadAnyString(uint indexGroup, uint indexOffset, int len, Encoding encoding)
        {
            string str;
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            this.BeforeAccess();
            try
            {
                str = this._client.ReadAnyString(indexGroup, indexOffset, len, encoding);
            }
            finally
            {
                this.AfterAccess();
            }
            return str;
        }

        public DeviceInfo ReadDeviceInfo()
        {
            DeviceInfo info;
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            this.BeforeAccess();
            try
            {
                info = this._client.ReadDeviceInfo();
            }
            finally
            {
                this.AfterAccess();
            }
            return info;
        }

        public StateInfo ReadState()
        {
            StateInfo info;
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            this.BeforeAccess();
            try
            {
                info = this._client.ReadState();
            }
            finally
            {
                this.AfterAccess();
            }
            return info;
        }

        public StateInfo ReadState(int timeout)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            using (new AdsTimeoutSetter(this, timeout))
            {
                return this.ReadState();
            }
        }

        public object ReadSymbol(ITcAdsSymbol symbol)
        {
            object obj2;
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            this.BeforeAccess();
            try
            {
                obj2 = this._client.ReadSymbol(symbol);
            }
            finally
            {
                this.AfterAccess();
            }
            return obj2;
        }

        public object ReadSymbol(string name, Type type, bool reloadSymbolInfo)
        {
            object obj2;
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            this.BeforeAccess();
            try
            {
                obj2 = this._client.ReadSymbol(name, type, reloadSymbolInfo);
            }
            finally
            {
                this.AfterAccess();
            }
            return obj2;
        }

        public ITcAdsSymbol ReadSymbolInfo(string name)
        {
            ITcAdsSymbol symbol;
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            this.BeforeAccess();
            try
            {
                symbol = this._client.ReadSymbolInfo(name);
            }
            finally
            {
                this.AfterAccess();
            }
            return symbol;
        }

        public int ReadWrite(uint indexGroup, uint indexOffset, AdsStream rdDataStream, AdsStream wrDataStream)
        {
            int num;
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            this.BeforeAccess();
            try
            {
                num = this._client.ReadWrite(indexGroup, indexOffset, rdDataStream, wrDataStream);
            }
            finally
            {
                this.AfterAccess();
            }
            return num;
        }

        public int ReadWrite(uint indexGroup, uint indexOffset, AdsStream rdDataStream, AdsStream wrDataStream, int timeout)
        {
            using (new AdsTimeoutSetter(this, timeout))
            {
                return this.ReadWrite(indexGroup, indexOffset, rdDataStream, wrDataStream);
            }
        }

        public int ReadWrite(int variableHandle, AdsStream rdDataStream, int rdOffset, int rdLength, AdsStream wrDataStream, int wrOffset, int wrLength)
        {
            int num;
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            this.BeforeAccess();
            try
            {
                num = this._client.ReadWrite(variableHandle, rdDataStream, rdOffset, rdLength, wrDataStream, wrOffset, wrLength);
            }
            finally
            {
                this.AfterAccess();
            }
            return num;
        }

        public int ReadWrite(uint indexGroup, uint indexOffset, AdsStream rdDataStream, int rdOffset, int rdLength, AdsStream wrDataStream, int wrOffset, int wrLength)
        {
            int num;
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            this.BeforeAccess();
            try
            {
                num = this._client.ReadWrite(indexGroup, indexOffset, rdDataStream, rdOffset, rdLength, wrDataStream, wrOffset, wrLength);
            }
            finally
            {
                this.AfterAccess();
            }
            return num;
        }

        public int ReadWrite(uint indexGroup, uint indexOffset, byte[] readBuffer, int rdOffset, int rdLength, byte[] writeBuffer, int wrOffset, int wrLength)
        {
            int num;
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            this.BeforeAccess();
            try
            {
                num = this._client.ReadWrite(indexGroup, indexOffset, readBuffer, rdOffset, rdLength, writeBuffer, wrOffset, wrLength);
            }
            finally
            {
                this.AfterAccess();
            }
            return num;
        }

        public int ReadWrite(uint indexGroup, uint indexOffset, AdsStream rdDataStream, int rdOffset, int rdLength, AdsStream wrDataStream, int wrOffset, int wrLength, int timeout)
        {
            using (new AdsTimeoutSetter(this, timeout))
            {
                return this.ReadWrite(indexGroup, indexOffset, rdDataStream, rdOffset, rdLength, wrDataStream, wrOffset, wrLength);
            }
        }

        public int ReadWrite(uint indexGroup, uint indexOffset, byte[] readBuffer, int rdOffset, int rdLength, byte[] writeBuffer, int wrOffset, int wrLength, int timeout)
        {
            using (new AdsTimeoutSetter(this, timeout))
            {
                return this.ReadWrite(indexGroup, indexOffset, readBuffer, rdOffset, rdLength, writeBuffer, wrOffset, wrLength);
            }
        }

        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        public void Resurrect()
        {
            AdsException exception;
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.ToString());
            }
            if (!this.TryResurrect(out exception))
            {
                throw new AdsException();
            }
        }

        public AdsErrorCode TryAddDeviceNotification(string variableName, AdsStream dataStream, int offset, int length, NotificationSettings settings, object userData, out uint handle)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            AdsErrorCode noError = AdsErrorCode.NoError;
            this.BeforeAccess();
            try
            {
                noError = this._client.TryAddDeviceNotification(variableName, dataStream, offset, length, settings, userData, out handle);
            }
            finally
            {
                this.AfterAccess();
            }
            return noError;
        }

        public AdsErrorCode TryAddDeviceNotificationEx(string variableName, NotificationSettings settings, object userData, Type type, int[] args, out uint handle)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            AdsErrorCode noError = AdsErrorCode.NoError;
            this.BeforeAccess();
            try
            {
                noError = this._client.TryAddDeviceNotificationEx(variableName, settings, userData, type, args, out handle);
            }
            finally
            {
                this.AfterAccess();
            }
            return noError;
        }

        public AdsErrorCode TryDeleteDeviceNotification(uint notificationHandle)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            AdsErrorCode noError = AdsErrorCode.NoError;
            this.BeforeAccess();
            try
            {
                noError = this._client.TryDeleteDeviceNotification(notificationHandle);
            }
            finally
            {
                this.AfterAccess();
            }
            return noError;
        }

        public AdsErrorCode TryInvokeRpcMethod(string symbolPath, int methodId, object[] parameters, out object retValue)
        {
            AdsErrorCode code;
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            this.BeforeAccess();
            try
            {
                code = this._client.TryInvokeRpcMethod(symbolPath, methodId, parameters, out retValue);
            }
            finally
            {
                this.AfterAccess();
            }
            return code;
        }

        public AdsErrorCode TryInvokeRpcMethod(string symbolPath, string methodName, object[] parameters, out object retValue)
        {
            AdsErrorCode code;
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            this.BeforeAccess();
            try
            {
                code = this._client.TryInvokeRpcMethod(symbolPath, methodName, parameters, out retValue);
            }
            finally
            {
                this.AfterAccess();
            }
            return code;
        }

        public AdsErrorCode TryInvokeRpcMethod(ITcAdsSymbol symbol, int methodId, object[] parameters, out object retValue)
        {
            AdsErrorCode code;
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            this.BeforeAccess();
            try
            {
                code = this._client.TryInvokeRpcMethod(symbol, methodId, parameters, out retValue);
            }
            finally
            {
                this.AfterAccess();
            }
            return code;
        }

        public AdsErrorCode TryInvokeRpcMethod(ITcAdsSymbol symbol, string methodName, object[] parameters, out object retValue)
        {
            AdsErrorCode code;
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            this.BeforeAccess();
            try
            {
                code = this._client.TryInvokeRpcMethod(symbol, methodName, parameters, out retValue);
            }
            finally
            {
                this.AfterAccess();
            }
            return code;
        }

        public AdsErrorCode TryRead(int variableHandle, AdsStream dataStream, int offset, int length, out int readBytes)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            AdsErrorCode noError = AdsErrorCode.NoError;
            this.BeforeAccess();
            try
            {
                noError = this._client.TryRead(variableHandle, dataStream, offset, length, out readBytes);
            }
            finally
            {
                this.AfterAccess();
            }
            return noError;
        }

        public AdsErrorCode TryRead(uint indexGroup, uint indexOffset, AdsStream dataStream, int offset, int length, out int readBytes)
        {
            AdsErrorCode code;
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            this.BeforeAccess();
            try
            {
                code = this._client.TryRead(indexGroup, indexOffset, dataStream, offset, length, out readBytes);
            }
            finally
            {
                this.AfterAccess();
            }
            return code;
        }

        public AdsErrorCode TryRead(uint indexGroup, uint indexOffset, byte[] readBuffer, int offset, int length, out int readBytes)
        {
            AdsErrorCode code;
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            this.BeforeAccess();
            try
            {
                code = this._client.TryRead(indexGroup, indexOffset, readBuffer, offset, length, out readBytes);
            }
            finally
            {
                this.AfterAccess();
            }
            return code;
        }

        public AdsErrorCode TryRead(uint indexGroup, uint indexOffset, AdsStream dataStream, int offset, int length, int timeout, out int readBytes)
        {
            using (new AdsTimeoutSetter(this, timeout))
            {
                return this.TryRead(indexGroup, indexOffset, dataStream, offset, length, out readBytes);
            }
        }

        public AdsErrorCode TryRead(uint indexGroup, uint indexOffset, byte[] readBuffer, int offset, int length, int timeout, out int readBytes)
        {
            using (new AdsTimeoutSetter(this, timeout))
            {
                return this.TryRead(indexGroup, indexOffset, readBuffer, offset, length, out readBytes);
            }
        }

        public AdsErrorCode TryReadState(out StateInfo stateInfo)
        {
            AdsErrorCode code;
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            this.BeforeAccess();
            try
            {
                code = this._client.TryReadState(out stateInfo);
            }
            finally
            {
                this.AfterAccess();
            }
            return code;
        }

        public AdsErrorCode TryReadState(int timeout, out StateInfo stateInfo)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            using (new AdsTimeoutSetter(this, timeout))
            {
                return this.TryReadState(out stateInfo);
            }
        }

        public AdsErrorCode TryReadWrite(int variableHandle, AdsStream rdDataStream, int rdOffset, int rdLength, AdsStream wrDataStream, int wrOffset, int wrLength, out int readBytes)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            AdsErrorCode noError = AdsErrorCode.NoError;
            this.BeforeAccess();
            try
            {
                noError = this._client.TryReadWrite(variableHandle, rdDataStream, rdOffset, rdLength, wrDataStream, wrOffset, wrLength, out readBytes);
            }
            finally
            {
                this.AfterAccess();
            }
            return noError;
        }

        public AdsErrorCode TryReadWrite(uint indexGroup, uint indexOffset, AdsStream rdDataStream, int rdOffset, int rdLength, AdsStream wrDataStream, int wrOffset, int wrLength, out int readBytes)
        {
            AdsErrorCode code;
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            this.BeforeAccess();
            try
            {
                code = this._client.TryReadWrite(indexGroup, indexOffset, rdDataStream, rdOffset, rdLength, wrDataStream, wrOffset, wrLength, out readBytes);
            }
            finally
            {
                this.AfterAccess();
            }
            return code;
        }

        public AdsErrorCode TryReadWrite(uint indexGroup, uint indexOffset, byte[] readBuffer, int rdOffset, int rdLength, byte[] writeBuffer, int wrOffset, int wrLength, out int readBytes)
        {
            AdsErrorCode code;
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            this.BeforeAccess();
            try
            {
                code = this._client.TryReadWrite(indexGroup, indexOffset, readBuffer, rdOffset, rdLength, writeBuffer, wrOffset, wrLength, out readBytes);
            }
            finally
            {
                this.AfterAccess();
            }
            return code;
        }

        public AdsErrorCode TryReadWrite(uint indexGroup, uint indexOffset, AdsStream rdDataStream, int rdOffset, int rdLength, AdsStream wrDataStream, int wrOffset, int wrLength, int timeout, out int readBytes)
        {
            using (new AdsTimeoutSetter(this, timeout))
            {
                return this.TryReadWrite(indexGroup, indexOffset, rdDataStream, rdOffset, rdLength, wrDataStream, wrOffset, wrLength, out readBytes);
            }
        }

        public AdsErrorCode TryReadWrite(uint indexGroup, uint indexOffset, byte[] readBuffer, int rdOffset, int rdLength, byte[] writeBuffer, int wrOffset, int wrLength, int timeout, out int readBytes)
        {
            using (new AdsTimeoutSetter(this, timeout))
            {
                return this.TryReadWrite(indexGroup, indexOffset, readBuffer, rdOffset, rdLength, writeBuffer, wrOffset, wrLength, out readBytes);
            }
        }

        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        public bool TryResurrect(out AdsException error)
        {
            bool flag;
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.ToString());
            }
            try
            {
                this._resurrecting = true;
                this._resurrectingTryCount++;
                flag = this._client.TryResurrect(out error);
                if (!flag)
                {
                    Module.TraceSession.TraceError("Resurrection failed!", error);
                    this.OnLost();
                }
                this._resurrections++;
            }
            finally
            {
                this._resurrecting = false;
            }
            return flag;
        }

        public AdsErrorCode TryWrite(int variableHandle, AdsStream dataStream, int offset, int length)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            AdsErrorCode noError = AdsErrorCode.NoError;
            this.BeforeAccess();
            try
            {
                noError = this._client.TryWrite(variableHandle, dataStream, offset, length);
            }
            finally
            {
                this.AfterAccess();
            }
            return noError;
        }

        public AdsErrorCode TryWrite(uint indexGroup, uint indexOffset, AdsStream dataStream, int offset, int length)
        {
            AdsErrorCode code;
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            this.BeforeAccess();
            try
            {
                code = this._client.TryWrite(indexGroup, indexOffset, dataStream, offset, length);
            }
            finally
            {
                this.AfterAccess();
            }
            return code;
        }

        public AdsErrorCode TryWrite(uint indexGroup, uint indexOffset, byte[] writeBuffer, int offset, int length)
        {
            AdsErrorCode code;
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            this.BeforeAccess();
            try
            {
                code = this._client.TryWrite(indexGroup, indexOffset, writeBuffer, offset, length);
            }
            finally
            {
                this.AfterAccess();
            }
            return code;
        }

        public AdsErrorCode TryWrite(uint indexGroup, uint indexOffset, AdsStream dataStream, int offset, int length, int timeout)
        {
            using (new AdsTimeoutSetter(this, timeout))
            {
                return this.TryWrite(indexGroup, indexOffset, dataStream, offset, length);
            }
        }

        public AdsErrorCode TryWrite(uint indexGroup, uint indexOffset, byte[] writeStream, int offset, int length, int timeout)
        {
            using (new AdsTimeoutSetter(this, timeout))
            {
                return this.TryWrite(indexGroup, indexOffset, writeStream, offset, length);
            }
        }

        public AdsErrorCode TryWriteControl(StateInfo stateInfo)
        {
            AdsErrorCode code;
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            this.BeforeAccess();
            try
            {
                code = this._client.TryWriteControl(stateInfo);
            }
            finally
            {
                this.AfterAccess();
            }
            return code;
        }

        public AdsErrorCode TryWriteControl(StateInfo stateInfo, int timeout)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            using (new AdsTimeoutSetter(this, timeout))
            {
                return this.TryWriteControl(stateInfo);
            }
        }

        public AdsErrorCode TryWriteControl(StateInfo stateInfo, AdsStream dataStream, int offset, int length)
        {
            AdsErrorCode code;
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            this.BeforeAccess();
            try
            {
                code = this._client.TryWriteControl(stateInfo, dataStream, offset, length);
            }
            finally
            {
                this.AfterAccess();
            }
            return code;
        }

        public AdsErrorCode TryWriteControl(StateInfo stateInfo, AdsStream dataStream, int offset, int length, int timeout)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            using (new AdsTimeoutSetter(this, timeout))
            {
                return this.TryWriteControl(stateInfo, dataStream, offset, length);
            }
        }

        public void Write(int variableHandle, AdsStream dataStream)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            this.BeforeAccess();
            try
            {
                this._client.Write(variableHandle, dataStream);
            }
            finally
            {
                this.AfterAccess();
            }
        }

        public void Write(uint indexGroup, uint indexOffset)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            this.BeforeAccess();
            try
            {
                this._client.Write(indexGroup, indexOffset);
            }
            finally
            {
                this.AfterAccess();
            }
        }

        public void Write(uint indexGroup, uint indexOffset, int timeout)
        {
            using (new AdsTimeoutSetter(this, timeout))
            {
                this.Write(indexGroup, indexOffset);
            }
        }

        public void Write(uint indexGroup, uint indexOffset, AdsStream dataStream)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            this.BeforeAccess();
            try
            {
                this._client.Write(indexGroup, indexOffset, dataStream);
            }
            finally
            {
                this.AfterAccess();
            }
        }

        public void Write(int variableHandle, AdsStream dataStream, int offset, int length)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            this.BeforeAccess();
            try
            {
                this._client.Write(variableHandle, dataStream, offset, length);
            }
            finally
            {
                this.AfterAccess();
            }
        }

        public void Write(uint indexGroup, uint indexOffset, AdsStream dataStream, int timeout)
        {
            using (new AdsTimeoutSetter(this, timeout))
            {
                this.Write(indexGroup, indexOffset, dataStream);
            }
        }

        public void Write(uint indexGroup, uint indexOffset, AdsStream dataStream, int offset, int length)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            this.BeforeAccess();
            try
            {
                this._client.Write(indexGroup, indexOffset, dataStream, offset, length);
            }
            finally
            {
                this.AfterAccess();
            }
        }

        public void Write(uint indexGroup, uint indexOffset, byte[] writeBuffer, int offset, int length)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            this.BeforeAccess();
            try
            {
                this._client.Write(indexGroup, indexOffset, writeBuffer, offset, length);
            }
            finally
            {
                this.AfterAccess();
            }
        }

        public void Write(uint indexGroup, uint indexOffset, AdsStream dataStream, int offset, int length, int timeout)
        {
            using (new AdsTimeoutSetter(this, timeout))
            {
                this.Write(indexGroup, indexOffset, dataStream, offset, length);
            }
        }

        public void Write(uint indexGroup, uint indexOffset, byte[] writeBuffer, int offset, int length, int timeout)
        {
            using (new AdsTimeoutSetter(this, timeout))
            {
                this.Write(indexGroup, indexOffset, writeBuffer, offset, length);
            }
        }

        public void WriteAny(int variableHandle, object value)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            this.BeforeAccess();
            try
            {
                this._client.WriteAny(variableHandle, value);
            }
            finally
            {
                this.AfterAccess();
            }
        }

        public void WriteAny(int variableHandle, object value, int[] args)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            this.BeforeAccess();
            try
            {
                this._client.WriteAny(variableHandle, value, args);
            }
            finally
            {
                this.AfterAccess();
            }
        }

        public void WriteAny(uint indexGroup, uint indexOffset, object value)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            this.BeforeAccess();
            try
            {
                this._client.WriteAny(indexGroup, indexOffset, value);
            }
            finally
            {
                this.AfterAccess();
            }
        }

        public void WriteAny(uint indexGroup, uint indexOffset, object value, int[] args)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            this.BeforeAccess();
            try
            {
                this._client.WriteAny(indexGroup, indexOffset, value, args);
            }
            finally
            {
                this.AfterAccess();
            }
        }

        public void WriteAny(uint indexGroup, uint indexOffset, object value, int[] args, int timeout)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            using (new AdsTimeoutSetter(this, timeout))
            {
                this.WriteAny(indexGroup, indexOffset, value, args);
            }
        }

        [Obsolete("This method is potentially unsafe!"), EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        public void WriteAnyString(int variableHandle, string value, int length, Encoding encoding)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            this.BeforeAccess();
            try
            {
                this._client.WriteAnyString(variableHandle, value, length, encoding);
            }
            finally
            {
                this.AfterAccess();
            }
        }

        [Obsolete("This method is potentially unsafe!"), EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        public void WriteAnyString(uint indexGroup, uint indexOffset, string value, int length, Encoding encoding)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            this.BeforeAccess();
            try
            {
                this._client.WriteAnyString(indexGroup, indexOffset, value, length, encoding);
            }
            finally
            {
                this.AfterAccess();
            }
        }

        public void WriteControl(StateInfo stateInfo)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            this.BeforeAccess();
            try
            {
                this._client.WriteControl(stateInfo);
            }
            finally
            {
                this.AfterAccess();
            }
        }

        public void WriteControl(StateInfo stateInfo, int timeout)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            using (new AdsTimeoutSetter(this, timeout))
            {
                this.WriteControl(stateInfo);
            }
        }

        public void WriteControl(StateInfo stateInfo, AdsStream dataStream, int offset, int length)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            this.BeforeAccess();
            try
            {
                this._client.WriteControl(stateInfo, dataStream, offset, length);
            }
            finally
            {
                this.AfterAccess();
            }
        }

        public void WriteControl(StateInfo stateInfo, AdsStream dataStream, int offset, int length, int timeout)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            using (new AdsTimeoutSetter(this, timeout))
            {
                this.WriteControl(stateInfo, dataStream, offset, length);
            }
        }

        public void WriteSymbol(ITcAdsSymbol symbol, object val)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            this.BeforeAccess();
            try
            {
                this._client.WriteSymbol(symbol, val);
            }
            finally
            {
                this.AfterAccess();
            }
        }

        public void WriteSymbol(string name, object value, bool reloadSymbolInfo)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            this.BeforeAccess();
            try
            {
                this._client.WriteSymbol(name, value, reloadSymbolInfo);
            }
            finally
            {
                this.AfterAccess();
            }
        }

        public ISession Session =>
            this._session;

        public int Id =>
            this._id;

        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        public TcAdsClient Client
        {
            get
            {
                this.BeforeAccess();
                return this._client;
            }
        }

        public TimeSpan AccessWaitTime
        {
            get
            {
                if (this._disposed)
                {
                    return TimeSpan.MaxValue;
                }
                TimeSpan zero = TimeSpan.Zero;
                if (this._lostTime != null)
                {
                    zero = this._session.Settings.ResurrectionTime - (DateTime.UtcNow - this._lostTime.Value);
                    if (zero < TimeSpan.Zero)
                    {
                        zero = TimeSpan.Zero;
                    }
                }
                return zero;
            }
        }

        public TwinCAT.ConnectionState State =>
            this._connectionState;

        public TwinCAT.ConnectionState ConnectionState
        {
            get
            {
                if (this._disposed)
                {
                    return TwinCAT.ConnectionState.None;
                }
                IConnectionStateObserver connectionObserver = this.ConnectionObserver;
                return ((connectionObserver != null) ? connectionObserver.ConnectionState : TwinCAT.ConnectionState.None);
            }
        }

        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        internal IConnectionStateObserver ConnectionObserver =>
            (!this._disposed ? ((this._interceptors != null) ? ((IConnectionStateObserver) this._interceptors.Lookup(typeof(ConnectionStateInterceptor))) : null) : null);

        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        public IAdsStateObserver AdsStateObserver =>
            (!this._disposed ? ((IAdsStateObserver) this._interceptors.Lookup(typeof(ConnectionStateInterceptor))) : null);

        public DateTime? ConnectionLostTime =>
            this._lostTime;

        public bool IsLost
        {
            get
            {
                if (this._disposed)
                {
                    return false;
                }
                if (!this.IsConnected)
                {
                    return false;
                }
                return this._interceptors.Find<IFailFastHandler>().IsLost;
            }
        }

        public bool IsActive
        {
            get
            {
                if (this._disposed)
                {
                    return false;
                }
                if (!this.IsConnected)
                {
                    return false;
                }
                return this._interceptors.Find<IFailFastHandler>().IsActive;
            }
        }

        public bool IsReconnecting
        {
            get
            {
                if (this._disposed)
                {
                    return false;
                }
                if (!this.IsConnected)
                {
                    return false;
                }
                return this._interceptors.Find<IFailFastHandler>().IsReconnecting;
            }
        }

        public AmsAddress ClientAddress =>
            (!this._disposed ? this._client?.ClientAddress : null);

        public bool IsConnected =>
            (!this._disposed ? ((this._connectionState == TwinCAT.ConnectionState.Connected) || (this._connectionState == TwinCAT.ConnectionState.Lost)) : false);

        public bool IsLocal =>
            (!this._disposed ? this._session.Address.netId.IsLocal : true);

        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        public ITcAdsRaw RawInterface
        {
            get
            {
                if (this._disposed)
                {
                    return null;
                }
                this.BeforeAccess();
                return this._client.RawInterface;
            }
        }

        public AmsAddress Address =>
            (!this._disposed ? this._session.Address : null);

        public bool Disposed =>
            this._disposed;

        public string Name
        {
            get
            {
                object[] args = new object[] { this._session.Address.ToString(), this._id };
                return string.Format(null, "Connection {0} (ID: {1})", args);
            }
        }

        public DateTime? ConnectionEstablishedAt =>
            this._connectionEstablishTime;

        public DateTime? ActiveSince =>
            this._connectionActiveSince;

        public int ResurrectingTries =>
            this._resurrectingTryCount;

        public int Resurrections =>
            this._resurrections;

        public int ConnectionLostCount =>
            this._connectionLostCount;

        public int Timeout
        {
            get => 
                this._session.Settings.Timeout;
            set
            {
                this._session.Settings.Timeout = value;
                if (this._client != null)
                {
                    this._client.Timeout = value;
                }
            }
        }
    }
}

