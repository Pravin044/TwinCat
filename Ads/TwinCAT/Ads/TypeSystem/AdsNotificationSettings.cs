namespace TwinCAT.Ads.TypeSystem
{
    using System;
    using System.ComponentModel;
    using TwinCAT.Ads;

    [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never), Obsolete("Use TwinCAT.Ads.NotificationSettings instead!")]
    public class AdsNotificationSettings : NotificationSettings
    {
        public AdsNotificationSettings(AdsTransMode mode, int cycleTime, int maxDelay) : base(mode, cycleTime, maxDelay)
        {
        }
    }
}

