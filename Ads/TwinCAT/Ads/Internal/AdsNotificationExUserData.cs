namespace TwinCAT.Ads.Internal
{
    using System;

    internal class AdsNotificationExUserData
    {
        public object userData;
        public Type type;
        public int[] args;

        public AdsNotificationExUserData(Type type, int[] args, object userData)
        {
            this.type = type;
            this.args = args;
            this.userData = userData;
        }
    }
}

