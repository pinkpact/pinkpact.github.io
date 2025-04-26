using System.Windows.Media;
using System.Windows;
using System;

namespace PinkPact.Controls
{
    /// <summary>
    /// Represents a host for a <see cref="DrawingVisual"/> object.
    /// </summary>
    public class DrawingVisualHost : FrameworkElement
    {
        protected override int VisualChildrenCount => 1;

        /// <summary>
        /// Gets the <see cref="System.Windows.Media.Visual"/> object associated with this instance.
        /// </summary>
        public DrawingVisual Visual
        { 
            get => GetValue(VisualProperty) as DrawingVisual; 
            set
            {
                RemoveVisualChild(Visual);
                SetValue(VisualProperty, value);
                AddVisualChild(Visual);
            }
        }

        public static readonly DependencyProperty VisualProperty = DependencyProperty.Register("Visual", typeof(DrawingVisual), typeof(DrawingVisualHost));

        /// <summary>
        /// Creates a new instance of the <see cref="DrawingVisualHost"/> class from a base <see cref="Visual"/>. 
        /// </summary>
        public DrawingVisualHost(DrawingVisual visual)
        {
            AddVisualChild(visual);
            SetValue(VisualProperty, visual);
        }

        protected override Visual GetVisualChild(int index)
        {
            if (index != 0) throw new ArgumentOutOfRangeException();
            return Visual;
        }
    }
}
