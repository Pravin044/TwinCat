namespace TwinCAT.Ads
{
    using System;
    using System.ComponentModel;

    public enum AdsTransMode
    {
        None = 0,
        ClientCycle = 1,
        ClientOnChange = 2,
        Cyclic = 3,
        OnChange = 4,
        CyclicInContext = 5,
        OnChangeInContext = 6,
        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        Client1Req = 10,
        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        CyclicC0 = 0x10,
        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        CyclicC1 = 0x11,
        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        CyclicC2 = 0x12,
        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        CyclicC3 = 0x13,
        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        CyclicC4 = 20,
        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        CyclicC5 = 0x15,
        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        CyclicC6 = 0x16,
        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        CyclicC7 = 0x17,
        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        CyclicC8 = 0x18,
        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        CyclicC9 = 0x19,
        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        CyclicC10 = 0x1a,
        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        CyclicC11 = 0x1b,
        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        CyclicC12 = 0x1c,
        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        CyclicC13 = 0x1d,
        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        CyclicC14 = 30,
        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        CyclicC15 = 0x1f
    }
}

