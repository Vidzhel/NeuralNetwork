using System;
using System.Globalization;
using System.Windows;

namespace SymbolRecognitionLib.Converters
{
    public class BoolInverser : BaseValueConverter<BoolInverser>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(bool)value;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
