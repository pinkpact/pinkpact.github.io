using System.Windows.Media.Effects;
using System.Windows.Media;
using System.Windows;
using System;

namespace PinkPact.Shaders
{
    /// <summary>
    /// Represents a alternating horizontal stripe overlay effect.
    /// </summary>
    public class StripeEffect : ShaderEffect
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
        /// Gets or sets the color of the stripes. Stripes alternate between this color and a darkened version of it.
        /// <para>The default value is <see cref="Colors.White"/>.</para>
        /// </summary>
        public Color Color
        {
            get => (Color)GetValue(ColorProperty);
            set => SetValue(ColorProperty, value);
        }

        /// <summary>
        /// Gets or sets the relative height of each stripe.
        /// <para>The default value is 4.5.</para>
        /// </summary>
        public double Height
        {
            get => (double)GetValue(HeightProperty);
            set => SetValue(HeightProperty, value);
        }

        /// <summary>
        /// Gets or sets the opacity of the stripe overlay.
        /// <para>The default value is 0.05.</para>
        /// </summary>
        public double Opacity
        {
            get => (double)GetValue(OpacityProperty);
            set => SetValue(OpacityProperty, value);
        }

        static readonly PixelShader shader = new PixelShader
        {
            UriSource = new Uri("pack://application:,,,/PinkPact;component/Resources/Shaders/stripe_overlay.ps", UriKind.RelativeOrAbsolute)
        };

        public static readonly DependencyProperty InputProperty = RegisterPixelShaderSamplerProperty("Input", typeof(StripeEffect), 0);

        public static readonly DependencyProperty OpacityProperty = DependencyProperty.Register("Opacity", typeof(double), typeof(StripeEffect), new UIPropertyMetadata(0.05, PixelShaderConstantCallback(2)));
        public static readonly DependencyProperty ColorProperty = DependencyProperty.Register("Color", typeof(Color), typeof(StripeEffect), new UIPropertyMetadata(Colors.White, PixelShaderConstantCallback(0)));
        public static readonly DependencyProperty HeightProperty = DependencyProperty.Register("Height", typeof(double), typeof(StripeEffect), new UIPropertyMetadata(4.5, PixelShaderConstantCallback(1)));

        /// <summary>
        /// Creates a new instance of the <see cref="StripeEffect"/> class.
        /// </summary>
        public StripeEffect()
        {
            PixelShader = shader;

            UpdateShaderValue(InputProperty);
            UpdateShaderValue(ColorProperty);
            UpdateShaderValue(HeightProperty);
            UpdateShaderValue(OpacityProperty);
        }
    }
}
