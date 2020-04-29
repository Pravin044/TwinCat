namespace TwinCAT.TypeSystem
{
    public interface IField : IAttributedInstance, IInstance, IBitSize
    {
        IDataType ParentType { get; }
    }
}

