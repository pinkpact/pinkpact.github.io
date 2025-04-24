using System.Windows.Media.Effects;
using System.Windows.Media;
using System.Windows;
using System;

namespace PinkPact.Shaders
{
    /// <summary>
    /// Represents a pixelation effect of varying intensity.
    /// </summary>
    public class PixelateEffect : ShaderEffect
    {
        /// <summary>
        /// Gets or sets the input brush.
        /// </summary>
        public Brush Input
        {
            get => (Brush)GetValue(InputProperty);
            set => SetValue(InputProperty, value);
        }

        /// <summary>
        /// Gets or sets the pixelation intensity.
        /// <para>The default value is 0.5.</para>
        /// </summary>
        public double Intensity
        {
            get => (double)GetValue(IntensityProperty);
            set => SetValue(IntensityProperty, value);
        }

        static readonly PixelShader shader = new PixelShader()
        {
            UriSource = new Uri("pack://application:,,,/PinkPact;component/Resources/Shaders/pixelate.ps", UriKind.RelativeOrAbsolute)
        };

        public static readonly DependencyProperty InputProperty = RegisterPixelShaderSamplerProperty("Input", typeof(PixelateEffect), 0);
        public static readonly DependencyProperty IntensityProperty = DependencyProperty.Register("Intensity", typeof(double), typeof(PixelateEffect), new UIPropertyMetadata(0.5, PixelShaderConstantCallback(0)));

        /// <summary>
        /// Creates a new instance of the <see cref="PixelateEffect"/> class.
        /// </summary>
        public PixelateEffect()
        {
            PixelShader = shader;
            Intensity = 1.0;

            UpdateShaderValue(InputProperty);
            UpdateShaderValue(IntensityProperty);
        }
    }
}