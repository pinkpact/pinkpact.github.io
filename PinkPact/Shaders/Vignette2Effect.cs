using System.Windows.Media.Effects;
using System.Windows.Media;
using System.Windows;
using System;

namespace PinkPact.Shaders
{
    /// <summary>
    /// Represents another basic vignette effect of varying intensity. This effect is a refinement of the more primitive-looking <see cref="VignetteEffect"/>.
    /// </summary>
    public class Vignette2Effect : ShaderEffect
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
        /// Gets or sets the vignette's intensity.
        /// <para>The default value is 1.</para>
        /// </summary>
        public double Intensity
        {
            get => (double)GetValue(IntensityProperty);
            set => SetValue(IntensityProperty, value);
        }

        /// <summary>
        /// Gets or sets the vignette's color.
        /// <para>The default value is <see cref="Colors.White"/>.</para>
        /// </summary>
        public Color Color
        {
            get => (Color)GetValue(ColorProperty);
            set => SetValue(ColorProperty, value);
        }

        static readonly PixelShader shader = new PixelShader
        {
            UriSource = new Uri("pack://application:,,,/PinkPact;component/Resources/Shaders/vignette2.ps", UriKind.RelativeOrAbsolute)
        };

        public static readonly DependencyProperty InputProperty = RegisterPixelShaderSamplerProperty("Input", typeof(Vignette2Effect), 0);

        public static readonly DependencyProperty ColorProperty = DependencyProperty.Register("Color", typeof(Color), typeof(Vignette2Effect), new UIPropertyMetadata(Colors.White, PixelShaderConstantCallback(0)));
        public static readonly DependencyProperty IntensityProperty = DependencyProperty.Register("Intensity", typeof(double), typeof(Vignette2Effect), new UIPropertyMetadata(1.0, PixelShaderConstantCallback(1)));

        /// <summary>
        /// Creates a new instance of the <see cref="Vignette2Effect"/> class.
        /// </summary>
        public Vignette2Effect()
        {
            PixelShader = shader;

            UpdateShaderValue(InputProperty);
            UpdateShaderValue(ColorProperty);
            UpdateShaderValue(IntensityProperty);
        }
    }
}
