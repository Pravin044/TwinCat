namespace TwinCAT.Ads.TypeSystem
{
    using System;
    using System.ComponentModel;
    using TwinCAT;
    using TwinCAT.Ads;
    using TwinCAT.TypeSystem;

    [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
    public class AdsSymbolServer : ISymbolServer
    {
        private AdsSession _session;
        private IAdsSymbolLoader _loader;

        internal AdsSymbolServer(AdsSession session)
        {
            if (session == null)
            {
                throw new ArgumentNullException("session");
            }
            this._session = session;
        }

        private void createLoader()
        {
            if (this._loader == null)
            {
                this._loader = (AdsSymbolLoader) this.OnCreateLoader();
            }
        }

        private ISymbolLoader OnCreateLoader()
        {
            if (!this._session.IsConnected)
            {
                throw new SessionNotConnectedException("Cannot create symbol loader!", this._session);
            }
            this._loader = this._session.Connection.CreateSymbolLoader(this._session, this._session.Settings.SymbolLoader);
            return this._loader;
        }

        public ReadOnlyDataTypeCollection DataTypes
        {
            get
            {
                ReadOnlyDataTypeCollection dataTypes;
                try
                {
                    this.createLoader();
                    dataTypes = this._loader.DataTypes;
                }
                catch (Exception exception)
                {
                    Module.Trace.TraceError(exception);
                    throw;
                }
                return dataTypes;
            }
        }

        public ReadOnlySymbolCollection Symbols
        {
            get
            {
                ReadOnlySymbolCollection symbols;
                try
                {
                    this.createLoader();
                    symbols = this._loader.Symbols;
                }
                catch (Exception exception)
                {
                    Module.Trace.TraceError(exception);
                    throw;
                }
                return symbols;
            }
        }
    }
}

