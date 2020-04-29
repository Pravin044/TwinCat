namespace TwinCAT.Ads
{
    using System;

    public sealed class AdsNotificationErrorEventArgs : EventArgs
    {
        private System.Exception e;

        public AdsNotificationErrorEventArgs(System.Exception e)
        {
            this.e = e;
        }

        public System.Exception Exception =>
            this.e;
    }
}

