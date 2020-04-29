namespace TwinCAT.ValueAccess
{
    using System;
    using System.ComponentModel;
    using System.Runtime.InteropServices;
    using TwinCAT.TypeSystem;

    [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
    public interface IAccessorNotification
    {
        void OnRegisterNotification(ISymbol symbol, SymbolNotificationType type, INotificationSettings settings);
        void OnUnregisterNotification(ISymbol symbol, SymbolNotificationType type);
        bool TryGetNotificationSettings(ISymbol symbol, out INotificationSettings settings);

        INotificationSettings DefaultNotificationSettings { get; }
    }
}

