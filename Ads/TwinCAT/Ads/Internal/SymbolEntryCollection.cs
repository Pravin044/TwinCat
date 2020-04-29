namespace TwinCAT.Ads.Internal
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.InteropServices;

    internal class SymbolEntryCollection : IList<AdsSymbolEntry>, ICollection<AdsSymbolEntry>, IEnumerable<AdsSymbolEntry>, IEnumerable
    {
        private List<AdsSymbolEntry> _list;
        private Dictionary<string, List<AdsSymbolEntry>> _table;

        public SymbolEntryCollection(int capacity)
        {
            this._list = new List<AdsSymbolEntry>(capacity);
            this._table = new Dictionary<string, List<AdsSymbolEntry>>(capacity, StringComparer.OrdinalIgnoreCase);
        }

        public void Add(AdsSymbolEntry item)
        {
            this.addTableEntry(item);
            this._list.Add(item);
        }

        private void addTableEntry(AdsSymbolEntry entry)
        {
            if (!this._table.ContainsKey(entry.name))
            {
                List<AdsSymbolEntry> list1 = new List<AdsSymbolEntry>();
                list1.Add(entry);
                this._table.Add(entry.name, list1);
            }
            this._table[entry.name].Add(entry);
        }

        public void Clear()
        {
            this._table.Clear();
            this._list.Clear();
        }

        public bool Contains(AdsSymbolEntry item) => 
            this._table.ContainsKey(item.name);

        public void CopyTo(AdsSymbolEntry[] array, int arrayIndex)
        {
            this._list.CopyTo(array, arrayIndex);
        }

        public IEnumerator<AdsSymbolEntry> GetEnumerator() => 
            this._list.GetEnumerator();

        public int IndexOf(AdsSymbolEntry item) => 
            this._list.IndexOf(item);

        public void Insert(int index, AdsSymbolEntry item)
        {
            this.addTableEntry(item);
            this._list.Insert(index, item);
        }

        public bool Remove(AdsSymbolEntry item)
        {
            bool flag = this.removeTableEntry(item);
            this._list.Remove(item);
            return flag;
        }

        public void RemoveAt(int index)
        {
            AdsSymbolEntry entry = this._list[index];
            this.removeTableEntry(entry);
            this._list.RemoveAt(index);
        }

        private bool removeTableEntry(AdsSymbolEntry entry)
        {
            List<AdsSymbolEntry> list = null;
            return (this._table.TryGetValue(entry.name, out list) && list.Remove(entry));
        }

        IEnumerator IEnumerable.GetEnumerator() => 
            this._list.GetEnumerator();

        public bool TryGetSymbol(int index, out AdsSymbolEntry entry)
        {
            if ((index < 0) || (index >= this._list.Count))
            {
                entry = null;
                return false;
            }
            entry = this._list[index];
            return true;
        }

        public bool TryGetSymbol(string name, out IList<AdsSymbolEntry> entry)
        {
            List<AdsSymbolEntry> list = null;
            if (this._table.TryGetValue(name, out list))
            {
                entry = list.AsReadOnly();
                return true;
            }
            entry = null;
            return false;
        }

        public AdsSymbolEntry this[int index]
        {
            get => 
                this._list[index];
            set
            {
                throw new NotImplementedException();
            }
        }

        public int Count =>
            this._list.Count;

        public bool IsReadOnly =>
            false;
    }
}

