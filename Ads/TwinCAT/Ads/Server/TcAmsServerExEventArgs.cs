namespace TwinCAT.Ads.Server
{
    using System;

    internal class TcAmsServerExEventArgs : EventArgs
    {
        private System.Exception _ex;

        internal TcAmsServerExEventArgs(System.Exception ex)
        {
            this._ex = ex;
        }

        internal System.Exception Exception =>
            this._ex;
    }
}

