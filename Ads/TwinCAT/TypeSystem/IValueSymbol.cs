namespace TwinCAT.TypeSystem
{
    using System;
    using System.Runtime.CompilerServices;

    public interface IValueSymbol : IValueRawSymbol, IHierarchicalSymbol, ISymbol, IAttributedInstance, IInstance, IBitSize
    {
        event EventHandler<ValueChangedArgs> ValueChanged;

        object ReadValue();
        object ReadValue(int timeout);
        void WriteValue(object value);
        void WriteValue(object value, int timeout);

        INotificationSettings NotificationSettings { get; set; }

        SymbolAccessRights AccessRights { get; }
    }
}

