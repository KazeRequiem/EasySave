using EasySave.ViewModels;
using EasySave.Views;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace EasySave
{
    class Program
    {
        static void Main(string[] args)
        {
            var viewModel = new MainViewModel();
            if (args.Length > 0)
            {
                string command = string.Join("", args);

                List<int> jobsToRun = ParseArguments(command);

                Console.WriteLine($"[CLI] Exécution automatique de {jobsToRun.Count} job(s)...");

                foreach (int id in jobsToRun)
                {
                    viewModel.ExecuteJob(id);
                }

                Console.WriteLine("[CLI] Exécution terminée.");
                return;
            }


            var view = new MainView(viewModel);

            SetupLanguage();

            view.Start();
        }

        static List<int> ParseArguments(string arg)
        {
            var ids = new List<int>();

            if (arg.Contains('-'))
            {
                var parts = arg.Split('-');
                if (parts.Length == 2 && int.TryParse(parts[0], out int start) && int.TryParse(parts[1], out int end))
                {
                    for (int i = start; i <= end; i++) ids.Add(i);
                }
            }
            else if (arg.Contains(';'))
            {
                var parts = arg.Split(';');
                foreach (var part in parts)
                {
                    if (int.TryParse(part, out int id)) ids.Add(id);
                }
            }
            else if (int.TryParse(arg, out int singleId))
            {
                ids.Add(singleId);
            }

            return ids.Distinct().OrderBy(x => x).ToList();
        }

        static void SetupLanguage()
        {
            Console.Clear();
            Console.WriteLine("1. English (Default)");
            Console.WriteLine("2. Français");
            Console.Write("Language (en/fr) : ");

            string choice = Console.ReadLine()?.ToLower();

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