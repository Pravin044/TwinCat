namespace TwinCAT.Ads
{
    using System;
    using System.IO;
    using TwinCAT.Ads.Internal;

    internal static class NotificationSettingsMarshaller
    {
        internal static int Marshal(AdsNotificationAttrib settings, byte[] buffer, int offset, bool extended)
        {
            byte[] buffer2 = new byte[MarshalSize(extended)];
            using (BinaryWriter writer = new BinaryWriter(new MemoryStream(buffer2)))
            {
                writer.Write(settings.cbLength);
                writer.Write(settings.nTransMode);
                writer.Write(settings.nMaxDelay);
                writer.Write(settings.nCycleTime);
                if (extended)
                {
                    writer.Write((ulong) 0UL);
                    writer.Write((ulong) 0UL);
                }
            }
            Array.Copy(buffer2, 0, buffer, offset, buffer2.Length);
            return buffer2.Length;
        }

        internal static int Marshal(NotificationSettings settings, int dataLength, BinaryWriter writer, bool extended)
        {
            int cycleTime = settings.CycleTime;
            if ((settings.NotificationMode != AdsTransMode.CyclicInContext) && (settings.NotificationMode != AdsTransMode.OnChangeInContext))
            {
                cycleTime *= 0x2710;
            }
            writer.Write(dataLength);
            writer.Write((int) settings.NotificationMode);
            writer.Write(settings.MaxDelay);
            writer.Write(cycleTime);
            if (extended)
            {
                writer.Write((ulong) 0UL);
                writer.Write((ulong) 0UL);
            }
            return MarshalSize(extended);
        }

        internal static int Marshal(NotificationSettings settings, int dataLength, byte[] buffer, int offset, bool extended)
        {
            byte[] buffer2 = new byte[MarshalSize(extended)];
            using (BinaryWriter writer = new BinaryWriter(new MemoryStream(buffer2)))
            {
                Marshal(settings, dataLength, writer, extended);
            }
            Array.Copy(buffer2, 0, buffer, offset, buffer2.Length);
            return buffer2.Length;
        }

        internal static int MarshalSize(bool extended)
        {
            int num = 0x10;
            if (extended)
            {
                num += 0x10;
            }
            return num;
        }
    }
}

