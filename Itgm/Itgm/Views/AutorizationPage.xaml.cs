using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Itgm.Views
{
    /// <summary>
    /// Interaction logic for AutorizationPage.xaml
    /// </summary>
    public partial class AutorizationPage : UserControl
    {
        public AutorizationPage()
        {
            InitializeComponent();
        }

        private void UserControl_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (Keyboard.FocusedElement is TextBox)
            {
                Keyboard.ClearFocus();
            }
        }
    }
}
