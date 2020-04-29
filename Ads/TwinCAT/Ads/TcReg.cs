namespace TwinCAT.Ads
{
    using Microsoft.Win32;
    using System;
    using System.IO;
    using System.Runtime.InteropServices;

    internal static class TcReg
    {
        private static TwinCATSubSystemType _tcInstallation;
        private static Version _tcVersion;
        private static string _tcInstallationPath;
        private const string KeyNameAmsNetId = "AmsNetId";
        private const string KeyNameSystem = "System";
        public const string KeyNameInstPathTc2 = "InstallationPath";
        public const string KeyNameInstPathTc3 = "InstallDir";
        public const string KeyNameCurrentVersion = "CurrentVersion";
        private const string KeyPathTcRouterControlSet = @"SYSTEM\CurrentControlSet\services\TcRouter";
        private const string ValueNameImagePath = "ImagePath";

        static TcReg()
        {
            try
            {
                _tcInstallation = GetTwinCATInstallationType(out _tcInstallationPath, out _tcVersion);
            }
            catch (Exception)
            {
                _tcInstallation = TwinCATSubSystemType.None;
            }
        }

        private static TwinCATSubSystemType GetTwinCATInstallationType(out string installationPath, out Version version)
        {
            installationPath = null;
            TwinCATSubSystemType none = TwinCATSubSystemType.None;
            string path = null;
            if (!TryGetTcRouterImagePath(out path))
            {
                throw new ApplicationException("TwinCAT not installed!");
            }
            string str2 = null;
            string str3 = null;
            Version version2 = null;
            Version version3 = null;
            TryGetTwinCAT2Path(out str2, out version2);
            TryGetTwinCAT3Path(out str3, out version3);
            if ((str2 != null) && path.StartsWith(str2, StringComparison.OrdinalIgnoreCase))
            {
                installationPath = str2;
                version = version2;
                none = TwinCATSubSystemType.Tc2;
            }
            else
            {
                if ((str3 == null) || !path.StartsWith(str3, StringComparison.OrdinalIgnoreCase))
                {
                    throw new ApplicationException("TwinCAT is not installed!");
                }
                installationPath = str3;
                version = version3;
                none = TwinCATSubSystemType.Tc3;
            }
            return none;
        }

        private static bool TryGetTcRouterImagePath(out string path)
        {
            path = null;
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\services\TcRouter"))
            {
                if (key != null)
                {
                    path = (string) key.GetValue("ImagePath", null);
                    if (path != null)
                    {
                        string str = @"\??\";
                        if (path.StartsWith(str))
                        {
                            path = path.Substring(str.Length);
                        }
                        return true;
                    }
                }
            }
            return false;
        }

        private static bool TryGetTwinCAT2Path(out string path, out Version version)
        {
            path = null;
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(RootTc2))
            {
                if (key != null)
                {
                    path = (string) key.GetValue("InstallationPath", null);
                    if ((path != null) && (path != string.Empty))
                    {
                        if ((path[path.Length - 1] != Path.DirectorySeparatorChar) && (path[path.Length - 1] != Path.AltDirectorySeparatorChar))
                        {
                            path = path + Path.DirectorySeparatorChar.ToString();
                        }
                        RegistryKey key2 = null;
                        try
                        {
                            if (key.OpenSubKey("2.11.000") != null)
                            {
                                version = new Version("2.11");
                            }
                            else
                            {
                                key2 = key.OpenSubKey("2.10.000");
                                version = new Version("2.10");
                            }
                        }
                        finally
                        {
                            if (key2 != null)
                            {
                                key2.Close();
                            }
                        }
                        return true;
                    }
                }
            }
            version = null;
            return false;
        }

        private static bool TryGetTwinCAT3Path(out string path, out Version version)
        {
            path = null;
            RegistryKey key = null;
            try
            {
                key = Registry.LocalMachine.OpenSubKey(RootTc3);
                if (key != null)
                {
                    string name = (string) key.GetValue("CurrentVersion", null);
                    if (name != null)
                    {
                        key = key.OpenSubKey(name);
                    }
                    if (key != null)
                    {
                        path = (string) key.GetValue("InstallDir", null);
                        if ((path != null) && (path != string.Empty))
                        {
                            if ((path[path.Length - 1] != Path.DirectorySeparatorChar) && (path[path.Length - 1] != Path.AltDirectorySeparatorChar))
                            {
                                path = path + Path.DirectorySeparatorChar.ToString();
                            }
                            version = (name == null) ? new Version("3.0") : new Version(name);
                            return true;
                        }
                    }
                }
            }
            finally
            {
                if (key != null)
                {
                    key.Close();
                }
            }
            version = null;
            return false;
        }

        public static string RootPath
        {
            get
            {
                if (_tcInstallation == TwinCATSubSystemType.Tc2)
                {
                    return RootTc2;
                }
                if (_tcInstallation != TwinCATSubSystemType.Tc3)
                {
                    throw new ApplicationException("TwinCAT is not installed!");
                }
                return RootTc3;
            }
        }

        public static AmsNetId LocalNetId
        {
            get
            {
                AmsNetId id;
                try
                {
                    using (RegistryKey key = SystemRegKey)
                    {
                        string name = "AmsNetId";
                        object obj2 = key.GetValue(name);
                        if (obj2 == null)
                        {
                            throw new ApplicationException($"Registry value '{name}' of key '{key.Name}' does not exist!");
                        }
                        id = new AmsNetId((byte[]) obj2);
                    }
                }
                catch (Exception)
                {
                    id = null;
                }
                return id;
            }
        }

        private static string RootTc2 =>
            (!Is64BitProcess ? @"Software\Beckhoff\TwinCAT" : @"Software\Wow6432Node\Beckhoff\TwinCAT");

        private static string RootTc3 =>
            (!Is64BitProcess ? @"Software\Beckhoff\TwinCAT3" : @"Software\Wow6432Node\Beckhoff\TwinCAT3");

        private static bool IsWow64 =>
            (Environment.get_Is64BitOperatingSystem() && !Environment.get_Is64BitProcess());

        private static bool Is64OperatingSystem =>
            Environment.get_Is64BitOperatingSystem();

        private static bool Is64BitProcess =>
            Environment.get_Is64BitProcess();

        private static RegistryKey SystemRegKey
        {
            get
            {
                string name = RootPath + @"\System";
                RegistryKey key = Registry.LocalMachine.OpenSubKey(name);
                if (key == null)
                {
                    throw new ApplicationException($"Registry key '{name}' not found!");
                }
                return key;
            }
        }

        private enum TwinCATSubSystemType
        {
            None,
            Tc2,
            Tc3
        }
    }
}

