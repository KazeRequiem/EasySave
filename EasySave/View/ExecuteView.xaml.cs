using EasySave.Models;
using EasySave.ViewModels;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace EasySave.Views
{
    public partial class ExecuteView : UserControl
    {
        private MainViewModel _vm;

        public ExecuteView()
        {
            InitializeComponent();
            _vm = new MainViewModel();

            // On remplit la liste déroulante avec tes travaux existants
            CmbJobs.ItemsSource = _vm.backupJobs;
        }

        // --- CAS 1 : Lancer UN SEUL travail ---
        private async void BtnRunOne_Click(object sender, RoutedEventArgs e)
        {
            if (CmbJobs.SelectedItem is BackupJob selectedJob)
            {
                // On prépare la barre de chargement
                PbProgress.IsIndeterminate = true;
                TxtStatus.Text = $"⏳ Exécution de '{selectedJob.name}' en cours...";

                // On lance le travail en arrière-plan (await Task.Run) pour ne pas figer l'écran
                await Task.Run(() => _vm.ExecuteJob(selectedJob.id));

                // C'est fini !
                PbProgress.IsIndeterminate = false;
                PbProgress.Value = 100;
                TxtStatus.Text = $"✅ Le travail '{selectedJob.name}' est terminé.";
                MessageBox.Show("Sauvegarde terminée avec succès !", "Succès");
            }
            else
            {
                MessageBox.Show("Veuillez sélectionner un travail dans la liste.", "Attention");
            }
        }

        // --- CAS 2 : Lancer TOUS les travaux ---
        private async void BtnRunAll_Click(object sender, RoutedEventArgs e)
        {
            if (_vm.backupJobs.Count == 0)
            {
                MessageBox.Show("Aucun travail à exécuter.");
                return;
            }

            TxtStatus.Text = "🚀 Lancement de tous les travaux...";
            PbProgress.IsIndeterminate = false;
            PbProgress.Value = 0;
            PbProgress.Maximum = _vm.backupJobs.Count; // La barre ira de 0 au nombre total de jobs

            foreach (var job in _vm.backupJobs)
            {
                TxtStatus.Text = $"⏳ Exécution de '{job.name}'...";

                // On attend que celui-ci finisse avant de passer au suivant
                await Task.Run(() => _vm.ExecuteJob(job.id));

                // On avance la barre de 1
                PbProgress.Value += 1;
            }

            TxtStatus.Text = "✅ Tous les travaux sont terminés !";
            MessageBox.Show("Exécution globale terminée.", "Terminé");
        }
    }
}