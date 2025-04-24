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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PinkPact
{
    /// <summary>
    /// Interaction logic for Title.xaml
    /// </summary>
    public partial class Title : UserControl
    {
        public Title()
        {
            InitializeComponent();

            var bounceAnim = new DoubleAnimation(0, +25, TimeSpan.FromSeconds(5));
            bounceAnim.EasingFunction = new SineEase() { EasingMode = EasingMode.EaseInOut };
            bounceAnim.RepeatBehavior = RepeatBehavior.Forever;
            bounceAnim.AutoReverse = true;

            Timeline.SetDesiredFrameRate(bounceAnim, 15);

            (logo.RenderTransform as TranslateTransform).BeginAnimation(TranslateTransform.YProperty, bounceAnim);
        }
    }
}
