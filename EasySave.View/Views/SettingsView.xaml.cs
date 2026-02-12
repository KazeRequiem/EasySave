using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using EasySave.ViewModels;
using EasySave.View.Resources;
using EasyLog;

namespace EasySave.View.Views
{
    public partial class SettingsView : UserControl
    {
        private MainViewModel viewModel;
        public SettingsView(MainViewModel sharedModel)
        {
            InitializeComponent();
            this.viewModel = sharedModel;
            ChargerInterface();
        }
        private void ChargerInterface()
        {
            try
            {
                var settings = viewModel.CurrentSettings;

                if (settings != null)
                {
                    TxtBusinessSoft.Text = settings.applicationSoftware;
                    TxtCryptoPath.Text = settings.cryptoSoftPath;
                    TxtCryptoKey.Text = settings.cryptoKey;

                    CmbLogType.SelectedIndex = (settings.logType == LogFormat.Json) ? 0 : 1;

                    if (settings.extensionsToEncrypt != null)
                    {
                        TxtExtensions.Text = string.Join(", ", settings.extensionsToEncrypt);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Strings.MsgError, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnBrowse_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Executables (*.exe)|*.exe|All files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                TxtCryptoPath.Text = openFileDialog.FileName;
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                viewModel.UpdateApplicationSoftware(TxtBusinessSoft.Text);
                viewModel.UpdateCryptPath(TxtCryptoPath.Text);
                viewModel.UpdateCryptKey(TxtCryptoKey.Text);
                string selectedLog = (CmbLogType.SelectedIndex == 0) ? "json" : "xml";
                viewModel.UpdateLogType(selectedLog);
                var extensions = TxtExtensions.Text.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var ext in extensions)
                {
                    viewModel.AddEncryptionExtension(ext.Trim());
                }
                MessageBox.Show(Strings.MsgParamsSaved, Strings.MsgSuccess, MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Strings.MsgError, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}