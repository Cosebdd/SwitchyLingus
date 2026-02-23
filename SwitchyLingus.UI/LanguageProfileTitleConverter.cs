using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using SwitchyLingus.Core.Config;

namespace SwitchyLingus.UI
{
    public class LanguageProfileTitleConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (!(value is string profileName))
                return null;
            var profile = AppConfig.CurrentConfig.LanguageProfiles[profileName];
            return $"{profile.Name} ({profile})";
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}