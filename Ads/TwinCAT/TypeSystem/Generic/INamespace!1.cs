namespace TwinCAT.TypeSystem.Generic
{
    using System;

    public interface INamespace<T> where T: IDataType
    {
        string Name { get; }

        ReadOnlyDataTypeCollection<T> DataTypes { get; }
    }
}

