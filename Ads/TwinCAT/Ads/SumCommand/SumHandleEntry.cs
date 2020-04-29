namespace TwinCAT.Ads.SumCommand
{
    using System;
    using TwinCAT.Ads;

    public class SumHandleEntry
    {
        protected uint handle;
        protected AdsErrorCode errorCode;

        internal SumHandleEntry(uint handle, AdsErrorCode errorCode)
        {
            this.handle = handle;
            this.errorCode = errorCode;
        }

        public uint Handle =>
            this.handle;

        public AdsErrorCode ErrorCode =>
            this.errorCode;
    }
}

