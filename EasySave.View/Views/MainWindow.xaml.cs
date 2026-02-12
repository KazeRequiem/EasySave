using EasySave.ViewModels;
using System.Windows;

namespace EasySave.View.Views
{
    public partial class MainWindow : Window
    {
        private MainViewModel sharedViewModel = new MainViewModel();
        private JobView jobView;
        private SettingsView settingsView;
        private ExecuteView executeView;
        public MainWindow()
        {
            InitializeComponent();
            jobView = new JobView(sharedViewModel);
            settingsView = new SettingsView(sharedViewModel);
            executeView = new ExecuteView(sharedViewModel);
            ActiveContent.Content = jobView;
        }

        private void BtnJobs_Click(object sender, RoutedEventArgs e)
        {
            ActiveContent.Content = jobView;
        }

        private void BtnExecute_Click(object sender, RoutedEventArgs e)
        {
            ActiveContent.Content = executeView;
        }

        private void BtnSettings_Click(object sender, RoutedEventArgs e)
        {
            ActiveContent.Content = settingsView;
        }
    }
}