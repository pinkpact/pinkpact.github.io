using System.Windows.Controls;
using System.Diagnostics;
using System.Windows;
using System;

namespace PinkPact.Controls
{
    /// <summary>
    /// Represents a trail group. A trail group is a collection <see cref="DrawingVisualHost"/> objects representing the trails, and the original <see cref="FrameworkElement"/>.
    /// </summary>
    public class TrailGroup : Grid
    {
        /// <summary>
        /// Gets or sets the original <see cref="FrameworkElement"/> after which any trail will be drawn. 
        /// <para>This value will always be last child of a <see cref="TrailGroup"/>.</para>
        /// </summary>
        public FrameworkElement Original 
        {
            get => original; 
            set
            {
                Children.RemoveAt(Children.Count - 1);
                original = value;
                Children.Add(original);
            }
        }

        /// <summary>
        /// Gets or sets the delay between two consecutive trails.
        /// </summary>
        public TimeSpan TrailCooldown { get; set; }

        /// <summary>
        /// Gets or sets the time it takes for one trail to fade out.
        /// </summary>
        public TimeSpan FadeDuration { get; set; }

        internal Point lastPosition;
        internal Stopwatch timer = new Stopwatch();
        FrameworkElement original;

        /// <summary>
        /// Creates a new instace of the <see cref="TrailGroup"/> class based on an original <see cref="FrameworkElement"/>.
        /// </summary>
        /// <param name="original"></param>
        public TrailGroup(FrameworkElement original)
        {
            this.original = original;
            Children.Add(original);
        }
    }
}
