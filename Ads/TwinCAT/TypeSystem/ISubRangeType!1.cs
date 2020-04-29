namespace TwinCAT.TypeSystem
{
    public interface ISubRangeType<T> : ISubRangeType, IDataType, IBitSize where T: struct
    {
        T LowerBound { get; }

        T UpperBound { get; }
    }
}

