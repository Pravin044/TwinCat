namespace TwinCAT.Ads.Server
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Pack=1)]
    internal class TcAdsDelDeviceNotificationReqHeader : ITcAdsHeader
    {
        internal uint _notificationHandle;
        public void WriteToBuffer(BinaryWriter writer)
        {
            writer.Write(this._notificationHandle);
        }
    }
}

