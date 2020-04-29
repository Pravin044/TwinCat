namespace TwinCAT.TypeSystem
{
    public interface IPointerType : IDataType, IBitSize
    {
        IDataType ReferencedType { get; }
    }
}

