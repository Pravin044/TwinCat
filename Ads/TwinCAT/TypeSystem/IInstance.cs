namespace TwinCAT.TypeSystem
{
    using System;

    public interface IInstance : IBitSize
    {
        IDataType DataType { get; }

        string TypeName { get; }

        string InstanceName { get; }

        string InstancePath { get; }

        bool IsStatic { get; }

        bool IsReference { get; }

        bool IsPointer { get; }

        string Comment { get; }
    }
}

