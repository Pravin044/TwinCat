namespace TwinCAT.Ads.ValueAccess
{
    using System;
    using System.Runtime.InteropServices;
    using TwinCAT;
    using TwinCAT.Ads;
    using TwinCAT.TypeSystem;
    using TwinCAT.ValueAccess;

    internal abstract class AdsValueAccessorBase : RpcNotificationAccessorBase, IAccessorValueAny
    {
        protected bool automaticReconnection;

        protected AdsValueAccessorBase(IAccessorValueFactory valueFactory, IConnection connection, NotificationSettings settings) : base(valueFactory, connection, settings)
        {
        }

        protected AdsValueAccessorBase(IAccessorValueFactory valueFactory, ISession session, NotificationSettings settings) : base(valueFactory, session, settings)
        {
        }

        public abstract int TryReadAnyValue(ISymbol symbol, Type valueType, out object value, out DateTime utcReadTime);
        public abstract int TryUpdateAnyValue(ISymbol symbol, ref object valueObject, out DateTime utcReadTime);
        public abstract int TryWriteAnyValue(ISymbol symbol, object valueObject, out DateTime utcReadTime);

        internal bool AutomaticReconnection
        {
            get => 
                this.automaticReconnection;
            set => 
                (this.automaticReconnection = value);
        }
    }
}

