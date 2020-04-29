namespace TwinCAT.Ads
{
    using System;
    using System.ComponentModel;

    [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
    public interface ICommunicationInterceptor
    {
        AdsErrorCode BeforeDisconnect(Func<AdsErrorCode> action);
        AdsErrorCode Communicate(Func<AdsErrorCode> action);
        AdsErrorCode Communicate(Action action, ref AdsErrorCode result);
        AdsErrorCode CommunicateReadState(Func<AdsErrorCode> action, ref StateInfo adsState);
        AdsErrorCode CommunicateWriteState(Func<AdsErrorCode> action, ref StateInfo adsState);
        AdsErrorCode Connect(Func<AdsErrorCode> action);
        AdsErrorCode Disconnect(Func<AdsErrorCode> action);

        string ID { get; }
    }
}

