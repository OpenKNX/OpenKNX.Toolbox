using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace OpenKNX.Toolbox.Converter
{
    public class BoolToFloatConverter : IValueConverter
    {
        public float TrueValue { get; set; } = 1;
        public float FalseValue { get; set; } = 0;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value is bool boolValue)
            {
                return boolValue ? TrueValue : FalseValue;
            }
            return FalseValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
