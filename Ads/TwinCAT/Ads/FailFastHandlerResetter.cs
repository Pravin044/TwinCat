namespace TwinCAT.Ads
{
    using System;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.CompilerServices;

    [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
    public static class FailFastHandlerResetter
    {
        public static void Reset(AdsConnection connection)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            if (connection.IsConnected)
            {
                ((IFailFastHandler) Enumerable.FirstOrDefault<ICommunicationInterceptor>(connection.Client.Interceptors.CombinedInterceptors, item => item is IFailFastHandler)).Reset();
            }
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly FailFastHandlerResetter.<>c <>9 = new FailFastHandlerResetter.<>c();
            public static Func<ICommunicationInterceptor, bool> <>9__0_0;

            internal bool <Reset>b__0_0(ICommunicationInterceptor item) => 
                (item is IFailFastHandler);
        }
    }
}

