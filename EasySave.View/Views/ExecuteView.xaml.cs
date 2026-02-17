using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using EasySave.Models;
using EasySave.ViewModels;
using EasySave.View.Resources;

namespace EasySave.View.Views
{
    public partial class ExecuteView : UserControl
    {
        private MainViewModel viewModel;
        private DispatcherTimer progressTimer;

        public ExecuteView(MainViewModel sharedModel)
        {
            InitializeComponent();
            this.viewModel = sharedModel;

            CmbJobs.ItemsSource = viewModel.backupJobs;
            CmbJobs.DisplayMemberPath = "name";

            progressTimer = new DispatcherTimer();
            progressTimer.Interval = TimeSpan.FromMilliseconds(100);
            progressTimer.Tick += ProgressTimer_Tick;

            this.Loaded += ExecuteView_Loaded;
        }

        private void ExecuteView_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                PbProgress.Value = 0;
                isRunningAll = false;
                progressTimer.Stop();
                
                CmbJobs.ItemsSource = null;
                CmbJobs.ItemsSource = viewModel.backupJobs;
                CmbJobs.DisplayMemberPath = "name";
            }
            catch (Exception ex)
            {
                MessageBox.Show(Strings.LoadingError + ex.Message);
            }
        }

        private bool isRunningAll = false;
        private void ProgressTimer_Tick(object sender, EventArgs e)
        {
            if (isRunningAll)
            {
                PbProgress.Value = viewModel.GetGlobalProgress();
            }
            else if (CmbJobs.SelectedItem is BackupJob selectedJob)
            {
                var states = viewModel.GetCurrentStates();
                var jobStatus = states.FirstOrDefault(s => s.name == selectedJob.name);
                if (jobStatus != null)
                {
                    PbProgress.Value = jobStatus.progression;
                }
            }
        }

        private async void BtnRunOne_Click(object sender, RoutedEventArgs e)
        {
            if (CmbJobs.SelectedItem is BackupJob selectedJob)
            {
                PbProgress.IsIndeterminate = false;
                PbProgress.Value = 0;

                progressTimer.Start();

                try
                {
                    await viewModel.ExecuteJob(selectedJob.id);
                }
                catch (ArgumentException ex) when (string.Equals(ex.Message, "Other process detected while running.", StringComparison.Ordinal))
                {
                    MessageBox.Show(Strings.BusinessSoftwareError, Strings.MsgWarning, MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(Strings.ExecutionError + ex, Strings.MsgError, MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    await Task.Delay(200);
                    progressTimer.Stop();
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

            isRunningAll = true;
            PbProgress.Value = 0;
            progressTimer.Start();

            try
            {
                foreach (var job in viewModel.backupJobs)
                {
                    await viewModel.ExecuteJob(job.id);
                }
            }
            catch (ArgumentException ex) when (ex.Message.Contains("Other process detected"))
            {
                MessageBox.Show(Strings.BusinessSoftwareDetectedError, Strings.MsgWarning, MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show(Strings.ExecutionGlobalError + ex.Message, Strings.MsgError, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                await Task.Delay(200);
                progressTimer.Stop();
                PbProgress.Value = 100;
                isRunningAll = false;
            }
        }
    }
}