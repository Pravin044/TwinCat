namespace TwinCAT.TypeSystem
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.InteropServices;

    [DebuggerDisplay("Type = { Type }, Category = {Category}")]
    public class AnyTypeSpecifier
    {
        public readonly DataTypeCategory Category;
        public readonly System.Type Type;
        public readonly int StrLen;
        public readonly IList<IDimensionCollection> DimLengths;
        public readonly AnyTypeSpecifier ElementType;

        public AnyTypeSpecifier(object prototype)
        {
            System.Type type = prototype.GetType();
            this.Type = type;
            if (!type.IsArray)
            {
                if (type != typeof(string))
                {
                    this.Category = DataTypeCategory.Primitive;
                    this.StrLen = -1;
                    this.DimLengths = null;
                    this.ElementType = null;
                }
                else
                {
                    string str = (string) prototype;
                    this.Category = DataTypeCategory.String;
                    this.StrLen = str.Length;
                    this.DimLengths = null;
                    this.ElementType = null;
                }
            }
            else
            {
                Array array = (Array) prototype;
                int rank = array.Rank;
                int[] indices = new int[rank];
                int[] numArray2 = new int[rank];
                DimensionCollection dimensions = new DimensionCollection();
                for (int i = 0; i < rank; i++)
                {
                    indices[i] = array.GetLowerBound(i);
                    numArray2[i] = array.GetLength(i);
                    dimensions.Add(new Dimension(indices[i], numArray2[i]));
                }
                this.Category = DataTypeCategory.Array;
                this.StrLen = -1;
                this.DimLengths = new List<IDimensionCollection> { dimensions.AsReadOnly() }.AsReadOnly();
                System.Type elementType = this.Type.GetElementType();
                if (array.Length <= 0)
                {
                    this.ElementType = new AnyTypeSpecifier(elementType);
                }
                else
                {
                    object obj2 = array.GetValue(indices);
                    this.ElementType = new AnyTypeSpecifier(obj2);
                }
            }
        }

        public AnyTypeSpecifier(System.Type type)
        {
            this.StrLen = -1;
            this.DimLengths = null;
            this.ElementType = null;
            if (!type.IsArray)
            {
                this.Category = (type != typeof(string)) ? DataTypeCategory.Primitive : DataTypeCategory.String;
            }
            else
            {
                this.Category = DataTypeCategory.Array;
                this.ElementType = new AnyTypeSpecifier(type.GetElementType());
            }
            this.Type = type;
        }

        public AnyTypeSpecifier(System.Type type, IList<IDimensionCollection> dimLengths) : this(type, dimLengths, 0)
        {
        }

        public AnyTypeSpecifier(System.Type type, int strLen) : this(type)
        {
            if (type != typeof(string))
            {
                throw new ArgumentOutOfRangeException("type");
            }
            this.StrLen = strLen;
        }

        private AnyTypeSpecifier(System.Type type, IList<IDimensionCollection> dimLengths, int jaggedLevel) : this(type)
        {
            if (!type.IsArray)
            {
                throw new ArgumentOutOfRangeException("type");
            }
            this.DimLengths = dimLengths;
            System.Type elementType = type.GetElementType();
            if (!elementType.IsArray)
            {
                this.ElementType = new AnyTypeSpecifier(elementType);
            }
            else
            {
                IList<IDimensionCollection> list = new List<IDimensionCollection>();
                for (int i = 1; i < dimLengths.Count; i++)
                {
                    list.Add(dimLengths[i]);
                }
                this.ElementType = new AnyTypeSpecifier(elementType, list, ++jaggedLevel);
            }
        }

        internal void GetAnyTypeArgs(out System.Type tp, out int[] args)
        {
            if (this.Category != DataTypeCategory.Array)
            {
                if (this.Category != DataTypeCategory.String)
                {
                    tp = this.Type;
                    args = null;
                }
                else
                {
                    tp = this.Type;
                    int[] numArray3 = new int[] { this.StrLen };
                    args = numArray3;
                }
            }
            else
            {
                tp = this.Type;
                int num = 1;
                foreach (IDimensionCollection dimensions in this.DimLengths)
                {
                    num *= dimensions.ElementCount;
                }
                if (this.ElementType.Category != DataTypeCategory.String)
                {
                    int[] numArray2 = new int[] { num };
                    args = numArray2;
                }
                else
                {
                    int[] numArray1 = new int[] { num, this.ElementType.StrLen };
                    args = numArray1;
                }
            }
        }
    }
}

