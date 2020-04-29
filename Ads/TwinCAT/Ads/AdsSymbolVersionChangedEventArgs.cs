namespace TwinCAT.Ads
{
    using System;

    public sealed class AdsSymbolVersionChangedEventArgs : EventArgs
    {
        internal short _symbolVersion;

        public AdsSymbolVersionChangedEventArgs(byte symbolVersion)
        {
            this._symbolVersion = symbolVersion;
        }

        public AdsSymbolVersionChangedEventArgs(AdsSymbolVersionChangedEventArgs eventArgs)
        {
            this._symbolVersion = eventArgs._symbolVersion;
        }

        public short SymbolVersion =>
            this._symbolVersion;
    }
}

