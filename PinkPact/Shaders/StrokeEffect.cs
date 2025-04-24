using System.Windows.Media.Effects;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using System;

using PinkPact.Helpers;

namespace PinkPact.Shaders
{
    /// <summary>
    /// Represents a basic stroke effect of varying thickness.
    /// </summary>
    public class StrokeEffect : ShaderEffect
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
        /// Gets or sets the stroke's brush.
        /// <para>The default value is <see cref="Brushes.White"/>.</para>
        /// </summary>
        public Brush Brush
        {
            get => (Brush)GetValue(BrushProperty);
            set => SetValue(BrushProperty, value);
        }

        /// <summary>
        /// Gets or sets the stroke's thinkness.
        /// <para>The default value is 1.</para>
        /// </summary>
        public double Thickness
        {
            get => (double)GetValue(ThicknessProperty);
            set => SetValue(ThicknessProperty, value);
        }

        /// <summary>
        /// Gets or sets a value representing the minimum opacity a pixel can have to be qualified to have a stroke.
        /// <para>The default value is 1.</para>
        /// </summary>
        public double AlphaThreshold
        {
            get => (double)GetValue(AlphaThresholdProperty);
            set => SetValue(AlphaThresholdProperty, value);
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

        /// <summary>
        /// Gets or sets the virtual resolution. The resolution controls how defined the stroke will appear (smaller resolution -> more defined, bigger strokes).
        /// <para>The default resolution is 1920 x 1080.</para>
        /// </summary>
        public Point Resolution { get; set; } = new Point(1920, 1080);

        static readonly PixelShader shader = new PixelShader
        {
            UriSource = new Uri("pack://application:,,,/PinkPact;component/Resources/Shaders/stroke.ps", UriKind.RelativeOrAbsolute)
        };

        public static readonly DependencyProperty InputProperty = RegisterPixelShaderSamplerProperty("Input", typeof(StrokeEffect), 0);
        static readonly DependencyProperty BrushTextureProperty = RegisterPixelShaderSamplerProperty("BrushTexture", typeof(StrokeEffect), 1);

        public static readonly DependencyProperty BrushProperty = DependencyProperty.Register("Brush", typeof(Brush), typeof(StrokeEffect), new PropertyMetadata(Brushes.White, OnBrushChanged));
        public static readonly DependencyProperty ViewportProperty = DependencyProperty.Register("Viewport", typeof(FrameworkElement), typeof(StrokeEffect), new PropertyMetadata(null, OnBrushChanged));
        public static readonly DependencyProperty ThicknessProperty = DependencyProperty.Register("Thickness", typeof(double), typeof(StrokeEffect), new UIPropertyMetadata(1.0, PixelShaderConstantCallback(0)));
        public static readonly DependencyProperty AlphaThresholdProperty = DependencyProperty.Register("AlphaThreshold", typeof(double), typeof(StrokeEffect), new UIPropertyMetadata(0.999999, PixelShaderConstantCallback(1)));
        static readonly DependencyProperty ResolutionProperty = DependencyProperty.Register("Resolution", typeof(Point), typeof(StrokeEffect), new UIPropertyMetadata(new Point(1920, 1080), PixelShaderConstantCallback(2)));

        /// <summary>
        /// Creates a new instance of the <see cref="StrokeEffect"/> class.
        /// </summary>
        public StrokeEffect()
        {
            CompositionTarget.Rendering += OnRender;

            PixelShader = shader;

            OnBrushChanged(this, default);

            UpdateShaderValue(InputProperty);
            UpdateShaderValue(BrushTextureProperty);
            UpdateShaderValue(ResolutionProperty);
            UpdateShaderValue(ThicknessProperty);
        }

        ~StrokeEffect()
        {
            CompositionTarget.Rendering -= OnRender;
        }

        void OnRender(object sender, EventArgs e)
        {
            if (Viewport is null)
            {
                if ((Point)GetValue(ResolutionProperty) != Resolution) SetValue(ResolutionProperty, Resolution);
                return;
            }

            var size = Viewport.ActualSize();
            SetValue(ResolutionProperty, new Point(size.Width, size.Height));
        }

        static void OnBrushChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var obj = d as StrokeEffect;
            var size = obj.Viewport?.ActualSize() ?? new Size(obj.Resolution.X, obj.Resolution.Y);

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
