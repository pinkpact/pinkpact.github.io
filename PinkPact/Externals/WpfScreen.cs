using System.Collections.Generic;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows;
using System.Drawing;
using System;

using PinkPact.Helpers;

namespace PinkPact.Externals
{
    public class WpfScreen
    {
        public bool IsPrimary
        {
            get => screen.Primary;
        }

        public string DeviceName
        {
            get => screen.DeviceName;
        }

        public Rect DeviceBounds
        {
            get => GetRect(screen.Bounds);
        }

        public Rect WorkingArea
        {
            get => GetRect(screen.WorkingArea);
        }

        public double ScalingFactor { get; }

        public static WpfScreen Primary
        {
            get => new WpfScreen(Screen.PrimaryScreen, 0);
        }

        private readonly Screen screen;

        private WpfScreen(Screen screen, double scaling_factor)
        {
            this.screen = screen;
            ScalingFactor = scaling_factor;
        }

        private Rect GetRect(Rectangle value)
        {
            return new Rect
            {
                X = value.X,
                Y = value.Y,
                Width = value.Width,
                Height = value.Height
            };
        }

        public static IEnumerable<WpfScreen> AllScreens()
        {
            foreach (Screen screen in Screen.AllScreens) yield return new WpfScreen(screen, 0);
        }

        public static WpfScreen GetScreenFrom(Window window)
        {
            var windowInteropHelper = new WindowInteropHelper(window);
            return new WpfScreen(Screen.FromHandle(windowInteropHelper.Handle), WindowHelper.GetScalingFactorOfMonitor(window));
        }

        public static WpfScreen GetScreenFrom(System.Windows.Point point)
        {
            int x = (int)Math.Round(point.X);
            int y = (int)Math.Round(point.Y);

            var drawingPoint = new System.Drawing.Point(x, y);
            var screen = Screen.FromPoint(drawingPoint);

            return new WpfScreen(screen, 0);
        }
    }
}