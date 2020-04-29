namespace TwinCAT.Ads.Internal
{
    using System;
    using TwinCAT.Ads;

    internal class ActiveCommunicationState : FailFastHandlerState
    {
        private bool _tripped;
        private AdsErrorCode _error;

        public ActiveCommunicationState(TimeSpan timeout) : base(timeout)
        {
            this._tripped = false;
            Module.TraceSession.TraceInformation("FailFastHandlerInterceptor --> Active");
        }

        protected override IFailFastHandlerState OnNextState() => 
            (!this._tripped ? ((IFailFastHandlerState) this) : ((IFailFastHandlerState) new LostCommunicationState(base.timeout, this._error)));

        protected override void OnTrip(AdsErrorCode error)
        {
            this._tripped = true;
            this._error = error;
        }
    }
}

