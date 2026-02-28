using System.Windows;

namespace SwitchyLingus.UI
{
    public partial class ConfirmDialog : Window
    {
        public ConfirmDialog(string message, string title)
        {
            InitializeComponent();
            Title = title;
            MessageText.Text = message;
        }

        private void YesClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
