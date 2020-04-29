namespace TwinCAT.Ads
{
    using System;
    using System.ComponentModel;
    using TwinCAT;
    using TwinCAT.Ads.Internal;

    public interface IAdsConnection : IConnection, IConnectionStateProvider, IAdsNotifications, IAdsSymbolicAccess, IAdsAnyAccess, IAdsHandleAccess, IAdsReadWriteAccess, IAdsStateControl, ITcAdsRpcInvoke
    {
        DeviceInfo ReadDeviceInfo();

        AmsAddress ClientAddress { get; }

        bool IsLocal { get; }

        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        ITcAdsRaw RawInterface { get; }

        AmsAddress Address { get; }
    }
}

