namespace TwinCAT.Ads.Reactive
{
    using System;
    using TwinCAT.Ads;
    using TwinCAT.TypeSystem;
    using TwinCAT.ValueAccess;

    public sealed class SymbolNotification : Notification
    {
        private IAccessorValueFactory _valueFactory;
        private ISymbol _symbol;
        private bool _valCreated;

        internal SymbolNotification(AdsNotificationEventArgs args, ISymbol symbol, IAccessorValueFactory valueFactory) : base(args)
        {
            this._valueFactory = valueFactory;
            this._symbol = symbol;
        }

        public ISymbol Symbol =>
            (this._symbol as IValueSymbol);

        public override object Value
        {
            get
            {
                if (!this._valCreated)
                {
                    ISymbol symbol = this.Symbol;
                    if (((base._bytes != null) && (this._valueFactory != null)) && (symbol != null))
                    {
                        base.val = this._valueFactory.CreateValue(symbol, base._bytes, 0, base.timeStamp.UtcDateTime);
                        this._valCreated = true;
                    }
                }
                return base.val;
            }
        }
    }
}

