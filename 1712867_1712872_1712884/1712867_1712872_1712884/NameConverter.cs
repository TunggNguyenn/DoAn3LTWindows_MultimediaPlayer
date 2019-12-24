using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace _1712867_1712872_1712884
{
    class NameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var playerFullName = value as string;
            var playerName = "";

            var tokens = playerFullName.Split(new string[] { "." }, StringSplitOptions.None);

            playerName = tokens[0];

            return playerName;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
