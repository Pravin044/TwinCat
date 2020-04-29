namespace TwinCAT.ValueAccess
{
    using System;

    public interface IAccessorValueFactory2 : IAccessorValueFactory
    {
        void SetValueAccessor(IAccessorRawValue accessor);

        IAccessorRawValue ValueAccessor { get; }
    }
}

