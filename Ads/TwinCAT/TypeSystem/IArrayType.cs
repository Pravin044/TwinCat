namespace TwinCAT.TypeSystem
{
    using System;

    public interface IArrayType : IDataType, IBitSize
    {
        ReadOnlyDimensionCollection Dimensions { get; }

        IDataType ElementType { get; }

        bool IsJagged { get; }

        int JaggedLevel { get; }
    }
}

