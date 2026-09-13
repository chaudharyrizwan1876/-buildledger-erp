using System.Windows;
using WpfApplication = System.Windows.Application;

namespace BuildLedger.App.Services;

public enum AppTheme { Light, Dark }

public static class ThemeManager
{
    public static AppTheme Current { get; private set; } = AppTheme.Light;

    public static void Apply(AppTheme theme)
    {
        Current = theme;
        var dictionaries = WpfApplication.Current.Resources.MergedDictionaries;
        var uri = theme == AppTheme.Light
            ? "Themes/Colors.Light.xaml"
            : "Themes/Colors.Dark.xaml";

        var newDict = new ResourceDictionary { Source = new Uri(uri, UriKind.Relative) };
        dictionaries[0] = newDict;
    }
}
