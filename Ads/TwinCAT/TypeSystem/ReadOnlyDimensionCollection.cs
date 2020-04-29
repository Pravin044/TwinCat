namespace TwinCAT.TypeSystem
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using TwinCAT.Ads;

    public class ReadOnlyDimensionCollection : ReadOnlyCollection<IDimension>, IDimensionCollection, IList<IDimension>, ICollection<IDimension>, IEnumerable<IDimension>, IEnumerable
    {
        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        public ReadOnlyDimensionCollection(DimensionCollection coll) : base(coll)
        {
        }

        public int[] GetDimensionLengths() => 
            ((DimensionCollection) base.Items).GetDimensionLengths();

        internal AdsDatatypeArrayInfo[] ToArray() => 
            ((DimensionCollection) base.Items).ToArray();

        public int ElementCount =>
            ((DimensionCollection) base.Items).ElementCount;

        public int[] LowerBounds =>
            ((DimensionCollection) base.Items).LowerBounds;

        public int[] UpperBounds =>
            ((DimensionCollection) base.Items).UpperBounds;
    }
}

