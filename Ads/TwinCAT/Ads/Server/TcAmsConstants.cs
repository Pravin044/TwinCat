namespace TwinCAT.Ads.Server
{
    using System;

    internal class TcAmsConstants
    {
        internal const ushort AMS_PORT_USEDEFAULT = 0xffff;
        internal const ushort AMS_ADS = 4;
        internal const ushort AMS_REQ = 0;
        internal const ushort AMS_RES = 1;
        internal const ushort AMS_ADSREQ = 4;
        internal const ushort AMS_ADSRES = 5;
        internal const ushort AMS_TCP = 0;
        internal const ushort AMS_UDP = 0x400;
        internal const ushort ADSCOMMANDID_INVALID = 0;
        internal const ushort ADSCOMMANDID_RDDEVINFO = 1;
        internal const ushort ADSCOMMANDID_RD = 2;
        internal const ushort ADSCOMMANDID_WR = 3;
        internal const ushort ADSCOMMANDID_RDSTATE = 4;
        internal const ushort ADSCOMMANDID_WRCONTROL = 5;
        internal const ushort ADSCOMMANDID_ADDNOT = 6;
        internal const ushort ADSCOMMANDID_DELNOT = 7;
        internal const ushort ADSCOMMANDID_NOT = 8;
        internal const ushort ADSCOMMANDID_RDWR = 9;
    }
}

