namespace TwinCAT.TypeSystem
{
    using System;

    public interface IDataType : IBitSize
    {
        int Id { get; }

        DataTypeCategory Category { get; }

        string Name { get; }

        string Namespace { get; }

        string FullName { get; }

        bool IsPrimitive { get; }

        bool IsContainer { get; }

        bool IsPointer { get; }

        bool IsReference { get; }

        ReadOnlyTypeAttributeCollection Attributes { get; }

        string Comment { get; }
    }
}

