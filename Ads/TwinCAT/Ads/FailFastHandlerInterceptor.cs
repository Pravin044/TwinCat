namespace TwinCAT.Ads
{
    using System;
    using System.ComponentModel;
    using System.Linq;
    using TwinCAT.Ads.Internal;

    [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
    public class FailFastHandlerInterceptor : CommunicationInterceptor, IFailFastHandler, IPreventRejected
    {
        private object _synchronizer;
        private static TimeSpan s_defaultTimeout = TimeSpan.FromSeconds(21.0);
        private bool _preventRejectedConnection;
        private TimeSpan _timeout;
        private IFailFastHandlerState _state;
        internal static AdsErrorCode[] TrippingErrors = new AdsErrorCode[] { AdsErrorCode.WSA_ConnRefused, AdsErrorCode.PortDisabled, AdsErrorCode.PortNotConnected, AdsErrorCode.ClientSyncTimeOut, AdsErrorCode.TargetMachineNotFound, AdsErrorCode.TargetPortNotFound, AdsErrorCode.ClientPortNotOpen };
        private AdsErrorCode _trippedError;

        public FailFastHandlerInterceptor() : this(s_defaultTimeout)
        {
        }

        public FailFastHandlerInterceptor(TimeSpan timeout) : base("FailFastHandlerInterceptor")
        {
            this._synchronizer = new object();
            this._state = new ActiveCommunicationState(timeout);
            this._timeout = timeout;
        }

        public AdsErrorCode Guard()
        {
            AdsErrorCode noError = AdsErrorCode.NoError;
            object obj2 = this._synchronizer;
            lock (obj2)
            {
                this._state = this._state.NextState();
                noError = this._state.Guard();
                this._state = this._state.NextState();
            }
            return noError;
        }

        internal static bool IsTrippingError(AdsErrorCode errorCode, bool preventRejectedConnectionError) => 
            ((!preventRejectedConnectionError || (AdsErrorCode.WSA_ConnRefused != errorCode)) && Enumerable.Contains<AdsErrorCode>(TrippingErrors, errorCode));

        protected override void OnAfterCommunicate(AdsErrorCode errorCode)
        {
            if (IsTrippingError(errorCode, this._preventRejectedConnection))
            {
                this.Trip(errorCode);
            }
            else
            {
                this.Succeed();
            }
            base.OnAfterCommunicate(errorCode);
        }

        protected override void OnAfterConnect(AdsErrorCode errorCode)
        {
            base.OnAfterConnect(errorCode);
        }

        protected override void OnAfterDisconnect(AdsErrorCode errorCode)
        {
            base.OnAfterDisconnect(errorCode);
        }

        protected override AdsErrorCode OnBeforeCommunicate() => 
            this.Guard();

        protected override AdsErrorCode OnBeforeConnect()
        {
            this.Reset();
            return base.OnBeforeConnect();
        }

        public void Reset()
        {
            this._state = new ActiveCommunicationState(this._timeout);
        }

        public void Succeed()
        {
            object obj2 = this._synchronizer;
            lock (obj2)
            {
                this._state = this._state.NextState();
                this._state.Succeed();
                this._state = this._state.NextState();
                this._trippedError = AdsErrorCode.NoError;
            }
        }

        public void Trip(AdsErrorCode error)
        {
            object obj2 = this._synchronizer;
            lock (obj2)
            {
                this._trippedError = error;
                this._state = this._state.NextState();
                this._state.Trip(error);
                this._state = this._state.NextState();
            }
        }

        bool IPreventRejected.PreventRejectedConnection
        {
            get => 
                this._preventRejectedConnection;
            set => 
                (this._preventRejectedConnection = value);
        }

        public TimeSpan Timeout =>
            this._timeout;

        public IFailFastHandlerState CurrentState =>
            this._state;

        public AdsErrorCode TrippedError
        {
            get
            {
                object obj2 = this._synchronizer;
                lock (obj2)
                {
                    return this._trippedError;
                }
            }
        }

        public bool IsActive =>
            (this._state is ActiveCommunicationState);

        public bool IsReconnecting =>
            (this._state is ReconnectingCommunicationState);

        public bool IsLost =>
            (this._state is LostCommunicationState);
    }
}

