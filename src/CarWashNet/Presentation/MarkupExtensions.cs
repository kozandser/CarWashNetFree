using CarWashNet.Domain.Model;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;

namespace CarWashNet.Presentation
{
    public class EntityStateToColorConverter : MarkupExtension, IValueConverter
    {
        private static EntityStateToColorConverter _converter = null;

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return _converter ?? (_converter = new EntityStateToColorConverter());
        }
        public object Convert(object value, Type targetType,
                              object parameter, CultureInfo culture)
        {
            if (value is EntityStateEnum)
            {
                var val = (EntityStateEnum)value;
                switch (val)
                {
                    case EntityStateEnum.Preparing:
                        return (Color)ColorConverter.ConvertFromString("#95a5a6");
                    //case EntityStateEnum.Active:
                    //    break;
                    case EntityStateEnum.Unused:
                        return (Color)ColorConverter.ConvertFromString("#e74c3c");
                    //case EntityStateEnum.Deleted:
                    //    break;
                    default:
                        return Colors.Transparent;
                }
            }
            return Colors.Transparent;
        }

        public object ConvertBack(object value, Type targetType,
                                  object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
