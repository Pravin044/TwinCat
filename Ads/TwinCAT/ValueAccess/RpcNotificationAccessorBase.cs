namespace TwinCAT.ValueAccess
{
    using System;
    using System.ComponentModel;
    using System.Runtime.InteropServices;
    using TwinCAT;
    using TwinCAT.TypeSystem;

    [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
    public abstract class RpcNotificationAccessorBase : ValueAccessor, IAccessorNotification, IAccessorRpc
    {
        protected INotificationSettings _notificationSettings;

        protected RpcNotificationAccessorBase(IAccessorValueFactory valueFactory, IConnection connection) : base(valueFactory, connection)
        {
        }

        protected RpcNotificationAccessorBase(IAccessorValueFactory valueFactory, ISession session) : base(valueFactory, session)
        {
        }

        protected RpcNotificationAccessorBase(IAccessorValueFactory valueFactory, IConnection connection, INotificationSettings defaultSettings) : base(valueFactory, connection)
        {
            this._notificationSettings = defaultSettings;
        }

        protected RpcNotificationAccessorBase(IAccessorValueFactory valueFactory, ISession session, INotificationSettings defaultSettings) : base(valueFactory, session)
        {
            this._notificationSettings = defaultSettings;
        }

        public abstract void OnRegisterNotification(ISymbol symbol, SymbolNotificationType type, INotificationSettings settings);
        public abstract void OnUnregisterNotification(ISymbol symbol, SymbolNotificationType type);
        public bool TryGetNotificationSettings(ISymbol symbol, out INotificationSettings settings)
        {
            IValueSymbol symbol2 = (IValueSymbol) symbol;
            if (symbol2 != null)
            {
                settings = symbol2.NotificationSettings;
                return true;
            }
            settings = null;
            return false;
        }

        public abstract int TryInvokeRpcMethod(IInstance instance, IRpcMethod method, object[] parameters, out object returnValue, out DateTime utcInvokeTime);

        public INotificationSettings DefaultNotificationSettings =>
            this._notificationSettings;
    }
}

