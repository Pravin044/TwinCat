namespace TwinCAT.Ads
{
    using System;

    public enum AdsReservedIndexGroups : uint
    {
        PlcRWIB = 0x4000,
        PlcRWOB = 0x4010,
        PlcRWMB = 0x4020,
        PlcRWRB = 0x4030,
        PlcRWDB = 0x4040,
        SymbolTable = 0xf000,
        SymbolName = 0xf001,
        SymbolValue = 0xf002,
        SymbolHandleByName = 0xf003,
        SymbolValueByName = 0xf004,
        SymbolValueByHandle = 0xf005,
        SymbolReleaseHandle = 0xf006,
        SymbolInfoByName = 0xf007,
        SymbolVersion = 0xf008,
        SymbolInfoByNameEx = 0xf009,
        SymbolDownload = 0xf00a,
        SymbolUpload = 0xf00b,
        SymbolUploadInfo = 0xf00c,
        SymbolNote = 0xf010,
        IOImageRWIB = 0xf020,
        IOImageRWIX = 0xf021,
        IOImageRWOB = 0xf030,
        IOImageRWOX = 0xf031,
        IOImageClearI = 0xf040,
        IOImageClearO = 0xf050,
        SumCommandRead = 0xf080,
        SumCommandWrite = 0xf081,
        SumCommandReadWrite = 0xf082,
        SumCommandReadEx = 0xf083,
        SumCommandReadEx2 = 0xf084,
        SumCommandAddDevNote = 0xf085,
        SumCommandDelDevNote = 0xf086,
        DeviceData = 0xf100
    }
}

