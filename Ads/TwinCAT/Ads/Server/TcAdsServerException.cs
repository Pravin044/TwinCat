namespace TwinCAT.Ads.Server
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class TcAdsServerException : Exception
    {
        private TcAdsAmsServerErrorCode errorCode;

        internal TcAdsServerException(string message) : base(message)
        {
            this.errorCode = TcAdsAmsServerErrorCode.Unknown;
        }

        internal TcAdsServerException(string message, TcAdsAmsServerErrorCode errorCode) : base(message)
        {
            this.errorCode = errorCode;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }
            info.AddValue("ErrorCode", (int) this.errorCode);
            base.GetObjectData(info, context);
        }

        public TcAdsAmsServerErrorCode ErrorCode
        {
            get => 
                this.errorCode;
            set => 
                (this.errorCode = value);
        }
    }
}

