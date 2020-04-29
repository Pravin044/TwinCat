namespace TwinCAT.TypeSystem
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    public class ReadOnlySubItemCollection : ReadOnlyCollection<ITcAdsSubItem>
    {
        public ReadOnlySubItemCollection() : base(new List<ITcAdsSubItem>())
        {
        }

        public ReadOnlySubItemCollection(IList<ITcAdsSubItem> coll) : base(coll)
        {
        }
    }
}

