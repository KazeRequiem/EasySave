using EasySave.Models;
using EasySave.View.Resources;
using System.Windows;

namespace EasySave.View.Views
{
    public partial class JobForm : Window
    {
        public string JobName => TxtName.Text;
        public string SourcePath => TxtSource.Text;
        public string DestPath => TxtDest.Text;
        public BackupType SelectedType => CmbType.SelectedIndex == 0 ? BackupType.Full : BackupType.Differential;

        public JobForm(string name = "", string source = "", string dest = "", BackupType type = BackupType.Full)
        {
            InitializeComponent();
            TxtName.Text = name;
            TxtSource.Text = source;
            TxtDest.Text = dest;
            CmbType.SelectedIndex = (type == BackupType.Full) ? 0 : 1;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtName.Text) || string.IsNullOrWhiteSpace(TxtSource.Text) || string.IsNullOrWhiteSpace(TxtDest.Text))
            {
                MessageBox.Show(Strings.MsgEmptyFields, Strings.MsgWarning, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            this.DialogResult = true;
        }
    }
}