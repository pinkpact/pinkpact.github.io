using System.Windows.Media.Effects;
using System.Windows.Media;
using System.Windows;
using System;

namespace PinkPact.Shaders
{
    /// <summary>
    /// Represents an effect of varying intensity that simulates a 3D chromatic shift (red and blue edge ghosting). The displacement increases towards the edges.
    /// </summary>
    public class Chromatic3DEffect : ShaderEffect
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
        /// Gets or sets the amount of channel displacement near edges.
        /// <para>The default value is 5.</para>
        /// </summary>
        public double Intensity
        {
            get => (double)GetValue(IntensityProperty);
            set => SetValue(IntensityProperty, value);
        }

        static readonly PixelShader shader = new PixelShader()
        {
            UriSource = new Uri("pack://application:,,,/PinkPact;component/Resources/Shaders/chroma3d.ps", UriKind.RelativeOrAbsolute)
        };

        public static readonly DependencyProperty InputProperty = RegisterPixelShaderSamplerProperty("Input", typeof(Chromatic3DEffect), 0);
        public static readonly DependencyProperty IntensityProperty = DependencyProperty.Register("Intensity", typeof(double), typeof(Chromatic3DEffect), new UIPropertyMetadata(5.0, PixelShaderConstantCallback(0)));

        /// <summary>
        /// Creates a new instance of the <see cref="Chromatic3DEffect"/> class.
        /// </summary>
        public Chromatic3DEffect()
        {
            PixelShader = shader;
            Intensity = 5.0;

            UpdateShaderValue(InputProperty);
            UpdateShaderValue(IntensityProperty);
        }
    }
}
