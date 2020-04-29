namespace TwinCAT.TypeSystem
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using TwinCAT.Ads;

    public class DimensionCollection : IDimensionCollection, IList<IDimension>, ICollection<IDimension>, IEnumerable<IDimension>, IEnumerable
    {
        private List<IDimension> _list;

        public DimensionCollection()
        {
            this._list = new List<IDimension>();
        }

        public DimensionCollection(IEnumerable<IDimension> coll)
        {
            this._list = new List<IDimension>();
            this._list.AddRange(coll);
        }

        public DimensionCollection(int[] dimLengths)
        {
            this._list = new List<IDimension>();
            for (int i = 0; i < dimLengths.Length; i++)
            {
                this._list.Add(new Dimension(0, dimLengths[i]));
            }
        }

        internal DimensionCollection(AdsDatatypeArrayInfo[] arrayInfos)
        {
            this._list = new List<IDimension>();
            if (arrayInfos == null)
            {
                throw new ArgumentNullException("arrayInfos");
            }
            int length = arrayInfos.GetLength(0);
            for (int i = 0; i < length; i++)
            {
                AdsDatatypeArrayInfo info = arrayInfos[i];
                Dimension item = new Dimension(info.LowerBound, info.Elements);
                this._list.Add(item);
            }
        }

        internal DimensionCollection(Array array)
        {
            this._list = new List<IDimension>();
            int rank = array.Rank;
            for (int i = 0; i < rank; i++)
            {
                this._list.Add(new Dimension(array.GetLowerBound(i), array.GetLength(i)));
            }
        }

        public DimensionCollection(int length)
        {
            this._list = new List<IDimension>();
            Dimension item = new Dimension(0, length);
            this._list.Add(item);
        }

        public void Add(IDimension item)
        {
            this._list.Add(item);
        }

        public ReadOnlyDimensionCollection AsReadOnly() => 
            new ReadOnlyDimensionCollection(this);

        public void Clear()
        {
            this._list.Clear();
        }

        public bool Contains(IDimension item) => 
            this._list.Contains(item);

        public void CopyTo(IDimension[] array, int arrayIndex)
        {
            this._list.CopyTo(array, arrayIndex);
        }

        public int[] GetDimensionLengths()
        {
            int[] upperBounds = this.UpperBounds;
            int[] lowerBounds = this.LowerBounds;
            int[] numArray3 = new int[upperBounds.Length];
            for (int i = 0; i < upperBounds.Length; i++)
            {
                numArray3[i] = (upperBounds[i] - lowerBounds[i]) + 1;
            }
            return numArray3;
        }

        public IEnumerator<IDimension> GetEnumerator() => 
            this._list.GetEnumerator();

        public int IndexOf(IDimension item) => 
            this._list.IndexOf(item);

        public void Insert(int index, IDimension item)
        {
            this._list.Insert(index, item);
        }

        public bool Remove(IDimension item) => 
            this._list.Remove(item);

        public void RemoveAt(int index)
        {
            this._list.RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator() => 
            this._list.GetEnumerator();

        internal AdsDatatypeArrayInfo[] ToArray()
        {
            AdsDatatypeArrayInfo[] infoArray = null;
            if (this._list.Count > 0)
            {
                infoArray = new AdsDatatypeArrayInfo[this.Count];
                for (int i = 0; i < this._list.Count; i++)
                {
                    IDimension dimension = this[i];
                    infoArray[i] = new AdsDatatypeArrayInfo(dimension.LowerBound, dimension.ElementCount);
                }
            }
            return infoArray;
        }

        public IDimension this[int index]
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

        public int ElementCount
        {
            get
            {
                int num = 0;
                if (this.Count > 0)
                {
                    num = 1;
                    foreach (IDimension dimension in this)
                    {
                        num *= dimension.ElementCount;
                    }
                }
                return num;
            }
        }

        public int[] LowerBounds
        {
            get
            {
                int[] numArray = new int[this.Count];
                for (int i = 0; i < this.Count; i++)
                {
                    numArray[i] = this[i].LowerBound;
                }
                return numArray;
            }
        }

        public int[] UpperBounds
        {
            get
            {
                int[] numArray = new int[this.Count];
                for (int i = 0; i < this.Count; i++)
                {
                    numArray[i] = (this[i].LowerBound + this[i].ElementCount) - 1;
                }
                return numArray;
            }
        }
    }
}

