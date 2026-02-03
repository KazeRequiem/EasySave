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
            this.viewModel = viewModel;
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

                Console.WriteLine($"\n({Strings.NavigationHelp})");

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
                int choice = ShowInteractiveMenu(Strings.MainMenuTitle, mainOptions);
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
                    Console.WriteLine($"\n{Strings.PressAnyKey}");
                    Console.ReadKey(true);
                }
            }
        }

        private void CreateJobView()
        {
            Console.WriteLine($"--- {Strings.TitleCreate} ---");

            Console.Write($"{Strings.PromptJobName} ");
            string name = Console.ReadLine();

            Console.Write($"{Strings.PromptSource} ");
            string source = Console.ReadLine();

            Console.Write($"{Strings.PromptDest} ");
            string dest = Console.ReadLine();

            string[] typeOptions = { Strings.TypeFull, Strings.TypeDiff };
            int typeChoice = ShowInteractiveMenu(Strings.PromptType, typeOptions);
            BackupType type = (typeChoice == 0) ? BackupType.Full : BackupType.Differential;

            Console.Clear();
            viewModel.CreateJob(name, source, dest, type);
        }

        private void ModifyJobView()
        {
            ListJobsView();
            Console.Write($"\n{Strings.PromptModifyId} ");
            if (int.TryParse(Console.ReadLine(), out int id))
            {
                var job = viewModel.backupJobs.FirstOrDefault(j => j.id == id);
                if (job != null)
                {
                    Console.WriteLine(string.Format(Strings.TitleModify, job.name));

                    Console.Write(string.Format(Strings.PromptNewName, job.name));
                    string newName = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(newName)) newName = job.name;

                    Console.Write(string.Format(Strings.PromptNewSource, job.sourcePath));
                    string newSource = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(newSource)) newSource = job.sourcePath;

                    Console.Write(string.Format(Strings.PromptNewDest, job.destinationPath));
                    string newDest = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(newDest)) newDest = job.destinationPath;

                    string[] typeOptions = { Strings.TypeFull, Strings.TypeDiff };
                    int typeChoice = ShowInteractiveMenu(string.Format(Strings.PromptNewType, job.type), typeOptions);
                    BackupType newType = (typeChoice == 0) ? BackupType.Full : BackupType.Differential;

                    Console.Clear();
                    viewModel.ModifyJob(id, newName, newSource, newDest, newType);
                }
                else Console.WriteLine($"❌ {Strings.ErrorIdNotFound}");
            }
        }

        private void DeleteJobView()
        {
            ListJobsView();
            Console.Write($"\n{Strings.PromptDeleteId} ");
            if (int.TryParse(Console.ReadLine(), out int id))
            {
                viewModel.DeleteJob(id);
            }
        }

        private void ListJobsView()
        {
            Console.WriteLine($"--- {Strings.TitleList} ---");
            if (viewModel.backupJobs.Count == 0)
            {
                Console.WriteLine(Strings.ListEmpty);
                return;
            }

            Console.WriteLine("------------------------------------------------");
            Console.WriteLine(Strings.ListHeader);
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
            Console.Write($"\n{Strings.PromptExecuteId} ");

            string input = Console.ReadLine()?.Trim().ToLower();

            if (input == "all")
            {
                Console.WriteLine("\nLancement de tous les travaux...");

                foreach (var job in viewModel.backupJobs)
                {
                    viewModel.ExecuteJob(job.id);
                }
            }
            else if (int.TryParse(input, out int id))
            {
                viewModel.ExecuteJob(id);
            }
            else
            {
                Console.WriteLine("Entrée invalide.");
            }
        }
    }
}