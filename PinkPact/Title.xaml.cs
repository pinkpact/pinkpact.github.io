using PinkPact.Externals;
using PinkPact.Helpers;
using PinkPact.Shaders;
using PinkPact.Controls.Specific;
using PinkPact.Controls;
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
using System.Diagnostics;

using static PinkPact.Properties.Settings;
using System.Threading;
using PinkPact.Playback;

namespace PinkPact
{
    /// <summary>
    /// Interaction logic for Title.xaml
    /// </summary>
    public partial class Title : UserControl
    {
        internal const int fadeDuration = 2000,
                           shineDuration = 3250;

        int glitchMaxTime = 1500;
        int selectedMenuOption = 1;
        bool lastUp = false;

        StackPanel selected;

        readonly BitmapImage arrow = new BitmapImage(new Uri("pack://application:,,,/PinkPact;component/Resources/Title/title_arrow.png", UriKind.RelativeOrAbsolute)),
                             selectedArrow = new BitmapImage(new Uri("pack://application:,,,/PinkPact;component/Resources/Title/title_arrow_selected.png", UriKind.RelativeOrAbsolute));

        readonly MainWindow parent;

        public Title(MainWindow parent)
        {
            this.parent = parent;
            InitializeComponent();

            newGameOption.Tag = (Action)(async () =>
            {
                HotkeyManager.Disable("title");
                Audio.PlayResource("Resources/Sounds/Extra/newgame.mp3", channel: "sfx").Dispose();

                var a = new DayOne(parent);
                parent.ui_layer.IsHitTestVisible = false;

                await OpacityProperty.Animate<DoubleAnimation, double>(this, Opacity, 0, 1000, 12, easing: new SineEase() { EasingMode = EasingMode.EaseOut });

                await parent.game_layer.Children.Add(a);
                await a.Day1();
            });
            continueOption.Tag = (Action)(() => MessageBox.Show("continue"));
            settingsOption.Tag = (Action)(() => MessageBox.Show("no settings yet, sorry"));
            //Loaded += (_, __) => _ = Show();
        }

        public async Task ShowLogo()
        {
            parent.ui_layer.Effect = new MonitorFilmEffect() 
            { 
                Intensity = 0.3, 
                StretchFactors = new Point(0.5, 0.5), 
                Viewport = viewport 
            };

            // Pop in logo

            logo.Visibility = Visibility.Visible;
            logo.Opacity = 0;

            Audio.PlayResource("Resources/Sounds/Extra/intro_sound.mp3", channel: "sfx").Dispose();

            var token = new CancellationTokenSource();
            HotkeyManager.Add("logo", () => { token.Cancel(); HotkeyManager.Remove("logo"); }, Key.Enter);
            try
            {
                _ = TranslateTransform.YProperty.Animate<DoubleAnimation, double>(logoTranslation, 20, 0, fadeDuration, 5, easing: new QuinticEase() { EasingMode = EasingMode.EaseOut });
                _ = OpacityProperty.Animate<DoubleAnimation, double>(logo, 0, 1, fadeDuration, 5, easing: new SineEase());
                _ = MonitorFilmEffect.IntensityProperty.Animate<DoubleAnimation, double>(parent.ui_layer.Effect, 0.3, 0, fadeDuration * 3, easing: new ElasticEase() { EasingMode = EasingMode.EaseOut });
                _ = MonitorFilmEffect.StretchFactorsProperty.Animate<PointAnimation, Point>(new Point(0.5, 0.5), new Point(1, 1), fadeDuration * 3, easing: new ElasticEase() { EasingMode = EasingMode.EaseOut });

                await Task.Delay(fadeDuration, token.Token);

                // Shine
                await TranslateTransform.XProperty.Animate<DoubleAnimation, double>(shineTranslation, -250, 1050, shineDuration, 10, easing: new QuinticEase() { EasingMode = EasingMode.EaseInOut }, cancelToken: token.Token);

                // Fade out
                await OpacityProperty.Animate<DoubleAnimation, double>(logo, logo.Opacity, 0, fadeDuration, 5, easing: new SineEase());
            }
            catch // Force fade out
            {
                await OpacityProperty.Animate<DoubleAnimation, double>(logo, logo.Opacity, 0, fadeDuration, 5, easing: new SineEase());
            }

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
            // Make the title grid visible

            titleGrid.Children.Clear();
            await titleGrid.Children.Add(titleBg);
            await titleGrid.Children.Add(actualTitleBg);
            titleGrid.Visibility = Visibility.Visible;

            // Animate it into appearance

            titleBg.RenderTransform = new TranslateTransform();
            _ = TranslateTransform.YProperty.Animate<DoubleAnimation, double>(titleBg.RenderTransform, 15, 0, 3000, 20, easing: new SineEase());
            await OpacityProperty.Animate<DoubleAnimation, double>(titleGrid, 0, 0.9, 3000, 5, false, easing: new SineEase());
            await 1000;

            // Show notification window

            var notif = ResourceHelper.GetImageResource("Resources/Others/notification.png");

            notif.Width = 500;
            notif.Opacity = 0;
            notif.VerticalAlignment = VerticalAlignment.Bottom;
            notif.HorizontalAlignment = HorizontalAlignment.Left;
            notif.Margin = new Thickness(40, 0, 0, 40);
            notif.RenderTransform = new TranslateTransform();
            notif.IsHitTestVisible = false;

            _ = titleGrid.Children.Add(notif);
            _ = TranslateTransform.YProperty.Animate<DoubleAnimation, double>(notif.RenderTransform, 20, 0, 250, easing: new SineEase() { EasingMode = EasingMode.EaseOut });
            await OpacityProperty.Animate<DoubleAnimation, double>(notif, 0, 1, 250, easing: new SineEase() { EasingMode = EasingMode.EaseOut });

            // Wait for the grid to be clicked

            Audio.PlayResource("Resources/Sounds/Extra/notification.mp3").Dispose();

            parent.ui_layer.IsHitTestVisible = false;
            Cursor = Cursors.Hand;
            await this.AwaitClick();
            parent.ui_layer.IsHitTestVisible = true;

            // Begin glitching the title grid

            bool doGlitching = true;
            var glitch = new ChunkGlitchEffect() { Intensity = 0, Seed = 0 };
            titleGrid.AddEffects(glitch);

            await 500;

            // Set the glitch & vignette

            glitch.Intensity = 0.7;
            glitch.Seed = MathHelper.Random(1, 1000);

            titleVignette.Intensity = 0.6;
            await 1000;

            Audio.PlayResource("Resources/Sounds/Extra/glitch.mp3").Dispose();

            // Weaken the glitch and begin shaking and constant glitching

            glitch.Intensity = 0.15;
            ((Action)(async () =>
            {
                while (doGlitching)
                {
                    glitch.Seed = MathHelper.Random(1, 1000);
                    await (int)MathHelper.Random(100, glitchMaxTime);
                }
            }
            ))();

            _ = titleGrid.ShakeWhile(1.5, 150, () => doGlitching);

            // Maximize and disable maximization controls

            if (parent.WindowState != WindowState.Maximized) parent.Maximize();
            parent.viewportButtons.Visibility = Visibility.Collapsed;
            HotkeyManager.Disable("maximize");
            Cursor = Cursors.Arrow;

            // Make the vignette more intense & add the film effect

            titleVignette.Intensity = 1.5;

            var film = new MonitorFilmEffect() { Intensity = 0.75, Viewport = parent.viewport };
            parent.game_layer.Effect = film;

            // Animate the film effect
            // Make the pixels more apparent
            await MonitorFilmEffect.StretchFactorsProperty.Animate<PointAnimation, Point>(film, new Point(1, 1), new Point(0.5, 0.5), duration: 250, framerate: 60, easing: new QuinticEase() { EasingMode = EasingMode.EaseOut });
            await 500;

            // Soften the effect
            _ = MonitorFilmEffect.IntensityProperty.Animate<DoubleAnimation, double>(film, 0.75, 0.25, duration: 500, reverse: true, easing: new QuinticEase() { EasingMode = EasingMode.EaseOut });
            await MonitorFilmEffect.StretchFactorsProperty.Animate<PointAnimation, Point>(film, new Point(0.5, 0.5), new Point(1, 1), duration: 500, framerate: 60, easing: new SineEase() { EasingMode = EasingMode.EaseInOut });

            // Bring it back
            await MonitorFilmEffect.StretchFactorsProperty.Animate<PointAnimation, Point>(film, new Point(1, 1), new Point(0.5, 0.5), duration: 500, framerate: 60, easing: new CubicEase() { EasingMode = EasingMode.EaseOut });

            //Show error messages
            await 250;

            // Show the error window

            var wnd = new PinkWindow() { Title = "ERROR", WindowContent = new PinkErrorWindow(), Tag = "errwnd" };
            wnd.ExitButton.IsEnabled = false;
            Audio.PlayResource("Resources/Sounds/Extra/wnd_appear.mp3", channel: "sfx").Dispose();

            _ = parent.ui_layer.Children.Add(wnd);
            _ = wnd.Shake(5, 200);

            // Wait for it to be clicked

            bool clicked = false;
            (wnd.WindowContent as PinkErrorWindow).Button.Click += (_, __) => clicked = true;
            while (!clicked) await 5;

            // Make it disappear

            _ = wnd.Shake(12, 250);
            await 100;
            parent.ui_layer.Children.Remove(wnd);

            // For the next 10 seconds, fill the screen with error windows
            double dtime = 250;
            int totalMs = 0,
                time = 1000;

            var watch = new Stopwatch();
            var windows = new List<PinkWindow>();
            for (int i = 0; watch.ElapsedMilliseconds < 10000; i++)
            {
                // Make an error window appear

                wnd = new PinkWindow()
                {
                    Title = (int)MathHelper.Random(1, 100) % 2 == 0 ? "ERROR" : "WARNING",
                    VerticalAlignment = VerticalAlignment.Top,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    WindowContent = new PinkErrorWindow(),
                    RenderTransformOrigin = new Point(0.5, 0.5),
                    RenderTransform = new TranslateTransform(MathHelper.Random(i <= 1 ? 0 : -wnd.Width / 2, parent.game_layer.Width - wnd.Width / (i <= 1 ? 1 : 2)),
                                                             MathHelper.Random(i <= 1 ? 0 : -wnd.Height / 2, parent.game_layer.Height - wnd.Height / (i <= 1 ? 1 : 2))),
                };

                wnd.ExitButton.IsEnabled = false;

                Audio.PlayResource("Resources/Sounds/Extra/wnd_appear.mp3", channel: "sfx").Dispose();
                _ = parent.ui_layer.Children.Add(wnd);
                _ = wnd.Shake(5, 200);

                // The first 2 windows can be clicked

                if (i <= 1)
                {
                    // Wait for them to be clicked

                    clicked = false;
                    (wnd.WindowContent as PinkErrorWindow).Button.Click += (_, __) => clicked = true;
                    while (!clicked) await 5;
                    _ = wnd.Shake(12, 250);
                }
                else if (!watch.IsRunning) watch.Restart();

                // Disable clicking them

                (wnd.WindowContent as PinkErrorWindow).Button.IsEnabled = false;

                // Dont await if first 2 windows

                if (i <= 1)
                {
                    await 250;
                    continue;
                }

                if (i == 2) Audio.PlayResource("Resources/Sounds/Extra/tension.mp3").Dispose();


                // Await and reduce waiting time and increase effects

                await time;

                totalMs += time;
                time -= (int)dtime;
                time = MathHelper.Clamp(time, 100);
                dtime /= 1.25;

                glitchMaxTime -= (int)dtime;
                glitch.Intensity += 0.01;
                titleVignette.Intensity += 0.05;

                _ = titleGrid.Shake(3, (int)dtime);
            }
            watch.Stop();

            // Remove windows

            doGlitching = false;
            for (int i = 0; i < parent.ui_layer.Children.Count; i++)
                if (parent.ui_layer.Children[i] is PinkWindow)
                    parent.ui_layer.Children.RemoveAt(i--);

            // Stop effects

            parent.game_layer.Effect = null;
            parent.game_layer.RemoveEffectWhere(_ => true);

            parent.ui_layer.Effect = null;
            parent.ui_layer.RemoveEffectWhere(_ => true);

            titleGrid.Visibility = Visibility.Collapsed;

            // Remove everything but the title

            await 2000;
            titleGrid.Children.Remove(titleBg);
            titleGrid.Children.Remove(notif);

            actualTitleBg.Visibility = Visibility.Visible;

            // Glitch the title screen for a second

            titleVignette.Intensity = 3;
            glitch.Intensity = 0.5;

            titleGrid.Visibility = Visibility.Visible;

            // Restore it

            await 150;
            glitch.Intensity = 0;
            titleVignette.Intensity = 0.2;

            parent.viewportButtons.Visibility = Visibility.Visible;
            HotkeyManager.Enable("maximize");

            await ShowTitle(false);
        }

        public async Task ShowTitle(bool fadeIn)
        {
            var logoAnim = new DoubleAnimation(5, -20, TimeSpan.FromSeconds(5))
            {
                EasingFunction = new SineEase() { EasingMode = EasingMode.EaseInOut },
                RepeatBehavior = RepeatBehavior.Forever,
                AutoReverse = true
            };

            Timeline.SetDesiredFrameRate(logoAnim, 30);
            titleLogoImage.RenderTransform.BeginAnimation(TranslateTransform.YProperty, logoAnim);

            var heartsieAnim = new DoubleAnimation(-5, 5, TimeSpan.FromSeconds(3))
            {
                EasingFunction = new SineEase() { EasingMode = EasingMode.EaseInOut },
                RepeatBehavior = RepeatBehavior.Forever,
                AutoReverse = true
            };

            Timeline.SetDesiredFrameRate(heartsieAnim, 10);
            titleHeart1.RenderTransform.BeginAnimation(TranslateTransform.YProperty, heartsieAnim);

            heartsieAnim.From = 15;
            heartsieAnim.To = -5;
            heartsieAnim.BeginTime = TimeSpan.FromSeconds(1);
            titleHeart2.RenderTransform.BeginAnimation(TranslateTransform.YProperty, heartsieAnim);

            titleGrid.Children.Remove(titleBg);
            titleGrid.Visibility = Visibility.Visible;
            actualTitleGrid.Visibility = Visibility.Visible;
            actualTitleBg.Visibility = Visibility.Visible;

            foreach (StackPanel opt in optionGrid.Children) (opt.Children[1] as Image).Source = new BitmapImage(new Uri("pack://application:,,,/PinkPact;component/Resources/Title/title_arrow.png", UriKind.RelativeOrAbsolute));

            ToggleMenuOption(1);
            _ = SelectMenuOption(0, false);

            if (fadeIn)
            {
                foreach (StackPanel opt in optionGrid.Children) opt.Opacity = 0;

                _ = OpacityProperty.Animate<DoubleAnimation, double>(titleGrid, 0, 1, 2000, 5, easing: new SineEase());
                _ = OpacityProperty.Animate<DoubleAnimation, double>(actualTitleGrid, 0, 1, 2000, 5, easing: new SineEase());

                await 1000;
                foreach (StackPanel opt in optionGrid.Children)
                {
                    _ = OpacityProperty.Animate<DoubleAnimation, double>(opt, 0, opt.IsEnabled ? 1 : 0.5, 500, 24, easing: new SineEase() { EasingMode = EasingMode.EaseOut });
                    _ = TranslateTransform.XProperty.Animate<DoubleAnimation, double>((opt.RenderTransform as TransformGroup).Children[0], -50, 0, 500, 24, easing: new SineEase() { EasingMode = EasingMode.EaseOut });

                    await 150;
                }
            }

            Func<Key[], bool> pred = _ => actualTitleGrid.Visibility == Visibility.Visible;

            HotkeyManager.Add("title", MenuUp, pred, Key.Up);
            HotkeyManager.Add("title", MenuUp, pred, Key.W);

            HotkeyManager.Add("title", MenuDown, pred, Key.Down);
            HotkeyManager.Add("title", MenuDown, pred, Key.S);

            HotkeyManager.Add("title", () => (selected.Tag as Action)(), Key.Enter);
        }

        public int SelectMenuOption(int index, bool up)
        {
            if (!optionGrid.Children.Cast<StackPanel>().Any(x => x.IsEnabled)) return index;

            if (index < 0) index = optionGrid.Children.Count - 1;
            else if (index >= optionGrid.Children.Count) index = 0;

            while (!optionGrid.Children[index].IsEnabled)
            {
                index += up ? -1 : 1;
                if (index < 0) index = optionGrid.Children.Count - 1;
                else if (index >= optionGrid.Children.Count) index = 0;
            }

            var opt = selected = optionGrid.Children[index] as StackPanel;
            var group = opt.RenderTransform as TransformGroup;
            var pos = group.Children[0] as TranslateTransform;
            var scale = group.Children[1] as ScaleTransform;

            scale.ScaleX = scale.ScaleY = 1.1;
            pos.X = 30;

            opt.Children[0].Visibility = Visibility.Visible;
            (opt.Children[1] as Image).Source = selectedArrow;

            for (int i = 0; i < optionGrid.Children.Count; i++)
            {
                if (i == index) continue;

                opt = optionGrid.Children[i] as StackPanel;
                group = opt.RenderTransform as TransformGroup;
                pos = group.Children[0] as TranslateTransform;
                scale = group.Children[1] as ScaleTransform;

                scale.ScaleX = scale.ScaleY = 0.9;
                pos.X = 0;

                opt.Children[0].Visibility = Visibility.Hidden;
                (opt.Children[1] as Image).Source = arrow;
            }

            return index;
        }

        public void ToggleMenuOption(int index)
        {
            if (index < 0 || index >= optionGrid.Children.Count) return;

            var opt = optionGrid.Children[index] as StackPanel;
            opt.IsEnabled = !opt.IsEnabled;
            opt.Opacity = opt.IsEnabled ? 1 : 0.5;
        }

        void MenuUp()
        {
            Audio.PlayResource("Resources/Sounds/Extra/click.mp3", channel: "sfx").Dispose();
            for (int i = 0; i < (lastUp ? 1 : 3); i++) selectedMenuOption = SelectMenuOption(selectedMenuOption, true) - 1;
            lastUp = true;
        }

        void MenuDown()
        {
            Audio.PlayResource("Resources/Sounds/Extra/click.mp3", channel: "sfx").Dispose();
            for (int i = 0; i < (lastUp ? 3 : 1); i++) selectedMenuOption = SelectMenuOption(selectedMenuOption, false) + 1;
            lastUp = false;
        }
    }
}