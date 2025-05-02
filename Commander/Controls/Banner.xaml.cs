using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace Commander.Controls;

public partial class Banner : UserControl
{
    #region Dependency Properties

    public static readonly DependencyProperty BannerTextProperty = DependencyProperty.Register(
        "BannerText", typeof(string), typeof(Banner), new PropertyMetadata(null, BannerTextChanged));

    public string? BannerText
    {
        get => (string?)GetValue(BannerTextProperty);
        set => SetValue(BannerTextProperty, value);
    }

    public void SetBannerText(DependencyObject obj, string? value) => obj.SetValue(BannerTextProperty, value);

    public static void BannerTextChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue != null && e.OldValue == null && obj is Banner banner)
            banner.ShowBanner();
        else if (e.NewValue == null && e.OldValue != null && obj is Banner banner2)
            banner2.HideBanner();
    }

    #endregion

    public Banner() => InitializeComponent();

    void ShowBanner()
    {
        BannerControl.Visibility = Visibility.Visible;
        var fade = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(transitionInMillis));
        BannerControl.BeginAnimation(OpacityProperty, fade);
        var heightAnimation = new DoubleAnimation(0, 40, TimeSpan.FromMilliseconds(transitionInMillis))
        {
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };
        BannerControl.BeginAnimation(HeightProperty, heightAnimation);
    }

    void HideBanner()
    {
        var fade = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(transitionInMillis));
        BannerControl.BeginAnimation(OpacityProperty, fade);
        var slide = new DoubleAnimation(0, -20, TimeSpan.FromMilliseconds(2000))
        {
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
        };
        fade.Completed += (s, _) => BannerControl.Visibility = Visibility.Collapsed;
        
        var heightAnimation = new DoubleAnimation(BannerControl.ActualHeight, 0, TimeSpan.FromMilliseconds(transitionInMillis))
        {
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
        };
        BannerControl.BeginAnimation(HeightProperty, heightAnimation);
    }

    void Dismiss_Click(object sender, RoutedEventArgs e)
    {

    }

    int transitionInMillis = 200;
}
