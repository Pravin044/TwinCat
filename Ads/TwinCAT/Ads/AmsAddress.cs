namespace TwinCAT.Ads
{
    using System;
    using System.Runtime.InteropServices;
    using System.Text.RegularExpressions;

    public class AmsAddress
    {
        protected internal AmsNetId netId;
        protected internal int port;
        public const string RegularExpressionPattern = @"^(?<AmsNetId>((?<First>\d{1,3})\.(?<Second>\d{1,3})\.(?<Third>\d{1,3})\.(?<Fourth>\d{1,3})\.(?<Fifth>\d{1,3})\.(?<Sixth>\d{1,3})) | Local | Empty | LocalHost)(:(?<AdsPort>\d+))?$";
        private static Regex regex = new Regex(@"^(?<AmsNetId>((?<First>\d{1,3})\.(?<Second>\d{1,3})\.(?<Third>\d{1,3})\.(?<Fourth>\d{1,3})\.(?<Fifth>\d{1,3})\.(?<Sixth>\d{1,3})) | Local | Empty | LocalHost)(:(?<AdsPort>\d+))?$", ((RegexOptions) RegexOptions.CultureInvariant) | (((RegexOptions) RegexOptions.IgnorePatternWhitespace) | (((RegexOptions) RegexOptions.Compiled) | ((RegexOptions) RegexOptions.IgnoreCase))));

        protected AmsAddress()
        {
        }

        public AmsAddress(int port)
        {
            this.netId = AmsNetId.Local;
            this.port = port;
        }

        public AmsAddress(string str)
        {
            if (!TryParse(str, out this.netId, out this.port))
            {
                throw new FormatException();
            }
        }

        public AmsAddress(AmsAddress address)
        {
            this.netId = address.netId;
            this.port = address.port;
        }

        public AmsAddress(AmsPort port)
        {
            this.netId = AmsNetId.Local;
            this.port = (int) port;
        }

        public AmsAddress(string netId, int port)
        {
            this.netId = new AmsNetId(netId);
            this.port = port;
        }

        public AmsAddress(string netId, AmsPort port)
        {
            this.netId = new AmsNetId(netId);
            this.port = (int) port;
        }

        public AmsAddress(AmsNetId netId, int port)
        {
            this.netId = netId;
            this.port = port;
        }

        public AmsAddress(byte[] netId, int port)
        {
            this.netId = new AmsNetId(netId);
            this.port = port;
        }

        public AmsAddress(AmsNetId netId, AmsPort port)
        {
            this.netId = netId;
            this.port = (int) port;
        }

        public AmsAddress(byte[] netId, AmsPort port)
        {
            this.netId = new AmsNetId(netId);
            this.port = (int) port;
        }

        public AmsAddress Clone() => 
            new AmsAddress(this);

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (!(obj is AmsAddress))
            {
                return false;
            }
            AmsAddress address = (AmsAddress) obj;
            return ((this.port == address.port) ? !(this.netId != address.netId) : false);
        }

        public override int GetHashCode() => 
            ((0x5b * ((0x5b * 10) + this.port)) + this.NetId.GetHashCode());

        private static int GetPort(Match match)
        {
            Group group = match.get_Groups().get_Item("AdsPort");
            int num = 0x2710;
            if (group.Value != string.Empty)
            {
                num = int.Parse(group.Value);
            }
            return num;
        }

        public static bool operator ==(AmsAddress o1, AmsAddress o2) => 
            (!Equals(o1, null) ? o1.Equals(o2) : Equals(o2, null));

        public static bool operator !=(AmsAddress o1, AmsAddress o2) => 
            !(o1 == o2);

        public static AmsAddress Parse(string str)
        {
            AmsAddress address = null;
            if (!TryParse(str, out address))
            {
                throw new FormatException();
            }
            return address;
        }

        public override string ToString() => 
            $"{this.netId.ToString()}:{this.port}";

        public static bool TryParse(string str, out AmsAddress address)
        {
            AmsNetId netId = null;
            int port = 0;
            if (TryParse(str, out netId, out port))
            {
                address = new AmsAddress(netId, port);
                return true;
            }
            address = null;
            return false;
        }

        internal static bool TryParse(string str, out AmsNetId netId, out int port)
        {
            Match match = regex.Match(str);
            if (match.Success)
            {
                netId = AmsNetId.GetNetId(match);
                port = GetPort(match);
                return true;
            }
            netId = null;
            port = 0;
            return false;
        }

        public AmsNetId NetId
        {
            get => 
                this.netId;
            set => 
                (this.netId = value.Clone());
        }

        public int Port
        {
            get => 
                this.port;
            set => 
                (this.port = value);
        }
    }
}

