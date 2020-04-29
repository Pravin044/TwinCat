namespace TwinCAT
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using TwinCAT.TypeSystem;

    public abstract class Session : ISession, IConnectionStateProvider, ISymbolServerProvider, IDisposable
    {
        protected ISessionProvider provider;
        private static int s_id;
        private int _id = ++s_id;
        protected IConnection connection;
        private DateTime _sessionEstablishedAt = DateTime.MaxValue;
        private bool _disposed;
        private ISymbolServer _symbolServer;
        [CompilerGenerated]
        private EventHandler<ConnectionStateChangedEventArgs> ConnectionStateChanged;

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

        protected Session(ISessionProvider provider)
        {
            this.provider = provider;
        }

        public void Close()
        {
            this.Dispose();
        }

        public IConnection Connect()
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            if (!this.IsConnected)
            {
                bool reconnect = this.connection != null;
                this.connection = this.OnConnect(reconnect);
            }
            return this.connection;
        }

        private void Connection_ConnectionStateChanged(object sender, ConnectionStateChangedEventArgs e)
        {
            if (this.ConnectionStateChanged != null)
            {
                this.ConnectionStateChanged(this, e);
            }
        }

        public bool Disconnect()
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            bool flag = false;
            if (this.IsConnected)
            {
                flag = this.OnDisconnect();
            }
            return flag;
        }

        public void Dispose()
        {
            if (!this._disposed)
            {
                this.Dispose(true);
                GC.SuppressFinalize(this);
                this._disposed = true;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && (this.connection != null))
            {
                this.connection.ConnectionStateChanged -= new EventHandler<ConnectionStateChangedEventArgs>(this.Connection_ConnectionStateChanged);
                IDisposable connection = this.connection as IDisposable;
                if (connection != null)
                {
                    connection.Dispose();
                }
                this.connection = null;
                this._sessionEstablishedAt = DateTime.MaxValue;
                this._symbolServer = null;
            }
        }

        protected abstract string GetSessionName();
        protected virtual IConnection OnConnect(bool reconnect)
        {
            if ((this.connection != null) && !reconnect)
            {
                this._sessionEstablishedAt = DateTime.UtcNow;
                this.connection.ConnectionStateChanged += new EventHandler<ConnectionStateChangedEventArgs>(this.Connection_ConnectionStateChanged);
            }
            return this.connection;
        }

        protected abstract ISymbolServer OnCreateSymbolServer();
        protected virtual bool OnDisconnect()
        {
            if (this.connection == null)
            {
                return false;
            }
            this.connection.Disconnect();
            return true;
        }

        protected abstract string OnGetAddress();

        public ISessionProvider Provider =>
            this.provider;

        public int Id =>
            this._id;

        public IConnection Connection =>
            this.connection;

        public bool IsConnected =>
            (!this._disposed ? ((this.connection != null) && this.connection.IsConnected) : false);

        public DateTime EstablishedAt =>
            this._sessionEstablishedAt;

        public bool Disposed =>
            this._disposed;

        public ISymbolServer SymbolServer
        {
            get
            {
                if (this._symbolServer == null)
                {
                    this._symbolServer = this.OnCreateSymbolServer();
                }
                return this._symbolServer;
            }
        }

        public string Name =>
            this.GetSessionName();

        public TwinCAT.ConnectionState ConnectionState =>
            (!this._disposed ? (!this.IsConnected ? TwinCAT.ConnectionState.Disconnected : this.connection.ConnectionState) : TwinCAT.ConnectionState.None);

        public string AddressSpecifier =>
            this.OnGetAddress();
    }
}

