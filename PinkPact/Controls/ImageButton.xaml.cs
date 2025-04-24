using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows;

using PinkPact.Helpers;
using System;

namespace PinkPact.Controls
{
    public partial class ImageButton : UserControl
    {
        public ImageSource Image
        {
            get => (ImageSource) GetValue(ImageProperty);
            set => SetValue(ImageProperty, value);
        }

        public ImageSource HoverImage
        {
            get => (ImageSource)GetValue(HoverImageProperty);
            set => SetValue(HoverImageProperty, value);
        }

        public ImageSource HoldImage
        {
            get => (ImageSource)GetValue(HoldImageProperty);
            set => SetValue(HoldImageProperty, value);
        }

        public ImageSource DisabledImage
        {
            get => (ImageSource)GetValue(DisabledImageProperty);
            set => SetValue(DisabledImageProperty, value);
        }

        public bool DisabledChangesOpacity 
        { 
            get => (bool)GetValue(DisabledChangesOpacityProperty);
            set => SetValue(DisabledChangesOpacityProperty, value);
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


        public event RoutedEventHandler Click
        {
            add { AddHandler(ClickEvent, value); }
            remove { RemoveHandler(ClickEvent, value); }
        }

        bool holding = false,
             enabledChanged = false,
             enabledSet = false;

        public static readonly DependencyProperty DisabledChangesOpacityProperty = DependencyProperty.Register("DisabledChangesOpacity", typeof(bool), typeof(ImageButton), new PropertyMetadata(true));

        public static readonly DependencyProperty ImageProperty = DependencyProperty.Register("Image", typeof(ImageSource), typeof(ImageButton));
        public static readonly DependencyProperty HoverImageProperty = DependencyProperty.Register("HoverImage", typeof(ImageSource), typeof(ImageButton));
        public static readonly DependencyProperty HoldImageProperty = DependencyProperty.Register("HoldImage", typeof(ImageSource), typeof(ImageButton));
        public static readonly DependencyProperty DisabledImageProperty = DependencyProperty.Register("DisabledImage", typeof(ImageSource), typeof(ImageButton));
        public static readonly DependencyProperty ScalingModeProperty = DependencyProperty.Register("ScalingMode", typeof(BitmapScalingMode), typeof(ImageButton), new PropertyMetadata(BitmapScalingMode.NearestNeighbor));
        public static readonly DependencyProperty EdgeModeProperty = DependencyProperty.Register("EdgeMode", typeof(EdgeMode), typeof(ImageButton), new PropertyMetadata(EdgeMode.Aliased));

        public static readonly RoutedEvent ClickEvent = EventManager.RegisterRoutedEvent("Click", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ImageButton));

        public ImageButton()
        {
            InitializeComponent();

            Loaded += (_, __) =>
            {
                img.Source = Image;

                RenderOptions.SetEdgeMode(img, EdgeMode);
                RenderOptions.SetBitmapScalingMode(img, ScalingMode);

                if (!double.IsInfinity(MaxWidth)) img.Width = MaxWidth;
                if (!double.IsInfinity(MaxHeight)) img.Height = MaxHeight;
            };
        }

        void OnMouseEnter(object sender, MouseEventArgs e)
        {
            img.Source = holding ? HoldImage : HoverImage;
        }

        void OnMouseLeave(object sender, MouseEventArgs e)
        {
            holding = false;
            img.Source = Image;
        }

        void OnMouseLDown(object sender, MouseButtonEventArgs e)
        {
            holding = true;
            img.Source = HoldImage;
        }

        void OnMouseLUp(object sender, MouseButtonEventArgs e)
        {
            holding = false;
            img.Source = IsMouseOver ? HoverImage : Image;
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
                img.Source = !IsEnabled ? DisabledImage : (holding ? HoldImage : (IsMouseOver ? HoverImage : Image));

                enabledSet = true;
            }
        }
    }
}
