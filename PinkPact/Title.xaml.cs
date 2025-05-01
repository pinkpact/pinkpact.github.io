using PinkPact.Helpers;
using PinkPact.Shaders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PinkPact
{
    /// <summary>
    /// Interaction logic for Title.xaml
    /// </summary>
    public partial class Title : UserControl
    {
        internal const int fadeDuration = 2000,
                           shineDuration = 3250;

        MainWindow parent;

        public Title(MainWindow parent)
        {
            this.parent = parent;
            InitializeComponent();
            //Loaded += (_, __) => _ = Show();
        }

        public async Task Show()
        {
            parent.ui_layer.Effect = new MonitorFilmEffect() { Intensity = 0.3, Viewport = viewport };

            // Pop in logo

            logo.Visibility = Visibility.Visible;
            logo.Opacity = 0;

            var logoPop = new DoubleAnimation(20, 0, TimeSpan.FromMilliseconds(fadeDuration));
            logoPop.EasingFunction = new QuinticEase() { EasingMode = EasingMode.EaseOut };
            Timeline.SetDesiredFrameRate(logoPop, 5);
            logoTranslation.BeginAnimation(TranslateTransform.YProperty, logoPop);

            // Fade in logo

            var fadeAnim = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(fadeDuration));
            fadeAnim.EasingFunction = new SineEase();
            Timeline.SetDesiredFrameRate(fadeAnim, 5);
            logo.BeginAnimation(OpacityProperty, fadeAnim);

            var monitorAnim = new DoubleAnimation(0.3, 0, TimeSpan.FromMilliseconds((fadeDuration + shineDuration) / 2));
            monitorAnim.EasingFunction = new ElasticEase() { EasingMode = EasingMode.EaseOut };
            parent.ui_layer.Effect.BeginAnimation(MonitorFilmEffect.IntensityProperty, monitorAnim);

            await Task.Delay(fadeDuration);

            // Shine

            var shineAnim = new DoubleAnimation(-250, 1050, TimeSpan.FromMilliseconds(shineDuration));
            shineAnim.EasingFunction = new QuinticEase() { EasingMode = EasingMode.EaseInOut };

            Timeline.SetDesiredFrameRate(shineAnim, 10);
            shineTranslation.BeginAnimation(TranslateTransform.XProperty, shineAnim);

            await Task.Delay(shineDuration);

            // Fade out

            fadeAnim.From = logo.Opacity;
            fadeAnim.To = 0;

            logo.BeginAnimation(OpacityProperty, fadeAnim);

            await Task.Delay(fadeDuration);

            // Value enforcement

            logo.BeginAnimation(OpacityProperty, null);
            shineTranslation.BeginAnimation(TranslateTransform.XProperty, null);
            logoTranslation.BeginAnimation(TranslateTransform.YProperty, null);

            logo.Opacity = 0;
            logo.Visibility = Visibility.Collapsed;
            viewport.Children.Remove(logo);

            parent.ui_layer.Effect = null;
        }

        public async Task FirstSequence()
        {
            titleGrid.Children.Clear();

            await titleGrid.Children.Add(titleBg);
            titleGrid.Visibility = Visibility.Visible;

            var fadeAnim = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(3000));
            fadeAnim.EasingFunction = new SineEase();
            Timeline.SetDesiredFrameRate(fadeAnim, 5);
            titleGrid.BeginAnimation(OpacityProperty, fadeAnim);

            await Task.Delay(3000);

            var glitch = new ChunkGlitchEffect() { Intensity = 0, Seed = 0 };
            titleGrid.AddEffects(glitch);

            await Task.Delay(500);

            glitch.Intensity = 0.7;
            glitch.Seed = MathHelper.Random(1, 1000);

            titleVignette.Intensity = 0.6;
            await Task.Delay(100);

            glitch.Intensity = 0;

            if (parent.WindowState != WindowState.Maximized) parent.Maximize();
            parent.viewportButtons.Visibility = Visibility.Collapsed;

            titleVignette.Intensity = 1.5;
            titleGrid.Cursor = Cursors.Help;

            var film = new MonitorFilmEffect() { Intensity = 0.75, Viewport = parent.viewport };
            parent.ui_layer.Effect = film;

            while (Mouse.LeftButton != MouseButtonState.Pressed)
            {

            }
        }
    }
}
