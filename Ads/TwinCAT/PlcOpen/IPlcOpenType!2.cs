namespace TwinCAT.PlcOpen
{
    using System.ComponentModel;

    [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
    public interface IPlcOpenType<T1, T2> : IPlcOpenType
    {
        T1 Value { get; }

        T2 Ticks { get; }
    }
}

