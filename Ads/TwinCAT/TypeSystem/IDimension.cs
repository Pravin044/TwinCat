namespace TwinCAT.TypeSystem
{
    using System;

    public interface IDimension
    {
        int ElementCount { get; }

        int LowerBound { get; }
    }
}

