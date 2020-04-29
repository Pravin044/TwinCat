namespace TwinCAT.Ads
{
    using System;

    internal class AdsArrayDimensionsInfo
    {
        private AdsDatatypeArrayInfo[] _dims;

        internal AdsArrayDimensionsInfo(AdsDatatypeArrayInfo[] dims)
        {
            if (dims == null)
            {
                throw new ArgumentNullException("dims");
            }
            this._dims = dims;
        }

        internal static int GetArrayElementCount(AdsDatatypeArrayInfo[] arrayInfo)
        {
            if (arrayInfo == null)
            {
                throw new ArgumentNullException("arrayInfo");
            }
            int elements = 0;
            if (arrayInfo.Length != 0)
            {
                elements = arrayInfo[0].Elements;
                for (int i = 1; i < arrayInfo.Length; i++)
                {
                    elements *= arrayInfo[i].Elements;
                }
            }
            return elements;
        }

        internal int Elements =>
            GetArrayElementCount(this._dims);

        internal int[] LowerBounds
        {
            get
            {
                int[] numArray = new int[this._dims.Length];
                for (int i = 0; i < this._dims.Length; i++)
                {
                    numArray[i] = this._dims[i].LowerBound;
                }
                return numArray;
            }
        }

        internal int[] UpperBounds
        {
            get
            {
                int[] numArray = new int[this._dims.Length];
                for (int i = 0; i < this._dims.Length; i++)
                {
                    numArray[i] = (this._dims[i].LowerBound + this._dims[i].Elements) - 1;
                }
                return numArray;
            }
        }

        internal int[] DimensionElements
        {
            get
            {
                int[] numArray = new int[this._dims.Length];
                for (int i = 0; i < this._dims.Length; i++)
                {
                    numArray[i] = this._dims[i].Elements;
                }
                return numArray;
            }
        }
    }
}

