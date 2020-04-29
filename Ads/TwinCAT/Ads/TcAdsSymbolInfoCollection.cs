namespace TwinCAT.Ads
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using TwinCAT.Ads.Internal;
    using TwinCAT.Ads.TypeSystem;

    public class TcAdsSymbolInfoCollection : ICollection, IEnumerable
    {
        private int _count;
        private TcAdsSymbolInfo _owner;
        private AdsParseSymbols _symbolParser;
        private bool _isEnumerating;

        internal TcAdsSymbolInfoCollection(AdsParseSymbols symbolParser)
        {
            this._owner = null;
            this._symbolParser = symbolParser;
            this._count = symbolParser.SymbolCount;
        }

        internal TcAdsSymbolInfoCollection(TcAdsSymbolInfo owner)
        {
            this._owner = owner;
            this._symbolParser = owner.symbolParser;
            this._count = this._symbolParser.GetSubSymbolCount(owner);
        }

        public virtual void CopyTo(Array array, int index)
        {
            if (array == null)
            {
                throw new ArgumentNullException("Null array reference", "array");
            }
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException("Index is out of range");
            }
            if (array.Rank > 1)
            {
                throw new ArgumentException("Array is multi-dimensional");
            }
            foreach (object obj2 in this)
            {
                array.SetValue(obj2, index);
                index++;
            }
        }

        public virtual IEnumerator GetEnumerator()
        {
            this._isEnumerating = true;
            return new AdsSymbolEnumerator(this);
        }

        public TcAdsSymbolInfo GetSymbol(int index) => 
            ((this._owner != null) ? this._symbolParser.GetSubSymbol(this._owner, index, true) : this._symbolParser.GetSymbol(index));

        public TcAdsSymbolInfo GetSymbol(string name)
        {
            TcAdsSymbolInfo info = null;
            string str;
            string str2;
            IList<int[]> list;
            StringComparer ordinalIgnoreCase = StringComparer.OrdinalIgnoreCase;
            SymbolParser.ArrayIndexType standard = SymbolParser.ArrayIndexType.Standard;
            bool flag = SymbolParser.TryParseArrayElement(name, out str, out str2, out list, out standard);
            if (!flag)
            {
                str = name;
            }
            foreach (TcAdsSymbolInfo info2 in this)
            {
                if (ordinalIgnoreCase.Compare(info2.ShortName, str) == 0)
                {
                    info = !flag ? info2 : info2.symbolParser.GetSymbol(name);
                    break;
                }
            }
            return info;
        }

        public TcAdsSymbolInfo this[int index] =>
            this.GetSymbol(index);

        public virtual int Count =>
            this._count;

        public virtual bool IsSynchronized =>
            false;

        public virtual object SyncRoot =>
            this;

        internal class AdsSymbolEnumerator : IEnumerator
        {
            private TcAdsSymbolInfo curSymbol;
            private int curIndex;
            private TcAdsSymbolInfoCollection symbolCollection;
            private bool isValid;

            public AdsSymbolEnumerator(TcAdsSymbolInfoCollection symbolCollection)
            {
                this.symbolCollection = symbolCollection;
                this.isValid = true;
                this.curIndex = 0;
            }

            private void CheckValid()
            {
                if (!this.isValid || !this.symbolCollection._isEnumerating)
                {
                    throw new InvalidOperationException();
                }
            }

            public virtual bool MoveNext()
            {
                TcAdsSymbolInfo symbol;
                int curIndex;
                this.CheckValid();
                if (this.symbolCollection._owner == null)
                {
                    curIndex = this.curIndex;
                    this.curIndex = curIndex + 1;
                    symbol = this.symbolCollection._symbolParser.GetSymbol(curIndex);
                }
                else
                {
                    curIndex = this.curIndex;
                    this.curIndex = curIndex + 1;
                    symbol = this.symbolCollection._symbolParser.GetSubSymbol(this.symbolCollection._owner, curIndex, true);
                }
                if (symbol == null)
                {
                    this.isValid = false;
                }
                this.curSymbol = symbol;
                return this.isValid;
            }

            public virtual void Reset()
            {
                if (!this.symbolCollection._isEnumerating)
                {
                    throw new InvalidOperationException();
                }
                this.curSymbol = null;
                this.curIndex = 0;
                this.isValid = true;
            }

            public object Current
            {
                get
                {
                    this.CheckValid();
                    return this.curSymbol;
                }
            }
        }
    }
}

