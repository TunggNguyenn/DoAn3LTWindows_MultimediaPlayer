using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace _1712867_1712872_1712884
{
    class ExtensionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var playerFullName = value as string;
            var playerExtensionName = "";

            var tokens = playerFullName.Split(new string[] { "." }, StringSplitOptions.None);

            playerExtensionName = tokens[1];

            if (playerExtensionName == "mp3")
            {
                return "/Images/mp3.png";
            }
            else
            {
                return "/Images/mp4.png";
            }

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
