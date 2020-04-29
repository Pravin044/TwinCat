namespace TwinCAT.Ads.Server
{
    using System;

    public class TcAdsServerExEventArgs : EventArgs
    {
        private System.Exception _ex;

        internal TcAdsServerExEventArgs(System.Exception ex)
        {
            this._ex = ex;
        }

        public System.Exception Exception =>
            this._ex;

        public string Message =>
            this._ex.Message;
    }
}

