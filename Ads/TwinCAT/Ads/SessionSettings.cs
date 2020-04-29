namespace TwinCAT.Ads
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using TwinCAT;

    public class SessionSettings : IAdsSessionSettings, ISessionSettings
    {
        public static TimeSpan DefaultCommunicationTimeout = TimeSpan.FromSeconds(5.0);
        public static TimeSpan DefaultResurrectionTime = TimeSpan.FromSeconds(21.0);
        private TimeSpan _resurrectionTime;
        private SymbolLoaderSettings _loaderSettings;

        public SessionSettings(int timeout)
        {
            this._resurrectionTime = DefaultResurrectionTime;
            this.Synchronized = false;
            this.Timeout = timeout;
        }

        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never), Obsolete("Synchronized Notification support will be removed in the future. Do not use.", false)]
        public SessionSettings(bool synchronized, int timeout)
        {
            this._resurrectionTime = DefaultResurrectionTime;
            this.Synchronized = synchronized;
            this.Timeout = timeout;
        }

        [Obsolete("Synchronized Notification support will be removed in the future. Do not use.", false)]
        public bool Synchronized { get; internal set; }

        public int Timeout { get; internal set; }

        public static SessionSettings Default =>
            new SessionSettings((int) DefaultCommunicationTimeout.TotalMilliseconds);

        public static SessionSettings FastWriteThrough =>
            new SessionSettings(200) { ResurrectionTime=TimeSpan.Zero };

        public TimeSpan ResurrectionTime
        {
            get => 
                this._resurrectionTime;
            set => 
                (this._resurrectionTime = value);
        }

        public SymbolLoaderSettings SymbolLoader
        {
            get
            {
                if (this._loaderSettings == null)
                {
                    this._loaderSettings = SymbolLoaderSettings.DefaultDynamic;
                }
                return this._loaderSettings;
            }
            set => 
                (this._loaderSettings = value);
        }
    }
}

