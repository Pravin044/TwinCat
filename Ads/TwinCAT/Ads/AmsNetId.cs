namespace TwinCAT.Ads
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Text.RegularExpressions;
    using TwinCAT.Ads.Internal;

    [Serializable, TypeConverter(typeof(AmsNetIdConverter))]
    public class AmsNetId : IComparable<AmsNetId>, IComparable
    {
        internal byte[] netId;
        private static AmsNetId _localNetId = null;
        internal const string LocalValue = "Local";
        internal const string EmptyValue = "Empty";
        internal const string LocalHostValue = "LocalHost";
        private const string RegularExpressionPattern = @"^(?<AmsNetId>((?<First>\d{1,3})\.(?<Second>\d{1,3})\.(?<Third>\d{1,3})\.(?<Fourth>\d{1,3})\.(?<Fifth>\d{1,3})\.(?<Sixth>\d{1,3})) | Local | Empty | LocalHost)$";
        private static Regex regex = new Regex(@"^(?<AmsNetId>((?<First>\d{1,3})\.(?<Second>\d{1,3})\.(?<Third>\d{1,3})\.(?<Fourth>\d{1,3})\.(?<Fifth>\d{1,3})\.(?<Sixth>\d{1,3})) | Local | Empty | LocalHost)$", ((RegexOptions) RegexOptions.CultureInvariant) | (((RegexOptions) RegexOptions.IgnorePatternWhitespace) | (((RegexOptions) RegexOptions.Compiled) | ((RegexOptions) RegexOptions.IgnoreCase))));

        internal AmsNetId()
        {
            this.netId = new byte[6];
        }

        public AmsNetId(string netId)
        {
            this.netId = new byte[6];
            this.netId = GetNetId(netId);
        }

        public AmsNetId(byte[] netId)
        {
            this.netId = new byte[6];
            if (netId.Length != 6)
            {
                throw new ArgumentException("Not a valid NetId", "netId");
            }
            netId.CopyTo(this.netId, 0);
        }

        public AmsNetId(AmsNetId netId)
        {
            this.netId = new byte[6];
            Array.Copy(netId.netId, 0, this.netId, 0, 6);
        }

        public AmsNetId Clone() => 
            new AmsNetId(this);

        public int CompareTo(object obj) => 
            this.CompareTo((AmsNetId) obj);

        public int CompareTo(AmsNetId other)
        {
            for (int i = 0; i < 6; i++)
            {
                if (this.netId[i] < other.netId[i])
                {
                    return -1;
                }
                if (this.netId[i] > other.netId[i])
                {
                    return 1;
                }
            }
            return 0;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (base.GetType() != obj.GetType())
            {
                return false;
            }
            AmsNetId id = (AmsNetId) obj;
            return this.NetIdsEqual(id.netId);
        }

        public static AmsNetId FromBinHexString(string str)
        {
            byte[] netId = new byte[6];
            for (int i = 0; i < str.Length; i += 2)
            {
                string s = str.Substring(i, 2);
                netId[i / 2] = byte.Parse(s, NumberStyles.HexNumber);
            }
            return new AmsNetId(netId);
        }

        public override int GetHashCode()
        {
            int num = 10;
            for (int i = 0; i < 6; i++)
            {
                num = (0x5b * num) + this.netId[i];
            }
            return num;
        }

        internal static byte[] GetNetBytes(Match match)
        {
            if (match == null)
            {
                throw new ArgumentNullException("match");
            }
            if (!match.Success)
            {
                throw new ArgumentException("Regular exception didn't match!", "match");
            }
            Group group = match.get_Groups().get_Item("AmsNetId");
            Group group2 = match.get_Groups().get_Item("First");
            if (group2.Value != string.Empty)
            {
                return new byte[] { byte.Parse(group2.Value), byte.Parse(match.get_Groups().get_Item("Second").Value), byte.Parse(match.get_Groups().get_Item("Third").Value), byte.Parse(match.get_Groups().get_Item("Fourth").Value), byte.Parse(match.get_Groups().get_Item("Fifth").Value), byte.Parse(match.get_Groups().get_Item("Sixth").Value) };
            }
            StringComparer ordinalIgnoreCase = StringComparer.OrdinalIgnoreCase;
            return ((ordinalIgnoreCase.Compare(group.Value, "Local") != 0) ? ((ordinalIgnoreCase.Compare(group.Value, "LocalHost") != 0) ? ((ordinalIgnoreCase.Compare(group.Value, "Empty") != 0) ? null : Empty.ToBytes()) : LocalHost.ToBytes()) : Local.ToBytes());
        }

        private static byte[] GetNetId(string str)
        {
            Match match = regex.Match(str);
            if (!match.Success)
            {
                throw new FormatException($"Cannot parse netId string '{str}'");
            }
            return GetNetBytes(match);
        }

        internal static AmsNetId GetNetId(Match match) => 
            new AmsNetId(GetNetBytes(match));

        [Obsolete("Use AmsNetId.ToString(\"g\",null) instead!"), EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        public static string GetNetIdString(byte[] netId) => 
            $"{netId[0]}.{netId[1]}.{netId[2]}.{netId[3]}.{netId[4]}.{netId[5]}";

        public static bool IsEqual(AmsNetId netIDA, AmsNetId netIDB) => 
            ((netIDA != null) ? netIDA.Equals(netIDB) : false);

        public static bool IsSameTarget(AmsNetId netIDA, AmsNetId netIDB)
        {
            int num1;
            if (IsEqual(netIDA, netIDB) || (netIDA.IsLocal && (netIDB == LocalHost)))
            {
                num1 = 1;
            }
            else
            {
                num1 = !(netIDA == LocalHost) ? 0 : ((int) netIDB.IsLocal);
            }
            return (bool) num1;
        }

        public bool NetIdsEqual(byte[] netId) => 
            NetIdsEqual(this.netId, netId);

        public static bool NetIdsEqual(byte[] netId1, byte[] netId2)
        {
            for (int i = 0; i < 6; i++)
            {
                if (netId1[i] != netId2[i])
                {
                    return false;
                }
            }
            return true;
        }

        public static bool operator ==(AmsNetId o1, AmsNetId o2) => 
            (!Equals(o1, null) ? o1.Equals(o2) : Equals(o2, null));

        public static bool operator !=(AmsNetId o1, AmsNetId o2) => 
            !(o1 == o2);

        public static AmsNetId Parse(string str)
        {
            AmsNetId netId = null;
            if (!TryParse(str, out netId))
            {
                throw new FormatException("Format of AmsNetId is not valid!");
            }
            return netId;
        }

        public string ToBinHex() => 
            ToBinHex(this);

        public static string ToBinHex(AmsNetId netId)
        {
            StringBuilder builder = new StringBuilder();
            foreach (byte num2 in netId.ToBytes())
            {
                builder.Append(num2.ToString("X2"));
            }
            return builder.ToString();
        }

        public byte[] ToBytes()
        {
            byte[] destinationArray = new byte[6];
            Array.Copy(this.netId, 0, destinationArray, 0, 6);
            return destinationArray;
        }

        public override string ToString() => 
            this.ToString("g", null);

        public string ToString(string format, IFormatProvider formatProvider)
        {
            if (format == null)
            {
                return this.ToString("g", formatProvider);
            }
            if (format == "g")
            {
                return $"{this.netId[0]}.{this.netId[1]}.{this.netId[2]}.{this.netId[3]}.{this.netId[4]}.{this.netId[5]}";
            }
            if (format == "x")
            {
                return $"{this.netId[0]:x2}.{this.netId[1]:x2}.{this.netId[2]:x2}.{this.netId[3]:x2}.{this.netId[4]:x2}.{this.netId[5]:x2}";
            }
            if (format != "X")
            {
                throw new FormatException($"Invalid format string: '{format}'.");
            }
            return $"{this.netId[0]:X2}.{this.netId[1]:X2}.{this.netId[2]:X2}.{this.netId[3]:X2}.{this.netId[4]:X2}.{this.netId[5]:X2}";
        }

        public static bool TryParse(string str, out AmsNetId netId)
        {
            Match match = regex.Match(str);
            if (match.Success)
            {
                netId = GetNetId(match);
                return true;
            }
            netId = null;
            return false;
        }

        public bool IsLocal =>
            this.Equals(Local);

        public static AmsNetId Empty =>
            new AmsNetId(new byte[6]);

        public static AmsNetId LocalHost =>
            new AmsNetId(new byte[] { 0x7f, 0, 0, 1, 1, 1 });

        public static AmsNetId Local
        {
            get
            {
                if (_localNetId == null)
                {
                    TcLocalSystem system = null;
                    try
                    {
                        _localNetId = TcLocalSystem.GetInstance(TransportProtocol.All).NetId.Clone();
                    }
                    catch (Exception)
                    {
                        _localNetId = TcReg.LocalNetId;
                    }
                    finally
                    {
                        if (system != null)
                        {
                            system.Release();
                        }
                    }
                }
                return _localNetId;
            }
        }
    }
}

