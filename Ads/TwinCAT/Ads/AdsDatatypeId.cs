namespace TwinCAT.Ads
{
    using System;
    using System.ComponentModel;

    public enum AdsDatatypeId
    {
        ADST_VOID = 0,
        ADST_INT8 = 0x10,
        ADST_UINT8 = 0x11,
        ADST_INT16 = 2,
        ADST_UINT16 = 0x12,
        ADST_INT32 = 3,
        ADST_UINT32 = 0x13,
        ADST_INT64 = 20,
        ADST_UINT64 = 0x15,
        ADST_REAL32 = 4,
        ADST_REAL64 = 5,
        ADST_BIGTYPE = 0x41,
        ADST_STRING = 30,
        ADST_WSTRING = 0x1f,
        ADST_REAL80 = 0x20,
        ADST_BIT = 0x21,
        [Browsable(false)]
        ADST_MAXTYPES = 0x22
    }
}

