namespace TwinCAT.TypeSystem
{
    public interface IResolvableType
    {
        IDataType ResolveType(DataTypeResolveStrategy type);
    }
}

