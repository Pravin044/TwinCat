namespace TwinCAT.TypeSystem
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using TwinCAT.Ads.TypeSystem;
    using TwinCAT.TypeSystem.Generic;

    internal class ArrayElementSymbolCollection : ISymbolCollection, ISymbolCollection<ISymbol>, IInstanceCollection<ISymbol>, IList<ISymbol>, ICollection<ISymbol>, IEnumerable<ISymbol>, IEnumerable
    {
        private ISymbol _arrayInstance;
        private IArrayType _arrayType;
        private ISymbolFactory _symbolFactory;

        internal ArrayElementSymbolCollection(ISymbol arrayInstance, IArrayType arrayType, ISymbolFactory factory)
        {
            this._arrayInstance = arrayInstance;
            this._arrayType = arrayType;
            this._symbolFactory = factory;
        }

        public void Add(ISymbol item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(string instancePath)
        {
            SymbolParser.ArrayIndexType type;
            if (!instancePath.StartsWith(this._arrayInstance.InstancePath, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            IList<int[]> jaggedIndices = null;
            return (SymbolParser.TryParseIndices(instancePath.Substring(this._arrayInstance.InstancePath.Length), out jaggedIndices, out type) && ArrayIndexConverter.TryCheckIndices(jaggedIndices, this._arrayType));
        }

        public bool Contains(ISymbol element)
        {
            string instancePath = element.InstancePath;
            return this.Contains(instancePath);
        }

        public bool ContainsName(string instanceNameWithIndices)
        {
            string str;
            SymbolParser.ArrayIndexType type;
            IList<int[]> list;
            string indicesStr = null;
            return (SymbolParser.TryParseArrayElement(instanceNameWithIndices, out str, out indicesStr, out list, out type) && ((StringComparer.OrdinalIgnoreCase.Compare(str, this._arrayInstance.InstanceName) == 0) && ArrayIndexConverter.TryCheckIndices(list, this._arrayType)));
        }

        public void CopyTo(ISymbol[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<ISymbol> GetEnumerator()
        {
            IEnumerator<int[]> enumerator = new ArrayIndexIterator(this._arrayType).GetEnumerator();
        Label_PostSwitchInIterator:;
            if (enumerator.MoveNext())
            {
                int[] currentIndex = enumerator.Current;
                ISymbol symbol = this._symbolFactory.CreateArrayElement(currentIndex, this._arrayInstance, this._arrayType);
                yield return symbol;
                goto Label_PostSwitchInIterator;
            }
            else
            {
                enumerator = null;
                if (this.IsOversampled)
                {
                    ISymbol symbol2 = ((ISymbolFactoryOversampled) this._symbolFactory).CreateOversamplingElement(this._arrayInstance);
                    yield return symbol2;
                }
            }
        }

        public ISymbol GetInstance(string instancePath)
        {
            ISymbol symbol = null;
            this.TryGetInstance(instancePath, out symbol);
            return symbol;
        }

        public IList<ISymbol> GetInstanceByName(string instanceNameWithIndices)
        {
            IList<ISymbol> symbols = null;
            this.TryGetInstanceByName(instanceNameWithIndices, out symbols);
            return symbols;
        }

        public int IndexOf(ISymbol item)
        {
            string instancePath = item.InstancePath;
            if (instancePath.StartsWith(this._arrayInstance.InstancePath, StringComparison.OrdinalIgnoreCase))
            {
                SymbolParser.ArrayIndexType type;
                IList<int[]> jaggedIndices = null;
                if (SymbolParser.TryParseIndices(instancePath.Substring(this._arrayInstance.InstancePath.Length), out jaggedIndices, out type))
                {
                    return ArrayIndexConverter.IndicesToSubIndex(jaggedIndices[0], this._arrayType);
                }
            }
            throw new ArgumentOutOfRangeException("item");
        }

        public void Insert(int index, ISymbol item)
        {
            throw new NotImplementedException();
        }

        public bool Remove(ISymbol item)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator() => 
            this.GetEnumerator();

        public bool TryGetInstance(string instancePath, out ISymbol symbol)
        {
            symbol = null;
            if (instancePath.StartsWith(this._arrayInstance.InstancePath, StringComparison.OrdinalIgnoreCase))
            {
                SymbolParser.ArrayIndexType type;
                IList<int[]> jaggedIndices = null;
                if (SymbolParser.TryParseIndices(instancePath.Substring(this._arrayInstance.InstancePath.Length), out jaggedIndices, out type))
                {
                    if (this.IsOversampled && ArrayIndexConverter.IsOversamplingIndex(jaggedIndices[0], this._arrayType))
                    {
                        symbol = ((ISymbolFactoryOversampled) this._symbolFactory).CreateOversamplingElement(this._arrayInstance);
                    }
                    else
                    {
                        ISymbol parent = this._arrayInstance;
                        IArrayType arrayType = this._arrayType;
                        for (int i = 0; i < jaggedIndices.Count; i++)
                        {
                            symbol = this._symbolFactory.CreateArrayElement(jaggedIndices[i], parent, arrayType);
                            parent = symbol;
                            arrayType = symbol.DataType as IArrayType;
                        }
                    }
                    return true;
                }
            }
            return false;
        }

        public bool TryGetInstanceByName(string instanceNameWithIndices, out IList<ISymbol> symbols)
        {
            ISymbol item = null;
            string str;
            SymbolParser.ArrayIndexType type;
            IList<int[]> list;
            symbols = new List<ISymbol>();
            string indicesStr = null;
            if (SymbolParser.TryParseArrayElement(instanceNameWithIndices, out str, out indicesStr, out list, out type) && (StringComparer.OrdinalIgnoreCase.Compare(str, this._arrayInstance.InstanceName) == 0))
            {
                ISymbol parent = this._arrayInstance;
                IArrayType arrayType = this._arrayType;
                for (int i = 0; i < list.Count; i++)
                {
                    item = this._symbolFactory.CreateArrayElement(list[i], parent, arrayType);
                    parent = item;
                    arrayType = parent.DataType as IArrayType;
                }
            }
            if (item != null)
            {
                symbols.Add(item);
            }
            return (symbols.Count > 0);
        }

        private bool IsOversampled =>
            (this._arrayInstance is IOversamplingArrayInstance);

        public ISymbol this[int index]
        {
            get
            {
                if (this.IsOversampled && ArrayIndexConverter.IsOversamplingElement(index, this._arrayType.Dimensions.LowerBounds, this._arrayType.Dimensions.UpperBounds))
                {
                    return ((ISymbolFactoryOversampled) this._symbolFactory).CreateOversamplingElement(this._arrayInstance);
                }
                int[] currentIndex = ArrayIndexConverter.SubIndexToIndices(index, this._arrayType);
                return this._symbolFactory.CreateArrayElement(currentIndex, this._arrayInstance, this._arrayType);
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public ISymbol this[string instancePath]
        {
            get
            {
                ISymbol instance = this.GetInstance(instancePath);
                if (instance == null)
                {
                    throw new ArgumentException("Symbol not found!");
                }
                return instance;
            }
        }

        public int Count
        {
            get
            {
                int elementCount = this._arrayType.Dimensions.ElementCount;
                if (this.IsOversampled)
                {
                    elementCount++;
                }
                return elementCount;
            }
        }

        public bool IsReadOnly =>
            true;

        public InstanceCollectionMode Mode =>
            InstanceCollectionMode.Names;

    }
}

