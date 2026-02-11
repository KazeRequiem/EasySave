using System.Globalization;
using System.Threading;
using System.Windows;

namespace EasySave.Views
{
    public partial class LanguageWindow : Window
    {
        public LanguageWindow()
        {
            InitializeComponent();
        }

        private void BtnFr_Click(object sender, RoutedEventArgs e)
        {
            ChangeLanguage("fr-FR");
        }

        private void BtnEn_Click(object sender, RoutedEventArgs e)
        {
            ChangeLanguage("en-US");
        }

        private void ChangeLanguage(string cultureCode)
        {
            var culture = new CultureInfo(cultureCode);

            // 1. On change la langue du Thread (pour les formats de date, etc.)
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;

            // 2. IMPORTANT : On force la langue dans ta classe de Ressources
            // C'est ça qui manquait pour que tes textes se mettent à jour !
            EasySave.Resources.Strings.Culture = culture;

            // 3. On lance la fenêtre principale maintenant que la langue est configurée
            MainWindow main = new MainWindow();
            main.Show();

            // 4. On ferme la fenêtre de choix
            this.Close();
        }
    }
}