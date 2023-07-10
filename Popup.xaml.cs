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
using System.Windows.Shapes;

namespace RBLXex_Rework {
    /// <summary>
    /// Interaction logic for Popup.xaml
    /// </summary>
    public partial class Popup : Window {
        private Point lastPoint;

        public Popup(string errorname, string errorbody) {
            InitializeComponent();

            error_title.Content = errorname;
            main_body.Text = errorbody;
        }

        private void accept_button_Click(object sender, RoutedEventArgs e) {
            Close();
        }

        private void top_bar_MouseMove(object sender, MouseEventArgs e) {
            if (e.LeftButton == MouseButtonState.Pressed) {
                Top = Top + (e.GetPosition(top_bar).Y - lastPoint.Y);
                Left = Left + (e.GetPosition(top_bar).X - lastPoint.X);
            }
        }

        private void top_bar_MouseDown(object sender, MouseButtonEventArgs e) {
            lastPoint = new Point(e.GetPosition(top_bar).X, e.GetPosition(top_bar).Y);
        }
    }
}
