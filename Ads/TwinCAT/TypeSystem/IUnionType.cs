namespace TwinCAT.TypeSystem
{
    public interface IUnionType : IDataType, IBitSize
    {
        ReadOnlyFieldCollection Fields { get; }
    }
}

