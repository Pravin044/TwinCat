namespace TwinCAT.Ads.Server
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using TwinCAT.Ads;

    internal class TcAdsParser
    {
        internal static byte[] AdsData(byte[] amsData, uint startIndex, uint dataLength)
        {
            byte[] destinationArray = new byte[dataLength];
            Array.Copy(amsData, (int) startIndex, destinationArray, 0, (int) dataLength);
            return destinationArray;
        }

        internal static unsafe TwinCAT.Ads.AdsState AdsState(byte[] amsData)
        {
            if (amsData.Length >= 2)
            {
                return *(((TwinCAT.Ads.AdsState*) &(amsData[0])));
            }
            return TwinCAT.Ads.AdsState.Invalid;
        }

        internal static byte[] BuildAdsBuffer(ITcAdsHeader adsHeader, byte[] adsData)
        {
            byte[] buffer = null;
            if (adsHeader != null)
            {
                int num = Marshal.SizeOf(adsHeader);
                buffer = (adsData == null) ? new byte[num] : new byte[num + adsData.Length];
                BinaryWriter writer = new BinaryWriter(new MemoryStream(buffer));
                adsHeader.WriteToBuffer(writer);
                if (adsData != null)
                {
                    writer.Write(adsData);
                }
            }
            return buffer;
        }

        internal static byte[] BuildDeviceNotificationBuffer(TcAdsStampHeader[] stampHeaders)
        {
            uint num = 0;
            TcAdsStampHeader[] headerArray = stampHeaders;
            int index = 0;
            while (index < headerArray.Length)
            {
                TcAdsStampHeader header = headerArray[index];
                TcAdsNotificationSample[] notificationSamples = header.NotificationSamples;
                num += (uint) 12;
                TcAdsNotificationSample[] sampleArray2 = notificationSamples;
                int num3 = 0;
                while (true)
                {
                    if (num3 >= sampleArray2.Length)
                    {
                        index++;
                        break;
                    }
                    TcAdsNotificationSample sample = sampleArray2[num3];
                    num = (num + 8) + sample.SampleSize;
                    num3++;
                }
            }
            byte[] buffer = new byte[num];
            BinaryWriter writer = new BinaryWriter(new MemoryStream(buffer));
            foreach (TcAdsStampHeader header2 in stampHeaders)
            {
                header2.WriteToBuffer(writer);
            }
            return buffer;
        }

        internal static unsafe uint DataLength(byte[] amsData, uint dataIndex) => 
            *(((uint*) &(amsData[dataIndex])));

        internal static TcAdsStampHeader[] ReadNotificationStampHeaders(byte[] amsData, uint numStampHeaders)
        {
            MemoryStream input = new MemoryStream(amsData);
            BinaryReader reader = new BinaryReader(input);
            input.Position = 8L;
            TcAdsStampHeader[] headerArray = new TcAdsStampHeader[numStampHeaders];
            for (int i = 0; i < numStampHeaders; i++)
            {
                headerArray[i] = new TcAdsStampHeader(reader);
            }
            input.Close();
            return headerArray;
        }

        internal static unsafe string ReadString(byte[] amsData, uint startIndex, uint length)
        {
            if (amsData.Length >= (startIndex + length))
            {
                return new string((sbyte*) &(amsData[startIndex]), 0, (int) length);
            }
            return "";
        }

        internal static unsafe uint ReadUInt(byte[] amsData, uint startIndex)
        {
            if (amsData.Length >= (startIndex + 4))
            {
                return *(((uint*) &(amsData[startIndex])));
            }
            return 0;
        }

        internal static unsafe ushort ReadUShort(byte[] amsData, uint startIndex)
        {
            if (amsData.Length >= (startIndex + 2))
            {
                return *(((ushort*) &(amsData[startIndex])));
            }
            return 0;
        }
    }
}

