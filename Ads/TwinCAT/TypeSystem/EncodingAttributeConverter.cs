namespace TwinCAT.TypeSystem
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;

    internal static class EncodingAttributeConverter
    {
        internal static Encoding GetEncoding(TypeAttribute att)
        {
            Encoding encoding = null;
            if (!TryGetEncoding(att, out encoding))
            {
                throw new ArgumentException("Attributte '{0}' is not the 'TcEncoding' attribute!", att.Name);
            }
            return encoding;
        }

        internal static bool TryGetEncoding(IEnumerable<ITypeAttribute> attributes, out Encoding encoding)
        {
            ITypeAttribute attribute = Enumerable.FirstOrDefault<ITypeAttribute>(attributes, att => string.CompareOrdinal(att.Name, "TcEncoding") == 0);
            if (attribute != null)
            {
                return TryGetEncoding(attribute, out encoding);
            }
            encoding = null;
            return false;
        }

        internal static bool TryGetEncoding(string encodingName, out Encoding encoding)
        {
            encoding = null;
            try
            {
                encoding = Encoding.GetEncoding(encodingName);
            }
            catch (ArgumentException)
            {
                int result = 0;
                if (int.TryParse(encodingName, out result))
                {
                    encoding = Encoding.GetEncoding(result);
                }
            }
            return (encoding != null);
        }

        internal static bool TryGetEncoding(ITypeAttribute att, out Encoding encoding)
        {
            encoding = null;
            return ((att.Name == "TcEncoding") && TryGetEncoding(att.Value, out encoding));
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly EncodingAttributeConverter.<>c <>9 = new EncodingAttributeConverter.<>c();
            public static Func<ITypeAttribute, bool> <>9__3_0;

            internal bool <TryGetEncoding>b__3_0(ITypeAttribute att) => 
                (string.CompareOrdinal(att.Name, "TcEncoding") == 0);
        }
    }
}

