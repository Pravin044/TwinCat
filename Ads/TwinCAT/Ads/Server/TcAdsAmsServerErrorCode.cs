namespace TwinCAT.Ads.Server
{
    using System;

    public enum TcAdsAmsServerErrorCode
    {
        None,
        Unknown,
        ConnectPortFailed,
        DisconnectPortFailed,
        ReceiveQueueOverflow,
        ReceiveNotifcationQueueOverflow
    }
}

