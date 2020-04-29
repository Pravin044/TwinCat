namespace TwinCAT.Ads
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct StateInfo
    {
        internal TwinCAT.Ads.AdsState adsState;
        internal short deviceState;
        public StateInfo(TwinCAT.Ads.AdsState adsState, short deviceState)
        {
            this.adsState = adsState;
            this.deviceState = deviceState;
        }

        public TwinCAT.Ads.AdsState AdsState
        {
            get => 
                this.adsState;
            set => 
                (this.adsState = value);
        }
        public short DeviceState
        {
            get => 
                this.deviceState;
            set => 
                (this.deviceState = value);
        }
        public override bool Equals(object ob) => 
            ((ob is StateInfo) && this.Equals((StateInfo) ob));

        public static bool operator ==(StateInfo a, StateInfo b) => 
            a.Equals(b);

        public static bool operator !=(StateInfo a, StateInfo b) => 
            !a.Equals(b);

        public bool Equals(StateInfo info) => 
            ((this.adsState == info.adsState) && (this.deviceState == info.deviceState));

        public override int GetHashCode() => 
            (this.adsState.GetHashCode() ^ this.deviceState.GetHashCode());
    }
}

