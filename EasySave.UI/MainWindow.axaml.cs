using Avalonia.Controls;
using Avalonia.Interactivity;
using EasySave.ViewModels; // Référence à ton projet Console
using EasySave.Models;     // Référence à ton projet Console
using System.Collections.Generic;

namespace EasySave.UI
{
    public partial class MainWindow : Window
    {
        private MainViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();

            _viewModel = new MainViewModel();

            RefreshList();
        }

        private void RefreshList()
        {
            JobsList.ItemsSource = null;
            JobsList.ItemsSource = _viewModel.backupJobs;
        }

        private void OnExecuteClick(object sender, RoutedEventArgs e)
        {
            var selectedJob = JobsList.SelectedItem as BackupJob;
            if (selectedJob != null)
            {
                _viewModel.ExecuteJob(selectedJob.id);
            }
        }

        private void OnCreateClick(object sender, RoutedEventArgs e)
        {
            _viewModel.CreateJob("JobTestAvalonia", @"C:\Source", @"C:\Dest", BackupType.Full);
            RefreshList();
        }
    }
}