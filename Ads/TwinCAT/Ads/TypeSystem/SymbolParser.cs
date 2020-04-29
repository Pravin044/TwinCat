namespace TwinCAT.Ads.TypeSystem
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Text.RegularExpressions;
    using TwinCAT.Ads;
    using TwinCAT.Ads.Internal;
    using TwinCAT.TypeSystem;
    using TwinCAT.TypeSystem.Generic;

    [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
    public static class SymbolParser
    {
        private const string arrIndex2 = @"(?<jagged>(?:\[\s*(?<arrayIndices>(?<indices>-?\d+)(?:,\s*(?<indices>-?\d+))*)\]))+";
        private const string arrIndexOver = @"(?:\[T(?<oversamplingIndex>\d+)\])";
        private const string indexSpec = @"(?:(?<jagged>(?:\[\s*(?<arrayIndices>(?<indices>-?\d+)(?:,\s*(?<indices>-?\d+))*)\]))+|(?:\[T(?<oversamplingIndex>\d+)\]))";
        private const string arrayInstSpec = @"(?:(?<name>[^\[\]\s]+)(?<indicesStr>(?<jagged>(?:\[\s*(?<arrayIndices>(?<indices>-?\d+)(?:,\s*(?<indices>-?\d+))*)\]))+|(?:\[T(?<oversamplingIndex>\d+)\])))";
        private static Regex arrayExpression = new Regex(@"(?:(?<name>[^\[\]\s]+)(?<indicesStr>(?<jagged>(?:\[\s*(?<arrayIndices>(?<indices>-?\d+)(?:,\s*(?<indices>-?\d+))*)\]))+|(?:\[T(?<oversamplingIndex>\d+)\])))", ((RegexOptions) RegexOptions.Compiled) | ((RegexOptions) RegexOptions.IgnoreCase));
        private static Regex arrayIndexExpression = new Regex(@"(?:(?<jagged>(?:\[\s*(?<arrayIndices>(?<indices>-?\d+)(?:,\s*(?<indices>-?\d+))*)\]))+|(?:\[T(?<oversamplingIndex>\d+)\]))", ((RegexOptions) RegexOptions.Compiled) | ((RegexOptions) RegexOptions.IgnoreCase));

        private static IList<int[]> getIndicesFromGroup(Group group)
        {
            List<int[]> list = new List<int[]>();
            int num = 0;
            while (num < group.get_Captures().Count)
            {
                Capture capture = group.get_Captures().get_Item(num);
                char[] separator = new char[] { ',' };
                string[] strArray = capture.Value.Split(separator);
                int[] item = new int[strArray.Length];
                int index = 0;
                while (true)
                {
                    if (index >= strArray.Length)
                    {
                        list.Add(item);
                        num++;
                        break;
                    }
                    item[index] = int.Parse(strArray[index]);
                    index++;
                }
            }
            return list;
        }

        private static ISymbol ParseSymbol(AdsBinaryReader symbolReader, Encoding encoding, ISymbolFactoryServices factoryServices)
        {
            AdsSymbolEntry entry = new AdsSymbolEntry(-1L, encoding, symbolReader);
            return factoryServices.SymbolFactory.CreateInstance(entry, null);
        }

        public static void ParseSymbols(AdsStream symbolStream, Encoding encoding, ISymbolFactoryServices factoryServices)
        {
            if (symbolStream == null)
            {
                throw new ArgumentNullException("symbolStream");
            }
            if (factoryServices == null)
            {
                throw new ArgumentNullException("factoryServices");
            }
            AdsBinaryReader symbolReader = new AdsBinaryReader(symbolStream);
            symbolStream.Position = 0L;
            while (symbolStream.Position < symbolStream.Length)
            {
                ISymbol symbol = ParseSymbol(symbolReader, encoding, factoryServices);
                if (symbol != null)
                {
                    factoryServices.Binder.Bind((IHierarchicalSymbol) symbol);
                }
            }
        }

        public static void ParseTypes(AdsStream dataTypeStream, Encoding encoding, IBinder binder, bool buildInTypesInStream, DataTypeCollection<IDataType> buildInTypes)
        {
            AdsBinaryReader reader = new AdsBinaryReader(dataTypeStream);
            string referencedType = null;
            while (dataTypeStream.Position < dataTypeStream.Length)
            {
                AdsDataTypeEntry entry = new AdsDataTypeEntry(true, encoding, reader);
                try
                {
                    IDataType type = null;
                    bool flag1 = buildInTypes.TryGetType(entry.entryName, out type);
                    DataType type2 = (DataType) type;
                    if (buildInTypesInStream && flag1)
                    {
                        continue;
                    }
                    DataType type3 = null;
                    int length = 0;
                    if (DataTypeStringParser.TryParseReference(entry.entryName, out referencedType))
                    {
                        type3 = new ReferenceType(entry, referencedType);
                    }
                    else if (DataTypeStringParser.TryParsePointer(entry.entryName, out referencedType))
                    {
                        type3 = new PointerType(entry, referencedType);
                    }
                    else
                    {
                        bool flag2;
                        if (DataTypeStringParser.TryParseString(entry.entryName, out length, out flag2))
                        {
                            type3 = !flag2 ? ((DataType) new StringType(length)) : ((DataType) new WStringType(length));
                        }
                        else if (entry.methodCount > 0)
                        {
                            type3 = new RpcStructType(entry);
                        }
                        else if (entry.subItems > 0)
                        {
                            bool flag3 = false;
                            bool flag4 = false;
                            if (entry.subItems > 1)
                            {
                                int num2 = 0;
                                AdsFieldEntry[] subEntries = entry.subEntries;
                                int index = 0;
                                while (true)
                                {
                                    if (index >= subEntries.Length)
                                    {
                                        flag3 = entry.BitSize < num2;
                                        break;
                                    }
                                    AdsFieldEntry entry2 = subEntries[index];
                                    if (!entry2.IsStatic && !entry2.IsProperty)
                                    {
                                        int num4 = 0;
                                        num4 = !entry2.IsBitType ? (entry2.Offset * 8) : entry2.Offset;
                                        flag4 |= num4 < num2;
                                        num2 += entry2.BitSize;
                                    }
                                    index++;
                                }
                            }
                            type3 = !flag3 ? ((DataType) new StructType(entry)) : ((DataType) new UnionType(entry));
                        }
                        else if (entry.arrayDim > 0)
                        {
                            type3 = new ArrayType(entry);
                        }
                        else if (entry.enumInfoCount <= 0)
                        {
                            if (DataTypeStringParser.TryParseSubRange(entry.entryName, out referencedType))
                            {
                                type3 = (DataType) SubRangeTypeFactory.Create(entry, binder);
                            }
                            else if (((entry.baseTypeId > AdsDatatypeId.ADST_VOID) && (entry.typeName != null)) && (entry.typeName != string.Empty))
                            {
                                type3 = new AliasType(entry);
                            }
                            else
                            {
                                DataTypeCategory cat = CategoryConverter.FromId(entry.DataTypeId);
                                if (cat != DataTypeCategory.Unknown)
                                {
                                    Type tp = null;
                                    if (PrimitiveTypeConverter.TryGetManagedType(entry.DataTypeId, out tp))
                                    {
                                        if (cat == DataTypeCategory.Primitive)
                                        {
                                            type3 = new PrimitiveType(entry, PrimitiveTypeConverter.GetPrimitiveFlags(entry.DataTypeId), tp);
                                        }
                                        else if (cat == DataTypeCategory.String)
                                        {
                                            type3 = new StringType(entry);
                                        }
                                    }
                                }
                                if (type3 == null)
                                {
                                    type3 = new DataType(cat, entry);
                                }
                            }
                        }
                        else
                        {
                            AdsDatatypeId baseTypeId = entry.baseTypeId;
                            if (baseTypeId == AdsDatatypeId.ADST_INT16)
                            {
                                type3 = new EnumType<short>(entry);
                            }
                            else if (baseTypeId == AdsDatatypeId.ADST_INT32)
                            {
                                type3 = new EnumType<int>(entry);
                            }
                            else
                            {
                                switch (baseTypeId)
                                {
                                    case AdsDatatypeId.ADST_INT8:
                                        type3 = new EnumType<sbyte>(entry);
                                        break;

                                    case AdsDatatypeId.ADST_UINT8:
                                        type3 = new EnumType<byte>(entry);
                                        break;

                                    case AdsDatatypeId.ADST_UINT16:
                                        type3 = new EnumType<ushort>(entry);
                                        break;

                                    case AdsDatatypeId.ADST_UINT32:
                                        type3 = new EnumType<uint>(entry);
                                        break;

                                    case AdsDatatypeId.ADST_INT64:
                                        type3 = new EnumType<long>(entry);
                                        break;

                                    case AdsDatatypeId.ADST_UINT64:
                                        type3 = new EnumType<ulong>(entry);
                                        break;

                                    default:
                                        throw new AdsException("Enum base type mismatch!");
                                }
                            }
                        }
                    }
                    binder.RegisterType(type3);
                }
                catch (Exception exception)
                {
                    Module.Trace.TraceWarning($"Cannot parse DataTypeEntry. Skipping dataType '{entry.entryName}'!", exception);
                }
            }
        }

        internal static bool TryParseArrayElement(string nameWithIndices, out string instanceName, out string indicesStr, out IList<int[]> jaggedIndices, out ArrayIndexType type)
        {
            string str = nameWithIndices.Trim();
            Match match = arrayExpression.Match(str);
            if (match.Success)
            {
                Group group = match.get_Groups().get_Item("name");
                instanceName = group.get_Captures().get_Item(0).Value;
                Group group2 = match.get_Groups().get_Item("indicesStr");
                indicesStr = group2.get_Captures().get_Item(0).Value;
                Group group3 = match.get_Groups().get_Item("indices");
                Group group4 = match.get_Groups().get_Item("arrayIndices");
                Group group5 = match.get_Groups().get_Item("oversamplingIndex");
                if (group4.get_Captures().Count > 0)
                {
                    jaggedIndices = getIndicesFromGroup(group4);
                    type = (jaggedIndices.Count <= 1) ? ArrayIndexType.Standard : ArrayIndexType.Jagged;
                    return true;
                }
                if (group5.get_Captures().Count > 0)
                {
                    jaggedIndices = getIndicesFromGroup(group5);
                    type = ArrayIndexType.Oversample;
                    return true;
                }
            }
            instanceName = null;
            indicesStr = null;
            jaggedIndices = null;
            type = ArrayIndexType.Standard;
            return false;
        }

        internal static bool TryParseIndices(string indicesStr, out IList<int[]> jaggedIndices, out ArrayIndexType type)
        {
            string str = indicesStr.Trim();
            Match match = arrayIndexExpression.Match(str);
            if (match.Success)
            {
                Group group = match.get_Groups().get_Item("arrayIndices");
                Group group2 = match.get_Groups().get_Item("oversamplingIndex");
                if (group.get_Captures().Count > 0)
                {
                    jaggedIndices = getIndicesFromGroup(group);
                    type = (jaggedIndices.Count <= 1) ? ArrayIndexType.Standard : ArrayIndexType.Jagged;
                    return true;
                }
                if (group2.get_Captures().Count > 0)
                {
                    jaggedIndices = getIndicesFromGroup(group2);
                    type = ArrayIndexType.Oversample;
                    return true;
                }
            }
            jaggedIndices = null;
            type = ArrayIndexType.Standard;
            return false;
        }

        internal static bool TryParseParentPath(IInstance symbol, out string parentPath, out string parentName)
        {
            bool flag = false;
            char[] separator = new char[] { '.' };
            string[] sourceArray = symbol.InstancePath.Split(separator);
            int length = sourceArray.GetLength(0);
            if (length < 2)
            {
                parentPath = null;
                parentName = null;
            }
            else if (sourceArray[length - 2] == string.Empty)
            {
                parentPath = null;
                parentName = null;
            }
            else
            {
                parentName = sourceArray[length - 2];
                string[] destinationArray = new string[length - 1];
                Array.Copy(sourceArray, destinationArray, (int) (length - 1));
                parentPath = string.Join<string>(".", destinationArray);
                flag = true;
            }
            return flag;
        }

        internal static bool TryParseType(string typeName, IBinder resolver, out IDataType type)
        {
            if (typeName == null)
            {
                throw new ArgumentNullException("typeName");
            }
            if (resolver == null)
            {
                throw new ArgumentNullException("resolver");
            }
            if (typeName == string.Empty)
            {
                throw new ArgumentException();
            }
            DataType type2 = null;
            int length = -1;
            bool isUnicode = false;
            if (DataTypeStringParser.TryParseString(typeName, out length, out isUnicode))
            {
                type2 = !isUnicode ? ((DataType) new StringType(length)) : ((DataType) new WStringType(length));
                resolver.RegisterType(type2);
                resolver.OnTypeGenerated(type2);
                type = type2;
                return true;
            }
            DimensionCollection dims = null;
            string baseType = null;
            if (DataTypeStringParser.TryParseArray(typeName, out dims, out baseType))
            {
                IDataType type3 = null;
                if (resolver.TryResolveType(baseType, out type3))
                {
                    type2 = new ArrayType(typeName, (DataType) type3, dims, AdsDataTypeFlags.DataType);
                    resolver.RegisterType(type2);
                    resolver.OnTypeGenerated(type2);
                    type = type2;
                    return true;
                }
                resolver.OnTypeResolveError(typeName);
                object[] args = new object[] { typeName };
                Module.Trace.TraceWarning("Array Type '{0}' could not be resolved. Not in DataType tables. Ignoring Type!", args);
                type = null;
                return false;
            }
            string referencedType = null;
            if (DataTypeStringParser.TryParsePointer(typeName, out referencedType))
            {
                type2 = new PointerType(typeName, referencedType);
                resolver.RegisterType(type2);
                resolver.OnTypeGenerated(type2);
                type = type2;
                return true;
            }
            if (DataTypeStringParser.TryParseReference(typeName, out referencedType))
            {
                type2 = new ReferenceType(typeName, referencedType, resolver.PlatformPointerSize);
                resolver.RegisterType(type2);
                resolver.OnTypeGenerated(type2);
                type = type2;
                return true;
            }
            type2 = (DataType) SubRangeTypeFactory.Create(typeName, resolver);
            if (type2 == null)
            {
                type = null;
                resolver.OnTypeResolveError(typeName);
                return false;
            }
            resolver.RegisterType(type2);
            resolver.OnTypeGenerated(type2);
            type = type2;
            return true;
        }

        internal enum ArrayIndexType
        {
            Standard,
            Jagged,
            Oversample
        }
    }
}

