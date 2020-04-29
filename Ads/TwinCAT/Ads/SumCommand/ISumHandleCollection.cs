namespace TwinCAT.Ads.SumCommand
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;

    [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
    public interface ISumHandleCollection : IList<SumHandleEntry>, ICollection<SumHandleEntry>, IEnumerable<SumHandleEntry>, IEnumerable
    {
        uint[] ValidHandles { get; }
    }
}

