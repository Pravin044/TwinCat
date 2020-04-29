namespace TwinCAT.TypeSystem
{
    using System;
    using System.ComponentModel;
    using System.Runtime.InteropServices;
    using System.Text.RegularExpressions;
    using TwinCAT.Ads;

    [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
    public static class DataTypeStringParser
    {
        private const string rangeSpec = @"\s*(?<lb>-?\d*)\s*\.\.\s*(?<ub>-?\d*\s*)";
        private const string typeName = "[a-zA-Z_$][a-zA-Z_$0-9]*";
        private const string arraySpec = @"(?<arrayType>^ARRAY\b)\s*\[(\s*(?<lb>-?\d*)\s*\.\.\s*(?<ub>-?\d*\s*),)*\s*(?<lb>-?\d*)\s*\.\.\s*(?<ub>-?\d*\s*)\]\s*OF\s*(?<elementType>.*)";
        private const string arraySpec2 = @"(?<arrayType>^ARRAY\b)\s*\[({0},)*{0}\]\s*OF\s*(?<elementType>{2})$";
        private const string stringSpec = @"^((?:W)?STRING\((?<length>\d*)\))|((?:W)?STRING\[(?<length>\d*)\])$";
        private const string pointerSpec = @"^POINTER\sTO\s(?<pointerType>.+)$";
        private const string referenceSpec = @"^REFERENCE\sTO\s(?<referenceType>.+)$";
        private const string subrangeSpec = @"^(?<baseType>\w+)\s*\((?<lb>-?\d*)\.\.(?<ub>-?\d*)\)$";
        private static Regex arrayExpression = new Regex(@"(?<arrayType>^ARRAY\b)\s*\[(\s*(?<lb>-?\d*)\s*\.\.\s*(?<ub>-?\d*\s*),)*\s*(?<lb>-?\d*)\s*\.\.\s*(?<ub>-?\d*\s*)\]\s*OF\s*(?<elementType>.*)", ((RegexOptions) RegexOptions.Compiled) | ((RegexOptions) RegexOptions.IgnoreCase));
        private static Regex stringExpression = new Regex(@"^((?:W)?STRING\((?<length>\d*)\))|((?:W)?STRING\[(?<length>\d*)\])$", ((RegexOptions) RegexOptions.Compiled) | ((RegexOptions) RegexOptions.IgnoreCase));
        private static Regex pointerExpression = new Regex(@"^POINTER\sTO\s(?<pointerType>.+)$", ((RegexOptions) RegexOptions.Compiled) | ((RegexOptions) RegexOptions.IgnoreCase));
        private static Regex referenceExpression = new Regex(@"^REFERENCE\sTO\s(?<referenceType>.+)$", ((RegexOptions) RegexOptions.Compiled) | ((RegexOptions) RegexOptions.IgnoreCase));
        private static Regex subRangeExpression = new Regex(@"^(?<baseType>\w+)\s*\((?<lb>-?\d*)\.\.(?<ub>-?\d*)\)$", ((RegexOptions) RegexOptions.Compiled) | ((RegexOptions) RegexOptions.IgnoreCase));

        internal static bool IsArray(string typeName)
        {
            AdsDatatypeArrayInfo[] dims = null;
            string baseType = null;
            return TryParseArray(typeName, out dims, out baseType);
        }

        internal static bool IsIntrinsicType(string typeName)
        {
            string[] textArray1 = new string[] { "TOD", "DT", "TIME", "DATE", "LTIME" };
            StringComparer ordinalIgnoreCase = StringComparer.OrdinalIgnoreCase;
            foreach (string str in textArray1)
            {
                if (ordinalIgnoreCase.Compare(str, typeName) == 0)
                {
                    return true;
                }
            }
            return false;
        }

        internal static bool IsPointer(string typeName)
        {
            string str;
            if (string.IsNullOrEmpty(typeName))
            {
                throw new ArgumentOutOfRangeException("typteName");
            }
            return TryParsePointer(typeName, out str);
        }

        internal static bool IsReference(string typeName)
        {
            string str;
            return TryParseReference(typeName, out str);
        }

        internal static bool IsString(string typeName)
        {
            int num;
            bool flag;
            return TryParseString(typeName, out num, out flag);
        }

        internal static bool IsSubRange(string typeName)
        {
            string str;
            return TryParseSubRange(typeName, out str);
        }

        internal static bool TryParse<T>(string str, out T value) where T: struct, IConvertible
        {
            try
            {
                value = (T) Convert.ChangeType(str, typeof(T), null);
            }
            catch (OverflowException)
            {
                value = default(T);
            }
            catch (NotSupportedException)
            {
                value = default(T);
            }
            return false;
        }

        internal static bool TryParse(string str, Type type, out object value)
        {
            try
            {
                value = Convert.ChangeType(str, type, null);
                return true;
            }
            catch (OverflowException)
            {
                value = null;
            }
            catch (NotSupportedException)
            {
                value = null;
            }
            catch (FormatException)
            {
                value = null;
            }
            return false;
        }

        internal static bool TryParseArray(string typeName, out AdsDatatypeArrayInfo[] dims, out string baseType)
        {
            Match match = arrayExpression.Match(typeName);
            dims = null;
            baseType = null;
            if (!match.Success)
            {
                return false;
            }
            baseType = match.get_Groups().get_Item("elementType").Value;
            Group group = match.get_Groups().get_Item("ub");
            Group group2 = match.get_Groups().get_Item("lb");
            int count = group.get_Captures().Count;
            dims = new AdsDatatypeArrayInfo[count];
            for (int i = 0; i < count; i++)
            {
                object obj2 = group2.get_Captures().get_Item(i);
                int lowerBound = int.Parse(group2.get_Captures().get_Item(i).Value);
                int num4 = int.Parse(group.get_Captures().get_Item(i).Value);
                int elements = (num4 - lowerBound) + 1;
                dims[i] = new AdsDatatypeArrayInfo(lowerBound, elements);
            }
            return true;
        }

        internal static bool TryParseArray(string typeName, out DimensionCollection dims, out string baseType)
        {
            AdsDatatypeArrayInfo[] infoArray = null;
            if (TryParseArray(typeName, out infoArray, out baseType))
            {
                dims = new DimensionCollection(infoArray);
                return true;
            }
            dims = null;
            baseType = null;
            return false;
        }

        internal static bool TryParsePointer(string typeName, out string referencedType)
        {
            if (string.IsNullOrEmpty(typeName))
            {
                throw new ArgumentOutOfRangeException("typteName");
            }
            referencedType = null;
            Match match = pointerExpression.Match(typeName);
            if (!match.Success)
            {
                return false;
            }
            Group group = match.get_Groups().get_Item("pointerType");
            referencedType = group.get_Captures().get_Item(0).Value;
            return true;
        }

        internal static bool TryParseReference(string typeName, out string referencedType)
        {
            referencedType = null;
            Match match = referenceExpression.Match(typeName);
            if (!match.Success)
            {
                return false;
            }
            Group group = match.get_Groups().get_Item("referenceType");
            referencedType = group.get_Captures().get_Item(0).Value;
            return true;
        }

        internal static bool TryParseString(string typeName, out int length, out bool isUnicode)
        {
            string str = typeName.Trim();
            Match match = stringExpression.Match(str);
            length = -1;
            isUnicode = false;
            if (!match.Success)
            {
                return false;
            }
            Group group = match.get_Groups().get_Item("length");
            int count = group.get_Captures().Count;
            length = int.Parse(group.get_Captures().get_Item(0).Value);
            isUnicode = (str[0] == 'w') || (str[0] == 'W');
            return true;
        }

        internal static bool TryParseSubRange(string typeName, out string baseType)
        {
            Match match = subRangeExpression.Match(typeName);
            baseType = null;
            if (!match.Success)
            {
                return false;
            }
            Group group = match.get_Groups().get_Item("baseType");
            baseType = group.get_Captures().get_Item(0).Value;
            return true;
        }

        internal static bool TryParseSubRange(string typeName, out string baseType, out string lowerBound, out string upperBound)
        {
            Match match = subRangeExpression.Match(typeName);
            baseType = null;
            lowerBound = null;
            upperBound = null;
            if (!match.Success)
            {
                return false;
            }
            Group group = match.get_Groups().get_Item("baseType");
            Group group2 = match.get_Groups().get_Item("lb");
            Group group3 = match.get_Groups().get_Item("ub");
            baseType = group.get_Captures().get_Item(0).Value;
            lowerBound = group2.get_Captures().get_Item(0).Value;
            upperBound = group3.get_Captures().get_Item(0).Value;
            return true;
        }

        internal static bool TryParseSubRange<T>(string typeName, out string baseType, out T lowerBound, out T upperBound) where T: struct, IConvertible
        {
            lowerBound = default(T);
            upperBound = default(T);
            string str = null;
            string str2 = null;
            return (TryParseSubRange(typeName, out baseType, out str, out str2) && (TryParse<T>(str, out lowerBound) & TryParse<T>(str2, out upperBound)));
        }

        internal static bool TryParseSubRange(string typeName, Type managedBaseType, out string baseType, out object lowerBound, out object upperBound)
        {
            lowerBound = null;
            upperBound = null;
            string str = null;
            string str2 = null;
            return (TryParseSubRange(typeName, out baseType, out str, out str2) && (TryParse(str, managedBaseType, out lowerBound) & TryParse(str2, managedBaseType, out upperBound)));
        }
    }
}

