namespace TwinCAT.Ads.SumCommand
{
    using System;
    using TwinCAT.Ads;

    public class SumHandleInstancePathEntry : SumHandleEntry
    {
        private string _instancePath;

        internal SumHandleInstancePathEntry(string instancePath, uint handle, AdsErrorCode errorCode) : base(handle, errorCode)
        {
            this._instancePath = instancePath;
        }

        public string InstancePath =>
            this._instancePath;
    }
}

