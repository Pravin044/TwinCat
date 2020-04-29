namespace TwinCAT.Ads
{
    using System;
    using TwinCAT;

    public class AdsTimeoutSetter : IDisposable
    {
        private int _newTimeout = -1;
        private int _oldTimeout = -1;
        private IConnection _connection;
        private bool _used;
        private bool _disposed;

        public AdsTimeoutSetter(IConnection connection, int timeout)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("client");
            }
            this._connection = connection;
            this._newTimeout = timeout;
            if (connection.IsConnected && (this._newTimeout > 0))
            {
                this._oldTimeout = this._connection.Timeout;
                this._connection.Timeout = this._newTimeout;
                this._used = true;
            }
        }

        public void Dispose()
        {
            if (!this._disposed)
            {
                this.Dispose(true);
                this._disposed = true;
                GC.SuppressFinalize(this);
            }
        }

        private void Dispose(bool disposing)
        {
            if (disposing && this._used)
            {
                if (this._connection != null)
                {
                    this._connection.Timeout = this._oldTimeout;
                }
                this._used = false;
            }
        }

        ~AdsTimeoutSetter()
        {
            this.Dispose(false);
        }
    }
}

