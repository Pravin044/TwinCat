namespace TwinCAT.Ads
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Reflection;
    using System.Runtime.InteropServices;

    public class ReadOnlyTcAdsDataTypeCollection : ReadOnlyCollection<ITcAdsDataType>, IEnumerable<IDataType>, IEnumerable
    {
        internal ReadOnlyTcAdsDataTypeCollection(TcAdsDataTypeCollection coll) : base(coll)
        {
        }

        public bool Contains(string typeName) => 
            ((TcAdsDataTypeCollection) base.Items).Contains(typeName);

        IEnumerator<IDataType> IEnumerable<IDataType>.GetEnumerator() => 
            ((IEnumerator<IDataType>) base.GetEnumerator());

        public bool TryGetDataType(string typeName, out ITcAdsDataType type) => 
            ((TcAdsDataTypeCollection) base.Items).TryGetDataType(typeName, out type);

        public ITcAdsDataType this[string typeName] =>
            ((TcAdsDataTypeCollection) base.Items)[typeName];
    }
}

