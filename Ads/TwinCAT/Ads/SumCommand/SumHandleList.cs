namespace TwinCAT.Ads.SumCommand
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.CompilerServices;

    [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
    internal class SumHandleList : List<SumHandleEntry>, ISumHandleCollection, IList<SumHandleEntry>, ICollection<SumHandleEntry>, IEnumerable<SumHandleEntry>, IEnumerable
    {
        public uint[] ValidHandles =>
            Enumerable.ToArray<uint>((IEnumerable<uint>) (from e in this
                select e.Handle into h
                where h != 0
                select h));

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly SumHandleList.<>c <>9 = new SumHandleList.<>c();
            public static Func<SumHandleEntry, uint> <>9__1_0;
            public static Func<uint, bool> <>9__1_1;

            internal uint <get_ValidHandles>b__1_0(SumHandleEntry e) => 
                e.Handle;

            internal bool <get_ValidHandles>b__1_1(uint h) => 
                (h != 0);
        }
    }
}

