namespace TwinCAT.TypeSystem
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.InteropServices;
    using TwinCAT.Ads.TypeSystem;
    using TwinCAT.TypeSystem.Generic;

    [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
    public class SymbolNavigator<T> where T: ISymbol
    {
        private IInstanceCollection<T> _symbols;
        public const char DefaultPathSeparator = '.';
        private char _pathSeparator;

        public SymbolNavigator(IInstanceCollection<T> symbols)
        {
            this._pathSeparator = '.';
            if (symbols == null)
            {
                throw new ArgumentNullException("symbols");
            }
            this._symbols = symbols;
        }

        public SymbolNavigator(IInstanceCollection<T> symbols, char sep)
        {
            this._pathSeparator = '.';
            if (symbols == null)
            {
                throw new ArgumentNullException("symbols");
            }
            this._pathSeparator = sep;
            this._symbols = symbols;
        }

        private bool TryGetSubSymbol(IArrayInstance root, int[] indices, out T found)
        {
            IArrayType dataType = (IArrayType) root.DataType;
            ISymbol symbol = null;
            T local = default(T);
            if (!root.TryGetElement(indices, out symbol))
            {
                found = default(T);
                return false;
            }
            local = (T) symbol;
            found = local;
            return true;
        }

        private bool TryGetSubSymbol(T root, string[] relativePath, int index, out T found)
        {
            string instanceName = null;
            IList<int[]> list;
            SymbolParser.ArrayIndexType type;
            string indicesStr = null;
            IList<ISymbol> list2;
            if (!SymbolParser.TryParseArrayElement(relativePath[index], out instanceName, out indicesStr, out list, out type))
            {
                if (root.Category == DataTypeCategory.Struct)
                {
                    return this.TryGetSubSymbol((IStructInstance) root, relativePath, index, out found);
                }
                found = default(T);
                return false;
            }
            ((IStructInstance) root).MemberInstances.TryGetInstanceByName(instanceName, out list2);
            IArrayInstance instance2 = (IArrayInstance) list2[0];
            T local = default(T);
            bool flag = true;
            int num = 0;
            while (true)
            {
                if (num < list.Count)
                {
                    flag &= this.TryGetSubSymbol(instance2, list[num], out local);
                    if (flag)
                    {
                        instance2 = local as IArrayInstance;
                        num++;
                        continue;
                    }
                }
                if (flag)
                {
                    found = local;
                }
                else
                {
                    found = default(T);
                }
                return flag;
            }
        }

        private bool TryGetSubSymbol(IStructInstance root, string[] relativeInstancePath, int index, out T symbol)
        {
            IList<ISymbol> symbols = null;
            string instanceName = relativeInstancePath[index];
            if (!root.MemberInstances.TryGetInstanceByName(instanceName, out symbols) || (symbols.Count <= 0))
            {
                symbol = default(T);
                return false;
            }
            T local = symbols[0];
            index++;
            if (relativeInstancePath.Length > index)
            {
                return this.TryGetSubSymbol(local, relativeInstancePath, index, out symbol);
            }
            symbol = local;
            return true;
        }

        public bool TryGetSymbol(string path, out T found)
        {
            int index = 0;
            char[] separator = new char[] { this.PathSeparator };
            string[] relativePath = path.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            T root = default(T);
            if (relativePath.Length != 0)
            {
                IList<T> symbols = null;
                if (this._symbols.TryGetInstanceByName(relativePath[index], out symbols) && (symbols.Count > 0))
                {
                    root = symbols[0];
                    index++;
                    if (relativePath.Length > index)
                    {
                        return this.TryGetSubSymbol(root, relativePath, index, out found);
                    }
                    found = root;
                    return true;
                }
            }
            found = default(T);
            return false;
        }

        public char PathSeparator
        {
            get => 
                this._pathSeparator;
            set => 
                (this._pathSeparator = value);
        }
    }
}

