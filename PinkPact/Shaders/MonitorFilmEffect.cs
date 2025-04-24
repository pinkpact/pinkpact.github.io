using System.Windows.Media.Effects;
using System.Windows.Media;
using System.Windows;
using System;

using PinkPact.Helpers;

namespace PinkPact.Shaders
{
    /// <summary>
    /// Represents an effect of varying intensity that simulates how a monitor would be viewed through a camera lens, making the pixels appear more exaggerated.
    /// </summary>
    public class MonitorFilmEffect : ShaderEffect
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
        /// Gets or sets the brightness of the pixel grid.
        /// <para>The default value is 0.5.</para>
        /// </summary>
        public double Intensity
        {
            get => (double)GetValue(IntensityProperty);
            set => SetValue(IntensityProperty, value);
        }

        /// <summary>
        /// Gets or sets the bending intensity at the edges of the visual.
        /// <para>The default value is 1.</para>
        /// </summary>
        public double EdgeBending
        {
            get => (double)GetValue(EdgeBendingPorperty);
            set => SetValue(EdgeBendingPorperty, value);
        }

        /// <summary>
        /// Gets or sets the stretch factors. This value determines how the pixel grid will be stretched on either axis.
        /// </summary>
        public Point StretchFactors 
        { 
            get => (Point)GetValue(StretchFactorsProperty); 
            set => SetValue(StretchFactorsProperty, value);
        }

        /// <summary>
        /// Gets or sets the virtual resolution. The resolution controls how the pixel grid will be spaced out on the visual.
        /// <para>The default resolution is 1920 x 1080.</para>
        /// </summary>
        public Point Resolution { get; set; } = new Point(1920, 1080);

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
            UriSource = new Uri("pack://application:,,,/PinkPact;component/Resources/Shaders/monitor_film.ps", UriKind.RelativeOrAbsolute)
        };

        public static readonly DependencyProperty InputProperty = RegisterPixelShaderSamplerProperty("Input", typeof(MonitorFilmEffect), 0);
        public static readonly DependencyProperty EdgeBendingPorperty = DependencyProperty.Register("EdgeBending", typeof(double), typeof(MonitorFilmEffect), new UIPropertyMetadata(1d, PixelShaderConstantCallback(2)));
        public static readonly DependencyProperty IntensityProperty = DependencyProperty.Register("Intensity", typeof(double), typeof(MonitorFilmEffect), new UIPropertyMetadata(0.5, PixelShaderConstantCallback(1)));
        public static readonly DependencyProperty StretchFactorsProperty = DependencyProperty.Register("StretchFactors", typeof(Point), typeof(MonitorFilmEffect), new PropertyMetadata(new Point(1, 1)));
        public static readonly DependencyProperty ViewportProperty = DependencyProperty.Register("Viewport", typeof(FrameworkElement), typeof(MonitorFilmEffect));
        static readonly DependencyProperty ResolutionProperty = DependencyProperty.Register("Resolution", typeof(Point), typeof(MonitorFilmEffect), new UIPropertyMetadata(new Point(1920, 1080), PixelShaderConstantCallback(0)));


        /// <summary>
        /// Creates a new instance of the <see cref="MonitorFilmEffect"/> class.
        /// </summary>
        public MonitorFilmEffect()
        {
            CompositionTarget.Rendering += OnRender;

            PixelShader = shader;

            UpdateShaderValue(InputProperty);
            UpdateShaderValue(IntensityProperty);
            UpdateShaderValue(ResolutionProperty);
        }

        ~MonitorFilmEffect()
        {
            CompositionTarget.Rendering -= OnRender;
        }

        void OnRender(object sender, EventArgs e)
        {
            if (Viewport is null)
            {
                if ((Point)GetValue(ResolutionProperty) != Resolution) SetValue(ResolutionProperty, new Point(Resolution.X * StretchFactors.X, Resolution.Y * StretchFactors.Y));
                return;
            }

            var size = Viewport.ActualSize();
            SetValue(ResolutionProperty, new Point(size.Width * StretchFactors.X, size.Height * StretchFactors.Y));
        }
    }
}