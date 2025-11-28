using System.Globalization;
using System.Windows.Data;

namespace FolderLockApp.GUI.Views;

public class BoolToStatusConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isLocked)
        {
            return isLocked ? "🔒 Locked" : "🔓 Unlocked";
        }
        return "Unknown";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
