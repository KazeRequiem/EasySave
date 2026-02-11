using EasySave.Models;
using EasySave.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace EasySave.Views
{
    public partial class JobView : UserControl
    {
        private MainViewModel _vm;

        public JobView()
        {
            InitializeComponent();
            _vm = new MainViewModel();

            LoadJobs();
        }

        private void LoadJobs()
        {
            
            DgdJobs.ItemsSource = null;
            DgdJobs.ItemsSource = _vm.backupJobs;
        }

       
        private void BtnCreate_Click(object sender, RoutedEventArgs e)
        {
            JobForm form = new JobForm();

            
            if (form.ShowDialog() == true)
            {
                try
                {
                   
                    _vm.CreateJob(form.JobName, form.JobSource, form.JobDest, form.JobType);

                    
                    LoadJobs();
                }
                catch (Exception ex)
                {
                    
                    MessageBox.Show($"Une erreur s'est produite lors de la sauvegarde :\n{ex.Message}", "Erreur Critique", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        
        private void BtnModify_Click(object sender, RoutedEventArgs e)
        {
            if (DgdJobs.SelectedItem is BackupJob selectedJob)
            {
                JobForm form = new JobForm(selectedJob); 
                if (form.ShowDialog() == true)
                {
                    _vm.ModifyJob(selectedJob.id, form.JobName, form.JobSource, form.JobDest, form.JobType);
                    LoadJobs();
                }
            }
            else
            {
                MessageBox.Show("Veuillez sélectionner un travail dans la liste.", "Attention");
            }
        }

        
        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (DgdJobs.SelectedItem is BackupJob selectedJob)
            {
                var result = MessageBox.Show($"Voulez-vous vraiment supprimer '{selectedJob.name}' ?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    _vm.DeleteJob(selectedJob.id);
                    LoadJobs();
                }
            }
            else
            {
                MessageBox.Show("Veuillez sélectionner un travail à supprimer.", "Attention");
            }
        }
    }
}