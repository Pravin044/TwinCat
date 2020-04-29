namespace TwinCAT.Ads
{
    using System;
    using System.Resources;
    using TwinCAT.Ads.Internal;

    internal class ResMan
    {
        private static System.Resources.ResourceManager _rm;

        internal static string GetString(string name) => 
            ResourceManager.GetString(name);

        internal static string GetString(string message, AdsErrorCode adsErrorCode)
        {
            string str = GetString("AdsError_" + ((uint) adsErrorCode).ToString());
            if (str == null)
            {
                str = "A unknown Ads-Error has occurred.";
            }
            string str2 = $"Ads-Error 0x{(uint) adsErrorCode:X} : {str}";
            return (!string.IsNullOrEmpty(message) ? $"{message} ({str2})" : str2);
        }

        private static System.Resources.ResourceManager ResourceManager
        {
            get
            {
                if (_rm == null)
                {
                    _rm = new System.Resources.ResourceManager("TwinCAT.Ads.Resource", typeof(TcAdsDllWrapper).Assembly);
                }
                return _rm;
            }
        }
    }
}

