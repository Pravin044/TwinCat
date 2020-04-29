namespace TwinCAT.Ads
{
    using System;

    public interface IFailFastHandler
    {
        AdsErrorCode Guard();
        void Reset();
        void Succeed();
        void Trip(AdsErrorCode errorCode);

        IFailFastHandlerState CurrentState { get; }

        bool IsActive { get; }

        bool IsReconnecting { get; }

        bool IsLost { get; }
    }
}

