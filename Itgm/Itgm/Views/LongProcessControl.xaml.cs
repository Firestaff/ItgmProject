using System.Windows;
using System.Windows.Controls;

namespace Itgm.Views
{
    /// <summary>
    /// Interaction logic for LongProcessControl.xaml
    /// </summary>
    public partial class LongProcessControl : UserControl
    {
        public static readonly DependencyProperty IsColorInvertedProperty =
            DependencyProperty.Register(
                "IsColorInverted", 
                typeof(bool), 
                typeof(LongProcessControl), 
                new FrameworkPropertyMetadata(false));

        public bool IsColorInverted
        {
            get { return (bool)GetValue(IsColorInvertedProperty); }
            set { SetValue(IsColorInvertedProperty, value); }
        }

        public LongProcessControl()
        {
            InitializeComponent();
        }
    }
}
