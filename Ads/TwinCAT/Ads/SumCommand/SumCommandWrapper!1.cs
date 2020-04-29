namespace TwinCAT.Ads.SumCommand
{
    using System;
    using TwinCAT.Ads;

    public class SumCommandWrapper<T> : ISumCommand where T: ISumCommand
    {
        protected T innerCommand;

        internal SumCommandWrapper()
        {
        }

        public AdsErrorCode Result =>
            ((this.innerCommand == null) ? AdsErrorCode.NoError : this.innerCommand.Result);

        public AdsErrorCode[] SubResults =>
            this.innerCommand?.SubResults;

        public bool Executed =>
            ((this.innerCommand != null) && this.innerCommand.Executed);

        public bool Succeeded =>
            ((this.innerCommand != null) && this.innerCommand.Succeeded);

        public bool Failed =>
            ((this.innerCommand != null) && this.innerCommand.Failed);
    }
}

