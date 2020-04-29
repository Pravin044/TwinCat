namespace TwinCAT.Ads.Internal
{
    using System;
    using System.Text;
    using TwinCAT.Ads;

    internal class SubStructureReader<T> where T: IAdsCustomMarshal<T>, new()
    {
        internal static T[] Read(uint elementCount, long parentEndPosition, Encoding encoding, AdsBinaryReader reader)
        {
            if (elementCount == 0)
            {
                return null;
            }
            T[] sourceArray = new T[elementCount];
            int index = 0;
            while (true)
            {
                if (index < elementCount)
                {
                    if (reader.BaseStream.Position < parentEndPosition)
                    {
                        sourceArray[index] = Activator.CreateInstance<T>();
                        sourceArray[index].Read(parentEndPosition, encoding, reader);
                        index++;
                        continue;
                    }
                    T[] destinationArray = new T[index];
                    Array.Copy(sourceArray, 0, destinationArray, 0, index);
                    sourceArray = destinationArray;
                }
                return sourceArray;
            }
        }
    }
}

