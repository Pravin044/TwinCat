namespace TwinCAT.TypeSystem
{
    using TwinCAT.ValueAccess;

    public interface IValueAccessorProvider
    {
        IAccessorRawValue ValueAccessor { get; }
    }
}

