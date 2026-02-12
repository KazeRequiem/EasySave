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
            this.Loaded += ExecuteView_Loaded;
        }

        private void ExecuteView_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                CmbJobs.ItemsSource = null;
                CmbJobs.ItemsSource = viewModel.backupJobs;
                CmbJobs.DisplayMemberPath = "name";
            }
            catch (Exception ex)    
            {
                MessageBox.Show("Erreur chargement liste : " + ex.Message);
            }
        }

        private async void BtnRunOne_Click(object sender, RoutedEventArgs e)
        {
            if (CmbJobs.SelectedItem is BackupJob selectedJob)
            {
                PbProgress.IsIndeterminate = true;
                PbProgress.Value = 0;

                try
                {
                    await Task.Run(() => viewModel.ExecuteJob(selectedJob.id));

                    MessageBox.Show(Strings.MsgJobDone, Strings.MsgSuccess, MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (ArgumentException ex) when (string.Equals(ex.Message, "Other process detected while running.", StringComparison.Ordinal))
                {
                    MessageBox.Show(
                        "Impossible de lancer la sauvegarde : Le logiciel métier est en cours d'exécution.\nVeuillez le fermer et réessayer.",
                        Strings.MsgWarning,
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning
                    );
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Une erreur est survenue lors de l'exécution :\n{ex.Message}",
                        Strings.MsgError,
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                    );
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
            catch (ArgumentException ex) when (ex.Message.Contains("Other process detected"))
            {
                MessageBox.Show("Sauvegarde interrompue : Logiciel métier détecté.", Strings.MsgWarning, MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'exécution globale :\n{ex.Message}", Strings.MsgError, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                PbProgress.IsIndeterminate = false;
                PbProgress.Value = 100;
            }
        }
    }
}