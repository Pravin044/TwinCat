namespace TwinCAT.TypeSystem
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using TwinCAT.Ads;

    internal static class AnyArrayConverter
    {
        public static bool IsJagged(ITcAdsDataType type)
        {
            bool flag = false;
            if (((type.Category == DataTypeCategory.Array) && (type.BaseType != null)) && (type.BaseType.Category == DataTypeCategory.Array))
            {
                flag = true;
            }
            return flag;
        }

        public static bool TryGetJaggedDimensions(ITcAdsDataType type, out IList<IDimensionCollection> dimLengths)
        {
            List<IDimensionCollection> list = new List<IDimensionCollection>();
            for (ITcAdsDataType type2 = type; (type2 != null) && (type2.Category == DataTypeCategory.Array); type2 = type2.BaseType)
            {
                list.Add(type2.Dimensions);
            }
            if (list.Count > 0)
            {
                dimLengths = list;
                return true;
            }
            dimLengths = null;
            return false;
        }

        public static bool TryGetJaggedDimensions(IArrayType type, out IList<IDimensionCollection> dimLengths)
        {
            IArrayType type3;
            List<IDimensionCollection> list = new List<IDimensionCollection>();
            for (IDataType type2 = type; (type2 != null) && (type2.Category == DataTypeCategory.Array); type2 = type3.ElementType)
            {
                type3 = (IArrayType) type2;
                list.Add(type3.Dimensions);
            }
            if (list.Count > 0)
            {
                dimLengths = list;
                return true;
            }
            dimLengths = null;
            return false;
        }
    }
}

