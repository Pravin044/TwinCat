namespace TwinCAT.Ads
{
    using System;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using TwinCAT.Ads.Internal;

    [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
    public class PreventConnectionRejectedError : IDisposable
    {
        private bool _old;
        private AdsConnection _connection;
        private bool _used;
        private bool _disposed;

        public PreventConnectionRejectedError(AdsConnection connection)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            this._connection = connection;
            this._old = this.getCurrentValue();
            this.register(true);
            this._used = true;
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
                this.register(this._old);
                this._used = false;
            }
        }

        ~PreventConnectionRejectedError()
        {
            this.Dispose(false);
        }

        private bool getCurrentValue()
        {
            bool preventRejectedConnection = false;
            if ((this._connection.IsConnected && (this._connection.Client.Interceptors != null)) && (this._connection.Client.Interceptors.CombinedInterceptors.Count > 0))
            {
                IPreventRejected rejected = (IPreventRejected) Enumerable.FirstOrDefault<ICommunicationInterceptor>(this._connection.Client.Interceptors.CombinedInterceptors, item => item is IPreventRejected);
                if (rejected != null)
                {
                    preventRejectedConnection = rejected.PreventRejectedConnection;
                }
            }
            return preventRejectedConnection;
        }

        private void register(bool set)
        {
            if (this._connection.IsConnected && (this._connection.Client.Interceptors != null))
            {
                foreach (ICommunicationInterceptor interceptor in this._connection.Client.Interceptors.CombinedInterceptors)
                {
                    IPreventRejected rejected = interceptor as IPreventRejected;
                    if (rejected != null)
                    {
                        rejected.PreventRejectedConnection = set;
                    }
                }
            }
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly PreventConnectionRejectedError.<>c <>9 = new PreventConnectionRejectedError.<>c();
            public static Func<ICommunicationInterceptor, bool> <>9__4_0;

            internal bool <getCurrentValue>b__4_0(ICommunicationInterceptor item) => 
                (item is IPreventRejected);
        }
    }
}

