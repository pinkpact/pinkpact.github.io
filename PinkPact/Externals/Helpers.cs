using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows.Media.Animation;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Windows.Media.Effects;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Threading.Tasks;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Input;
using System.Globalization;
using System.Net.Sockets;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Text;
using System.Data;
using System.Linq;
using System.Net;
using System.IO;
using System;

using static PinkPact.PInvoke;

using PinkPact.Shaders;
using PinkPact.Controls;
using PinkPact.Animations;

namespace PinkPact.Helpers
{
    /// <summary>
    /// Provides static methods useful for UI interactions.
    /// </summary>
    public static class UiHelper
    { 
        /// <summary>
        /// Gets the actual size of <paramref name="element"/> as rendered on-screen.
        /// </summary>
        public static Size ActualSize(this FrameworkElement element)
        {
            var parent = element.Parent as UIElement;

            Point bottomLeft = element.TranslatePoint(new Point(0, 0), parent);
            Point topRight = element.TranslatePoint(new Point(element.ActualWidth, element.ActualHeight), parent);

            return new Size(topRight.X - bottomLeft.X, topRight.Y - bottomLeft.Y);
        }

        /// <summary>
        /// Adds an <see cref="Effect"/> to <paramref name="element"/>, creating a new <see cref="EffectDecorator"/> stack, or adding to an existing one.
        /// <para>Note: this method replaces <paramref name="element"/> as its parent's child with an <see cref="EffectDecorator"/>.</para>
        /// </summary>
        /// <returns>The <see cref="EffectDecorator"/> which replaced <paramref name="element"/>.</returns>
        /// <exception cref="ArgumentException"></exception>
        public static EffectDecorator AddEffects(this FrameworkElement element, params Effect[] effects)
        {
            if (effects.Length == 0) throw new ArgumentException("At least 1 effect must be specified.");

            var parent = VisualTreeHelper.GetParent(element) as FrameworkElement;

            int child_index = 0;
            DependencyObject child;

            while ((child = VisualTreeHelper.GetChild(parent, child_index)) != element &&
                   !(child is EffectDecorator) &&
                   !((child as EffectDecorator)?.Affected != element))
                
                   if (VisualTreeHelper.GetChildrenCount(parent) <= ++child_index) return null;

            parent.RemoveChild(element);

            var decorator = new EffectDecorator(child is EffectDecorator ? child as EffectDecorator : element) { Effect = effects[0] };
            for (int i = 1; i < effects.Length; i++) decorator = new EffectDecorator(decorator) { Effect = effects[i] };

            parent.InsertChildAt(child_index, decorator);

            return decorator;
        }

        /// <summary>
        /// Inserts <paramref name="child"/> at index <paramref name="index"/> in <paramref name="parent"/>'s child tree.
        /// </summary>
        /// <returns>The <paramref name="child"/> element.</returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static FrameworkElement InsertChildAt(this FrameworkElement parent, int index, FrameworkElement child)
        {
            if (parent is Panel panel)  panel.Children.Insert(index, child);
            else if (parent is ContentControl contentControl) contentControl.Content = child;
            else if (parent is Decorator decorator) decorator.Child = child;
            else throw new InvalidOperationException("Unsupported parent type");

            return child;
        }

        /// <summary>
        /// Removes all children of <paramref name="parent"/> for which <paramref name="predicate"/>(child) returns <see langword="true"/>.
        /// </summary>
        public static void RemoveWhere(this FrameworkElement parent, Func<FrameworkElement, bool> predicate)
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is FrameworkElement c && predicate(c))
                {
                    int count = VisualTreeHelper.GetChildrenCount(parent);
                    parent.RemoveChild(c);

                    if (VisualTreeHelper.GetChildrenCount(parent) < count) i--;
                }
            }
        }

        /// <summary>
        /// Removes the first child of <paramref name="parent"/> for which <paramref name="predicate"/>(child) returns <see langword="true"/>.
        /// </summary>
        public static void RemoveFirst(this FrameworkElement parent, Func<FrameworkElement, bool> predicate)
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is FrameworkElement c && predicate(c))
                {
                    int count = VisualTreeHelper.GetChildrenCount(parent);
                    parent.RemoveChild(c);

                    if (VisualTreeHelper.GetChildrenCount(parent) < count) return;
                }
            }
        }

        /// <summary>
        /// Removes <paramref name="child"/> from <paramref name="parent"/> child tree.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public static void RemoveChild(this FrameworkElement parent, FrameworkElement child)
        {
            if (parent is Panel panel) panel.Children.Remove(child);
            else if (parent is ContentControl contentControl && contentControl.Content == child) contentControl.Content = null;
            else if (parent is Decorator decorator && decorator.Child == child) decorator.Child = null;
            else throw new InvalidOperationException("Unsupported parent type");
        }

        /// <summary>
        /// Gets the location of <paramref name="element"/> relative to its parent.
        /// </summary>
        public static Point RelativeLocation(this FrameworkElement element)
        {
            var container = VisualTreeHelper.GetParent(element) as UIElement;
            return element.TranslatePoint(new Point(0, 0), container);
        }

        /// <summary>
        /// Fades <paramref name="element"/> out over a period of time, then removes it as a child from its parent.
        /// </summary>
        public static async Task FadeOut(this FrameworkElement element, TimeSpan duration, EasingFunctionBase easingFunction = null)
        {
            var anim = new DoubleAnimation(element.Opacity, 0, duration) { EasingFunction = easingFunction };
            element.BeginAnimation(FrameworkElement.OpacityProperty, anim);

            await Task.Delay(duration);
            (VisualTreeHelper.GetParent(element) as FrameworkElement).RemoveChild(element);
        }

        /// <summary>
        /// Draws the visual represented by <paramref name="element"/> on a <see cref="DrawingVisualHost"/> at the location <paramref name="location"/>.
        /// </summary>
        public static DrawingContext DrawVisual(this DrawingVisualHost drawing, FrameworkElement element, Point location)
        {
            var brush = new VisualBrush(element);
            var dc = drawing.Visual.RenderOpen();

            dc.DrawRectangle(brush, null, new Rect(location.X, location.Y, element.ActualWidth, element.ActualHeight));

            return dc;
        }

        /// <summary>
        /// Toggles a trail for <paramref name="element"/>, meaning every time <paramref name="element"/> updates its position, it will leave a trail behind it.
        /// <para> To toggle the trail on/off, call this method on either the original <see cref="FrameworkElement"/>, or a previously returned <see cref="TrailGroup"/>.<br/>When toggled on, <paramref name="element"/> will be replaced as the child of its parent with a <see cref="TrailGroup"/>.<br/>Inversely, when toggled off, the parent will regain the original <see cref="FrameworkElement"/> as its child at its orignal index.</para>
        /// <para>Note: calling this method on a <see cref="TrailGroup"/> will always toggle the trail off.</para>
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public static TrailGroup ToggleTrailing(this FrameworkElement element, TimeSpan cooldown = default, TimeSpan fadeDuration = default)
        {
            // Set up the position checker

            TrailGroup group = null;
            void positionChecker(object sender, EventArgs e)
            {
                // Check whether the position has changed (and the cooldown is over)

                if (group.timer.Elapsed < group.TrailCooldown ||
                    element.RelativeLocation() == group.lastPosition)
                    
                    return;

                // Setup the new visual drawing

                var host = new DrawingVisualHost(new DrawingVisual()) 
                {
                    Width = element.Width,
                    Height = element.Height,
                    Margin = element.Margin,
                    Opacity = element.Opacity / 2,
                    RenderTransform = element.RenderTransform,
                    LayoutTransform = element.LayoutTransform,
                    RenderTransformOrigin = element.RenderTransformOrigin,
                    HorizontalAlignment = element.HorizontalAlignment,
                    VerticalAlignment = element.VerticalAlignment,
                };
                
                // Insert the visual as the first child (meaning it will be shown at the bottom), draw it, and fade it out

                group.Children.Insert(0, host);
                host.DrawVisual(element, new Point(-element.ActualWidth / 2, -element.ActualHeight / 2)).Close();
                _ = host.FadeOut(group.FadeDuration, new SineEase() { EasingMode = EasingMode.EaseOut });

                // Restart the cooldown and update the position

                group.lastPosition = element.RelativeLocation();
                group.timer.Restart();
            }

            // Check if toggle off (element is a TrailGroup or it has a TrailGroup parent)

            if (element is TrailGroup || VisualTreeHelper.GetParent(element) is TrailGroup)
            {
                // Clear the group and find the index in the parent

                group = element is TrailGroup ? element as TrailGroup : VisualTreeHelper.GetParent(element) as TrailGroup;
                group.Children.Clear();

                var parent = VisualTreeHelper.GetParent(group) as FrameworkElement;
                int child_index = 0;

                while (VisualTreeHelper.GetChild(parent, child_index) != group)
                if (VisualTreeHelper.GetChildrenCount(parent) <= ++child_index) throw new InvalidOperationException();

                // Remove the poisiton check and reinsert the original element

                CompositionTarget.Rendering -= positionChecker;

                parent.RemoveChild(group);
                parent.InsertChildAt(child_index, group.Original);

                return null;
            }
            else
            {
                // Find the original element to remove it from its parent

                var parent = VisualTreeHelper.GetParent(element) as FrameworkElement;
                int child_index = 0;

                while (VisualTreeHelper.GetChild(parent, child_index) != element)
                if (VisualTreeHelper.GetChildrenCount(parent) <= ++child_index) throw new InvalidOperationException();

                parent.RemoveChild(element);

                // Create the new trail group and insert it at the required position

                group = new TrailGroup(element) { Name = element.Name, TrailCooldown = cooldown, FadeDuration = fadeDuration };
                parent.InsertChildAt(child_index, group);

                // Start the first cooldown and start position checking (first time will not count)

                group.timer.Start();
                group.lastPosition = element.RelativeLocation();
                CompositionTarget.Rendering += positionChecker;

                return group;
            }
        }

        /// <summary>
        /// Shakes <paramref name="element"/> by a maximum of <paramref name="intensity"/> units for a period of time.
        /// </summary>
        public static async Task Shake(this FrameworkElement element, double intensity, TimeSpan time)
        {
            // Setup the animations

            var transform = new TranslateTransform();
            var anim = new RandomDoubleAnimation(-intensity, intensity, time);
            var intensity_anim = new DoubleAnimation(intensity, 0, time) { EasingFunction = new SineEase() { EasingMode = EasingMode.EaseOut } };
            var delay_anim = new Int32Animation(25, 100, time) { EasingFunction = new SineEase() { EasingMode = EasingMode.EaseOut } };

            // Add the translate transform

            if (element.RenderTransform is TransformGroup g) g.Children.Add(transform);
            else
            {
                var original = element.RenderTransform;
                var group = (element.RenderTransform = new TransformGroup()) as TransformGroup;

                group.Children.Add(original);
                group.Children.Add(transform);
            }

            // Begin the animations

            anim.BeginAnimation(RandomDoubleAnimation.FromProperty, intensity_anim);
            anim.BeginAnimation(RandomDoubleAnimation.ToProperty, intensity_anim);
            anim.BeginAnimation(RandomDoubleAnimation.MillisecondDelayProperty, delay_anim);

            transform.BeginAnimation(TranslateTransform.XProperty, anim);
            transform.BeginAnimation(TranslateTransform.YProperty, anim);

            await Task.Delay(time);

            // Remove the translate transform and restore the original transform

            if (element.RenderTransform is TransformGroup gr) gr.Children.Remove(transform);
            else
            {
                var group = element.RenderTransform as TransformGroup;
                var original = group.Children[0];

                group.Children.Clear();
                element.RenderTransform = original;
            }
        }
    }

    /// <summary>
    /// Provides static methods useful for window interactions.
    /// </summary>
    public static class WindowHelper
    {
        public static double GetScalingFactorOfMonitor(Window window)
        {
            var hmonitor = MonitorFromWindow(new WindowInteropHelper(window).Handle, 0x00000002);
            GetDpiForMonitor(hmonitor, 0, out _, out uint ydpi);

            return ydpi / 96.0d;
        }
    }

    /// <summary>
    /// Provides static methods useful for dealing with fonts.
    /// </summary>
    public static class FontHelper
    {
        public static Size MeasureString(this Visual visual, string candidate, double fontSize, FontFamily family, FontStyle style, FontWeight weight, FontStretch stretch)
        {
            var formattedText = new FormattedText(candidate,
                                                  CultureInfo.CurrentCulture,
                                                  FlowDirection.LeftToRight,
                                                  new Typeface(family, style, weight, stretch),
                                                  fontSize,
                                                  Brushes.Black,
                                                  new NumberSubstitution(),
                                                  VisualTreeHelper.GetDpi(visual).PixelsPerDip);

            return new Size(formattedText.Width, formattedText.Height);
        }

        public static FontFamily RetrieveFontFamily(string fontName, string resourceFolderPath)
        {
            // Preconditions

            if (string.IsNullOrEmpty(resourceFolderPath)) throw new ArgumentNullException(nameof(resourceFolderPath));

            // Search through all font families found

            foreach (var family in Fonts.GetFontFamilies(new Uri("pack://application:,,,/PinkPact;component/" + resourceFolderPath.Trim('/') + "/", UriKind.RelativeOrAbsolute)))
            if (family.Source.EndsWith(fontName, StringComparison.OrdinalIgnoreCase)) return family;

            // Fallback to system fonts

            return Fonts.SystemFontFamilies.FirstOrDefault(f => f.Source.EndsWith(fontName, StringComparison.OrdinalIgnoreCase)) ?? 
                   new FontFamily(fontName);
        }
    }

    /// <summary>
    /// Provides static methods which simplify animating objects.
    /// </summary>
    public static class AnimationHelper
    {
        readonly static DoubleAnimation opacityAnim = new DoubleAnimation() { FillBehavior = FillBehavior.HoldEnd },
                                        wAnim = new DoubleAnimation() { FillBehavior = FillBehavior.HoldEnd },
                                        hAnim = new DoubleAnimation() { FillBehavior = FillBehavior.HoldEnd };

        readonly static ThicknessAnimation marginAnim = new ThicknessAnimation() { FillBehavior = FillBehavior.HoldEnd };
        readonly static ColorAnimation colorAnim = new ColorAnimation() { FillBehavior = FillBehavior.HoldEnd };

        readonly static Dictionary<Type, EasingFunctionBase> funcs = new Dictionary<Type, EasingFunctionBase>()
        {
            { typeof(BackEase), new BackEase() },
            { typeof(BounceEase), new BounceEase() },
            { typeof(CircleEase), new CircleEase() },
            { typeof(CubicEase), new CubicEase() },
            { typeof(ElasticEase), new ElasticEase() },
            { typeof(ExponentialEase), new ExponentialEase() },
            { typeof(PowerEase), new PowerEase() },
            { typeof(QuadraticEase), new QuadraticEase() },
            { typeof(QuarticEase), new QuarticEase() },
            { typeof(QuinticEase), new QuinticEase() },
            { typeof(SineEase), new SineEase() },
        };
        readonly static Timeline[] animations = new Timeline[] { opacityAnim, wAnim, hAnim, marginAnim, colorAnim };

        static AnimationHelper()
        {
            for (int i = 0; i < animations.Length; i++) Timeline.SetDesiredFrameRate(animations[i], 120);
        }

        /// <summary>
        /// Animates an object's opacity.
        /// </summary>
        public static async void AnimateOpacity(this FrameworkElement obj, double from, double to, int duration, Type easing, EasingMode mode)
        {
            if (!funcs.ContainsKey(easing)) throw new ArgumentException("The type of " + easing.Name + " does not represent a valid EasingFunctionBase", "easing");

            opacityAnim.From = from;
            opacityAnim.To = to;
            opacityAnim.Duration = TimeSpan.FromMilliseconds(duration);
            opacityAnim.EasingFunction = easing is null ? null : funcs[easing];
            (opacityAnim.EasingFunction as EasingFunctionBase).EasingMode = mode;
            obj.BeginAnimation(UIElement.OpacityProperty, opacityAnim);

            await Task.Delay(duration);
            obj.Opacity = to;
            obj.BeginAnimation(UIElement.OpacityProperty, null);
        }

        /// <summary>
        /// Animates an object's opacity.
        /// </summary>
        public static async Task AnimateOpacityAsync(this FrameworkElement obj, double from, double to, int duration, Type easing, EasingMode mode)
        {
            if (!funcs.ContainsKey(easing)) throw new ArgumentException("The type of " + easing.Name + " does not represent a valid EasingFunctionBase", "easing");

            opacityAnim.From = from;
            opacityAnim.To = to;
            opacityAnim.Duration = TimeSpan.FromMilliseconds(duration);
            opacityAnim.EasingFunction = easing is null ? null : funcs[easing];
            (opacityAnim.EasingFunction as EasingFunctionBase).EasingMode = mode;
            obj.BeginAnimation(UIElement.OpacityProperty, opacityAnim);

            await Task.Delay(duration);
            obj.Opacity = to;
            obj.BeginAnimation(UIElement.OpacityProperty, null);
        }

        /// <summary>
        /// Animates a brush's opacity.
        /// </summary>
        public static async void AnimateBrushOpacity(this Brush brush, double from, double to, int duration, Type easing, EasingMode mode)
        {
            if (!funcs.ContainsKey(easing)) throw new ArgumentException("The type of " + easing.Name + " does not represent a valid EasingFunctionBase", "easing");

            opacityAnim.From = from;
            opacityAnim.To = to;
            opacityAnim.Duration = TimeSpan.FromMilliseconds(duration);
            opacityAnim.EasingFunction = easing is null ? null : funcs[easing];
            (opacityAnim.EasingFunction as EasingFunctionBase).EasingMode = mode;
            brush.BeginAnimation(Brush.OpacityProperty, opacityAnim);

            await Task.Delay(duration);
            brush.Opacity = to;
            brush.BeginAnimation(Brush.OpacityProperty, null);
        }

        /// <summary>
        /// Animates a brush's opacity.
        /// </summary>
        public static async Task AnimateBrushOpacityAsync(this Brush brush, double from, double to, int duration, Type easing, EasingMode mode)
        {
            if (!funcs.ContainsKey(easing)) throw new ArgumentException("The type of " + easing.Name + " does not represent a valid EasingFunctionBase", "easing");

            opacityAnim.From = from;
            opacityAnim.To = to;
            opacityAnim.Duration = TimeSpan.FromMilliseconds(duration);
            opacityAnim.EasingFunction = easing is null ? null : funcs[easing];
            (opacityAnim.EasingFunction as EasingFunctionBase).EasingMode = mode;
            brush.BeginAnimation(Brush.OpacityProperty, opacityAnim);

            await Task.Delay(duration);
            brush.Opacity = to;
            brush.BeginAnimation(Brush.OpacityProperty, null);
        }

        /// <summary>
        /// Animates a <see cref="SolidColorBrush"/>'s color.
        /// </summary>
        public static async void AnimateColor(this SolidColorBrush brush, Color from, Color to, int duration, Type easing, EasingMode mode)
        {
            if (!funcs.ContainsKey(easing)) throw new ArgumentException("The type of " + easing.Name + " does not represent a valid EasingFunctionBase", "easing");

            colorAnim.From = from;
            colorAnim.To = to;
            colorAnim.Duration = TimeSpan.FromMilliseconds(duration);
            colorAnim.EasingFunction = easing is null ? null : funcs[easing];
            (colorAnim.EasingFunction as EasingFunctionBase).EasingMode = mode;
            brush.BeginAnimation(SolidColorBrush.ColorProperty, colorAnim);

            await Task.Delay(duration);
            brush.Color = to;
            brush.BeginAnimation(SolidColorBrush.ColorProperty, null);
        }

        /// <summary>
        /// Animates a <see cref="SolidColorBrush"/>'s color.
        /// </summary>
        public static async Task AnimateColorAsync(this SolidColorBrush brush, Color from, Color to, int duration, Type easing, EasingMode mode)
        {
            if (!funcs.ContainsKey(easing)) throw new ArgumentException("The type of " + easing.Name + " does not represent a valid EasingFunctionBase", "easing");

            colorAnim.From = from;
            colorAnim.To = to;
            colorAnim.Duration = TimeSpan.FromMilliseconds(duration);
            colorAnim.EasingFunction = easing is null ? null : funcs[easing];
            (colorAnim.EasingFunction as EasingFunctionBase).EasingMode = mode;
            brush.BeginAnimation(SolidColorBrush.ColorProperty, colorAnim);

            await Task.Delay(duration);
            brush.Color = to;
            brush.BeginAnimation(SolidColorBrush.ColorProperty, null);
        }

        /// <summary>
        /// Animates a scaling transform's X scale.
        /// </summary>
        public static async void AnimateWidth(this ScaleTransform obj, double from, double to, int duration, Type easing, EasingMode mode)
        {
            if (!funcs.ContainsKey(easing)) throw new ArgumentException("The type of " + easing.Name + " does not represent a valid EasingFunctionBase", "easing");

            wAnim.From = from;
            wAnim.To = to;
            wAnim.Duration = TimeSpan.FromMilliseconds(duration);
            wAnim.EasingFunction = easing is null ? null : funcs[easing];
            (wAnim.EasingFunction as EasingFunctionBase).EasingMode = mode;
            obj.BeginAnimation(ScaleTransform.ScaleXProperty, wAnim);

            await Task.Delay(duration);
            obj.ScaleX = to;
            obj.BeginAnimation(ScaleTransform.ScaleXProperty, null);
        }

        /// <summary>
        /// Animates a scaling transform's X scale.
        /// </summary>
        public static async Task AnimateWidthAsync(this ScaleTransform obj, double from, double to, int duration, Type easing, EasingMode mode)
        {
            if (!funcs.ContainsKey(easing)) throw new ArgumentException("The type of " + easing.Name + " does not represent a valid EasingFunctionBase", "easing");

            wAnim.From = from;
            wAnim.To = to;
            wAnim.Duration = TimeSpan.FromMilliseconds(duration);
            wAnim.EasingFunction = easing is null ? null : funcs[easing];
            (wAnim.EasingFunction as EasingFunctionBase).EasingMode = mode;
            obj.BeginAnimation(ScaleTransform.ScaleXProperty, wAnim);

            await Task.Delay(duration);
            obj.ScaleX = to;
            obj.BeginAnimation(ScaleTransform.ScaleXProperty, null);
        }

        /// <summary>
        /// Animates a scaling transform's Y scale.
        /// </summary>
        public static async void AnimateHeight(this ScaleTransform obj, double from, double to, int duration, Type easing, EasingMode mode)
        {
            if (!funcs.ContainsKey(easing)) throw new ArgumentException("The type of " + easing.Name + " does not represent a valid EasingFunctionBase", "easing");

            hAnim.From = from;
            hAnim.To = to;
            hAnim.Duration = TimeSpan.FromMilliseconds(duration);
            hAnim.EasingFunction = easing is null ? null : funcs[easing];
            (hAnim.EasingFunction as EasingFunctionBase).EasingMode = mode;
            obj.BeginAnimation(ScaleTransform.ScaleYProperty, hAnim);

            await Task.Delay(duration);
            obj.ScaleY = to;
            obj.BeginAnimation(ScaleTransform.ScaleYProperty, null);
        }

        /// <summary>
        /// Animates a scaling transform's Y scale.
        /// </summary>
        public static async Task AnimateHeightAsync(this ScaleTransform obj, double from, double to, int duration, Type easing, EasingMode mode)
        {
            if (!funcs.ContainsKey(easing)) throw new ArgumentException("The type of " + easing.Name + " does not represent a valid EasingFunctionBase", "easing");

            hAnim.From = from;
            hAnim.To = to;
            hAnim.Duration = TimeSpan.FromMilliseconds(duration);
            hAnim.EasingFunction = easing is null ? null : funcs[easing];
            (hAnim.EasingFunction as EasingFunctionBase).EasingMode = mode;
            obj.BeginAnimation(ScaleTransform.ScaleYProperty, hAnim);

            await Task.Delay(duration);
            obj.ScaleY = to;
            obj.BeginAnimation(ScaleTransform.ScaleYProperty, null);
        }

        /// <summary>
        /// Animates a framework element's width.
        /// </summary>
        public static async void AnimateWidth(this FrameworkElement obj, double from, double to, int duration, Type easing, EasingMode mode)
        {
            if (!funcs.ContainsKey(easing)) throw new ArgumentException("The type of " + easing.Name + " does not represent a valid EasingFunctionBase", "easing");

            wAnim.From = from;
            wAnim.To = to;
            wAnim.Duration = TimeSpan.FromMilliseconds(duration);
            wAnim.EasingFunction = easing is null ? null : funcs[easing];
            (wAnim.EasingFunction as EasingFunctionBase).EasingMode = mode;
            obj.BeginAnimation(FrameworkElement.WidthProperty, wAnim);

            await Task.Delay(duration);
            obj.Width = to;
            obj.BeginAnimation(FrameworkElement.WidthProperty, null);
        }

        /// <summary>
        /// Animates a framework element's width.
        /// </summary>
        public static async void AnimateWidthAsync(this FrameworkElement obj, double from, double to, int duration, Type easing, EasingMode mode)
        {
            if (!funcs.ContainsKey(easing)) throw new ArgumentException("The type of " + easing.Name + " does not represent a valid EasingFunctionBase", "easing");

            wAnim.From = from;
            wAnim.To = to;
            wAnim.Duration = TimeSpan.FromMilliseconds(duration);
            wAnim.EasingFunction = easing is null ? null : funcs[easing];
            (wAnim.EasingFunction as EasingFunctionBase).EasingMode = mode;
            obj.BeginAnimation(FrameworkElement.WidthProperty, wAnim);

            await Task.Delay(duration);
            obj.Width = to;
            obj.BeginAnimation(FrameworkElement.WidthProperty, null);
        }

        /// <summary>
        /// Animates a framework element's height.
        /// </summary>
        public static async void AnimateHeight(this FrameworkElement obj, double from, double to, int duration, Type easing, EasingMode mode)
        {
            if (!funcs.ContainsKey(easing)) throw new ArgumentException("The type of " + easing.Name + " does not represent a valid EasingFunctionBase", "easing");

            hAnim.From = from;
            hAnim.To = to;
            hAnim.Duration = TimeSpan.FromMilliseconds(duration);
            hAnim.EasingFunction = easing is null ? null : funcs[easing];
            (hAnim.EasingFunction as EasingFunctionBase).EasingMode = mode;
            obj.BeginAnimation(FrameworkElement.HeightProperty, hAnim);

            await Task.Delay(duration);
            obj.Height = to;
            obj.BeginAnimation(FrameworkElement.HeightProperty, null);
        }

        /// <summary>
        /// Animates a framework element's height.
        /// </summary>
        public static async void AnimateHeightAsync(this FrameworkElement obj, double from, double to, int duration, Type easing, EasingMode mode)
        {
            if (!funcs.ContainsKey(easing)) throw new ArgumentException("The type of " + easing.Name + " does not represent a valid EasingFunctionBase", "easing");

            hAnim.From = from;
            hAnim.To = to;
            hAnim.Duration = TimeSpan.FromMilliseconds(duration);
            hAnim.EasingFunction = easing is null ? null : funcs[easing];
            (hAnim.EasingFunction as EasingFunctionBase).EasingMode = mode;
            obj.BeginAnimation(FrameworkElement.HeightProperty, hAnim);

            await Task.Delay(duration);
            obj.Height = to;
            obj.BeginAnimation(FrameworkElement.HeightProperty, null);
        }

        /// <summary>
        /// Animates a translate transform's X positon.
        /// </summary>
        public static async void AnimatePositionX(this TranslateTransform obj, double from, double to, int duration, Type easing, EasingMode mode)
        {
            if (!funcs.ContainsKey(easing)) throw new ArgumentException("The type of " + easing.Name + " does not represent a valid EasingFunctionBase", "easing");

            wAnim.From = from;
            wAnim.To = to;
            wAnim.Duration = TimeSpan.FromMilliseconds(duration);
            wAnim.EasingFunction = easing is null ? null : funcs[easing];
            (wAnim.EasingFunction as EasingFunctionBase).EasingMode = mode;
            obj.BeginAnimation(TranslateTransform.XProperty, wAnim);

            await Task.Delay(duration);
            obj.X = to;
            obj.BeginAnimation(TranslateTransform.XProperty, null);
        }

        /// <summary>
        /// Animates a translate transform's X positon.
        /// </summary>
        public static async Task AnimatePositionXAsync(this TranslateTransform obj, double from, double to, int duration, Type easing, EasingMode mode)
        {
            if (!funcs.ContainsKey(easing)) throw new ArgumentException("The type of " + easing.Name + " does not represent a valid EasingFunctionBase", "easing");

            wAnim.From = from;
            wAnim.To = to;
            wAnim.Duration = TimeSpan.FromMilliseconds(duration);
            wAnim.EasingFunction = easing is null ? null : funcs[easing];
            (wAnim.EasingFunction as EasingFunctionBase).EasingMode = mode;
            obj.BeginAnimation(TranslateTransform.XProperty, wAnim);

            await Task.Delay(duration);
            obj.X = to;
            obj.BeginAnimation(TranslateTransform.XProperty, null);
        }

        /// <summary>
        /// Animates a translate transform's Y positon.
        /// </summary>
        public static async void AnimatePositionY(this TranslateTransform obj, double from, double to, int duration, Type easing, EasingMode mode)
        {
            if (!funcs.ContainsKey(easing)) throw new ArgumentException("The type of " + easing.Name + " does not represent a valid EasingFunctionBase", "easing");

            hAnim.From = from;
            hAnim.To = to;
            hAnim.Duration = TimeSpan.FromMilliseconds(duration);
            hAnim.EasingFunction = easing is null ? null : funcs[easing];
            (hAnim.EasingFunction as EasingFunctionBase).EasingMode = mode;
            obj.BeginAnimation(TranslateTransform.YProperty, hAnim);

            await Task.Delay(duration);
            obj.Y = to;
            obj.BeginAnimation(TranslateTransform.YProperty, null);
        }

        /// <summary>
        /// Animates a translate transform's Y positon.
        /// </summary>
        public static async Task AnimatePositionYAsync(this TranslateTransform obj, double from, double to, int duration, Type easing, EasingMode mode)
        {
            if (!funcs.ContainsKey(easing)) throw new ArgumentException("The type of " + easing.Name + " does not represent a valid EasingFunctionBase", "easing");

            hAnim.From = from;
            hAnim.To = to;
            hAnim.Duration = TimeSpan.FromMilliseconds(duration);
            hAnim.EasingFunction = easing is null ? null : funcs[easing];
            (hAnim.EasingFunction as EasingFunctionBase).EasingMode = mode;
            obj.BeginAnimation(TranslateTransform.YProperty, hAnim);

            await Task.Delay(duration);
            obj.Y = to;
            obj.BeginAnimation(TranslateTransform.YProperty, null);
        }

        /// <summary>
        /// Animates an object's margin.
        /// </summary>
        public static async void AnimateMargin(this FrameworkElement obj, Thickness from, Thickness to, int duration, Type easing, EasingMode mode)
        {
            if (!funcs.ContainsKey(easing)) throw new ArgumentException("The type of " + easing.Name + " does not represent a valid EasingFunctionBase", "easing");

            marginAnim.From = from;
            marginAnim.To = to;
            marginAnim.Duration = TimeSpan.FromMilliseconds(duration);
            marginAnim.EasingFunction = easing is null ? null : funcs[easing];
            (marginAnim.EasingFunction as EasingFunctionBase).EasingMode = mode;
            obj.BeginAnimation(FrameworkElement.MarginProperty, marginAnim);

            await Task.Delay(duration);
            obj.Margin = to;
            obj.BeginAnimation(FrameworkElement.MarginProperty, null);
        }

        /// <summary>
        /// Animates an object's margin.
        /// </summary>
        public static async Task AnimateMarginAsync(this FrameworkElement obj, Thickness from, Thickness to, int duration, Type easing, EasingMode mode)
        {
            if (!funcs.ContainsKey(easing)) throw new ArgumentException("The type of " + easing.Name + " does not represent a valid EasingFunctionBase", "easing");

            marginAnim.From = from;
            marginAnim.To = to;
            marginAnim.Duration = TimeSpan.FromMilliseconds(duration);
            marginAnim.EasingFunction = easing is null ? null : funcs[easing];
            (marginAnim.EasingFunction as EasingFunctionBase).EasingMode = mode;
            obj.BeginAnimation(FrameworkElement.MarginProperty, marginAnim);

            await Task.Delay(duration);
            obj.Margin = to;
            obj.BeginAnimation(FrameworkElement.MarginProperty, null);
        }
    }
    
    /// <summary>
    /// Provides static methods useful for mathematical operations.
    /// </summary>
    public static class MathHelper
    {
        static readonly RandomNumberGenerator rng = RandomNumberGenerator.Create();

        /// <summary>
        /// Computes an arithmetic expression. The expression can be combined with methods inside the <see cref="Math"/> class and can use ternary expressions.
        /// </summary>
        public static double Compute(string expression)
        {
            //Check if the expression contains NaN objects
            if (expression.Contains("NaN")) return 0;

            //Remove space chars from expression string and get the number of function calls
            expression = Regex.Replace(expression, @" +", "");
            int max = new Regex(@"Math").Matches(expression).Count;

            using (var dt = new DataTable())
            {
                expression = ReplaceTernary(expression, dt);

                //Replace operations inside function arguments
                foreach (var match in Regex.Matches(expression, @"(?<=Math\.[A-Z][A-Za-z0-9]+\(((\d+(\.\d+)?[*+/%^-]\d+(\.\d+)?[*+/%^-]?)*|,))(\d+(\.\d+)?[*+/%^-]\d+(\.\d+)?[*+/%^-]?)+(?=,|\))").Cast<Match>()) expression = expression.Replace(match.Value, dt.Compute(match.Value, "").ToString());

                //Execute all functions
                for (int i = 0; i < max; i++) foreach (var match in Regex.Matches(expression, @"Math\.[A-Z][A-Za-z0-9]+\(\d+(\.\d+)?\)").Cast<Match>()) expression = expression.Replace(match.Value, typeof(Math).GetMethod(Regex.Match(match.Value, @"(?<=Math\.)[A-Z][A-Za-z0-9]+(?=\()").Value, Enumerable.Repeat(typeof(double), Regex.Matches(match.Value, ",").Count + 1).ToArray()).Invoke(null, new object[] { Convert.ToDouble(Regex.Match(match.Value, @"(?<=Math\.[A-Z][A-Za-z0-9]+\()\d+(\.\d+)?(?=\))").Value) }).ToString());

                expression = ReplaceTernary(expression, dt);

                //Get final result
                return Convert.ToDouble(dt.Compute(expression, ""));
            }

            string ReplaceTernary(string str, DataTable dt)
            {
                foreach (var match in Regex.Matches(str, @"\[[^A-Za-z?><=\[\]]+?[><]\=?[^A-Za-z?><=\[\]]+?\?[^A-Za-z?><=:\[\]]+?\:[^A-Za-z?><=:\[\]]+\]").Cast<Match>())
                {
                    var condition = Regex.Match(match.Value, @"(?<=\[[^A-Za-z?><=\[\]]+?)[><]=?").Value;

                    //Calculate all operands
                    var op0 = Convert.ToDouble(dt.Compute(Regex.Match(match.Value, @"(?<=\[)[^A-Za-z?><=\[\]]+?(?=[><]=?)").Value, ""));
                    var op1 = Convert.ToDouble(dt.Compute(Regex.Match(match.Value, @"(?<=[><]=?)[^A-Za-z?><=\[\]]+?(?=\?)").Value, ""));
                    var op2 = dt.Compute(Regex.Match(match.Value, @"(?<=\?)[^A-Za-z?><=\[\]]+?(?=\:)").Value, "");
                    var op3 = dt.Compute(Regex.Match(match.Value, @"(?<=\:)[^A-Za-z?><=\[\]]+?(?=\])").Value, "");

                    if (condition.Contains(">")) str = str.Replace(match.Value, condition.Contains(">") ? (condition.Contains("=") ? (op0 >= op1 ? op2 : op3).ToString() :
                                                                                                                                                  (op0 > op1 ? op2 : op3).ToString()) :
                                                                                              condition.Contains("<") ? (condition.Contains("=") ? (op0 <= op1 ? op2 : op3).ToString() :
                                                                                                                                                   (op0 > op1 ? op2 : op3).ToString()) :
                                                                                                                                                   "");
                }

                return str;
            }
        }

        /// <summary>
        /// Clamps an <see cref="IComparable"/> between two values.
        /// </summary>
        public static T Clamp<T>(T value, T min, T max) where T : IComparable
        {
            return value.CompareTo(min) < 0 ? min : (value.CompareTo(max) > 0 ? max : value);
        }

        /// <summary>
        /// Clamps an <see cref="IComparable"/> to be higher than <paramref name="min"/> (or lower if <paramref name="doMax"/> is <see langword="true"/>).
        /// </summary>
        public static T Clamp<T>(T value, T min, bool doMax = false) where T : IComparable
        {
            return (!doMax ? value.CompareTo(min) < 0 : value.CompareTo(min) > 0) ? min : value;
        }

        /// <summary>
        /// Generates a random <see cref="double"/> value from <paramref name="min"/> to <paramref name="max"/>.
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        public static double Random(double min, double max)
        {
            if (min >= max) throw new ArgumentException("Minimum value must be less than maximum value.");

            var bytes = new byte[8];

            rng.GetBytes(bytes);
            var ulongRand = BitConverter.ToUInt64(bytes, 0);
            double normalized = ulongRand / (double) ulong.MaxValue;
            return min + (normalized * (max - min));
        }
    }

    /// <summary>
    /// Provides static methods which simplify obtaining and sending information to the keyboard.
    /// </summary>
    public static class KeyboardHelper
    {
        /// <summary>
        /// Presses a key.
        /// </summary>
        public static void PressKey(Key key)
        {
            var vk = KeyInterop.VirtualKeyFromKey(key);
            keybd_event((byte)vk, (byte)MapVirtualKey((uint)vk, 4), 0, 0);
            keybd_event((byte)vk, (byte)MapVirtualKey((uint)vk, 4), 0x0002, 0);
        }

        /// <summary>
        /// Checks if <paramref name="key"/> is currently held down.
        /// </summary>
        public static bool IsKeyDown(Key key)
        {
            return GetKeyState(KeyInterop.VirtualKeyFromKey(key)) < 0;
        }

        /// <summary>
        /// Checks if <paramref name="key"/> is toggled.
        /// </summary>
        public static bool IsKeyToggled(Key key)
        {
            return GetKeyState(KeyInterop.VirtualKeyFromKey(key)) == 1;
        }

        public class KeyHandler
        {
            /// <summary>
            /// The <c>Key</c> that is assigned to this <c>KeyHandler</c>.
            /// </summary>
            public Key Key
            {
                get => key;
                set
                {
                    key = value;
                    state = 0;
                }
            }

            /// <summary>
            /// Checks if the <c>Key</c> is in a toggled state.
            /// <para>
            /// Remark: the <c>Toggled</c> state is dependent to the instance of the <c>KeyHandler</c>, not the actual state of the key.
            /// </para>
            /// </summary>
            public bool Toggled { get; set; } = false;

            public long CheckState
            {
                get => state;
                set => state = value;
            }

            long state = 0;
            Key key = Key.None;

            /// <summary>
            /// Creates a new instance of the <c>KeyHandler</c> class.
            /// </summary>
            /// <param name="k"></param>
            public KeyHandler(Key k) => Key = k;

            /// <summary>
            /// Checks if <c>Key</c> is in a toggled state (dependent to its instance).
            /// <para>
            /// Returns: <c>Toggled</c>
            /// </para>
            /// </summary>
            /// <returns></returns>
            public bool CheckToggle()
            {
                if (Keyboard.IsKeyDown(Key)) if (state == 0) { state++; Toggled = true; } else if (state == -1) { Toggled = false; }
                if (Keyboard.IsKeyUp(Key)) if (Toggled) state = -1; else state = 0;
                return Toggled;
            }
        }

        public class HotkeyActionChecker
        {
            readonly Dictionary<Key, KeyHandler> handlers = new Dictionary<Key, KeyHandler>();
            readonly Action action;
            readonly Key[] keys;

            public HotkeyActionChecker(Action action, params Key[] keys) 
            {
                this.keys = keys;
                this.action = action;

                foreach (var key in keys) handlers.Add(key, new KeyHandler(key));
            }

            public bool Check()
            {
                // Here just do a general check on the keys
                // That is, if a key is not down, instant return.
                // In the end, at least 1 key must've JUST been pressed for the hotkey to work.
                // Each key's hold counter is reset ONLY when it's let go, so '1' is equivalent to an immediate press.
                // By holding it longer, the counter will just keep going, and so it won't be 1.
                // This prevents the holding issue.

                bool pressed = false;
                for (int i = 0; i < keys.Length; i++)
                {
                    var handler = handlers[keys[i]];
                    if (Keyboard.IsKeyDown(keys[i])) handler.CheckState++;
                    if (Keyboard.IsKeyUp(keys[i]))
                    {
                        handler.CheckState = 0;
                        return false;
                    }

                    pressed |= handler.CheckState == 1;
                }

                // This checks the order.
                // Realistically, when a human presses the keys, each key will have a lower hold counter than the previous.
                // So if that isn't respected, we'll know that the keys weren't pressed in order.

                for (int i = 1; i < keys.Length; i++) if (handlers[keys[i]].CheckState >= handlers[keys[i - 1]].CheckState) return false;

                if (pressed) action();
                return pressed;
            }
        }
    }

    /// <summary>
    /// Proivdes static methods which extend the functionality of <see cref="IEnumerable{T}"/> and <see cref="IEnumerator{T}"/> objects.
    /// </summary>
    public static class EnumeratorHelper
    {
        /// <summary>
        /// Gets the first element in <paramref name="e"/>, and resets the enumerator.
        /// </summary>
        public static T GetFirst<T>(this IEnumerator<T> e)
        {
            e.Reset();
            e.MoveNext();
            return e.Current;
        }

        /// <summary>
        /// Creates <paramref name="amount"/> of objects based on <paramref name="obj"/> by using <paramref name="creator"/>. The first parameter of <paramref name="creator"/> is <paramref name="obj"/>.
        /// </summary>
        public static IEnumerable<T> Create<T>(this T obj, Func<T, T> creator, int amount)
        {
            for (int i = 0; i < amount; i++) yield return creator(obj);
        }

        /// <summary>
        /// Creates <paramref name="amount"/> of objects based on <paramref name="obj"/> by using <paramref name="creator"/>. The first parameter of <paramref name="creator"/> is <paramref name="obj"/> and the second is the index of the current creation.
        /// </summary>
        public static IEnumerable<T> Create<T>(this T obj, Func<T, int, T> creator, int amount)
        {
            for (int i = 0; i < amount; i++) yield return creator(obj, i);
        }
    }

    /// <summary>
    /// Provides static methods which allow the addition of handlers to the events of <see cref="UIElement"/> objects.
    /// </summary>
    public static class EventHelper
    {
        #region Fields

        readonly static Dictionary<UIElement, object[]> clickDictionary = new Dictionary<UIElement, object[]>();

        #endregion

        /// <summary>
        /// Clears the invocation list of an object's event.
        /// </summary>
        public static void ClearInvocationList(object instance, string eventName)
        {
            instance.GetType().GetField(eventName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance).SetValue(instance, null);
        }

        /// <summary>
        /// Adds a <see cref="RoutedEventHandler"/> to an <see cref="UIElement"/> to be invoked when it's clicked.
        /// <para>
        /// If this method is called multiple times on the same <see cref="UIElement"/>, <paramref name="handler"/> will be added to the original handler.
        /// </para>
        /// </summary>
        public static void AddClickHandler(UIElement element, RoutedEventHandler handler)
        {
            //If the element is already registered, just add the new handler to the rest
            if (clickDictionary.ContainsKey(element))
            {
                clickDictionary[element][1] = (clickDictionary[element][1] as RoutedEventHandler) + handler;
                return;
            }

            clickDictionary.Add(element, new object[] { false, handler });

            element.MouseLeftButtonDown += OnElementLeftMouseButtonDown;
            element.MouseLeftButtonUp += OnElementLeftMouseButtonUp;
            element.MouseLeave += OnElementMouseLeave;
        }

        /// <summary>
        /// Called when the left mouse button is depressed onto the <see cref="UIElement"/>.
        /// </summary>
        private static void OnElementLeftMouseButtonDown(object sender, MouseButtonEventArgs e)
        {
            clickDictionary[sender as UIElement][0] = true;
        }

        /// <summary>
        /// Called when the left mouse button is released from the <see cref="UIElement"/>. If the button was initially depressed onto the element, this will invoke the click event.
        /// </summary>
        private static void OnElementLeftMouseButtonUp(object sender, MouseButtonEventArgs e)
        {
            //Get the object array of the element and return if the mouse wasn't initially depressed
            var data = clickDictionary[sender as UIElement];
            if (!(bool)data[0]) return;

            //Reset the check value and invoke the handlers
            data[0] = false;
            (data[1] as RoutedEventHandler).Invoke(sender, new RoutedEventArgs());
        }

        /// <summary>
        /// Called when the mouse leaves the <see cref="UIElement"/>.
        /// </summary>
        private static void OnElementMouseLeave(object sender, MouseEventArgs e)
        {
            clickDictionary[sender as UIElement][0] = false;
        }
    }

    /// <summary>
    /// Provides <see langword="static"/> methods useful for manipulation of <see cref="byte"/>[] objects.
    /// </summary>
    public static class ByteHelper
    {
        /// <summary>
        /// Removes all specified trailing and leading bytes from a <see cref="byte"/>[].
        /// </summary>
        public static byte[] Trim(this byte[] bytes, byte element)
        {
            bytes = bytes.TrimEnd(element);
            bytes = bytes.TrimStart(element);
            return bytes;
        }

        /// <summary>
        /// Removes all specified trailing bytes from a <see cref="byte"/>[].
        /// </summary>
        public static byte[] TrimEnd(this byte[] bytes, byte element)
        {
            //Declarations
            int removals = 0;

            //Check from the end to the start until element doesn't match
            for (int i = bytes.Length - 1; i > 0; i--)
            {
                if (bytes[i] != element) break;
                removals++;
            }

            Array.Resize(ref bytes, bytes.Length - removals);
            return bytes;
        }

        /// <summary>
        /// Removes all specified leading bytes from a <see cref="byte"/>[].
        /// </summary>
        public static byte[] TrimStart(this byte[] bytes, byte element)
        {
            return bytes.SkipWhile(x => x == element).ToArray();
        }

        /// <summary>
        /// Serializes an <see cref="object"/> to a <see cref="byte"/>[], which can be chosen to be encrypted.
        /// </summary>
        public static byte[] Serialize(this object obj)
        {
            using (var stream = new MemoryStream())
            {
                new BinaryFormatter().Serialize(stream, obj);

                //Encrypt adding only 3 control arguments, one more serialization key and two strings
                return stream.ToArray();
            }
        }

        /// <summary>
        /// Deserializes a <see cref="byte"/>[] and returns the <see cref="object"/> serialized into it.
        /// </summary>
        public static object Deserialize(this byte[] bytes)
        {
            using (var stream = new MemoryStream(bytes))
            {
                stream.Seek(0, SeekOrigin.Begin);
                return new BinaryFormatter().Deserialize(stream);
            }
        }

        /// <summary>
        /// Creates a <see cref="byte"/>[] of length <paramref name="length"/> from <paramref name="ptr"/>.
        /// </summary>
        public static byte[] FromIntPtr(this IntPtr ptr, int length)
        {
            var bytes = new byte[length];
            Marshal.Copy(ptr, bytes, 0, length);

            return bytes;
        }

        /// <summary>
        /// Creates an <see cref="IntPtr"/> from <paramref name="bytes"/>.
        /// </summary>
        public static IntPtr ToIntPtr(this byte[] bytes)
        {
            var ptr = new IntPtr();
            Marshal.Copy(bytes, 0, ptr, bytes.Length);

            return ptr;
        }

        /// <summary>
        /// Encrypts a <see cref="byte"/>[] using the AES cipher with a specified key and IV.
        /// </summary>
        public static byte[] Encrypt(this byte[] bytes, Key key)
        {
            byte[] skey = key.Serialize(),

            //Get the last bytes from the key's header
                   ivBytes = skey.Length >= 169 ? skey.Take(105).Skip(89).ToArray() : skey.Take(54).Skip(38).ToArray(),

            //Get 3 groups of bytes from the key's content
                   keyBytes = (skey = skey.Skip(skey.Length >= 169 ? 105 : 54).ToArray()).Take(8).Concat(skey.Skip(24).Take(8)).Concat(skey.Skip(skey.Length - 16)).ToArray();

            //Initialize AES object
            using (Aes aes = Aes.Create())
            {
                //Assign properties
                aes.Key = keyBytes;
                aes.IV = ivBytes;
                aes.Padding = PaddingMode.Zeros;
                aes.Mode = CipherMode.CBC;

                //Encrypt byte array
                using (var stream = new MemoryStream())
                {
                    using (var crypto = new CryptoStream(stream, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        crypto.Write(bytes, 0, bytes.Length);
                        crypto.FlushFinalBlock();
                    }

                    return stream.ToArray();
                }
            }
        }

        /// <summary>
        /// Decrypts a <see cref="byte"/>[] using the AES cipher from a specified key and IV.
        /// </summary>
        public static byte[] Decrypt(this byte[] bytes, Key key)
        {
            byte[] skey = key.Serialize(),

            //Get the last bytes from the key's header
                   ivBytes = skey.Length >= 169 ? skey.Take(105).Skip(89).ToArray() : skey.Take(54).Skip(38).ToArray(),

            //Get 3 groups of bytes from the key's content
                   keyBytes = (skey = skey.Skip(skey.Length >= 169 ? 105 : 54).ToArray()).Take(8).Concat(skey.Skip(24).Take(8)).Concat(skey.Skip(skey.Length - 16)).ToArray();

            //Initialize AES object
            using (Aes aes = Aes.Create())
            {
                //Assign properties
                aes.Key = keyBytes;
                aes.IV = ivBytes;
                aes.Padding = PaddingMode.Zeros;
                aes.Mode = CipherMode.CBC;

                //Decrypt byte array
                using (var stream = new MemoryStream())
                {
                    using (var crypto = new CryptoStream(stream, aes.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        crypto.Write(bytes, 0, bytes.Length);
                        crypto.FlushFinalBlock();
                    }

                    return stream.ToArray().TrimEnd(0);
                }
            }
        }

        /// <summary>
        /// Returns a combination of <paramref name="length"/> bytes from permutations of <paramref name="bytes"/>.
        /// </summary>
        public static byte[] Permute(this byte[] bytes, int length)
        {
            return Encoding.ASCII.GetBytes(Encoding.ASCII.GetString(bytes).Permute(length));
        }
    }

    /// <summary>
    /// Provides <see langword="static"/> methods that extend the functionality provided by the <see cref="Enumerable"/> class.
    /// </summary>
    public static class EnumHelper
    {
        /// <summary>
        /// Performs an action for each element of an <see cref="IEnumerable{T}"/>.
        /// </summary>
        /// <returns>The initial <see cref="IEnumerable{T}"/> to allow other operations on it.</returns>
        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            foreach (T item in enumerable) action(item);
            return enumerable;
        }

        /// <summary>
        /// Resizes an <see cref="IEnumerable{T}"/> to the specified size.
        /// <para>
        /// If the <paramref name="size"/> is bigger than the length of <paramref name="enumerable"/>, <paramref name="enumerable"/> is returned.
        /// </para>
        /// </summary>
        public static IEnumerable<T> Shrink<T>(this IEnumerable<T> enumerable, int size)
        {
            return enumerable.Where((x, i) => i < size);
        }
    }

    /// <summary>
    /// Provides static methods which allow better manipulation of the <see cref="string"/> type.
    /// </summary>
    public static class StringHelper
    {
        /// <summary>
        /// Returns a combination of <paramref name="length"/> characters from permutations of <paramref name="str"/>.
        /// </summary>
        public static string Permute(this string str, int length)
        {
            //Declare the final string
            string result = str;

            //Reset i after surpassing string length
            for (int i = 1; result.Length <= length; i = i + 1 > str.Length ? 1 : i + 1) result += str.Substring(str.Length - i, i) + str.Substring(0, str.Length - i);

            //Ensure the length
            return result.Substring(0, length);
        }

        /// <summary>
        /// Splits <paramref name="str"/> into a sequence of elements based on a <paramref name="separator"/>.
        /// <para>
        /// Any characters that match <paramref name="separator"/> that are found inside a pair from <paramref name="pairingChars"/> are ignored. Any character that is part of <paramref name="pairingChars"/> can be escaped.
        /// </para>
        /// </summary>
        public static string[] SplitSequence(this string str, string separator, Dictionary<char, char> pairingChars)
        {
            var sections = new List<string>();

            //Get the indexes of the separators in the string
            var separations = Regex.Matches(str, separator).Cast<Match>().Select(x => x.Index).ToHashSet();

            //Track state for each pair & detect pair enders
            var pairDict = pairingChars.ToDictionary(x => x, x => 0);
            var invPairs = pairingChars.Invert();

            //Each section will increase this
            int currentIndex = 0;
            for (int i = 0; i < str.Length; i++)
            {
                //Check for pair characters
                if ((pairingChars.ContainsKey(str[i]) || invPairs.ContainsKey(str[i])) && (i < 1 || str[i - 1] != '\\'))
                {
                    var pair = pairingChars.ContainsKey(str[i]) ? new KeyValuePair<char, char>(str[i], pairingChars[str[i]]) : new KeyValuePair<char, char>(invPairs[str[i]], str[i]);

                    //If the character is a pair ender, lower the state for that pair (same character pairs cannot be nested, so their state cannot pass 1)
                    pairDict[pair] = pairingChars.ContainsKey(str[i]) ? (invPairs.ContainsKey(str[i]) && pairDict[pair] >= 1 ? --pairDict[pair] : ++pairDict[pair]) : --pairDict[pair];
                }

                //Check for separator & if the current index is not in a pair
                if (separations.Contains(i) && pairDict.Values.Sum() == 0)
                {
                    //Ensure substring length such that it is always greater than 0 and doesn't include seaparator
                    sections.Add(str.Substring(currentIndex, i - currentIndex));
                    currentIndex = i + separator.Length;
                }
            }

            //Add the last section
            sections.Add(str.Substring(currentIndex, str.Length - currentIndex));
            return sections.ToArray();
        }

        /// <summary>
        /// Uncapitalizes the first character in a <see cref="string"/>.
        /// </summary>
        public static string LowerFirst(this string str)
        {
            return char.ToLower(str[0]) + str.Substring(1);
        }

        /// <summary>
        /// Capitalizes the first character in a <see cref="string"/>.
        /// </summary>
        public static string UpperFirst(this string str)
        {
            return char.ToUpper(str[0]) + str.Substring(1);
        }
    }

    /// <summary>
    /// Provides static methods useful for manipulating and obtaining local and public addresses.
    /// </summary>
    public static class IPAddressHelper
    {
        /// <summary>
        /// Gets the machine's local <see cref="IPAddress"/>.
        /// </summary>
        public static IPAddress GetLocalAddress()
        {
            return Dns.GetHostAddresses(Dns.GetHostName()).Where(x => x.AddressFamily == AddressFamily.InterNetwork).First();
        }

        /// <summary>
        /// Gets the public <see cref="IPAddress"/> assigned to the router.
        /// <para>
        /// This method may block the calling thread.
        /// </para>
        /// </summary>
        public static IPAddress GetPublicAddress()
        {
            //Try getting html from address until successful
            using (var client = new WebClient()) while (true)
                    try
                    {
                        return IPAddress.Parse(Regex.Match(
                                               client.DownloadString(new Uri("http://checkip.dyndns.org/")),
                                                                     @"(?<=Current IP Address: )[\d.]+(?=<)").Value);
                    }
                    catch
                    { }
        }

        /// <summary>
        /// Asynchronously gets the public <see cref="IPAddress"/> assigned to the router.
        /// <para>
        /// Being asynchronous, this method won't block the calling thread.
        /// </para>
        /// </summary>
        public static async Task<IPAddress> GetPublicAddressAsync()
        {
            //Try getting html from address until successful
            using (var client = new WebClient()) while (true)
                    try
                    {
                        return IPAddress.Parse(Regex.Match(
                                               client.DownloadString(new Uri("http://checkip.dyndns.org/")),
                                                                     @"(?<=Current IP Address: )[\d.]+(?=<)").Value);
                    }
                    catch
                    { await Task.Delay(1); }
        }
    }

    /// <summary>
    /// Provides static methods useful for manipulating <see cref="ObservableCollection{T}"/> objects while preserving their events' invocation lists.
    /// </summary>
    public static class ObservableHelper
    {
        /// <summary>
        /// Resizes a <see cref="ObservableCollection{T}"/> to a specified length.
        /// <para>
        /// The invocation list of the <see cref="ObservableCollection{T}.CollectionChanged"/> reamins unchanged.
        /// </para>
        /// </summary>
        public static ObservableCollection<T> Resize<T>(this ObservableCollection<T> collection, int size)
        {
            //Get the invocation list of the collection's changed event
            var invocationList = ((Delegate)typeof(ObservableCollection<T>).GetField("CollectionChanged", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance).GetValue(collection))?.GetInvocationList()?.Select(x => (NotifyCollectionChangedEventHandler)x)?.ToArray();

            //Reassign the collection & add the delegates to the event
            collection = new ObservableCollection<T>(collection.Shrink(size));
            for (int i = 0; i < (invocationList?.Length ?? 0); i++) collection.CollectionChanged += invocationList[i];
            return collection;
        }
    }

    /// <summary>
    /// Provides static methods useful for dealing with the limitations of conditional expressions.
    /// </summary>
    public static class ConditionalHelper
    {
        /// <summary>
        /// Represents a switch statement which can function for non-constants.
        /// </summary>
        public static void Switch<T>(T value, bool breakSwitch, Action defaultAction, Dictionary<T, Action> conditions)
        {
            foreach (var condition in conditions)
            {
                if (value.Equals(condition.Key))
                {
                    condition.Value();
                    if (breakSwitch) return;
                }
            }
            defaultAction?.Invoke();
        }
    }

    /// <summary>
    /// Provides static methods useful for simplifying asynchronous operations.
    /// </summary>
    public static class AsyncHelper
    {
        /// <summary>
        /// Returns a <see cref="TaskAwaiter"/> for an <see cref="int"/>. It is the equivalent of the <see cref="Task.GetAwaiter()"/> of the <see cref="Task.Delay(int)"/> method.
        /// </summary>
        public static TaskAwaiter GetAwaiter(this int n)
        {
            return Task.Delay(n).GetAwaiter();
        }
    }

    /// <summary>
    /// Provides static methods useful for acquiring time information.
    /// </summary>
    public static class TimeHelper
    {
        /// <summary>
        /// Gets the current UTC time.
        /// </summary>
        public static DateTime GetUtcTime()
        {
            // NTP message size - 16 bytes of the digest (RFC 2030)
            var ntpData = new byte[48];

            //Setting the Leap Indicator, Version Number and Mode values
            ntpData[0] = 0x1B;

            var addresses = Dns.GetHostEntry("time.windows.com").AddressList;

            //The UDP port number assigned to NTP is 123
            var ipEndPoint = new IPEndPoint(addresses[0], 123);

            //NTP uses UDP
            using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
            {
                socket.Connect(ipEndPoint);

                //Stops code hang if NTP is blocked
                socket.ReceiveTimeout = 3000;

                socket.Send(ntpData);
                socket.Receive(ntpData);
                socket.Close();
            }

            //Get the seconds part
            ulong intPart = BitConverter.ToUInt32(ntpData, 40);

            //Get the seconds fraction
            ulong fractPart = BitConverter.ToUInt32(ntpData, 44);

            //Convert From big-endian to little-endian
            intPart = SwapEndianness(intPart);
            fractPart = SwapEndianness(fractPart);

            var milliseconds = (intPart * 1000) + ((fractPart * 1000) / 0x100000000L);

            //UTC time
            var networkDateTime = (new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc)).AddMilliseconds((long)milliseconds);

            return networkDateTime;

            uint SwapEndianness(ulong x)
            {
                return (uint)(((x & 0x000000ff) << 24) +
                               ((x & 0x0000ff00) << 8) +
                               ((x & 0x00ff0000) >> 8) +
                               ((x & 0xff000000) >> 24));
            }
        }
    }

    /// <summary>
    /// Provides static methods which provide additional functionality to the <see cref="Type"/> class.
    /// </summary>
    public static class TypeHelper
    {
        /// <summary>
        /// Returns the type that called this method, or, if <paramref name="self"/> is <see langword="false"/>, the type that called the method where this method was called.
        /// </summary>
        public static Type GetCallingType(bool self = true)
        {
            //Get the type that called this method
            var type = new StackTrace().GetFrames().Where(x => !(x.GetMethod().Name == "MoveNext" && Attribute.IsDefined(x.GetMethod().DeclaringType, typeof(CompilerGeneratedAttribute))) && !(x.GetMethod().Name == "Start" && Regex.IsMatch(x.GetMethod().DeclaringType.Name, @"Async.+MethodBuilder"))).Skip(self ? 0 : 1).First().GetMethod().DeclaringType;

            //Get the actual type name from compiler generated types if necessary
            return type.Assembly.GetType(Regex.Replace(type.FullName, @"\+.*$", ""));
        }

        /// <summary>
        /// Returns the type at the specified frame, starting from the caller of this method, which is frame 1.
        /// </summary>
        public static Type GetCallingType(uint level)
        {
            //Get the type that called this method
            var type = new StackTrace().GetFrames().Where(x => !(x.GetMethod().Name == "MoveNext" && Attribute.IsDefined(x.GetMethod().DeclaringType, typeof(CompilerGeneratedAttribute))) && !(x.GetMethod().Name == "Start" && Regex.IsMatch(x.GetMethod().DeclaringType.Name, @"Async.+MethodBuilder"))).Skip((int)level).First().GetMethod().DeclaringType;

            //Get the actual type name from compiler generated types if necessary
            return type.Assembly.GetType(Regex.Replace(type.FullName, @"\+.*$", ""));
        }

        /// <summary>
        /// Returns a type's methods which represent overloaded operators.
        /// </summary>
        public static MethodInfo[] GetOperatorMethods(this Type t)
        {
            return t.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).Where(x => x.Name.StartsWith("op_")).ToArray();
        }

        /// <summary>
        /// Parses a string to an object of type <typeparamref name="T"/>.
        /// </summary>
        /// <exception cref="InvalidDataException"></exception>
        public static T Parse<T>(string str)
        {
            //If parsing to string, then just remove the quotation marks
            if (typeof(T) == typeof(string)) return Regex.IsMatch(str, @"^"".+""$") ? (T)(object)str.Substring(1, str.Length - 2) : (T)(object)str;

            //Check for arrays or enums
            if (typeof(T).IsArray || typeof(T).IsEnum)
                return typeof(T).IsEnum ? (T)Enum.Parse(typeof(T), str) :
                       (T)typeof(Enumerable).GetMethod("ToArray", BindingFlags.Static | BindingFlags.Public)
                                            .MakeGenericMethod(typeof(T).GetElementType())
                                            .Invoke(null, new object[] { typeof(Enumerable)
                                                                         .GetMethod("Cast", BindingFlags.Static | BindingFlags.Public)
                                                                         .MakeGenericMethod(typeof(T).GetElementType())
                                                                         .Invoke(null, new object[]
                                                                         {
                                                                            str.Substring(1, str.Length - 2)
                                                                               .SplitSequence(", ", new Dictionary<char, char>() { { '{', '}' }, { '"', '"' } })
                                                                               .Select(x => typeof(TypeHelper)
                                                                               .GetMethod("Parse", BindingFlags.Static | BindingFlags.Public)
                                                                               .MakeGenericMethod(typeof(T).GetElementType())
                                                                               .Invoke(null, new object[] { x }))
                                                                         }
                                            )});

            //Get the parse method
            var parseMethod = typeof(T).GetMethod("Parse", new Type[] { typeof(string) }) ?? throw new InvalidDataException($"{typeof(T).Name} cannot be parsed from a string.");

            //Return the methods result
            return (T)parseMethod.Invoke(null, new object[] { str });
        }

        /// <summary>
        /// Parses a string to an object of type <paramref name="type"/>.
        /// </summary>
        /// <exception cref="InvalidDataException"></exception>
        public static object Parse(Type type, string str)
        {
            //If parsing to string, then just remove the quotation marks
            if (type == typeof(string)) return Regex.IsMatch(str, @"^"".+""$") ? str.Substring(1, str.Length - 2) : str;

            //Check for arrays or enums
            if (type.IsArray || type.IsEnum)
                return type.IsEnum ? Enum.Parse(type, str) :
                       typeof(Enumerable).GetMethod("ToArray", BindingFlags.Static | BindingFlags.Public)
                                            .MakeGenericMethod(type.GetElementType())
                                            .Invoke(null, new object[] { typeof(Enumerable)
                                                                         .GetMethod("Cast", BindingFlags.Static | BindingFlags.Public)
                                                                         .MakeGenericMethod(type.GetElementType())
                                                                         .Invoke(null, new object[]
                                                                         {
                                                                            str.Substring(1, str.Length - 2)
                                                                               .SplitSequence(", ", new Dictionary<char, char>() { { '{', '}' }, { '"', '"' } })
                                                                               .Select(x => typeof(TypeHelper)
                                                                               .GetMethod("Parse", BindingFlags.Static | BindingFlags.Public)
                                                                               .MakeGenericMethod(type.GetElementType())
                                                                               .Invoke(null, new object[] { x }))
                                                                         }
                                            )});

            //Get the parse method
            var parseMethod = type.GetMethod("Parse", new Type[] { typeof(string) }) ?? throw new InvalidDataException($"{type.Name} cannot be parsed from a string.");

            //Return the methods result
            return parseMethod.Invoke(null, new object[] { str });
        }

        /// <summary>
        /// Detects what <see cref="Type"/> <paramref name="str"/> resembles.
        /// <para>
        /// Only works for types that have a known constant representation.
        /// </para>
        /// </summary>
        public static Type DetectType(string str)
        {
            if (str.Length == 0) return null;

            //Array
            if (str[0].Equals('{')) return DetectType(str.Substring(1).SplitSequence(", ", new Dictionary<char, char>() { { '{', '}' }, { '"', '"' } })[0]).MakeArrayType();

            //String
            if (str[0].Equals('"')) return typeof(string);

            //Char
            if (str[0].Equals('\'')) return typeof(char);

            //Bool
            if (str.Equals("true") || str.Equals("false")) return typeof(bool);

            //Double
            if (Regex.IsMatch(str, @"^-?(\d+\.(\d{9,16})|\d+(\.\d+)?d)", RegexOptions.IgnoreCase)) return typeof(double);

            //Float
            if (Regex.IsMatch(str, @"^-?(\d+\.(\d{1,8})|\d+(\.\d+)?f)", RegexOptions.IgnoreCase)) return typeof(float);

            //Long
            if (Regex.IsMatch(str, @"^-?(\d{11,19}|\d+l)", RegexOptions.IgnoreCase)) return typeof(long);

            //Int
            if (Regex.IsMatch(str, @"^-?\d{1,10}")) return typeof(int);

            //TimeSpan
            if (Regex.IsMatch(str, @"^(\d{1,2}\:){0,2}\d{1,2}(\.\d+)?")) return typeof(TimeSpan);

            //DateTime
            if (Regex.IsMatch(str, @"^(1[012]|[1-9])[./]([12]?[0-9]|3[01])[./][0-9]+")) return typeof(DateTime);

            //Default type is string
            return typeof(string);
        }
    }

    /// <summary>
    /// Provides static methods that extend the functionality of a <see cref="Dictionary{TKey, TValue}"/>.
    /// </summary>
    public static class DictionaryHelper
    {
        /// <summary>
        /// Combines two dictionaries together.
        /// </summary>
        public static Dictionary<TKey, TValue> Combine<TKey, TValue>(this Dictionary<TKey, TValue> dict, Dictionary<TKey, TValue> second)
        {
            foreach (var kvp in second) dict.Add(kvp.Key, kvp.Value);
            return dict;
        }

        /// <summary>
        /// Inverts a <see cref="Dictionary{TKey, TValue}"/>.
        /// </summary>
        public static Dictionary<TValue, TKey> Invert<TKey, TValue>(this Dictionary<TKey, TValue> dict)
        {
            return dict.ToDictionary(x => x.Value, x => x.Key);
        }
    }
}