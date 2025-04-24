using System.Windows.Media.Animation;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Threading.Tasks;
using System.Windows.Shapes;
using System.Windows.Input;
using System.Windows.Media;
using PinkPact.Animations;
using System.Windows;
using System.Linq;
using System;

using PinkPact.Externals;
using PinkPact.Shaders;
using PinkPact.Helpers;


using static System.Math;

using static PinkPact.Helpers.MathHelper;
using static PinkPact.Helpers.KeyboardHelper;


namespace PinkPact
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        readonly SolidColorBrush windowBgColor = new BrushConverter().ConvertFrom("#fef8f6") as SolidColorBrush;
        readonly List<HotkeyActionChecker> hotkeys = new List<HotkeyActionChecker>();

        WpfScreen currentScreen;
        double lastWindowScale;

        void Update()
        {
            UpdateWindowScale();

            // Hotkey checks

            if (IsActive) foreach (var hotkey in hotkeys) hotkey.Check();
        }

        public MainWindow()
        {
            InitializeComponent();

            RenderOptions.SetBitmapScalingMode(mainBorder, BitmapScalingMode.HighQuality);
            RenderOptions.SetBitmapScalingMode(viewport, BitmapScalingMode.HighQuality);

            // Add hotkeys

            hotkeys.Add(new HotkeyActionChecker(() => ToggleViewportBoxes(), Key.LeftCtrl, Key.D, Key.B));
            hotkeys.Add(new HotkeyActionChecker(() => Maximize(WindowState == WindowState.Maximized), Key.F4));

            // Set up screen & viewport data too

            Update();

            Action minAction = () => WindowState = WindowState.Minimized,
                   maxAction = () => Maximize(WindowState == WindowState.Maximized),
                   exitAction = () => Exit();

            dragBar.MouseDown += (_, __) => DragMove();

            minBtn.Click += (_, __) => minAction();
            minBtnVP.Click += (_, __) => minAction();

            maxBtn.Click += (_, __) => maxAction();
            maxBtnVP.Click += (_, __) => maxAction();

            exitBtn.Click += (_, __) => exitAction();
            exitBtnVP.Click += (_, __) => exitAction();

            CompositionTarget.Rendering += (_, __) => Update();

            format.LinkHandlerAdded += (_, a) =>
            {
                a.Handler.Click = (h) => MessageBox.Show("hi");
            };

            // DEMO SHOWCASE

            //format.PreviewCharacterWritten += (s, e) => Console.WriteLine(e.Character + " " + e.TotalIndex + " " + e.GroupIndex);
            //format.Append(" (i,lb)[newline]. (p,ff`SD Auto Pilot`,s3,f3)[crazy effects] (p,w4,s2,c#f542ef-#983dff)[peepee] (w3)[poopoo]?");

            //var decotrator = game_layer.AddEffects(new Chromatic3DEffect() { Intensity = 5 },
            //                                       new Vignette2Effect());

            ((Action)(async () =>
            {
                while (!Keyboard.IsKeyDown(Key.K)) await Task.Delay(1);

                await Task.Delay(2000);
                await format.SetAsync("this is scrolling text");

                await Task.Delay(2000);
                await format.SetAsync("this is (p)[pinned] text");

                await Task.Delay(2000);
                await format.SetAsync("this is (b)[bold] text");

                await Task.Delay(2000);
                await format.SetAsync("this is (i)[italic] text");

                await Task.Delay(2000);
                await format.SetAsync("(s4)[this is shaky text] with (s8)[variable] intensity");

                await Task.Delay(2000);
                await format.SetAsync("(w5)[this is wavy text] with (w15)[variable] intensity");

                await Task.Delay(3000);
                await format.SetAsync("this is (c#eb34de)[single colored] text");

                await Task.Delay(2000);
                await format.SetAsync("this is (c#eb34de-#ff99f8)[multiple colored] text");

                await Task.Delay(3000);
                await format.SetAsync("this is (f1.7)[scaled] text");

                await Task.Delay(2000);
                await format.SetAsync("this is (ff`SD Auto Pilot`)[custom fonted] text");

                await Task.Delay(2000);
                await format.SetAsync("this is (fd300)[slowwwwww] text and (fd2)[fastttttttttt] text");

                await Task.Delay(2000);
                await format.SetAsync("and (w4)[all of these can be] (w5,f1.5,c#eb34de-#ff99f8)[combined!!]");

                await Task.Delay(3000);
                await format.SetAsync("");
                viewport.Effect = new Chromatic3DEffect() { Intensity = 1 };
                await format.SetAsync("this is a cool (w4)[3D] chroma effect");

                var anim = new DoubleAnimation(1, 10, TimeSpan.FromSeconds(3)) { EasingFunction = new SineEase() { EasingMode = EasingMode.EaseInOut } };
                viewport.Effect.BeginAnimation(Chromatic3DEffect.IntensityProperty, anim);

                await Task.Delay(5000);
                await format.SetAsync("");
                viewport.Effect = new PixelateEffect() { Intensity = 1 };
                await format.SetAsync("this is a pixelation effect");

                anim = new DoubleAnimation(1, 5, TimeSpan.FromSeconds(3)) { EasingFunction = new SineEase() { EasingMode = EasingMode.EaseInOut } };
                viewport.Effect.BeginAnimation(PixelateEffect.IntensityProperty, anim);

                await Task.Delay(4000);
                await format.SetAsync("");
                viewport.Effect = null;

                while (!format.UpToDate) await format.ForceShow();
                Console.WriteLine("done");
            }
            ))();
        }

        public void Maximize(bool unmaximize = false)
        {
            // Formality set

            WindowState = unmaximize ? WindowState.Normal : WindowState.Maximized;
            //Topmost = !unmaximize;

            // Window children management

            mainGrid.Children.Clear();

            if (unmaximize)
            {
                mainGrid.Children.Add(bgDecorator);
                mainGrid.Children.Add(windowBar);
            }

            mainGrid.Children.Add(viewport);
            viewportButtons.Visibility = unmaximize ? Visibility.Collapsed : Visibility.Visible;

            // Set the grid size to fill the screen

            mainGrid.Width = unmaximize ? 950 : currentScreen.DeviceBounds.Width;
            mainGrid.Height = unmaximize ? 567 : currentScreen.DeviceBounds.Height;
            mainGrid.Background = unmaximize ? windowBgColor : Brushes.Black;

            mainBorder.BorderThickness = new Thickness(unmaximize ? 4 : 0);

            // Scaling

            viewport.Margin = new Thickness(unmaximize ? -3 : 0, unmaximize ? 35 : 0, 0, unmaximize ? 8 : 0);

            var scale = Min(currentScreen.DeviceBounds.Width / 1920, currentScreen.DeviceBounds.Height / 1080);

            viewportScale.ScaleX = viewportScale.ScaleY = unmaximize ? 0.487 : scale;
            windowScale.ScaleX = windowScale.ScaleY = unmaximize ? lastWindowScale : (1d / currentScreen.ScalingFactor);

            GC.Collect();
        }

        public void UpdateWindowScale()
        {
            var screen = WpfScreen.GetScreenFrom(this);

            // If the screen size hasn't changed, don't waste time with scaling

            if (screen.ScalingFactor == currentScreen?.ScalingFactor &&
                screen.DeviceBounds.Width == currentScreen?.DeviceBounds.Width &&
                screen.DeviceBounds.Height == currentScreen?.DeviceBounds.Height) return;

            // The window is not maximized here, as it couldn't have moved if it was

            currentScreen = screen;

            double w = currentScreen.DeviceBounds.Width - (currentScreen.DeviceBounds.Width < 950 ? 100 : 0),
                   h = currentScreen.DeviceBounds.Height - (currentScreen.DeviceBounds.Height < 568 ? 56.25 : 0);

            double scale = Min(w / 950, h / 568);

            windowScale.ScaleX = windowScale.ScaleY = lastWindowScale = Clamp(scale, 1, true) * (1d / currentScreen.ScalingFactor);

            // Update the maximized window

            if (WindowState == WindowState.Maximized)
            {
                Maximize(true);
                Maximize();
            }
        }

        public void ToggleViewportBoxes()
        {
            // Add the boxes

            if (viewport.Children.Count == 0 || (viewport.Children[viewport.Children.Count - 1] as Rectangle)?.Tag as string != "dbgbox")
            {
                for (int i = 0; i < 4; i++)
                {
                    viewport.Children.Insert(Clamp(viewport.Children.Count, 0), new Rectangle()
                    {
                        Width = 70,
                        Height = 70,
                        Tag = "dbgbox",
                        Stroke = Brushes.Red,
                        StrokeThickness = 10,
                        VerticalAlignment = i <= 1 ? VerticalAlignment.Top : VerticalAlignment.Bottom,
                        HorizontalAlignment = (i & 1) == 0 ? HorizontalAlignment.Left : HorizontalAlignment.Right,

                        // yes, i did this ------^ just to feel smart
                    });
                }

                return;
            }

            // Remove them

            if (viewport.Children.Count >= 4 && (viewport.Children[viewport.Children.Count - 1] as Rectangle)?.Tag as string == "dbgbox") viewport.Children.RemoveRange(viewport.Children.Count - 4, 4);
        }

        public void Exit()
        {
            Close();
            Environment.Exit(0);
        }
    }
}
