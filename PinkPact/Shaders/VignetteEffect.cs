using System.Windows.Media.Effects;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using System;

using PinkPact.Helpers;

namespace PinkPact.Shaders
{
    /// <summary>
    /// Represents a basic vignette effect of varying intensity.
    /// </summary>
    public class VignetteEffect : ShaderEffect
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
        /// Gets or sets the vignette's brush.
        /// <para>The default value is <see cref="Brushes.White"/>.</para>
        /// </summary>
        public Brush Brush
        {
            get => (Brush)GetValue(BrushProperty);
            set => SetValue(BrushProperty, value);
        }

        /// <summary>
        /// Gets or sets the vignette's intensity. Values under 1.0 will produce glow around the edges, providing a more rectangular vignette.
        /// <para>The default value is 1.</para>
        /// </summary>
        public double Intensity
        {
            get => (double)GetValue(IntensityProperty);
            set => SetValue(IntensityProperty, value);
        }

        /// <summary>
        /// Gets or sets the viewport over which this effect would be applied to. The effect's resolution will be taken from the viewport's size.
        /// <para>If not <see langword="null"/>, overrides the Resolution property.</para>
        /// </summary>
        public FrameworkElement Viewport
        {
            get => GetValue(ViewportProperty) as FrameworkElement;
            set => SetValue(ViewportProperty, value);
        }

        static readonly PixelShader shader = new PixelShader
        {
            UriSource = new Uri("pack://application:,,,/PinkPact;component/Resources/Shaders/vignette.ps", UriKind.RelativeOrAbsolute)
        };

        public static readonly DependencyProperty InputProperty = RegisterPixelShaderSamplerProperty("Input", typeof(VignetteEffect), 0);
        static readonly DependencyProperty BrushTextureProperty = RegisterPixelShaderSamplerProperty("BrushTexture", typeof(VignetteEffect), 1);

        public static readonly DependencyProperty BrushProperty = DependencyProperty.Register("Brush", typeof(Brush), typeof(VignetteEffect), new PropertyMetadata(Brushes.White, OnBrushChanged));
        public static readonly DependencyProperty IntensityProperty = DependencyProperty.Register("Intensity", typeof(double), typeof(VignetteEffect), new UIPropertyMetadata(1.0, PixelShaderConstantCallback(0)));
        public static readonly DependencyProperty ViewportProperty = DependencyProperty.Register("Viewport", typeof(FrameworkElement), typeof(VignetteEffect), new PropertyMetadata(null, OnBrushChanged));

        /// <summary>
        /// Creates a new instance of the <see cref="VignetteEffect"/> class.
        /// </summary>
        public VignetteEffect()
        {
            PixelShader = shader;

            UpdateShaderValue(InputProperty);
            UpdateShaderValue(BrushTextureProperty);
            UpdateShaderValue(IntensityProperty);
        }

        static void OnBrushChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var obj = d as VignetteEffect;
            var size = obj.Viewport?.ActualSize() ?? new Size(double.MaxValue, double.MaxValue);

            var grid = new Grid()
            {
                Width = size.Width,
                Height = size.Height,
                Background = obj.Brush
            };

            var brush = new VisualBrush(grid);
            obj.SetValue(BrushTextureProperty, brush);
        }
    }
}
