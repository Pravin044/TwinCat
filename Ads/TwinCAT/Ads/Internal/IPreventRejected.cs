namespace TwinCAT.Ads.Internal
{
    using System;

    internal interface IPreventRejected
    {
        bool PreventRejectedConnection { get; set; }
    }
}

