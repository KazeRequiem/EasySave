using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using EasySave.Models;
using EasySave.ViewModels;
using EasySave.View.Resources;

namespace EasySave.View.Views
{
    public partial class ExecuteView : UserControl
    {
        private MainViewModel viewModel;

        public ExecuteView(MainViewModel sharedModel)
        {
            InitializeComponent();
            this.viewModel = sharedModel;
            CmbJobs.ItemsSource = viewModel.backupJobs;
            CmbJobs.DisplayMemberPath = "name";
        }

        private async void BtnRunOne_Click(object sender, RoutedEventArgs e)
        {
            if (CmbJobs.SelectedItem is BackupJob selectedJob)
            {
                PbProgress.IsIndeterminate = true;
                try
                {
                    await Task.Run(() => viewModel.ExecuteJob(selectedJob.id));
                    MessageBox.Show(Strings.MsgJobDone, Strings.MsgSuccess, MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, Strings.MsgError, MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    PbProgress.IsIndeterminate = false;
                    PbProgress.Value = 100;
                }
            }
            else
            {
                MessageBox.Show(Strings.MsgSelectJob, Strings.MsgWarning, MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async void BtnRunAll_Click(object sender, RoutedEventArgs e)
        {
            if (viewModel.backupJobs == null || viewModel.backupJobs.Count == 0)
            {
                MessageBox.Show(Strings.MsgSelectJob, Strings.MsgWarning, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            PbProgress.IsIndeterminate = true;

            try
            {
                await Task.Run(() =>
                {
                    foreach (var job in viewModel.backupJobs)
                    {
                        viewModel.ExecuteJob(job.id);
                    }
                });

                MessageBox.Show(Strings.MsgJobDone, Strings.MsgSuccess, MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Strings.MsgError, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                PbProgress.IsIndeterminate = false;
                PbProgress.Value = 100;
            }
        }
    }
}