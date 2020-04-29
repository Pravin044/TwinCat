namespace TwinCAT.Ads
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct AdsVersion
    {
        private byte _version;
        private byte _revision;
        private int _build;
        public AdsVersion(int version, int revision, int build)
        {
            if ((version < 0) || (version > 0xff))
            {
                throw new ArgumentOutOfRangeException("version");
            }
            if ((revision < 0) || (revision > 0xff))
            {
                throw new ArgumentOutOfRangeException("revision");
            }
            this._version = (byte) version;
            this._revision = (byte) revision;
            this._build = build;
        }

        public byte Version
        {
            get => 
                this._version;
            set => 
                (this._version = value);
        }
        public byte Revision
        {
            get => 
                this._revision;
            set => 
                (this._revision = value);
        }
        public int Build
        {
            get => 
                this._build;
            set => 
                (this._build = value);
        }
        public bool IsEmpty =>
            ((this._version == 0) && ((this._revision == 0) && (this._build == 0)));
        public System.Version ConvertToStandard() => 
            AdsVersionConverter.Convert(this);
    }
}

