namespace TwinCAT.PlcOpen
{
    using System;
    using System.ComponentModel;
    using System.Runtime.InteropServices;

    [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
    public abstract class PlcOpenDateConverterBase
    {
        protected PlcOpenDateConverterBase()
        {
        }

        public static byte[] GetBytes(DateTime dateTime) => 
            BitConverter.GetBytes(ToTicks(dateTime));

        public static int GetMarshalSize() => 
            4;

        public static DateTime ToDateTime(uint dateValue)
        {
            uint num;
            uint num2;
            uint num3;
            uint num4;
            uint num5;
            uint num6;
            if (dateValue < 0)
            {
                goto TR_0000;
            }
            else if (dateValue <= uint.MaxValue)
            {
                uint[] numArray = new uint[] { 0x1f, 0x1c, 0x1f, 30, 0x1f, 30, 0x1f, 0x1f, 30, 0x1f, 30, 0x1f };
                num = 0;
                num2 = 0;
                num3 = 0;
                num4 = 0;
                num5 = 0;
                num6 = 0;
                bool flag = false;
                uint num7 = 0;
                num2 = dateValue / 0x15180;
                num6 = dateValue % 0x15180;
                num3 = num6 / 0xe10;
                num6 = num6 % 0xe10;
                num5 = (uint) (num6 / 60);
                num6 = (uint) (num6 % 60);
                num = (num2 / 0x16d) + 0x7b2;
                num2 = num2 % 0x16d;
                num7 = ((num - 1) - 0x7b0) / 4;
                if ((num % 4) == 0)
                {
                    flag = true;
                }
                if (num7 <= num2)
                {
                    num2 -= num7;
                }
                else
                {
                    if (flag)
                    {
                        num7--;
                    }
                    num--;
                    num2 = 0x16d - (num7 - num2);
                }
                num2++;
                uint num8 = 0;
                for (uint i = 1; i < 13; i++)
                {
                    num7 = num8;
                    if (!flag || (i != 2))
                    {
                        num7 += numArray[((int) i) - 1];
                    }
                    else
                    {
                        num7 += (uint) 0x1d;
                    }
                    if (num2 <= num7)
                    {
                        num4 = i;
                        num2 -= num8;
                        break;
                    }
                    num8 = num7;
                }
            }
            else
            {
                goto TR_0000;
            }
            return new DateTime((int) num, (int) num4, (int) num2, (int) num3, (int) num5, (int) num6);
        TR_0000:
            throw new ArgumentOutOfRangeException();
        }

        public static uint ToTicks(DateTime date)
        {
            if ((date < new DateTime(0x7b2, 1, 1, 0, 0, 0)) || (date >= new DateTime(0x83a, 2, 7, 0, 0, 0)))
            {
                throw new ArgumentOutOfRangeException("DateTime is out of range for PLCOpen DATE data type!");
            }
            uint[] numArray = new uint[] { 0x1f, 0x1c, 0x1f, 30, 0x1f, 30, 0x1f, 0x1f, 30, 0x1f, 30, 0x1f };
            uint num = (uint) ((date.Day + (0x16d * (date.Year - 0x7b2))) + (((date.Year - 1) - 0x7b0) / 4));
            if (((date.Year % 4) == 0) && (date.Month > 2))
            {
                num++;
            }
            for (uint i = 1; i < date.Month; i++)
            {
                num += numArray[((int) i) - 1];
            }
            return (uint) (((((num - 1) * 0x15180) + (date.Hour * 0xe10)) + (date.Minute * 60)) + date.Second);
        }

        internal static bool TryParseDateTime(string str, out DateTime dateTime) => 
            DateTime.TryParse(str, out dateTime);

        internal static bool TryParseTimeSpan(string str, out TimeSpan span) => 
            TimeSpan.TryParse(str, out span);
    }
}

