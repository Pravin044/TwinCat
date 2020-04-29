namespace TwinCAT.Ads
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct DeviceInfo
    {
        internal string name;
        internal AdsVersion version;
        public string Name
        {
            get => 
                this.name;
            set => 
                (this.name = value);
        }
        public AdsVersion Version
        {
            get => 
                this.version;
            set => 
                (this.version = value);
        }
        public bool IsEmpty =>
            ((this.name == null) && this.version.IsEmpty);
    }
}

