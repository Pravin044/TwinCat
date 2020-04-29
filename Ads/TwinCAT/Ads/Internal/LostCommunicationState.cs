namespace TwinCAT.Ads.Internal
{
    using System;
    using TwinCAT.Ads;

    internal class LostCommunicationState : FailFastHandlerState
    {
        private readonly DateTime _lostTime;
        private readonly AdsErrorCode _error;

        public LostCommunicationState(TimeSpan timeout, AdsErrorCode causingError) : base(timeout)
        {
            this._lostTime = DateTime.UtcNow;
            this._error = causingError;
            object[] args = new object[] { causingError };
            Module.TraceSession.TraceInformation("FailFastHandlerInterceptor --> Lost (Caused by '{0}')", args);
        }

        protected override AdsErrorCode OnGuard() => 
            this._error;

        protected override IFailFastHandlerState OnNextState() => 
            (((DateTime.UtcNow - this._lostTime) < base.timeout) ? ((IFailFastHandlerState) this) : ((IFailFastHandlerState) new ReconnectingCommunicationState(base.timeout)));
    }
}

