namespace TwinCAT.Ads
{
    using System;
    using System.ComponentModel;

    public class AdsClientSettings
    {
        private TransportProtocol _protocol;
        private CommunicationInterceptors _interceptors;
        private bool _clientCycle;
        private bool _synchronized;
        private int _timeout;

        private AdsClientSettings()
        {
            this._timeout = 0x1388;
        }

        public AdsClientSettings(int timeout)
        {
            this._timeout = 0x1388;
            Default._timeout = timeout;
        }

        private static CommunicationInterceptors CreateDefaultInterceptors()
        {
            CommunicationInterceptors interceptors = new CommunicationInterceptors();
            interceptors.Combine(new FailFastHandlerInterceptor());
            return interceptors;
        }

        public static AdsClientSettings Default =>
            new AdsClientSettings { 
                _protocol=TransportProtocol.All,
                _interceptors=CreateDefaultInterceptors(),
                _timeout=0x1388,
                _synchronized=false
            };

        [Obsolete, EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        public static AdsClientSettings FastReconnection =>
            FastWriteThrough;

        public static AdsClientSettings FastWriteThrough
        {
            get
            {
                AdsClientSettings settings = Default;
                settings._protocol = TransportProtocol.All;
                settings._interceptors = null;
                settings._timeout = 200;
                return settings;
            }
        }

        public static AdsClientSettings CompatibilityDefault
        {
            get
            {
                AdsClientSettings settings = Default;
                settings._interceptors = null;
                settings._synchronized = true;
                settings._timeout = 0x1388;
                return settings;
            }
        }

        public TransportProtocol Protocol =>
            this._protocol;

        public CommunicationInterceptors Interceptors =>
            this._interceptors;

        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        public bool ClientCycle =>
            this._clientCycle;

        public bool Synchronize =>
            this._synchronized;

        public int Timeout =>
            this._timeout;
    }
}

