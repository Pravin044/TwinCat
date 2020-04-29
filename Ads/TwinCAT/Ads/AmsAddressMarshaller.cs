namespace TwinCAT.Ads
{
    using System;

    public static class AmsAddressMarshaller
    {
        public static byte[] Marshal(AmsAddress address)
        {
            byte[] destinationArray = new byte[8];
            Array.Copy(address.NetId.netId, destinationArray, 6);
            Array.Copy(BitConverter.GetBytes((ushort) address.Port), 0, destinationArray, 6, 2);
            return destinationArray;
        }

        public static int SizeOf(AmsAddress address) => 
            8;
    }
}

