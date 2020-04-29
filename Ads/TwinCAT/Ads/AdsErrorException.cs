namespace TwinCAT.Ads
{
    using System;
    using System.Runtime.Serialization;
    using System.Security;

    [Serializable]
    public class AdsErrorException : AdsException
    {
        private AdsErrorCode _errorCode;

        public AdsErrorException()
        {
        }

        public AdsErrorException(string message, AdsErrorCode errorCode) : base(ResMan.GetString(message, errorCode))
        {
            this._errorCode = errorCode;
        }

        public static AdsErrorException Create(AdsErrorCode adsErrorCode) => 
            CreateException(null, adsErrorCode);

        public static AdsErrorException Create(string message, AdsErrorCode adsErrorCode) => 
            CreateException(message, adsErrorCode);

        private static AdsErrorException CreateException(string message, AdsErrorCode adsErrorCode)
        {
            if (adsErrorCode == AdsErrorCode.NoError)
            {
                throw new ArgumentException("No error indicated!", "adsErrorCode");
            }
            return new AdsErrorException(ResMan.GetString(message, adsErrorCode), adsErrorCode);
        }

        [SecurityCritical]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }
            this._errorCode = (AdsErrorCode) info.GetInt32("ErrorCode");
            this.GetObjectData(info, context);
        }

        public AdsErrorCode ErrorCode =>
            this._errorCode;
    }
}

