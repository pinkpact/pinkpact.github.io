using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Data;
using System.Windows;
using System;
using PinkPact.Helpers;

namespace PinkPact.Controls
{
    public partial class GifImageButton : UserControl
    {
        public string Gif
        {
            get => (string)GetValue(GifProperty);
            set => SetValue(GifProperty, value);
        }

        public string HoverGif
        {
            get => (string)GetValue(HoverGifProperty);
            set => SetValue(HoverGifProperty, value);
        }

        public string HoldGif
        {
            get => (string)GetValue(HoldGifProperty);
            set => SetValue(HoldGifProperty, value);
        }

        public string DisabledGif
        {
            get => (string)GetValue(DisabledGifProperty);
            set => SetValue(DisabledGifProperty, value);
        }

        public BitmapScalingMode ScalingMode
        {
            get => (BitmapScalingMode)GetValue(ScalingModeProperty);
            set => SetValue(ScalingModeProperty, value);
        }

        public EdgeMode EdgeMode
        {
            get => (EdgeMode)GetValue(EdgeModeProperty);
            set => SetValue(EdgeModeProperty, value);
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

        public bool DisabledChangesOpacity
        {
            get => (bool)GetValue(DisabledChangesOpacityProperty);
            set => SetValue(DisabledChangesOpacityProperty, value);
        }

        
        public event RoutedEventHandler Click
        {
            add { AddHandler(ClickEvent, value); }
            remove { RemoveHandler(ClickEvent, value); }
        }

        bool holding = false,
             enabledChanged = false,
             enabledSet = false;

        public static readonly DependencyProperty DisabledChangesOpacityProperty = DependencyProperty.Register("DisabledChangesOpacity", typeof(bool), typeof(GifImageButton), new PropertyMetadata(true));

        public static readonly DependencyProperty GifProperty = DependencyProperty.Register("Gif", typeof(string), typeof(GifImageButton));
        public static readonly DependencyProperty HoverGifProperty = DependencyProperty.Register("HoverGif", typeof(string), typeof(GifImageButton));
        public static readonly DependencyProperty HoldGifProperty = DependencyProperty.Register("HoldGif", typeof(string), typeof(GifImageButton));
        public static readonly DependencyProperty DisabledGifProperty = DependencyProperty.Register("DisabledGif", typeof(string), typeof(GifImageButton));
        public static readonly DependencyProperty ScalingModeProperty = DependencyProperty.Register("ScalingMode", typeof(BitmapScalingMode), typeof(GifImageButton), new PropertyMetadata(BitmapScalingMode.NearestNeighbor));
        public static readonly DependencyProperty EdgeModeProperty = DependencyProperty.Register("EdgeMode", typeof(EdgeMode), typeof(GifImageButton), new PropertyMetadata(EdgeMode.Aliased));
        public static readonly DependencyProperty FromPackUriProperty = DependencyProperty.Register("FromPackUri", typeof(bool), typeof(GifImageButton), new PropertyMetadata(false));
        public static readonly DependencyProperty DelayMsProperty = DependencyProperty.Register("DelayMs", typeof(int), typeof(GifImageButton), new PropertyMetadata(50));

        public static readonly RoutedEvent ClickEvent = EventManager.RegisterRoutedEvent("Click", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ImageButton));

        public GifImageButton()
        {
            InitializeComponent();

            Loaded += (_, __) =>
            {
                Console.WriteLine(DelayMs);
                img.SetBinding(GifImage.FromPackUriProperty, new Binding("FromPackUri") { Source = RelativeSource.Self });
                img.SetBinding(GifImage.DelayMsProperty, new Binding("DelayMs") { Source = RelativeSource.Self });
                
                img.DelayMs = DelayMs;
                img.GifSource = Gif;
                img.AutoStart = true;

                RenderOptions.SetEdgeMode(img, EdgeMode);
                RenderOptions.SetBitmapScalingMode(img, ScalingMode);

                if (!double.IsInfinity(MaxWidth)) img.Width = MaxWidth;
                if (!double.IsInfinity(MaxHeight)) img.Height = MaxHeight;
            };
        }

        void OnMouseEnter(object sender, MouseEventArgs e)
        {
            img.GifSource = holding ? HoldGif : HoverGif;
        }

        void OnMouseLeave(object sender, MouseEventArgs e)
        {
            holding = false;
            img.GifSource = Gif;
        }

        void OnMouseLDown(object sender, MouseButtonEventArgs e)
        {
            holding = true;
            img.GifSource = HoldGif;
        }

        void OnMouseLUp(object sender, MouseButtonEventArgs e)
        {
            holding = false;
            img.GifSource = IsMouseOver ? HoverGif : Gif;
        }

        private void OnClick(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(ClickEvent, this));
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (DisabledChangesOpacity && e.Property.Name == "IsEnabled")
            {
                enabledChanged = IsEnabled != enabledChanged;
                if (!enabledSet || enabledChanged) Opacity += 0.2 * (IsEnabled ? 1 : -1);
                img.GifSource = !IsEnabled ? DisabledGif : (holding ? HoldGif : (IsMouseOver ? HoverGif : Gif));

                enabledSet = true;
            }
        }
    }
}
