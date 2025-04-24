using System.Windows.Media.Animation;
using System.Security.Cryptography;
using System.Diagnostics;
using System.Windows;
using System;

using PinkPact.Helpers;


namespace PinkPact.Animations
{
    /// <summary>
    /// Animates a double value by producing random values within a specified range.
    /// </summary>
    public class RandomDoubleAnimation : DoubleAnimationBase
    {
        /// <summary>
        /// Gets or sets the beginning of the random range.
        /// </summary>
        public double From
        {
            get => (double)GetValue(FromProperty);
            set => SetValue(FromProperty, value);
        }  

        /// <summary>
        /// Gets or sets the end of the random range.
        /// </summary>
        public double To
        {
            get => (double)GetValue(ToProperty);
            set => SetValue(ToProperty, value);
        }

        /// <summary>
        /// Gets or sets the delay, in milliseconds, between each value change when this animation is played. Default value is 25.
        /// </summary>
        public int MillisecondDelay
        {
            get => (int)GetValue(MillisecondDelayProperty);
            set => SetValue(MillisecondDelayProperty, value);
        }

        public static readonly DependencyProperty MillisecondDelayProperty = DependencyProperty.Register("MillisecondDelay", typeof(int), typeof(RandomDoubleAnimation), new PropertyMetadata(25));
        public static readonly DependencyProperty FromProperty = DependencyProperty.Register("From", typeof(double), typeof(RandomDoubleAnimation), new PropertyMetadata(0.0));
        public static readonly DependencyProperty ToProperty = DependencyProperty.Register("To", typeof(double), typeof(RandomDoubleAnimation), new PropertyMetadata(1.0));

        double lastValue;
        readonly Stopwatch timer;

        /// <summary>
        /// Initializes a new instance of the <see cref="RandomDoubleAnimation"/> class with specified range and duration.
        /// </summary>
        /// <param name="from">The beginning of the random range.</param>
        /// <param name="to">The end of the random range.</param>
        /// <param name="duration">The duration of the animation.</param>
        public RandomDoubleAnimation(double from, double to, Duration duration)
        {
            From = from;
            To = to;
            Duration = duration;
            timer = Stopwatch.StartNew();
        }

        /// <summary>
        /// Calculates the current value of the animation.
        /// </summary>
        /// <param name="defaultOriginValue">The suggested origin value provided to the animation.</param>
        /// <param name="defaultDestinationValue">The suggested destination value provided to the animation.</param>
        /// <param name="animationClock">An AnimationClock that generates the CurrentTime or CurrentProgress value used by the host animation.</param>
        /// <returns>The calculated value of the property being animated.</returns>
        protected override double GetCurrentValueCore(double defaultOriginValue, double defaultDestinationValue, AnimationClock animationClock)
        {
            if (animationClock.CurrentProgress == null) return From;
            if (timer.ElapsedMilliseconds < MillisecondDelay) return lastValue;

            var value = lastValue = MathHelper.Random(From, To);
            timer.Restart();
            return value;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="RandomDoubleAnimation"/> class.
        /// </summary>
        /// <returns>A new instance of <see cref="RandomDoubleAnimation"/>.</returns>
        protected override Freezable CreateInstanceCore()
        {
            return new RandomDoubleAnimation(From, To, Duration);
        }
    }
}
