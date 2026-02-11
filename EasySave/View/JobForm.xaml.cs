using EasySave.Models;
using System.IO; // Indispensable pour vérifier si le dossier existe
using System.Windows;

namespace EasySave.Views
{
    public partial class JobForm : Window
    {
        // Propriétés pour récupérer les données saisies
        public string JobName => TxtName.Text;
        public string JobSource => TxtSource.Text;
        public string JobDest => TxtDest.Text;
        public BackupType JobType => CmbType.SelectedIndex == 0 ? BackupType.Full : BackupType.Differential;

        public JobForm()
        {
            InitializeComponent();
        }

        
        public JobForm(BackupJob job) : this()
        {
            TxtName.Text = job.name;
            TxtSource.Text = job.sourcePath;
            TxtDest.Text = job.destinationPath;
            CmbType.SelectedIndex = (job.type == BackupType.Full) ? 0 : 1;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            
            if (string.IsNullOrWhiteSpace(TxtName.Text) ||
                string.IsNullOrWhiteSpace(TxtSource.Text) ||
                string.IsNullOrWhiteSpace(TxtDest.Text))
            {
                MessageBox.Show("Veuillez remplir tous les champs !", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                return; 
            }

            
            if (!Directory.Exists(TxtSource.Text))
            {
                MessageBox.Show($"Le dossier Source n'existe pas :\n{TxtSource.Text}\n\nVeuillez vérifier le chemin.", "Dossier introuvable", MessageBoxButton.OK, MessageBoxImage.Error);
                return; 
            }

            
            DialogResult = true;
        }
    }
}