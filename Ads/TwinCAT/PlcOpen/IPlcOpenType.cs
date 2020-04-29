namespace TwinCAT.PlcOpen
{
    using System;
    using System.ComponentModel;

    [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
    public interface IPlcOpenType
    {
        Type ManagedValueType { get; }

        Type TicksValueType { get; }

        object UntypedValue { get; }
    }
}

