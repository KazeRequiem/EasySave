using EasySave.Models;
using EasySave.ViewModels;
using EasySave.Ressources;
using System;
using System.Linq;

namespace EasySave.Views
{
    public class MainView
    {
        private MainViewModel viewModel;

        public MainView(MainViewModel viewModel)
        {
            viewModel = viewModel;
        }

        private int ShowInteractiveMenu(string title, string[] options)
        {
            int selectedIndex = 0;
            ConsoleKey key;

            do
            {
                Console.Clear();
                Console.WriteLine($"=== {title} ===\n");

                for (int i = 0; i < options.Length; i++)
                {
                    if (i == selectedIndex)
                    {
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine($"> {options[i]} <");
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.WriteLine($"  {options[i]}");
                    }
                }

                Console.WriteLine("\n(Utilisez les flèches Haut/Bas pour naviguer et Entrée pour valider)");

                key = Console.ReadKey(true).Key;

                if (key == ConsoleKey.UpArrow)
                {
                    selectedIndex = (selectedIndex == 0) ? options.Length - 1 : selectedIndex - 1;
                }
                else if (key == ConsoleKey.DownArrow)
                {
                    selectedIndex = (selectedIndex == options.Length - 1) ? 0 : selectedIndex + 1;
                }

            } while (key != ConsoleKey.Enter);

            return selectedIndex;
        }

        public void Start()
        {
            string[] mainOptions = {
                Strings.MenuOption1, // Create
                Strings.MenuOption2, // Modify
                Strings.MenuOption3, // Delete
                Strings.MenuOption4, // List
                Strings.MenuOption5, // Execute
                Strings.MenuExit     // Exit
            };

            bool keepRunning = true;

            while (keepRunning)
            {
                int choice = ShowInteractiveMenu("EASYSAVE - MAIN MENU", mainOptions);
                Console.Clear();

                switch (choice)
                {
                    case 0: CreateJobView(); break;
                    case 1: ModifyJobView(); break;
                    case 2: DeleteJobView(); break;
                    case 3: ListJobsView(); break;
                    case 4: ExecuteJobView(); break;
                    case 5: keepRunning = false; break;
                }

                if (keepRunning)
                {
                    Console.WriteLine("\nAppuyez sur une touche pour revenir au menu...");
                    Console.ReadKey(true);
                }
            }
        }

        // --- 1. CRÉATION (CORRIGÉE AVEC INPUTS) ---
        private void CreateJobView()
        {
            Console.WriteLine("--- CRÉER UN NOUVEAU TRAVAIL ---");

            Console.Write("Nom du travail : ");
            string name = Console.ReadLine();

            Console.Write("Chemin Source (ex: C:\\DossierA) : ");
            string source = Console.ReadLine();

            Console.Write("Chemin Destination (ex: D:\\DossierB) : ");
            string dest = Console.ReadLine();

            // Sous-menu interactif pour le type de sauvegarde
            string[] typeOptions = { "Complète (Full)", "Différentielle (Differential)" };
            int typeChoice = ShowInteractiveMenu("Choisissez le type de sauvegarde :", typeOptions);
            BackupType type = (typeChoice == 0) ? BackupType.Full : BackupType.Differential;

            Console.Clear();
            viewModel.CreateJob(name, source, dest, type);
        }

        private void ModifyJobView()
        {
            ListJobsView();
            Console.Write("\nID du travail à modifier : ");
            if (int.TryParse(Console.ReadLine(), out int id))
            {
                var job = viewModel.backupJobs.FirstOrDefault(j => j.id == id);
                if (job != null)
                {
                    Console.WriteLine($"Modification de : {job.name} (Laissez vide pour ne pas changer)");

                    Console.Write($"Nouveau nom [{job.name}] : ");
                    string newName = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(newName)) newName = job.name;

                    Console.Write($"Nouvelle Source [{job.sourcePath}] : ");
                    string newSource = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(newSource)) newSource = job.sourcePath;

                    Console.Write($"Nouvelle Destination [{job.destinationPath}] : ");
                    string newDest = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(newDest)) newDest = job.destinationPath;

                    // Sous-menu pour changer le type
                    string[] typeOptions = { "Complète (Full)", "Différentielle (Differential)" };
                    int typeChoice = ShowInteractiveMenu($"Nouveau type [Actuel: {job.type}] :", typeOptions);
                    BackupType newType = (typeChoice == 0) ? BackupType.Full : BackupType.Differential;

                    Console.Clear();
                    viewModel.ModifyJob(id, newName, newSource, newDest, newType);
                }
                else Console.WriteLine("❌ ID introuvable.");
            }
        }

        private void DeleteJobView()
        {
            ListJobsView();
            Console.Write("\nID du travail à supprimer : ");
            if (int.TryParse(Console.ReadLine(), out int id))
            {
                viewModel.DeleteJob(id);
            }
        }

        private void ListJobsView()
        {
            Console.WriteLine("--- LISTE DES TRAVAUX ---");
            if (viewModel.backupJobs.Count == 0)
            {
                Console.WriteLine("Aucun travail enregistré.");
                return;
            }

            Console.WriteLine("------------------------------------------------");
            Console.WriteLine("ID | Nom             | Type    | Source -> Destination");
            Console.WriteLine("------------------------------------------------");
            foreach (var job in viewModel.backupJobs)
            {
                Console.WriteLine($"{job.id}  | {job.name,-15} | {job.type,-7} | {job.sourcePath} -> {job.destinationPath}");
            }
            Console.WriteLine("------------------------------------------------");
        }

        private void ExecuteJobView()
        {
            ListJobsView();
            Console.Write("\nEntrez l'ID du travail à exécuter : ");
            if (int.TryParse(Console.ReadLine(), out int id))
            {
                viewModel.ExecuteJob(id);
            }
        }
    }
}