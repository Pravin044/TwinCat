namespace TwinCAT.Ads
{
    using System;

    public sealed class AdsStateChangedEventArgs : EventArgs
    {
        private StateInfo _state;

        public AdsStateChangedEventArgs(AdsStateChangedEventArgs eventArgs)
        {
            this._state = eventArgs._state;
        }

        public AdsStateChangedEventArgs(StateInfo state)
        {
            this._state = state;
        }

        public StateInfo State =>
            this._state;
    }
}

