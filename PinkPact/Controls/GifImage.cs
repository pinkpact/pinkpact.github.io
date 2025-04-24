using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.Windows;
using System;

namespace PinkPact.Controls
{
    public class GifImage : Image
    {
        public int FrameIndex
        {
            get => (int)GetValue(FrameIndexProperty);
            set => SetValue(FrameIndexProperty, value);
        }

        public bool AutoStart
        {
            get => (bool)GetValue(AutoStartProperty);
            set => SetValue(AutoStartProperty, value);
        }

        public string GifSource
        {
            get => (string)GetValue(GifSourceProperty);
            set => SetValue(GifSourceProperty, value);
        }

        public bool FromPackUri
        {
            get => (bool)GetValue(FromPackUriProperty);
            set => SetValue(FromPackUriProperty, value);
        }

        public int DelayMs
        {
            get => (int)GetValue(DelayMsProperty);
            set => SetValue(DelayMsProperty, value);
        }

        static GifImage()
        {
            VisibilityProperty.OverrideMetadata(typeof(GifImage), new FrameworkPropertyMetadata(VisibilityPropertyChanged));
        }

        bool _isInitialized;
        GifBitmapDecoder _gifDecoder;
        Int32Animation _animation;

        public static readonly DependencyProperty AutoStartProperty = DependencyProperty.Register("AutoStart", typeof(bool), typeof(GifImage), new UIPropertyMetadata(false, AutoStartPropertyChanged));
        public static readonly DependencyProperty GifSourceProperty = DependencyProperty.Register("GifSource", typeof(string), typeof(GifImage), new UIPropertyMetadata(string.Empty, GifSourcePropertyChanged));
        public static readonly DependencyProperty FrameIndexProperty = DependencyProperty.Register("FrameIndex", typeof(int), typeof(GifImage), new UIPropertyMetadata(0, new PropertyChangedCallback(ChangingFrameIndex)));
        public static readonly DependencyProperty FromPackUriProperty = DependencyProperty.Register("FromPackUri", typeof(bool), typeof(GifImage), new PropertyMetadata(false));
        public static readonly DependencyProperty DelayMsProperty = DependencyProperty.Register("DelayMs", typeof(int), typeof(GifImage), new PropertyMetadata(50));

        public void StartAnimation()
        {
            if (!_isInitialized)
                this.Initialize();

            BeginAnimation(FrameIndexProperty, _animation);
        }

        public void StopAnimation()
        {
            BeginAnimation(FrameIndexProperty, null);
        }

        void Initialize()
        {
            _gifDecoder = new GifBitmapDecoder(new Uri((FromPackUri ? "pack://application:,,," : "") + GifSource, UriKind.RelativeOrAbsolute), BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
            _animation = new Int32Animation(0, _gifDecoder.Frames.Count, new Duration(TimeSpan.FromMilliseconds(DelayMs * _gifDecoder.Frames.Count - _gifDecoder.Frames.Count))) { RepeatBehavior = RepeatBehavior.Forever };
            Source = _gifDecoder.Frames[0];

            _isInitialized = true;
        }

        static void AutoStartPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
                (sender as GifImage).StartAnimation();
        }

        static void VisibilityPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if ((Visibility)e.NewValue == Visibility.Visible) ((GifImage)sender).StartAnimation();
            else ((GifImage)sender).StopAnimation();
        }

        static void GifSourcePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            (sender as GifImage).Initialize();
        }

        static void ChangingFrameIndex(DependencyObject obj, DependencyPropertyChangedEventArgs ev)
        {
            var gifImage = obj as GifImage;
            gifImage.Source = gifImage._gifDecoder.Frames[(int)ev.NewValue == gifImage._gifDecoder.Frames.Count ? 0 : (int)ev.NewValue];
        }
    }
}
