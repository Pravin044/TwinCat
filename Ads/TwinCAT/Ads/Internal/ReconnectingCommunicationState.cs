namespace TwinCAT.Ads.Internal
{
    using System;
    using TwinCAT.Ads;

    internal class ReconnectingCommunicationState : FailFastHandlerState
    {
        private bool _tripped;
        private AdsErrorCode _trippingError;
        private bool _succeeded;

        public ReconnectingCommunicationState(TimeSpan timeout) : base(timeout)
        {
            Module.TraceSession.TraceInformation("FailFastHandlerInterceptor --> Reconnectiong)");
        }

        protected override IFailFastHandlerState OnNextState() => 
            (!this._succeeded ? (!this._tripped ? ((IFailFastHandlerState) this) : ((IFailFastHandlerState) new LostCommunicationState(base.timeout, this._trippingError))) : ((IFailFastHandlerState) new ActiveCommunicationState(base.timeout)));

        protected override void OnSucceed()
        {
            this._succeeded = true;
        }

        protected override void OnTrip(AdsErrorCode error)
        {
            this._tripped = true;
            this._trippingError = error;
        }
    }
}

