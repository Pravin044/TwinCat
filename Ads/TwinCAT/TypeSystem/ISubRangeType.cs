namespace TwinCAT.TypeSystem
{
    public interface ISubRangeType : IDataType, IBitSize
    {
        IDataType BaseType { get; }
    }
}

