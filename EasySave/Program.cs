using EasySave.ViewModels;
using EasySave.Views;
using System;
using System.Globalization;
using System.Threading;

namespace EasySave
{
    class Program
    {
        static void Main(string[] args)
        {
            var viewModel = new MainViewModel();

            // 2. Instanciation de la Vue (en lui passant le ViewModel)
            var view = new MainView(viewModel);

            SetupLanguage();

            view.Start();
        }

        static void SetupLanguage()
        {
            Console.Clear();
            Console.WriteLine("1. English (Default)");
            Console.WriteLine("2. Français");
            Console.Write(" (en/fr) : ");

            string choice = Console.ReadLine();
            if (choice == "fr")
            {
                Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("fr-FR");
            }
            else
            {
                Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("en-US");
            }
        }
    }
}