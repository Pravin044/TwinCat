namespace TwinCAT.TypeSystem
{
    public interface IAttributedInstance : IInstance, IBitSize
    {
        ReadOnlyTypeAttributeCollection Attributes { get; }
    }
}

