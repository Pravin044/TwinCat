namespace TwinCAT.Ads
{
    using System;
    using System.Diagnostics;
    using TwinCAT;
    using TwinCAT.Ads.TypeSystem;
    using TwinCAT.TypeSystem;

    [DebuggerDisplay("SessionId = {Id} : Address: {Address}, ConnectionState: {ConnectionState}, Owner: {Owner}")]
    public class AdsSession : Session, IAdsSession, ISession, IConnectionStateProvider, ISymbolServerProvider, IInterceptionFactory
    {
        private AmsAddress _address;
        private SessionSettings _settings;
        private CommunicationInterceptors _interceptor;
        private FailFastHandlerInterceptor _failFastHandlerInterceptor;
        private ConnectionStateInterceptor _connectionStateObserver;
        private object _owner;

        public AdsSession(AmsAddress address) : this(address, SessionSettings.Default)
        {
        }

        public AdsSession(AmsAddress address, SessionSettings settings) : this(address, settings, null)
        {
        }

        public AdsSession(AmsNetId netId, int port) : this(netId, port, SessionSettings.Default)
        {
        }

        public AdsSession(AmsAddress address, SessionSettings settings, object owner) : base(SessionProvider<AdsSession, AmsAddress, SessionSettings>.Self)
        {
            this._owner = owner;
            this._address = address.Clone();
            this._settings = settings;
        }

        public AdsSession(AmsNetId netId, int port, SessionSettings settings) : this(new AmsAddress(netId, port), settings, null)
        {
        }

        private void _connection_AmsRouterNotification(object sender, AmsRouterNotificationEventArgs e)
        {
            AmsRouterState state = e.State;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (base.connection != null)
                {
                    ((AdsConnection) base.connection).AmsRouterNotification -= new AmsRouterNotificationEventHandler(this._connection_AmsRouterNotification);
                }
                this._failFastHandlerInterceptor = null;
                this._connectionStateObserver = null;
                this._interceptor = null;
            }
            base.Dispose(disposing);
        }

        ~AdsSession()
        {
            this.Dispose(false);
        }

        protected override string GetSessionName()
        {
            object[] args = new object[] { this._address.NetId, this._address.Port };
            return string.Format(null, "Session {0}:{1}", args);
        }

        protected override IConnection OnConnect(bool reconnect)
        {
            AdsConnection connection = null;
            if (base.connection != null)
            {
                ((AdsConnection) base.connection).Connect();
            }
            else
            {
                connection = new AdsConnection(this);
                connection.AmsRouterNotification += new AmsRouterNotificationEventHandler(this._connection_AmsRouterNotification);
                base.connection = connection;
            }
            return base.OnConnect(reconnect);
        }

        protected override ISymbolServer OnCreateSymbolServer() => 
            new AdsSymbolServer(this);

        protected override bool OnDisconnect()
        {
            IConnection connection = base.connection;
            return base.OnDisconnect();
        }

        protected override string OnGetAddress() => 
            this._address.ToString();

        ICommunicationInterceptor IInterceptionFactory.CreateInterceptor()
        {
            if (this._interceptor == null)
            {
                CommunicationInterceptors interceptors = new CommunicationInterceptors();
                if (this._settings.ResurrectionTime > TimeSpan.Zero)
                {
                    this._failFastHandlerInterceptor = new FailFastHandlerInterceptor(this._settings.ResurrectionTime);
                    interceptors.Combine(this._failFastHandlerInterceptor);
                }
                this._connectionStateObserver = new ConnectionStateInterceptor(this);
                interceptors.Combine(this._connectionStateObserver);
                this._interceptor = interceptors;
            }
            return this._interceptor;
        }

        public AmsAddress Address =>
            this._address;

        public SessionSettings Settings =>
            this._settings;

        public AdsConnection Connection =>
            ((AdsConnection) base.connection);

        internal ConnectionStateInterceptor ConnectionObserver =>
            this._connectionStateObserver;

        public AdsCommunicationStatistics Statistics =>
            new AdsCommunicationStatistics(this);

        public AmsNetId NetId =>
            this._address.NetId;

        public int Port =>
            this._address.Port;

        public object Owner =>
            this._owner;
    }
}

