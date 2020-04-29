namespace TwinCAT.Ads
{
    using System;
    using System.Collections.Generic;

    public class NotificationSettingsPriorityComparer : IComparer<NotificationSettings>
    {
        public int Compare(NotificationSettings x, NotificationSettings y) => 
            ((x.NotificationMode == y.NotificationMode) ? ((x.CycleTime == y.CycleTime) ? ((x.MaxDelay == y.MaxDelay) ? 0 : ((x.MaxDelay >= y.MaxDelay) ? -1 : 1)) : ((x.CycleTime >= y.CycleTime) ? -1 : 1)) : ((x.NotificationMode != AdsTransMode.Cyclic) ? -1 : 1));
    }
}

