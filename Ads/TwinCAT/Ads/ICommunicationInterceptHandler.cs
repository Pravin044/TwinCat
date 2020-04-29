namespace TwinCAT.Ads
{
    using System;
    using System.ComponentModel;

    [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
    public interface ICommunicationInterceptHandler
    {
        void AfterCommunicate(AdsErrorCode result);
        void AfterConnect(AdsErrorCode result);
        void AfterDisconnect(AdsErrorCode result);
        void AfterReadState(StateInfo adsState, AdsErrorCode result);
        void AfterWriteState(StateInfo adsState, AdsErrorCode result);
        AdsErrorCode BeforeCommunicate();
        AdsErrorCode BeforeConnect();
        AdsErrorCode BeforeDisconnect();
        AdsErrorCode BeforeReadState();
        AdsErrorCode BeforeWriteState(StateInfo adsState);
    }
}

