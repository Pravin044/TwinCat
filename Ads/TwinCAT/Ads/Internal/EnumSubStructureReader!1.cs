namespace TwinCAT.Ads.Internal
{
    using System;
    using System.Text;
    using TwinCAT.Ads;

    internal class EnumSubStructureReader<T> where T: IAdsEnumCustomMarshal<T>, new()
    {
        internal static T[] Read(uint elementCount, uint valueSize, long parentEndPosition, Encoding encoding, AdsBinaryReader reader)
        {
            if (elementCount == 0)
            {
                return null;
            }
            T[] localArray = new T[elementCount];
            for (int i = 0; (i < elementCount) && (reader.BaseStream.Position < parentEndPosition); i++)
            {
                localArray[i] = Activator.CreateInstance<T>();
                localArray[i].Read(valueSize, parentEndPosition, encoding, reader);
            }
            return localArray;
        }
    }
}

