using System;
using System.ComponentModel;
using System.Globalization;

namespace DqtApi.ModelBinding
{
    public class DateOnlyTypeConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return destinationType == typeof(DateOnly?) || destinationType == typeof(DateOnly?);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            return DateOnly.ParseExact((string)value, Constants.DateFormat);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            return ((DateOnly)value).ToString(Constants.DateFormat);
        }
    }
}
