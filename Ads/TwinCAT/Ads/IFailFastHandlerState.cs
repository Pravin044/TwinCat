namespace TwinCAT.Ads
{
    using System;
    using System.ComponentModel;

    [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
    public interface IFailFastHandlerState
    {
        AdsErrorCode Guard();
        IFailFastHandlerState NextState();
        void Succeed();
        void Trip(AdsErrorCode error);
    }
}

