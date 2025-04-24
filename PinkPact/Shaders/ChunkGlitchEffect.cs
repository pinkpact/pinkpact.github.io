using System.Windows.Media.Effects;
using System.Windows.Media;
using System.Windows;
using System;

namespace PinkPact.Shaders
{
    /// <summary>
    /// Represents a chunky glitch effect of varying intensity that distorts regions based on a random seed.
    /// </summary>
    public class ChunkGlitchEffect : ShaderEffect
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
        /// Gets or sets the frequency of glitch chunks across the visual. (0 = none, 1 = many).
        /// </summary>
        public double Intensity
        {
            get => (double)GetValue(IntensityProperty);
            set => SetValue(IntensityProperty, value);
        }

        /// <summary>
        /// Gets or sets a value that controls the randomness of the glitch chunks. Changing this value will yield a new glitch chunk layout.
        /// </summary>
        public double Seed
        {
            get => (double)GetValue(SeedProperty);
            set => SetValue(SeedProperty, value);
        }

        static readonly PixelShader shader = new PixelShader
        {
            UriSource = new Uri("pack://application:,,,/PinkPact;component/Resources/Shaders/chunk_glitch.ps", UriKind.RelativeOrAbsolute)
        };

        public static readonly DependencyProperty InputProperty = RegisterPixelShaderSamplerProperty("Input", typeof(ChunkGlitchEffect), 0);
        public static readonly DependencyProperty SeedProperty = DependencyProperty.Register("Seed", typeof(double), typeof(ChunkGlitchEffect), new UIPropertyMetadata(0.0, PixelShaderConstantCallback(1)));
        public static readonly DependencyProperty IntensityProperty = DependencyProperty.Register("Intensity", typeof(double), typeof(ChunkGlitchEffect), new UIPropertyMetadata(0.5, PixelShaderConstantCallback(0)));

        /// <summary>
        /// Creates a new instance of the <see cref="ChunkGlitchEffect"/> class.
        /// </summary>
        public ChunkGlitchEffect()
        {
            PixelShader = shader;

            UpdateShaderValue(InputProperty);
            UpdateShaderValue(IntensityProperty);
            UpdateShaderValue(SeedProperty);
        }
    }
}
