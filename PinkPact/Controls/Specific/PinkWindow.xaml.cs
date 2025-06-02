using PinkPact.Helpers;
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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PinkPact.Controls.Specific
{
    public partial class PinkWindow : UserControl
    {
        /// <summary>
        /// Gets or sets the title of this <see cref="PinkWindow"/>.
        /// </summary>
        public string Title
        {
            get => GetValue(TitleProperty) as string;
            set => SetValue(TitleProperty, value);
        }

        /// <summary>
        /// Determines if this <see cref="PinkWindow"/> can be dragged around.
        /// </summary>
        public bool Draggable
        {
            get => (bool)GetValue(DraggableProperty);
            set
            {
                SetValue(DraggableProperty, value);
                while (dragBar.ToggleDragging(this) != Draggable);
            }
        }

        /// <summary>
        /// Gets or sets the content of this <see cref="PinkWindow"/>.
        /// </summary>
        public UserControl WindowContent
        {
            get => GetValue(WindowContentProperty) as UserControl;
            set => SetValue(WindowContentProperty, value);
        }

        /// <summary>
        /// Gets the exit button of this <see cref="PinkWindow"/>.
        /// </summary>
        public ImageButton ExitButton => exitBtn;

        /// <summary>
        /// Occurs when the exit button of this <see cref="PinkWindow"/> is clicked.
        /// </summary>
        public event RoutedEventHandler ExitClicked
        {
            add => AddHandler(ExitClickedEvent, value);
            remove => RemoveHandler(ExitClickedEvent, value);
        }

        public static DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(PinkWindow));
        public static DependencyProperty DraggableProperty = DependencyProperty.Register("Draggable", typeof(bool), typeof(PinkWindow), new PropertyMetadata(true));
        public static DependencyProperty WindowContentProperty = DependencyProperty.Register("WindowContent", typeof(UserControl), typeof(PinkWindow), new PropertyMetadata(null, OnWindowContentChanged));

        public static RoutedEvent ExitClickedEvent = EventManager.RegisterRoutedEvent("ExitClicked", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(PinkWindow));

        /// <summary>
        /// Creates a new instance of the <see cref="PinkWindow"/> class.
        /// </summary>
        public PinkWindow()
        {
            InitializeComponent();
            exitBtn.Click += (_, __) => RaiseEvent(new RoutedEventArgs(ExitClickedEvent, this));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            Draggable = Draggable;
        }

        static void OnWindowContentChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            var wnd = o as PinkWindow;
            wnd.windowContent.Children.Clear();
            
            if (e.NewValue != null) wnd.windowContent.Children.Add(e.NewValue as UserControl);
        }
    }
}
