namespace TwinCAT.TypeSystem
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    public interface IDimensionCollection : IList<IDimension>, ICollection<IDimension>, IEnumerable<IDimension>, IEnumerable
    {
        int[] GetDimensionLengths();

        int[] LowerBounds { get; }

        int[] UpperBounds { get; }

        int ElementCount { get; }
    }
}

