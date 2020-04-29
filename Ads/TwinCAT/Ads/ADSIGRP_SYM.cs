namespace TwinCAT.Ads
{
    using System;

    internal enum ADSIGRP_SYM : uint
    {
        HNDBYNAME = 0xf003,
        VALBYNAME = 0xf004,
        VALBYHND = 0xf005,
        RELEASEHND = 0xf006,
        INFOBYNAME = 0xf007,
        VERSION = 0xf008,
        INFOBYNAMEEX = 0xf009,
        DOWNLOAD = 0xf00a,
        UPLOAD = 0xf00b,
        UPLOADINFO = 0xf00c,
        DOWNLOAD2 = 0xf00d,
        DT_UPLOAD = 0xf00e,
        UPLOADINFO2 = 0xf00f,
        SYMNOTE = 0xf010,
        DT_INFOBYNAMEEX = 0xf011,
        ADDRBYHND = 0xf012,
        POINTER_SUPPORT = 0xf013,
        POINTER_ACCESS = 0xf014,
        REFERENCE_SUPPORT = 0xf015,
        REFERENCE_ACCESS = 0xf016,
        GETSETFUNC_ACCESS = 0xf017,
        VALBYHND_WITHMASK = 0xf018,
        NOACCESS_TO_SUBSYM = 0xf019,
        POINTER_BITACCESS = 0xf01a,
        REFERENCE_BITACCESS = 0xf01b
    }
}

