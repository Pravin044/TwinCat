namespace TwinCAT.Ads
{
    using System;

    public sealed class AdsStateChangedEventArgs2 : EventArgs
    {
        public readonly StateInfo NewState;
        public readonly StateInfo OldState;
        public readonly IAdsSession Session;

        public AdsStateChangedEventArgs2(StateInfo newState, StateInfo oldState, IAdsSession session)
        {
            this.NewState = newState;
            this.OldState = oldState;
            this.Session = session;
        }

        public IAdsConnection Connection =>
            ((IAdsConnection) this.Session.Connection);
    }
}

