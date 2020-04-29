namespace TwinCAT.TypeSystem
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using TwinCAT.Ads;

    public class SubItemCollection : IList<ITcAdsSubItem>, ICollection<ITcAdsSubItem>, IEnumerable<ITcAdsSubItem>, IEnumerable
    {
        private List<ITcAdsSubItem> _list;

        internal SubItemCollection(IEnumerable<ITcAdsSubItem> coll)
        {
            this._list.AddRange(coll);
        }

        public void Add(ITcAdsSubItem item)
        {
            this._list.Add(item);
        }

        public ReadOnlySubItemCollection AsReadOnly() => 
            new ReadOnlySubItemCollection(this._list);

        public void Clear()
        {
            this._list.Clear();
        }

        public bool Contains(ITcAdsSubItem item) => 
            this._list.Contains(item);

        public void CopyTo(ITcAdsSubItem[] array, int arrayIndex)
        {
            this._list.CopyTo(array, arrayIndex);
        }

        public IEnumerator<ITcAdsSubItem> GetEnumerator() => 
            this._list.GetEnumerator();

        public int IndexOf(ITcAdsSubItem item) => 
            this._list.IndexOf(item);

        public void Insert(int index, ITcAdsSubItem item)
        {
            this._list.Insert(index, item);
        }

        public bool Remove(ITcAdsSubItem item) => 
            this._list.Remove(item);

        public void RemoveAt(int index)
        {
            this._list.RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator() => 
            this._list.GetEnumerator();

        public ITcAdsSubItem this[int index]
        {
            get => 
                this._list[index];
            set => 
                (this._list[index] = value);
        }

        public int Count =>
            this._list.Count;

        public bool IsReadOnly =>
            false;
    }
}

