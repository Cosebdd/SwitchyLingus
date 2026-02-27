using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Shapes;

namespace SwitchyLingus.UI
{
    /// <summary>
    /// Interaction logic for Options.xaml
    /// </summary>
    public partial class Options : Window
    {
        public Options()
        {
            InitializeComponent();
        }
        
        private void OptionsButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn || btn.ContextMenu == null) return;

            btn.ContextMenu.PlacementTarget = btn;
            btn.ContextMenu.Closed += ContextMenuOnClosed;
            btn.IsHitTestVisible = false;
            btn.ContextMenu.IsOpen = true;
        }

        private void ContextMenuOnClosed(object sender, RoutedEventArgs e)
        {
            OptionsButton.IsHitTestVisible = true;
            ((ContextMenu)sender).Closed -= ContextMenuOnClosed;
        }
    }
}
