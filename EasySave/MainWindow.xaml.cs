using System.Windows;
using EasySave.Views; 

namespace EasySave
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            
            ActiveContent.Content = new JobView();
        }

        private void BtnJobs_Click(object sender, RoutedEventArgs e)
        {
            ActiveContent.Content = new JobView();
        }

        private void BtnSettings_Click(object sender, RoutedEventArgs e)
        {
            ActiveContent.Content = new SettingsView();
        }

        private void BtnExecute_Click(object sender, RoutedEventArgs e)
        {

            ActiveContent.Content = new ExecuteView();
        }
    }
}