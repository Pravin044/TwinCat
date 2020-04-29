namespace TwinCAT.Ads
{
    using System;
    using System.ComponentModel;

    public enum AdsState : short
    {
        Invalid = 0,
        Idle = 1,
        Reset = 2,
        Init = 3,
        Start = 4,
        Run = 5,
        Stop = 6,
        SaveConfig = 7,
        LoadConfig = 8,
        PowerFailure = 9,
        PowerGood = 10,
        Error = 11,
        Shutdown = 12,
        Suspend = 13,
        Resume = 14,
        Config = 15,
        Reconfig = 0x10,
        Stopping = 0x11,
        Incompatible = 0x12,
        Exception = 0x13,
        [Browsable(false), EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        Maxstates = 0x11
    }
}

