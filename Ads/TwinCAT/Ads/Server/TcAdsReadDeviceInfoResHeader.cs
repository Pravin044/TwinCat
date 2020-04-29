namespace TwinCAT.Ads.Server
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using TwinCAT.Ads;

    [StructLayout(LayoutKind.Sequential, Pack=1)]
    internal class TcAdsReadDeviceInfoResHeader : ITcAdsHeader
    {
        internal AdsErrorCode _result;
        internal byte _majorVersion;
        internal byte _minorVersion;
        internal ushort _versionBuild;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x10)]
        internal byte[] _deviceName = new byte[0x10];
        public void WriteToBuffer(BinaryWriter writer)
        {
            writer.Write((uint) this._result);
            writer.Write(this._majorVersion);
            writer.Write(this._minorVersion);
            writer.Write(this._versionBuild);
            writer.Write(this._deviceName);
        }
    }
}

