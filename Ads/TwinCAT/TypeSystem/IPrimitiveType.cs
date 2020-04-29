namespace TwinCAT.TypeSystem
{
    public interface IPrimitiveType : IDataType, IBitSize
    {
        PrimitiveTypeFlags PrimitiveFlags { get; }
    }
}

