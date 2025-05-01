using System.Windows.Controls;
using System.Windows.Media;
using PinkPact.Helpers;
using System.Windows;
using System;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

namespace PinkPact.Shaders
{
    /// <summary>
    /// Represents an effect decorator. An <see cref="EffectDecorator"/> is often found stacked with other <see cref="EffectDecorator"/>s to give a <see cref="FrameworkElement"/> multiple effects.
    /// <para>All <see cref="EffectDecorator"/>s in a stack have the same child, and take that child's name.</para>
    /// </summary>
    public class EffectDecorator : Decorator
    {
        /// <summary>
        /// Gets or sets the element which this <see cref="EffectDecorator"/> and its specific stack affects.
        /// </summary>
        public UIElement Affected
        {
            get => child;
            set
            {
                if (value is EffectDecorator) throw new ArgumentException("Child must not be an EffectDecorator");

                var top = GetTopEffect();
                while (top?.GetType() == typeof(EffectDecorator))
                {
                    top.child = value;
                    if (!(top.Child is EffectDecorator)) top.Child = value;

                    top = top.Child as EffectDecorator;
                }
            }
        }

        UIElement child;
        static bool popping = false;

        /// <summary>
        /// Creates a new instance of the <see cref="EffectDecorator"/> class.
        /// </summary>
        public EffectDecorator()
        {
            var child = base.Child;
            if (child.GetType() == typeof(EffectDecorator)) this.child = (Child as EffectDecorator).Child;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="EffectDecorator"/> class that affects <paramref name="affected"/>.
        /// <para>If <paramref name="affected"/> is an <see cref="EffectDecorator"/>, calls <see cref="EffectDecorator"/>(<see cref="EffectDecorator"/> effect).</para>
        /// </summary>
        public EffectDecorator(FrameworkElement affected)
        {
            Name = affected.Name;

            if (!(affected is EffectDecorator)) child = Child = affected;
            else Child = affected;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="EffectDecorator"/> class that has the same affected element as <paramref name="effect"/>.
        /// </summary>
        /// <param name="effect"></param>
        public EffectDecorator(EffectDecorator effect)
        {
            Name = effect.Name;
            Child = effect;
        }

        /// <summary>
        /// Gets the topmost <see cref="EffectDecorator"/> in a stack.
        /// </summary>
        public EffectDecorator GetTopEffect()
        {
            var top = this;
            while (VisualTreeHelper.GetParent(top) is EffectDecorator) top = VisualTreeHelper.GetParent(top) as EffectDecorator;

            return top;
        }

        /// <summary>
        /// Gets the botommost <see cref="EffectDecorator"/> in a stack.
        /// </summary>
        public EffectDecorator GetBottomEffect()
        {
            var bottom = this;
            while (Child is EffectDecorator) bottom = Child as EffectDecorator;

            return bottom;
        }

        /// <summary>
        /// Removes this <see cref="EffectDecorator"/> from its corresponding stack.
        /// </summary>
        /// <returns>
        /// The <see cref="EffectDecorator"/> that followed this one in the stack, or <see langword="null"/> if this effect was at the top of the stack.
        /// </returns>
        public EffectDecorator Remove()
        {
            FrameworkElement parent;

            popping = true;
            var child = Child;
            Child = null;

            if ((parent = VisualTreeHelper.GetParent(this) as FrameworkElement) is EffectDecorator p) p.Child = child;
            else
            {
                int index = this.ChildIndex();

                parent.RemoveChild(this);
                parent.InsertChildAt(index, child as FrameworkElement);
            }

            popping = false;
            return parent is EffectDecorator ? parent as EffectDecorator : null;
        }

        /// <summary>
        /// Gets the next <see cref="EffectDecorator"/> in the stack, or <see langword="null"/> if this effect is at the top of the stack.
        /// </summary>
        public EffectDecorator Next()
        {
            var parent = VisualTreeHelper.GetParent(this);
            return parent is EffectDecorator ? parent as EffectDecorator : null;
        }

        protected override void OnVisualChildrenChanged(DependencyObject visualAdded, DependencyObject visualRemoved)
        {
            base.OnVisualChildrenChanged(visualAdded, visualRemoved);

            if (!popping && visualRemoved == Child)
            {
                var top = this;
                while (VisualTreeHelper.GetParent(top) is EffectDecorator)
                {
                    top.Child = null;
                    top.child = null;

                    top = VisualTreeHelper.GetParent(top) as EffectDecorator;
                }

                (VisualTreeHelper.GetParent(top) as FrameworkElement).RemoveChild(top);
                return;
            }
        }
    }
}
