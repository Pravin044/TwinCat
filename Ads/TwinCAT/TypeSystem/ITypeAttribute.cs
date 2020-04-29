namespace TwinCAT.TypeSystem
{
    using System;

    public interface ITypeAttribute
    {
        string Name { get; }

        string Value { get; }
    }
}

