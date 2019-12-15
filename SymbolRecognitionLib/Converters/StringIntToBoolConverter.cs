using System;
using System.Globalization;
using System.Windows;

namespace SymbolRecognitionLib.Converters
{
    public class StringIntToBoolConverter : BaseValueConverter<StringIntToBoolConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int intager;

            if (!int.TryParse((string)value, out intager))
                return false;

            return intager == 0 ? false : true;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
