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
    /// <summary>
    /// Interaction logic for PinkErrorWindow.xaml
    /// </summary>
    public partial class PinkErrorWindow : UserControl
    {
        public ImageButton Button => okBtn;

        public PinkErrorWindow()
        {
            InitializeComponent();
        }
    }
}
