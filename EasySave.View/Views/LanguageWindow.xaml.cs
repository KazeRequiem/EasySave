using System.Globalization;
using System.Threading;
using System.Windows;

namespace EasySave.View.Views
{
    public partial class LanguageWindow : Window
    {
        public LanguageWindow()
        {
            InitializeComponent();
        }

        private void BtnFr_Click(object sender, RoutedEventArgs e)
        {
            ChangeLanguage("fr");
        }

        private void BtnEn_Click(object sender, RoutedEventArgs e)
        {
            ChangeLanguage("en");
        }

        private void ChangeLanguage(string cultureCode)
        {
            var culture = new CultureInfo(cultureCode);
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;


            MainWindow main = new MainWindow();
            main.Show();
            this.Close();
        }
    }
}