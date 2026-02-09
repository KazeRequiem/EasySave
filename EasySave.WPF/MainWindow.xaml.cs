using EasySave.WPF.Models;
using EasySave.WPF.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EasySave.WPF
{
    public partial class MainWindow : Window
    {
        private MainViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();
            _viewModel = new MainViewModel();
            this.DataContext = _viewModel;
        }

        private void JobsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (JobsDataGrid.SelectedItem is BackupJob selectedJob)
            {
                txtName.Text = selectedJob.name;
                txtSource.Text = selectedJob.sourcePath;
                txtDest.Text = selectedJob.destinationPath;
                cmbType.SelectedIndex = (selectedJob.type == BackupType.Full) ? 0 : 1;
            }
        }

        private void CreateJob_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string name = txtName.Text;
                string source = txtSource.Text;
                string dest = txtDest.Text;
                BackupType type = (cmbType.SelectedIndex == 0) ? BackupType.Full : BackupType.Differential;

                _viewModel.CreateJob(name, source, dest, type);
                ClearFields();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message, "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ModifyJob_Click(object sender, RoutedEventArgs e)
        {
            if (JobsDataGrid.SelectedItem is BackupJob selectedJob)
            {
                try
                {
                    string name = txtName.Text;
                    string source = txtSource.Text;
                    string dest = txtDest.Text;
                    BackupType type = (cmbType.SelectedIndex == 0) ? BackupType.Full : BackupType.Differential;

                    _viewModel.ModifyJob(selectedJob.id, name, source, dest, type);
                    System.Windows.MessageBox.Show("Modifié avec succès !");
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show(ex.Message, "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void DeleteJob_Click(object sender, RoutedEventArgs e)
        {
            if (JobsDataGrid.SelectedItem is BackupJob selectedJob)
            {
                if (System.Windows.MessageBox.Show($"Supprimer '{selectedJob.name}' ?", "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    _viewModel.DeleteJob(selectedJob.id);
                    ClearFields();
                }
            }
        }

        private void ExecuteJob_Click(object sender, RoutedEventArgs e)
        {
            if (JobsDataGrid.SelectedItem is BackupJob selectedJob)
            {
                Task.Run(() =>
                {
                    try
                    {
                        _viewModel.ExecuteJob(selectedJob.id);
                        Dispatcher.Invoke(() => System.Windows.MessageBox.Show($"Job {selectedJob.name} terminé !"));
                    }
                    catch (Exception ex)
                    {
                        Dispatcher.Invoke(() => System.Windows.MessageBox.Show("Erreur: " + ex.Message));
                    }
                });
            }
        }

        // Fonctions vides pour Pause/Stop (pour calmer le compilateur)
        private void PauseJob_Click(object sender, RoutedEventArgs e) { System.Windows.MessageBox.Show("Fonction Pause à venir"); }
        private void StopJob_Click(object sender, RoutedEventArgs e) { System.Windows.MessageBox.Show("Fonction Stop à venir"); }

        private void BrowseSource_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK) txtSource.Text = dialog.SelectedPath;
            }
        }

        private void BrowseDest_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK) txtDest.Text = dialog.SelectedPath;
            }
        }

        private void ClearFields_Click(object sender, RoutedEventArgs e) { ClearFields(); }

        private void ClearFields()
        {
            txtName.Text = "";
            txtSource.Text = "";
            txtDest.Text = "";
            cmbType.SelectedIndex = 0;
            JobsDataGrid.SelectedItem = null;
        }
    }
}