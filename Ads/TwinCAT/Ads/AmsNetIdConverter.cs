namespace TwinCAT.Ads
{
    using System;
    using System.ComponentModel;
    using System.Globalization;

    public class AmsNetIdConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) => 
            ((sourceType != typeof(string)) ? base.CanConvertFrom(context, sourceType) : true);

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType) => 
            ((destinationType != typeof(string)) ? base.CanConvertTo(context, destinationType) : true);

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) => 
            (!(value is string) ? base.ConvertFrom(context, culture, value) : AmsNetId.Parse((string) value));

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType != typeof(string))
            {
                return base.ConvertTo(context, culture, value, destinationType);
            }
            AmsNetId id = (AmsNetId) value;
            return (!(id == AmsNetId.Empty) ? (!(id == AmsNetId.Local) ? (!(id == AmsNetId.LocalHost) ? id.ToString() : "LocalHost") : "Local") : "Empty");
        }

        public override TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            object[] objArray1 = new object[] { AmsNetId.Empty, AmsNetId.Local, AmsNetId.LocalHost };
            return new TypeConverter.StandardValuesCollection(objArray1);
        }

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context) => 
            base.GetStandardValuesExclusive(context);

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context) => 
            true;

        public override bool IsValid(ITypeDescriptorContext context, object value) => 
            base.IsValid(context, value);
    }
}

