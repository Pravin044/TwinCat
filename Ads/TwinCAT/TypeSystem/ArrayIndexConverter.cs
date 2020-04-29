namespace TwinCAT.TypeSystem
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using TwinCAT.Ads;
    using TwinCAT.Ads.TypeSystem;

    public static class ArrayIndexConverter
    {
        public static int ArraySubElementCount(int[] lowerBounds, int[] upperBounds, bool oversampled)
        {
            int num = 1;
            for (int i = 0; i < lowerBounds.Length; i++)
            {
                int num3 = Math.Abs((int) (upperBounds[i] - lowerBounds[i])) + 1;
                num *= num3;
            }
            if (oversampled)
            {
                num++;
            }
            return num;
        }

        public static int[] CalcSubIndexArray(int[] lowerBounds, int[] upperBounds)
        {
            if (lowerBounds == null)
            {
                throw new ArgumentNullException("lowerBounds");
            }
            if (upperBounds == null)
            {
                throw new ArgumentNullException("upperBounds");
            }
            if (lowerBounds.Length != upperBounds.Length)
            {
                throw new ArgumentException("Dimensions mismatch!");
            }
            int length = lowerBounds.Length;
            int[] numArray = new int[length];
            for (int i = 0; i < length; i++)
            {
                numArray[i] = (Math.Max(upperBounds[i], lowerBounds[i]) - Math.Min(upperBounds[i], lowerBounds[i])) + 1;
            }
            int[] numArray2 = new int[length];
            int index = lowerBounds.Length - 1;
            numArray2[index] = 1;
            for (int j = index - 1; j >= 0; j--)
            {
                numArray2[j] = numArray2[j + 1] * numArray[j + 1];
            }
            return numArray2;
        }

        public static void CheckIndices(int[] indices, int[] lowerBounds, int[] upperBounds, bool normalized, bool oversampled)
        {
            if (!TryCheckIndices(indices, lowerBounds, upperBounds, normalized, oversampled))
            {
                throw new ArgumentOutOfRangeException("indices");
            }
        }

        public static int[] DenormalizeIndices(int[] normalizedIndices, IArrayType type) => 
            DenormalizeIndices(normalizedIndices, type.Dimensions.LowerBounds, type.Dimensions.UpperBounds, IsOversampled(type));

        public static int[] DenormalizeIndices(int[] normalizedIndices, int[] lowerBounds, int[] upperBounds, bool oversampled)
        {
            CheckIndices(normalizedIndices, lowerBounds, upperBounds, true, oversampled);
            int[] numArray = new int[lowerBounds.Length];
            for (int i = 0; i < lowerBounds.Length; i++)
            {
                numArray[i] = (lowerBounds[i] > upperBounds[i]) ? (lowerBounds[i] - normalizedIndices[i]) : (lowerBounds[i] + normalizedIndices[i]);
            }
            return numArray;
        }

        public static string IndicesToString(int[] indices) => 
            $"[{string.Join<int>(",", indices)}]";

        public static int IndicesToSubIndex(int[] indices, IArrayType type) => 
            IndicesToSubIndex(indices, type, false);

        public static int IndicesToSubIndex(int[] indices, IArrayType type, bool normalizedIndices)
        {
            int[] numArray = indices;
            bool oversampled = IsOversampled(type);
            if (normalizedIndices)
            {
                numArray = DenormalizeIndices(indices, type.Dimensions.LowerBounds, type.Dimensions.UpperBounds, oversampled);
            }
            int subIndex = -1;
            if (!TryGetSubIndex(numArray, type.Dimensions.LowerBounds, type.Dimensions.UpperBounds, oversampled, out subIndex))
            {
                throw new ArgumentOutOfRangeException("indices");
            }
            return subIndex;
        }

        public static int IndicesToSubIndex(int[] indices, int[] lowerBounds, int[] upperBounds, bool oversampled)
        {
            int subIndex = -1;
            if (!TryGetSubIndex(indices, lowerBounds, upperBounds, oversampled, out subIndex))
            {
                throw new ArgumentOutOfRangeException("indices");
            }
            return subIndex;
        }

        internal static bool IsOversampled(IArrayType type)
        {
            IOversamplingSupport support = type as IOversamplingSupport;
            return ((support != null) && support.IsOversampled);
        }

        internal static bool IsOversamplingElement(int subIndex, int[] lowerBounds, int[] upperBounds)
        {
            if (lowerBounds == null)
            {
                throw new ArgumentNullException("lowerBounds");
            }
            if (upperBounds == null)
            {
                throw new ArgumentNullException("upperBounds");
            }
            if (lowerBounds.Length != upperBounds.Length)
            {
                throw new ArgumentException("Dimensions mismatch!");
            }
            if (lowerBounds.Length != 1)
            {
                throw new ArgumentOutOfRangeException("Oversampling arrays only support one Dimension!");
            }
            return IsOversamplingIndex(SubIndexToIndices(subIndex, lowerBounds, upperBounds, true), lowerBounds, upperBounds);
        }

        internal static bool IsOversamplingIndex(int[] indices, IArrayType type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            if (indices == null)
            {
                throw new ArgumentNullException("indices");
            }
            IOversamplingSupport support = type as IOversamplingSupport;
            if ((support == null) || !support.IsOversampled)
            {
                throw new ArgumentException("Specified type is not an Oversampling type", "type");
            }
            return IsOversamplingIndex(indices, type.Dimensions.LowerBounds, type.Dimensions.UpperBounds);
        }

        internal static bool IsOversamplingIndex(int[] indices, int[] lowerBounds, int[] upperBounds)
        {
            if (lowerBounds == null)
            {
                throw new ArgumentNullException("lowerBounds");
            }
            if (upperBounds == null)
            {
                throw new ArgumentNullException("upperBounds");
            }
            if (lowerBounds.Length != upperBounds.Length)
            {
                throw new ArgumentException("Dimensions mismatch!");
            }
            if (lowerBounds.Length != 1)
            {
                throw new ArgumentOutOfRangeException("Oversampling arrays only support one Dimension!");
            }
            CheckIndices(indices, lowerBounds, upperBounds, false, true);
            return ((lowerBounds[0] > upperBounds[0]) ? (indices[0] == (upperBounds[0] - 1)) : (indices[0] == (upperBounds[0] + 1)));
        }

        public static int[] NormalizeIndices(int[] indices, IArrayType type) => 
            NormalizeIndices(indices, type.Dimensions.LowerBounds, type.Dimensions.UpperBounds, IsOversampled(type));

        public static int[] NormalizeIndices(int[] indices, int[] lowerBounds, int[] upperBounds, bool oversampled)
        {
            CheckIndices(indices, lowerBounds, upperBounds, false, oversampled);
            int[] numArray = new int[lowerBounds.Length];
            for (int i = 0; i < lowerBounds.Length; i++)
            {
                numArray[i] = Math.Abs((int) (indices[i] - lowerBounds[i]));
            }
            return numArray;
        }

        public static string OversamplingSubElementToString(int elementCount) => 
            $"[T{elementCount:d}]";

        public static IList<int[]> StringToIndices(string indices)
        {
            IList<int[]> jaggedIndices = null;
            SymbolParser.ArrayIndexType standard = SymbolParser.ArrayIndexType.Standard;
            SymbolParser.TryParseIndices(indices, out jaggedIndices, out standard);
            return jaggedIndices;
        }

        public static string SubIndexToIndexString(int[] lowerBounds, int[] upperBounds, int subIndex)
        {
            bool flag = TryIsOversamplingElement(subIndex, lowerBounds, upperBounds);
            if ((lowerBounds.Length == 1) & flag)
            {
                return OversamplingSubElementToString(subIndex);
            }
            return IndicesToString(SubIndexToIndices(subIndex, lowerBounds, upperBounds, false));
        }

        public static int[] SubIndexToIndices(int subIndex, IArrayType type)
        {
            IOversamplingSupport support = type as IOversamplingSupport;
            return ((support == null) ? SubIndexToIndices(subIndex, type.Dimensions.LowerBounds, type.Dimensions.UpperBounds, false) : SubIndexToIndices(subIndex, type.Dimensions.LowerBounds, type.Dimensions.UpperBounds, support.IsOversampled));
        }

        public static int[] SubIndexToIndices(int subIndex, int[] lowerBounds, int[] upperBounds, bool oversampled)
        {
            if (lowerBounds == null)
            {
                throw new ArgumentNullException("lowerBounds");
            }
            if (upperBounds == null)
            {
                throw new ArgumentNullException("upperBounds");
            }
            if (lowerBounds.Length != upperBounds.Length)
            {
                throw new ArgumentException("Dimensions mismatch!");
            }
            if (subIndex < 0)
            {
                throw new ArgumentOutOfRangeException("subIndex");
            }
            if (oversampled && (lowerBounds.Length != 1))
            {
                throw new ArgumentException("Oversampling arrays only support one dimension!");
            }
            int length = lowerBounds.Length;
            int num2 = subIndex;
            int[] normalizedIndices = new int[length];
            int[] numArray2 = CalcSubIndexArray(lowerBounds, upperBounds);
            for (int i = 0; i < length; i++)
            {
                normalizedIndices[i] = num2 / numArray2[i];
                num2 = num2 % numArray2[i];
            }
            return DenormalizeIndices(normalizedIndices, lowerBounds, upperBounds, oversampled);
        }

        public static bool TryCheckElement(int subElement, int[] lowerBounds, int[] upperBounds, bool oversampled)
        {
            int num = ArraySubElementCount(lowerBounds, upperBounds, oversampled);
            return ((subElement >= 0) && (subElement < num));
        }

        public static bool TryCheckIndices(IList<int[]> indices, ITcAdsDataType type)
        {
            if (indices == null)
            {
                throw new ArgumentNullException("indices");
            }
            if (type.Category != DataTypeCategory.Array)
            {
                throw new ArgumentOutOfRangeException("type");
            }
            bool flag = true;
            bool isOversamplingArray = type.IsOversamplingArray;
            IDataType baseType = type;
            foreach (int[] numArray in indices)
            {
                ITcAdsDataType type3 = (ITcAdsDataType) baseType;
                flag |= TryCheckIndices(numArray, type3.Dimensions.LowerBounds, type3.Dimensions.UpperBounds, isOversamplingArray);
                if (!flag)
                {
                    break;
                }
                baseType = type.BaseType;
            }
            return flag;
        }

        public static bool TryCheckIndices(IList<int[]> indices, IArrayType type)
        {
            bool flag = true;
            IOversamplingSupport support = type as IOversamplingSupport;
            bool oversampled = false;
            if (support != null)
            {
                oversampled = support.IsOversampled;
            }
            IDataType elementType = type;
            foreach (int[] numArray in indices)
            {
                IArrayType type3 = (IArrayType) elementType;
                flag |= TryCheckIndices(numArray, type3.Dimensions.LowerBounds, type3.Dimensions.UpperBounds, oversampled);
                if (!flag)
                {
                    break;
                }
                elementType = (IArrayType) type.ElementType;
            }
            return flag;
        }

        public static bool TryCheckIndices(int[] indices, IArrayType type)
        {
            IOversamplingSupport support = type as IOversamplingSupport;
            return ((support == null) ? TryCheckIndices(indices, type.Dimensions.LowerBounds, type.Dimensions.UpperBounds, false) : TryCheckIndices(indices, type.Dimensions.LowerBounds, type.Dimensions.UpperBounds, support.IsOversampled));
        }

        public static bool TryCheckIndices(int[] indices, int[] lowerBounds, int[] upperBounds, bool oversampled) => 
            TryCheckIndices(indices, lowerBounds, upperBounds, false, oversampled);

        public static bool TryCheckIndices(int[] indices, int[] lowerBounds, int[] upperBounds, bool normalized, bool oversampled)
        {
            if (lowerBounds == null)
            {
                throw new ArgumentNullException("lowerBounds");
            }
            if (upperBounds == null)
            {
                throw new ArgumentNullException("upperBounds");
            }
            if (indices == null)
            {
                throw new ArgumentNullException("indices");
            }
            if (lowerBounds.Length != upperBounds.Length)
            {
                throw new ArgumentException("Dimensions mismatch!");
            }
            if (lowerBounds.Length != indices.Length)
            {
                throw new ArgumentException("Dimensions mismatch!");
            }
            if (oversampled)
            {
                if (indices.Length != 1)
                {
                    throw new ArgumentException("Only one dimension for oversampling arrays allowed!", "indices");
                }
                if (normalized)
                {
                    if (indices[0] == (Math.Abs((int) (upperBounds[0] - lowerBounds[0])) + 1))
                    {
                        return true;
                    }
                }
                else if (lowerBounds[0] <= upperBounds[0])
                {
                    if (indices[0] == (upperBounds[0] + 1))
                    {
                        return true;
                    }
                }
                else if (indices[0] == (upperBounds[0] - 1))
                {
                    return true;
                }
            }
            for (int i = 0; i < lowerBounds.Length; i++)
            {
                if (!normalized)
                {
                    if ((indices[i] < Math.Min(lowerBounds[i], upperBounds[i])) || (indices[i] > Math.Max(upperBounds[i], lowerBounds[i])))
                    {
                        return false;
                    }
                }
                else if ((indices[i] < 0) || (indices[i] > Math.Abs((int) (upperBounds[i] - lowerBounds[i]))))
                {
                    return false;
                }
            }
            return true;
        }

        public static bool TryGetSubIndex(int[] indices, int[] lowerBounds, int[] upperBounds, bool oversampled, out int subIndex)
        {
            subIndex = -1;
            bool flag = TryCheckIndices(indices, lowerBounds, upperBounds, false, oversampled);
            if (flag)
            {
                int num = 0;
                int length = lowerBounds.Length;
                int[] numArray = NormalizeIndices(indices, lowerBounds, upperBounds, oversampled);
                int[] numArray2 = CalcSubIndexArray(lowerBounds, upperBounds);
                int index = 0;
                while (true)
                {
                    if (index >= length)
                    {
                        subIndex = num;
                        break;
                    }
                    num += numArray[index] * numArray2[index];
                    index++;
                }
            }
            return flag;
        }

        internal static bool TryIsOversamplingElement(int subIndex, int[] lowerBounds, int[] upperBounds)
        {
            if (lowerBounds == null)
            {
                throw new ArgumentNullException("lowerBounds");
            }
            if (upperBounds == null)
            {
                throw new ArgumentNullException("upperBounds");
            }
            if (lowerBounds.Length != upperBounds.Length)
            {
                throw new ArgumentException("Dimensions mismatch!");
            }
            if (lowerBounds.Length != 1)
            {
                return false;
            }
            return IsOversamplingIndex(SubIndexToIndices(subIndex, lowerBounds, upperBounds, true), lowerBounds, upperBounds);
        }
    }
}

