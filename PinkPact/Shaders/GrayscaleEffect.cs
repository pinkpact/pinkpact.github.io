using System.Windows.Media.Effects;
using System.Windows.Media;
using System.Windows;
using System;

namespace PinkPact.Shaders
{
    /// <summary>
    /// Represents a simple grayscale effect of varying intensity.
    /// </summary>
    public class GrayscaleEffect : ShaderEffect
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
        /// Gets or sets the amount of grayscaling applied (0 = none, 1 = full).
        /// </summary>
        public double Intensity
        {
            get => (double)GetValue(IntensityProperty);
            set => SetValue(IntensityProperty, value);
        }

        static readonly PixelShader shader = new PixelShader()
        {
            UriSource = new Uri("pack://application:,,,/PinkPact;component/Resources/Shaders/grayscale.ps", UriKind.RelativeOrAbsolute)
        };

        public static readonly DependencyProperty InputProperty = RegisterPixelShaderSamplerProperty("Input", typeof(GrayscaleEffect), 0);
        public static readonly DependencyProperty IntensityProperty = DependencyProperty.Register("Intensity", typeof(double), typeof(GrayscaleEffect), new UIPropertyMetadata(0.5, PixelShaderConstantCallback(0)));

        /// <summary>
        /// Creates a new instance of the <see cref="GrayscaleEffect"/> class.
        /// </summary>
        public GrayscaleEffect()
        {
            PixelShader = shader;
            Intensity = 5.0;

            UpdateShaderValue(InputProperty);
            UpdateShaderValue(IntensityProperty);
        }
    }
}
