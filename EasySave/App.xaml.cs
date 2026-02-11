using EasySave.Views; 
using System.Windows;

namespace EasySave
{
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            
            LanguageWindow langWindow = new LanguageWindow();
            langWindow.Show();
        }
    }
}