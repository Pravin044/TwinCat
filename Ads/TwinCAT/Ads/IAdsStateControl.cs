namespace TwinCAT.Ads
{
    using System;
    using System.ComponentModel;
    using System.Runtime.InteropServices;

    [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
    public interface IAdsStateControl
    {
        StateInfo ReadState();
        AdsErrorCode TryReadState(out StateInfo stateInfo);
        AdsErrorCode TryWriteControl(StateInfo stateInfo);
        AdsErrorCode TryWriteControl(StateInfo stateInfo, AdsStream dataStream, int offset, int length);
        void WriteControl(StateInfo stateInfo);
        void WriteControl(StateInfo stateInfo, AdsStream dataStream, int offset, int length);
    }
}

