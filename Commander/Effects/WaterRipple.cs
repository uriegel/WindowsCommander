using System.Reflection;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace Commander;

public class WaterRipple : ShaderEffect
{
    public Brush Input
    {
        get => (Brush)GetValue(InputProperty);
        set => SetValue(InputProperty, value);
    }
    public Point Center
    {
        get => (Point)GetValue(CenterProperty);
        set => SetValue(CenterProperty, value);
    }
    public double Amplitude
    {
        get => (double)GetValue(AmplitudeProperty);
        set => SetValue(AmplitudeProperty, value);
    }
    public double Frequency
    {
        get => (double)GetValue(FrequencyProperty);
        set => SetValue(FrequencyProperty, value);
    }
    public double Phase
    {
        get => (double)GetValue(PhaseProperty);
        set => SetValue(PhaseProperty, value);
    }
    public double RatioControl
    {
        get => (double)GetValue(RatioControlProperty);
        set => SetValue(RatioControlProperty, value);
    }

    public WaterRipple()
    {
        PixelShader = new PixelShader();
        SetShader(PixelShader, "WaterRipple.ps");

        UpdateShaderValue(InputProperty);
        UpdateShaderValue(CenterProperty);
        UpdateShaderValue(AmplitudeProperty);
        UpdateShaderValue(FrequencyProperty);
        UpdateShaderValue(PhaseProperty);
        UpdateShaderValue(RatioControlProperty);
    }

    public static readonly DependencyProperty InputProperty =
            RegisterPixelShaderSamplerProperty("Input", typeof(WaterRipple), 0);
    public static readonly DependencyProperty CenterProperty =
        DependencyProperty.Register("Center", typeof(Point), typeof(WaterRipple),
            new UIPropertyMetadata(new Point(0.5D, 0.5D), PixelShaderConstantCallback(0)));
    public static readonly DependencyProperty AmplitudeProperty =
        DependencyProperty.Register("Amplitude", typeof(double), typeof(WaterRipple),
            new UIPropertyMetadata(((double)(1D)), PixelShaderConstantCallback(1)));
    public static readonly DependencyProperty FrequencyProperty =
        DependencyProperty.Register("Frequency", typeof(double), typeof(WaterRipple),
            new UIPropertyMetadata(((double)(25D)), PixelShaderConstantCallback(2)));
    public static readonly DependencyProperty PhaseProperty =
        DependencyProperty.Register("Phase", typeof(double), typeof(WaterRipple),
            new UIPropertyMetadata(((double)(0D)), PixelShaderConstantCallback(3)));
    public static readonly DependencyProperty RatioControlProperty =
        DependencyProperty.Register("RatioControl", typeof(double), typeof(WaterRipple),
            new UIPropertyMetadata(((double)(1D)), PixelShaderConstantCallback(4)));

    static void SetShader(PixelShader pixelShader, string shaderName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var stream = assembly.GetManifestResourceStream(assembly.GetName().Name + ".Effects." + shaderName);
        pixelShader.SetStreamSource(stream);
    }
}

