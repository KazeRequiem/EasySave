using System;
using System.Windows;
using System.Windows.Controls;
using EasySave.Models;
using EasySave.ViewModels;
using EasySave.View.Resources;

namespace EasySave.View.Views
{
    public partial class JobView : UserControl
    {
        private MainViewModel viewModel;

        public JobView(MainViewModel sharedModel)
        {
            InitializeComponent();
            this.viewModel = sharedModel;
            ChargerDonnees();
        }

        private void ChargerDonnees()
        {
            try
            {
                DgdJobs.ItemsSource = null;
                DgdJobs.ItemsSource = viewModel.backupJobs;
            }
            catch (Exception ex)
            {
                MessageBox.Show(Strings.DisplayError + ex.Message);
            }
        }

        private void BtnCreate_Click(object sender, RoutedEventArgs e)
        {
            var form = new JobForm();
            if (form.ShowDialog() == true)
            {
                try
                {
                    viewModel.CreateJob(form.JobName, form.SourcePath, form.DestPath, form.SelectedType);
                    ChargerDonnees();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(Strings.CreateJobError + ex.Message, Strings.MsgError, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnModify_Click(object sender, RoutedEventArgs e)
        {
            if (DgdJobs.SelectedItem is BackupJob selectedJob)
            {
                var form = new JobForm(selectedJob.name, selectedJob.sourcePath, selectedJob.destinationPath, selectedJob.type);
                if (form.ShowDialog() == true)
                {
                    try
                    {
                        viewModel.ModifyJob(selectedJob.id, form.JobName, form.SourcePath, form.DestPath, form.SelectedType);
                        ChargerDonnees();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(Strings.EditJobError + ex.Message, Strings.MsgError, MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show(Strings.MsgSelectJob, Strings.MsgWarning, MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (DgdJobs.SelectedItem is BackupJob selectedJob)
            {
                var rep = MessageBox.Show(Strings.MsgConfirmDelete, Strings.MsgConfirmation, MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (rep == MessageBoxResult.Yes)
                {
                    try
                    {
                        viewModel.DeleteJob(selectedJob.id);
                        ChargerDonnees();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(Strings.DeleteJobError + ex.Message, Strings.MsgError, MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show(Strings.MsgSelectJob, Strings.MsgWarning, MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}