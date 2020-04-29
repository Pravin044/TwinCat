namespace TwinCAT.Ads.Internal
{
    using System;
    using TwinCAT.Ads;

    internal class NotificationEntry
    {
        public AdsStream data;
        public int offset;
        public int length;
        public object userData;
        public int clientHandle;
        public int variableHandle;
    }
}

