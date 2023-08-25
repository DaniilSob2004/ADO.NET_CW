using System;
using System.Windows;

namespace first.Views
{
    public partial class StartWindow : Window
    {
        public StartWindow()
        {
            InitializeComponent();
        }

        private void IntroButton_Click(object sender, RoutedEventArgs e)
        {
            Hide();
            new MainWindow().ShowDialog();
            Show();
        }
    }
}
