using System.Globalization;
using System.Windows;
using System.Windows.Markup;

namespace Commander;

public partial class App : Application
{
    public App()
    {
        FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement),
            new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));
        Thread.CurrentThread.CurrentCulture = new CultureInfo("de-DE");
        Thread.CurrentThread.CurrentUICulture = new CultureInfo("de-DE");
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        ApplyTheme();
        ThemeHelper.StartListening();
        ThemeHelper.ThemeChanged += OnThemeChanged;
    }

    protected override void OnExit(ExitEventArgs e)
    {
        ThemeHelper.StopListening();
        ThemeHelper.ThemeChanged -= OnThemeChanged;
        base.OnExit(e);
    }

    void OnThemeChanged()
        => Dispatcher.Invoke(ApplyTheme); // Ensure we update on the UI thread

    void ApplyTheme()
    {
        string themePath = ThemeHelper.IsDarkTheme()
            ? "Themes/Colors.Dark.xaml"
            : "Themes/Colors.Light.xaml";

        var newDict = new ResourceDictionary { Source = new Uri(themePath, UriKind.Relative) };

        if (currentTheme != null)
            Resources.MergedDictionaries.Remove(currentTheme);

        Resources.MergedDictionaries.Add(newDict);
        currentTheme = newDict;
    }

    ResourceDictionary? currentTheme;
}

