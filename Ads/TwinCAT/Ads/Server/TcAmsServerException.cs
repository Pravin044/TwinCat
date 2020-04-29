namespace TwinCAT.Ads.Server
{
    using System;

    [Serializable]
    internal class TcAmsServerException : Exception
    {
        private TcAdsAmsServerErrorCode errorCode;

        internal TcAmsServerException(string message) : base(message)
        {
            this.errorCode = TcAdsAmsServerErrorCode.Unknown;
        }

        internal TcAmsServerException(string message, TcAdsAmsServerErrorCode errorCode) : base(message)
        {
            this.errorCode = errorCode;
        }

        internal TcAdsAmsServerErrorCode ErrorCode
        {
            get => 
                this.errorCode;
            set => 
                (this.errorCode = value);
        }
    }
}

