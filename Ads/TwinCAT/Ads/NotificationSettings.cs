namespace TwinCAT.Ads
{
    using System;
    using TwinCAT.TypeSystem;

    public class NotificationSettings : INotificationSettings, IComparable<INotificationSettings>
    {
        private static NotificationSettings _default = new NotificationSettings(AdsTransMode.OnChange, 200, 0);
        private AdsTransMode _mode;
        private int _cycleTime;
        private int _maxDelay;

        public NotificationSettings(AdsTransMode mode, int cycleTime, int maxDelay)
        {
            this._mode = mode;
            this._cycleTime = cycleTime;
            this._maxDelay = maxDelay;
        }

        public int CompareTo(INotificationSettings other) => 
            new NotificationSettingsPriorityComparer().Compare(this, (NotificationSettings) other);

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (base.GetType() != obj.GetType())
            {
                return false;
            }
            NotificationSettings settings = (NotificationSettings) obj;
            return ((this.NotificationMode == settings.NotificationMode) ? ((this.CycleTime == settings.CycleTime) ? (this.MaxDelay == settings.MaxDelay) : false) : false);
        }

        public override int GetHashCode() => 
            ((0x5b * ((0x5b * ((0x5b * 10) + this.NotificationMode)) + this.CycleTime)) + this.MaxDelay);

        private void OnCycleTimeChanged(int value)
        {
            this._cycleTime = value;
        }

        private void OnMaxDelayChanged(int value)
        {
            this._maxDelay = value;
        }

        private void OnModeChanged(AdsTransMode value)
        {
            this._mode = value;
        }

        public static bool operator ==(NotificationSettings o1, NotificationSettings o2) => 
            (!Equals(o1, null) ? o1.Equals(o2) : Equals(o2, null));

        public static bool operator !=(NotificationSettings o1, NotificationSettings o2) => 
            !(o1 == o2);

        public static NotificationSettings Default =>
            _default;

        public AdsTransMode NotificationMode =>
            this._mode;

        public int CycleTime =>
            this._cycleTime;

        public int MaxDelay =>
            this._maxDelay;
    }
}

