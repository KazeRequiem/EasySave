using EasySave.Models;
using EasySave.ViewModels;
using EasySave.Views;
using EasySave.Ressources; // Garde ce namespace comme dans tes fichiers précédents
using System;
using System.IO;
using System.Globalization;
using System.Threading;
using System.Linq;

namespace EasySave
{
    class Program
    {
        static void Main(string[] args)
        {
            // =================================================================================
            // 1. CONFIGURATION INITIALE
            // =================================================================================

            string projectPath = AppDomain.CurrentDomain.BaseDirectory;
            string basePath = Path.Combine(projectPath, "EasySave_Tests");
            string sourceDir = Path.Combine(basePath, "Source_Commune");

            // Création des fichiers de test
            if (!Directory.Exists(sourceDir)) PrepareTestFiles(sourceDir);

            // Démarrage du Moteur
            var viewModel = new MainViewModel();

            // =================================================================================
            // 2. CHOIX DE LA LANGUE
            // =================================================================================
            Console.Clear();
            Console.WriteLine("Select your language / Choisissez votre langue :");
            Console.WriteLine("1. English (Default)");
            Console.WriteLine("2. Français");
            Console.Write("\nChoice / Choix : ");

            string langChoice = Console.ReadLine();

            if (langChoice == "2")
            {
                Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("fr-FR");
                Console.WriteLine("✅ Langue : Français 🇫🇷");
            }
            else
            {
                Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("en-US");
                Console.WriteLine("✅ Language : English 🇬🇧");
            }
            Thread.Sleep(800);

            // =================================================================================
            // 3. BOUCLE PRINCIPALE (MENU)
            // =================================================================================

            MainView view = new MainView();
            bool keepRunning = true;

            while (keepRunning)
            {
                view.ShowMenu();
                string input = Console.ReadLine();
                Console.WriteLine();

                switch (input)
                {
                    case "1": // --- CRÉER ---
                        Console.WriteLine(">> " + Strings.MenuOption1);

                        int nextId = viewModel.backupJobs.Count + 1;
                        string jobName = $"Job_Auto_{nextId}";
                        string destDir = Path.Combine(basePath, $"Backup_{jobName}");

                        viewModel.CreateJob(jobName, sourceDir, destDir, BackupType.Full);
                        break;

                    case "2": // --- MODIFIER ---
                        Console.WriteLine(">> " + Strings.MenuOption2);
                        DisplayJobs(viewModel);

                        Console.Write("ID du job à modifier : ");
                        if (int.TryParse(Console.ReadLine(), out int idModif))
                        {
                            var jobToModify = viewModel.backupJobs.FirstOrDefault(j => j.id == idModif);

                            if (jobToModify != null)
                            {
                                Console.WriteLine($"\nModification du job : {jobToModify.name}");
                                Console.WriteLine("(Appuyez sur Entrée pour ne pas changer la valeur)");

                                Console.Write($"Nouveau nom [{jobToModify.name}] : ");
                                string newName = Console.ReadLine();
                                if (string.IsNullOrWhiteSpace(newName)) newName = jobToModify.name;

                                // Utilisation de sourcePath (comme dans ton fichier BackupJob.cs)
                                Console.Write($"Nouvelle Source : ");
                                string newSource = Console.ReadLine();
                                if (string.IsNullOrWhiteSpace(newSource)) newSource = jobToModify.sourcePath;

                                // Utilisation de destinationPath (comme dans ton fichier BackupJob.cs)
                                Console.Write($"Nouvelle Destination : ");
                                string newDest = Console.ReadLine();
                                if (string.IsNullOrWhiteSpace(newDest)) newDest = jobToModify.destinationPath;

                                viewModel.ModifyJob(idModif, newName, newSource, newDest, jobToModify.type);
                            }
                            else
                            {
                                Console.WriteLine("❌ ID introuvable.");
                            }
                        }
                        else
                        {
                            Console.WriteLine("❌ ID invalide.");
                        }
                        break;

                    case "3": // --- SUPPRIMER (C'est ici que j'ai corrigé) ---
                        Console.WriteLine(">> " + Strings.MenuOption3);

                        DisplayJobs(viewModel);

                        Console.Write("ID du job à supprimer : ");
                        if (int.TryParse(Console.ReadLine(), out int idDel))
                        {
                            // J'appelle la vraie fonction DeleteJob du ViewModel
                            viewModel.DeleteJob(idDel);
                        }
                        else
                        {
                            Console.WriteLine("❌ ID invalide.");
                        }
                        break;

                    case "4": // --- LISTER ---
                        Console.WriteLine(">> " + Strings.MenuOption4);
                        DisplayJobs(viewModel);
                        break;

                    case "5": // --- EXÉCUTER ---
                        Console.WriteLine(">> " + Strings.MenuOption5);
                        DisplayJobs(viewModel);

                        Console.Write("Entrez l'ID du travail à exécuter : ");
                        if (int.TryParse(Console.ReadLine(), out int idRun))
                        {
                            viewModel.ExecuteJob(idRun);
                        }
                        else
                        {
                            Console.WriteLine("❌ ID invalide.");
                        }
                        break;

                    case "6": // --- QUITTER ---
                        Console.WriteLine(Strings.MenuExit);
                        keepRunning = false;
                        break;

                    default:
                        Console.WriteLine("❌ Choix invalide.");
                        break;
                }

                if (keepRunning)
                {
                    Console.WriteLine("\nAppuyez sur une touche pour revenir au menu...");
                    Console.ReadKey();
                }
            }
        }

        static void DisplayJobs(MainViewModel vm)
        {
            if (vm.backupJobs.Count == 0)
            {
                Console.WriteLine("Aucun job enregistré.");
            }
            else
            {
                Console.WriteLine("------------------------------------------------");
                Console.WriteLine("ID | Nom             | Source -> Destination");
                Console.WriteLine("------------------------------------------------");
                foreach (var job in vm.backupJobs)
                {
                    // Utilisation de sourcePath et destinationPath pour correspondre à ton modèle
                    Console.WriteLine($"{job.id}  | {job.name,-15} | {job.sourcePath} -> {job.destinationPath}");
                }
                Console.WriteLine("------------------------------------------------");
            }
        }

        static void PrepareTestFiles(string source)
        {
            try
            {
                Directory.CreateDirectory(source);
                File.WriteAllText(Path.Combine(source, "Note.txt"), "Ceci est un test.");
                File.WriteAllBytes(Path.Combine(source, "Presentation.pptx"), new byte[100]);
            }
            catch { }
        }
    }
}