using EasySave.ViewModels;
using EasySave.Views;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace EasySave
{
    /// <summary>
    /// Main application entry class for EasySave.
    /// 
    /// This class is responsible for:
    /// - launching the application,
    /// - handling command-line execution of backup jobs,
    /// - starting the interactive user interface,
    /// - configuring the application language.
    /// </summary>
    class Program
    {
        /// <summary>
        /// Main entry point of the application.
        /// 
        /// If command-line arguments are provided, the application runs in CLI mode
        /// and executes the selected jobs automatically.
        /// 
        /// Otherwise, the interactive user interface is started after
        /// setting up the application language.
        /// </summary>
        static void Main(string[] args)
        {
            var viewModel = new MainViewModel();
            if (args.Length > 0)
            {
                string command = string.Join("", args);
                List<int> jobsToRun = ParseArguments(command);
                Console.WriteLine($"[CLI] Automatic execution of {jobsToRun.Count} job(s)...");
                foreach (int id in jobsToRun)
                {
                    viewModel.ExecuteJob(id);
                }
                Console.WriteLine("[CLI] Execution completed.");
                return;
            }
            var view = new MainView(viewModel);
            SetupLanguage();
            view.Start();
        }

        /// <summary>
        /// Parses the command-line arguments to extract
        /// the backup job identifiers to execute.
        /// 
        /// Supported formats:
        /// - range (e.g. "1-5"),
        /// - list (e.g. "1;3;7"),
        /// - single identifier.
        /// </summary>
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

        /// <summary>
        /// Configures the application user interface language
        /// based on the user's choice.
        /// 
        /// The current UI culture is updated accordingly.
        /// </summary>
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
