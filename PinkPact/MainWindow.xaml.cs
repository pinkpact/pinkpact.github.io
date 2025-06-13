using PinkPact.Animations;
using PinkPact.Controls.Specific;
using PinkPact.Externals;
using PinkPact.Helpers;
using PinkPact.Playback;
using PinkPact.Shaders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using static PinkPact.Helpers.KeyboardHelper;
using static PinkPact.Helpers.MathHelper;
using static PinkPact.Properties.Settings;
using static System.Math;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

namespace PinkPact
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        readonly SolidColorBrush windowBgColor = new BrushConverter().ConvertFrom("#fef8f6") as SolidColorBrush;

        WpfScreen currentScreen;
        double lastWindowScale;

        void Update()
        {
            UpdateWindowScale();
        }

        public MainWindow()
        {
            AudioChannel.RegisterChannel("sfx");
            AudioChannel.RegisterChannel("music");

            InitializeComponent();

            RenderOptions.SetBitmapScalingMode(mainBorder, BitmapScalingMode.HighQuality);
            RenderOptions.SetBitmapScalingMode(viewport, BitmapScalingMode.HighQuality);

            // Add hotkeys

            HotkeyManager.Add("dbgbox", () => ToggleViewportBoxes(), Key.LeftCtrl, Key.D, Key.B);
            HotkeyManager.Add("maximize", () => Maximize(WindowState == WindowState.Maximized), Key.F4);
            //HotkeyManager.Add("trailing", () => format.ToggleTrailing(TimeSpan.FromMilliseconds(10), TimeSpan.FromMilliseconds(1000)), Key.P);
            //HotkeyManager.Add(() => format.Margin = new Thickness(format.Margin.Left, format.Margin.Top + 100, format.Margin.Right, format.Margin.Bottom), Key.Up);
            //HotkeyManager.Add(() => format.Margin = new Thickness(format.Margin.Left, format.Margin.Top - 100, format.Margin.Right, format.Margin.Bottom), Key.Down);

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

            if (Default.fullscreen) Maximize();
            ((Action)(async () =>
            {
                var title = new Title(this);
                await game_layer.Children.Add(title);

                await 250;
                await title.ShowLogo();
                if (Default.firstLaunch)
                {
                    await title.FirstSequence();
                    Default.firstLaunch = false;
                    Default.Save();
                }
                else await title.ShowTitle(true);
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

            Default.fullscreen = WindowState == WindowState.Maximized;
            Default.Save();
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
