using Microsoft.Win32;

namespace Commander;

public static class ThemeHelper
{
    public static event Action? ThemeChanged;

    public static bool IsDarkTheme()
    {
        const string key = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";
        using var personalizeKey = Registry.CurrentUser.OpenSubKey(key);
        return (int?)personalizeKey?.GetValue("AppsUseLightTheme") == 0;
    }

    public static void StartListening()
        => SystemEvents.UserPreferenceChanged += OnUserPreferenceChanged;

    public static void StopListening() 
        => SystemEvents.UserPreferenceChanged -= OnUserPreferenceChanged;

    private static void OnUserPreferenceChanged(object? sender, UserPreferenceChangedEventArgs e)
    {
        if (e.Category == UserPreferenceCategory.General)
            ThemeChanged?.Invoke();
    }
}