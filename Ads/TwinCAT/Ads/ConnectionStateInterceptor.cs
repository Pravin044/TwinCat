namespace TwinCAT.Ads
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using TwinCAT;
    using TwinCAT.Ads.Internal;

    [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
    public class ConnectionStateInterceptor : CommunicationInterceptor, IConnectionStateProvider, IConnectionStateObserver, IAdsStateObserver, IPreventRejected
    {
        private IAdsSession _session;
        private object _synchronizer;
        private TwinCAT.ConnectionState _connectionState;
        private DateTime _lastSucceeded;
        private DateTime _lastAccess;
        private int _errorCount;
        private int _errorCountSinceLastAccess;
        private int _cycleCount;
        [CompilerGenerated]
        private EventHandler<ConnectionStateChangedEventArgs> ConnectionStateChanged;
        [CompilerGenerated]
        private EventHandler<AdsStateChangedEventArgs2> AdsStateChanged;
        private bool _preventRejectedConnection;
        private TwinCAT.Ads.StateInfo _adsState;

        public event EventHandler<AdsStateChangedEventArgs2> AdsStateChanged
        {
            [CompilerGenerated] add
            {
                EventHandler<AdsStateChangedEventArgs2> adsStateChanged = this.AdsStateChanged;
                while (true)
                {
                    EventHandler<AdsStateChangedEventArgs2> a = adsStateChanged;
                    EventHandler<AdsStateChangedEventArgs2> handler3 = (EventHandler<AdsStateChangedEventArgs2>) Delegate.Combine(a, value);
                    adsStateChanged = Interlocked.CompareExchange<EventHandler<AdsStateChangedEventArgs2>>(ref this.AdsStateChanged, handler3, a);
                    if (ReferenceEquals(adsStateChanged, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                EventHandler<AdsStateChangedEventArgs2> adsStateChanged = this.AdsStateChanged;
                while (true)
                {
                    EventHandler<AdsStateChangedEventArgs2> source = adsStateChanged;
                    EventHandler<AdsStateChangedEventArgs2> handler3 = (EventHandler<AdsStateChangedEventArgs2>) Delegate.Remove(source, value);
                    adsStateChanged = Interlocked.CompareExchange<EventHandler<AdsStateChangedEventArgs2>>(ref this.AdsStateChanged, handler3, source);
                    if (ReferenceEquals(adsStateChanged, source))
                    {
                        return;
                    }
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

        internal ConnectionStateInterceptor(IAdsSession session) : base("ConnectionStateObserverInterceptor")
        {
            this._synchronizer = new object();
            this._connectionState = TwinCAT.ConnectionState.Disconnected;
            this._lastSucceeded = DateTime.MinValue;
            this._lastAccess = DateTime.MinValue;
            this._session = session;
        }

        private void OnAdsStateChanged(TwinCAT.Ads.StateInfo oldState, TwinCAT.Ads.StateInfo newState)
        {
            if (this.AdsStateChanged != null)
            {
                this.AdsStateChanged(this, new AdsStateChangedEventArgs2(newState, oldState, this._session));
            }
        }

        protected override void OnAfterCommunicate(AdsErrorCode errorCode)
        {
            if (FailFastHandlerInterceptor.IsTrippingError(errorCode, this._preventRejectedConnection))
            {
                this.OnError(errorCode);
            }
            else
            {
                object obj2 = this._synchronizer;
                lock (obj2)
                {
                    this._lastSucceeded = DateTime.UtcNow;
                    this._cycleCount++;
                    this._errorCountSinceLastAccess = 0;
                    this._lastAccess = this._lastSucceeded;
                }
                this.setState(TwinCAT.ConnectionState.Connected);
            }
            base.OnAfterCommunicate(errorCode);
        }

        protected override void OnAfterConnect(AdsErrorCode error)
        {
            if (error != AdsErrorCode.NoError)
            {
                this.OnError(error);
            }
            else
            {
                object obj2 = this._synchronizer;
                lock (obj2)
                {
                    this._lastSucceeded = DateTime.UtcNow;
                    this._cycleCount++;
                    this._errorCountSinceLastAccess = 0;
                    this._lastAccess = this._lastSucceeded;
                }
                this.setState(TwinCAT.ConnectionState.Connected);
            }
        }

        protected override void OnAfterDisconnect(AdsErrorCode errorCode)
        {
            if (errorCode != AdsErrorCode.NoError)
            {
                this.OnError(errorCode);
            }
            else
            {
                object obj2 = this._synchronizer;
                lock (obj2)
                {
                    this._lastSucceeded = DateTime.UtcNow;
                    this._cycleCount = 0;
                    this._errorCountSinceLastAccess = 0;
                    this._lastAccess = this._lastSucceeded;
                }
                TwinCAT.Ads.StateInfo adsState = new TwinCAT.Ads.StateInfo();
                this.setAdsState(adsState);
                this.setState(TwinCAT.ConnectionState.Disconnected);
            }
        }

        protected override void OnAfterReadState(TwinCAT.Ads.StateInfo adsState, AdsErrorCode result)
        {
            if (result == AdsErrorCode.NoError)
            {
                this.setAdsState(adsState);
            }
        }

        protected override void OnAfterWriteState(TwinCAT.Ads.StateInfo adsState, AdsErrorCode result)
        {
            if (result == AdsErrorCode.NoError)
            {
                this.setAdsState(adsState);
            }
        }

        protected virtual void OnConnectionStatusChanged(TwinCAT.ConnectionState oldState, TwinCAT.ConnectionState newState)
        {
            if (this.ConnectionStateChanged != null)
            {
                ConnectionStateChangedReason error = ConnectionStateChangedReason.Error;
                if ((newState == TwinCAT.ConnectionState.Connected) && ((oldState == TwinCAT.ConnectionState.Disconnected) || (oldState == TwinCAT.ConnectionState.None)))
                {
                    error = ConnectionStateChangedReason.Established;
                }
                else if ((newState == TwinCAT.ConnectionState.Connected) && (oldState == TwinCAT.ConnectionState.Lost))
                {
                    error = ConnectionStateChangedReason.Resurrected;
                }
                else if (newState == TwinCAT.ConnectionState.Lost)
                {
                    error = ConnectionStateChangedReason.Lost;
                }
                else if (newState == TwinCAT.ConnectionState.Disconnected)
                {
                    error = ConnectionStateChangedReason.Closed;
                }
                this.ConnectionStateChanged(this, new SessionConnectionStateChangedEventArgs(error, newState, oldState, this._session, this._session.Connection, null));
            }
        }

        private void OnError(AdsErrorCode error)
        {
            object obj2 = this._synchronizer;
            lock (obj2)
            {
                this._errorCount++;
                this._errorCountSinceLastAccess++;
                this._lastAccess = DateTime.UtcNow;
            }
            this.setState(TwinCAT.ConnectionState.Lost);
            TwinCAT.Ads.StateInfo adsState = new TwinCAT.Ads.StateInfo();
            this.setAdsState(adsState);
        }

        private void setAdsState(TwinCAT.Ads.StateInfo adsState)
        {
            TwinCAT.Ads.StateInfo info = this._adsState;
            if (!adsState.Equals(info))
            {
                object obj2 = this._synchronizer;
                lock (obj2)
                {
                    this._adsState = adsState;
                }
                this.OnAdsStateChanged(info, this._adsState);
            }
        }

        private void setState(TwinCAT.ConnectionState newState)
        {
            TwinCAT.ConnectionState none = TwinCAT.ConnectionState.None;
            object obj2 = this._synchronizer;
            lock (obj2)
            {
                none = this._connectionState;
                this._connectionState = newState;
            }
            if (none != this._connectionState)
            {
                this.OnConnectionStatusChanged(none, newState);
            }
        }

        public DateTime LastSucceededAccess
        {
            get
            {
                object obj2 = this._synchronizer;
                lock (obj2)
                {
                    return this._lastSucceeded;
                }
            }
        }

        public DateTime LastAccess
        {
            get
            {
                object obj2 = this._synchronizer;
                lock (obj2)
                {
                    return this._lastAccess;
                }
            }
        }

        public int TotalErrors
        {
            get
            {
                object obj2 = this._synchronizer;
                lock (obj2)
                {
                    return this._errorCount;
                }
            }
        }

        public int ErrorsSinceLastSucceeded
        {
            get
            {
                object obj2 = this._synchronizer;
                lock (obj2)
                {
                    return this._errorCountSinceLastAccess;
                }
            }
        }

        public TwinCAT.ConnectionState ConnectionState
        {
            get
            {
                object obj2 = this._synchronizer;
                lock (obj2)
                {
                    return this._connectionState;
                }
            }
        }

        public int TotalCycles
        {
            get
            {
                object obj2 = this._synchronizer;
                lock (obj2)
                {
                    return this._cycleCount;
                }
            }
        }

        public TimeSpan Quality
        {
            get
            {
                object obj2 = this._synchronizer;
                lock (obj2)
                {
                    return (TimeSpan) (DateTime.UtcNow - this._lastSucceeded);
                }
            }
        }

        public TwinCAT.Ads.StateInfo StateInfo =>
            this._adsState;

        bool IPreventRejected.PreventRejectedConnection
        {
            get => 
                this._preventRejectedConnection;
            set => 
                (this._preventRejectedConnection = value);
        }
    }
}

