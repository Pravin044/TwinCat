namespace TwinCAT.Ads.Internal
{
    using System;
    using System.ComponentModel;
    using TwinCAT.Ads;

    [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
    internal abstract class FailFastHandlerState : IFailFastHandlerState
    {
        protected readonly TimeSpan timeout;

        protected FailFastHandlerState(TimeSpan timeout)
        {
            this.timeout = timeout;
        }

        public AdsErrorCode Guard() => 
            this.OnGuard();

        public IFailFastHandlerState NextState() => 
            this.OnNextState();

        protected virtual AdsErrorCode OnGuard() => 
            AdsErrorCode.NoError;

        protected abstract IFailFastHandlerState OnNextState();
        protected virtual void OnSucceed()
        {
        }

        protected virtual void OnTrip(AdsErrorCode error)
        {
        }

        public void Succeed()
        {
            this.OnSucceed();
        }

        public void Trip(AdsErrorCode error)
        {
            this.OnTrip(error);
        }
    }
}

