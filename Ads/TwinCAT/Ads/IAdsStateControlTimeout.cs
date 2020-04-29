namespace TwinCAT.Ads
{
    using System;
    using System.ComponentModel;
    using System.Runtime.InteropServices;

    [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
    public interface IAdsStateControlTimeout
    {
        StateInfo ReadState(int timeout);
        AdsErrorCode TryReadState(int timeout, out StateInfo stateInfo);
        AdsErrorCode TryWriteControl(StateInfo stateInfo, int timeout);
        AdsErrorCode TryWriteControl(StateInfo stateInfo, AdsStream dataStream, int offset, int length, int timeout);
        void WriteControl(StateInfo stateInfo, int timeout);
        void WriteControl(StateInfo stateInfo, AdsStream dataStream, int offset, int length, int timeout);
    }
}

