namespace TwinCAT.Ads.Server
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using TwinCAT.Ads;

    [StructLayout(LayoutKind.Sequential, Pack=1)]
    internal class TcAdsAddDeviceNotificationResHeader : ITcAdsHeader
    {
        internal AdsErrorCode _result;
        internal uint _notificationHandle;
        public void WriteToBuffer(BinaryWriter writer)
        {
            writer.Write((uint) this._result);
            writer.Write(this._notificationHandle);
        }
    }
}

