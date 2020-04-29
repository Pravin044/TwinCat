namespace TwinCAT.Ads
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
    public interface IAdsStateObserver
    {
        event EventHandler<AdsStateChangedEventArgs2> AdsStateChanged;

        TwinCAT.Ads.StateInfo StateInfo { get; }
    }
}

