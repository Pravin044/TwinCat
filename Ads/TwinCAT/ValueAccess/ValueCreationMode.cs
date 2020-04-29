namespace TwinCAT.ValueAccess
{
    using System;

    [Flags]
    public enum ValueCreationMode
    {
        None = 0,
        Primitives = 1,
        Enums = 2,
        FullDynamics = 4,
        PlcOpenTypes = 8,
        Default = 1
    }
}

