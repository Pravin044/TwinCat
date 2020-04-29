namespace TwinCAT.TypeSystem
{
    using System;

    public interface IEnumValue
    {
        string Name { get; }

        object Primitive { get; }

        byte[] RawValue { get; }

        Type ManagedBaseType { get; }

        int Size { get; }
    }
}

