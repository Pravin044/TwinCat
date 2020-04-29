namespace TwinCAT.Ads
{
    using System;
    using System.ComponentModel;

    [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
    public abstract class CommunicationInterceptor : ICommunicationInterceptor, ICommunicationInterceptHandler
    {
        private string _id;

        protected CommunicationInterceptor(string id)
        {
            this._id = id;
        }

        public void AfterCommunicate(AdsErrorCode errorCode)
        {
            this.OnAfterCommunicate(errorCode);
        }

        public void AfterConnect(AdsErrorCode errorCode)
        {
            this.OnAfterConnect(errorCode);
        }

        public void AfterDisconnect(AdsErrorCode errorCode)
        {
            this.OnAfterDisconnect(errorCode);
        }

        public void AfterReadState(StateInfo adsState, AdsErrorCode result)
        {
            this.OnAfterReadState(adsState, result);
        }

        public void AfterWriteState(StateInfo adsState, AdsErrorCode result)
        {
            this.OnAfterWriteState(adsState, result);
        }

        public AdsErrorCode BeforeCommunicate() => 
            this.OnBeforeCommunicate();

        public AdsErrorCode BeforeConnect() => 
            this.OnBeforeConnect();

        public AdsErrorCode BeforeDisconnect(Func<AdsErrorCode> action)
        {
            AdsErrorCode code = action();
            ((ICommunicationInterceptHandler) this).BeforeDisconnect();
            return code;
        }

        public AdsErrorCode BeforeReadState() => 
            this.OnBeforeReadState();

        public AdsErrorCode BeforeWriteState(StateInfo adsState) => 
            this.OnBeforeWriteState(adsState);

        public AdsErrorCode Communicate(Func<AdsErrorCode> action)
        {
            AdsErrorCode result = this.BeforeCommunicate();
            if (result == AdsErrorCode.NoError)
            {
                result = action();
            }
            this.AfterCommunicate(result);
            return result;
        }

        public AdsErrorCode Communicate(Action action, ref AdsErrorCode error)
        {
            error = this.BeforeCommunicate();
            if (error == AdsErrorCode.NoError)
            {
                action();
            }
            this.AfterCommunicate(error);
            return error;
        }

        public AdsErrorCode CommunicateReadState(Func<AdsErrorCode> action, ref StateInfo adsState)
        {
            AdsErrorCode noError = AdsErrorCode.NoError;
            noError = this.BeforeCommunicate();
            if (noError == AdsErrorCode.NoError)
            {
                noError = this.BeforeReadState();
            }
            if (noError == AdsErrorCode.NoError)
            {
                noError = action();
            }
            this.AfterReadState(adsState, noError);
            this.AfterCommunicate(noError);
            return noError;
        }

        public AdsErrorCode CommunicateWriteState(Func<AdsErrorCode> action, ref StateInfo adsState)
        {
            AdsErrorCode result = this.BeforeCommunicate();
            if (result == AdsErrorCode.NoError)
            {
                result = this.BeforeWriteState(adsState);
            }
            if (result == AdsErrorCode.NoError)
            {
                result = action();
            }
            this.AfterWriteState(adsState, result);
            this.AfterCommunicate(result);
            return result;
        }

        public AdsErrorCode Connect(Func<AdsErrorCode> action)
        {
            AdsErrorCode result = this.BeforeConnect();
            if (result == AdsErrorCode.NoError)
            {
                result = action();
            }
            this.AfterConnect(result);
            return result;
        }

        public AdsErrorCode Disconnect(Func<AdsErrorCode> action)
        {
            AdsErrorCode result = action();
            this.AfterDisconnect(result);
            return result;
        }

        protected virtual void OnAfterCommunicate(AdsErrorCode errorCode)
        {
        }

        protected virtual void OnAfterConnect(AdsErrorCode errorCode)
        {
        }

        protected virtual void OnAfterDisconnect(AdsErrorCode errorCode)
        {
        }

        protected virtual void OnAfterReadState(StateInfo adsState, AdsErrorCode result)
        {
        }

        protected virtual void OnAfterWriteState(StateInfo adsState, AdsErrorCode result)
        {
        }

        protected virtual AdsErrorCode OnBeforeCommunicate() => 
            AdsErrorCode.NoError;

        protected virtual AdsErrorCode OnBeforeConnect() => 
            AdsErrorCode.NoError;

        protected virtual AdsErrorCode OnBeforeDisconnect() => 
            AdsErrorCode.NoError;

        protected virtual AdsErrorCode OnBeforeReadState() => 
            AdsErrorCode.NoError;

        protected virtual AdsErrorCode OnBeforeWriteState(StateInfo adsState) => 
            AdsErrorCode.NoError;

        AdsErrorCode ICommunicationInterceptHandler.BeforeDisconnect() => 
            this.OnBeforeDisconnect();

        public string ID =>
            this._id;
    }
}

