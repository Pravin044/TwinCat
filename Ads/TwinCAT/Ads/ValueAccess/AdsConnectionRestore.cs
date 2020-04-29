namespace TwinCAT.Ads.ValueAccess
{
    using System;
    using TwinCAT;
    using TwinCAT.Ads;

    internal class AdsConnectionRestore : IDisposable
    {
        protected IAdsConnection connection;
        protected bool disconnectOnDispose;
        private bool _isDisposed;

        internal AdsConnectionRestore(AdsValueAccessorBase accessor)
        {
            if (accessor == null)
            {
                throw new ArgumentNullException("connection");
            }
            if (accessor.AutomaticReconnection)
            {
                this.connection = (IAdsConnection) accessor.Session.Connection;
                if (this.connection == null)
                {
                    throw new ArgumentException("accessor");
                }
                if (this.connection == null)
                {
                    throw new ClientNotConnectedException();
                }
                if (!this.connection.IsConnected)
                {
                    if (this.connection.Session == null)
                    {
                        throw new ClientNotConnectedException();
                    }
                    this.connection = (IAdsConnection) this.connection.Session.Connect();
                    this.disconnectOnDispose = true;
                }
            }
        }

        public void Dispose()
        {
            if (!this._isDisposed)
            {
                this.Dispose(true);
                this._isDisposed = true;
                GC.SuppressFinalize(this);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && this.disconnectOnDispose)
            {
                this.connection.Disconnect();
            }
        }
    }
}

