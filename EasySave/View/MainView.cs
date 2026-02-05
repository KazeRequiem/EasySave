using EasySave.Models;
using EasySave.ViewModels;
using EasySave.Resources;
using System;
using System.Linq;

namespace EasySave.Views
{
    /// <summary>
    /// Main view of the EasySave application.
    /// 
    /// This class manages the console-based user interface,
    /// displays interactive menus, and forwards user actions
    /// to the corresponding ViewModel methods.
    /// </summary>
    public class MainView
    {
        private MainViewModel viewModel;

        public MainView(MainViewModel viewModel)
        {
            this.viewModel = viewModel;
        }

        /// <summary>
        /// Displays an interactive menu in the console and allows
        /// the user to navigate using the keyboard.
        /// 
        /// Returns the index of the selected menu option.
        /// </summary>
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

        /// <summary>
        /// Starts the main application loop.
        /// 
        /// Displays the main menu and handles user navigation
        /// until the user chooses to exit the application.
        /// </summary>
        public void Start()
        {
            string[] mainOptions = {
                Strings.MenuOption1,
                Strings.MenuOption2,
                Strings.MenuOption3,
                Strings.MenuOption4,
                Strings.MenuOption5,
                Strings.MenuExit
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

        /// <summary>
        /// Displays the interface for creating a new backup job.
        /// 
        /// Prompts the user for all required parameters and
        /// delegates job creation to the ViewModel.
        /// </summary>
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

        /// <summary>
        /// Displays the interface for modifying an existing backup job.
        /// 
        /// Allows the user to update job properties while keeping
        /// current values if no input is provided.
        /// </summary>
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
                else Console.WriteLine($"{Strings.ErrorIdNotFound}");
            }
        }

        /// <summary>
        /// Displays the interface for deleting a backup job.
        /// 
        /// Prompts the user for the job ID and delegates
        /// the deletion to the ViewModel.
        /// </summary>
        private void DeleteJobView()
        {
            ListJobsView();
            Console.Write($"\n{Strings.PromptDeleteId} ");
            if (int.TryParse(Console.ReadLine(), out int id))
            {
                viewModel.DeleteJob(id);
            }
        }

        /// <summary>
        /// Displays the list of existing backup jobs.
        /// 
        /// Shows job details such as ID, name, type,
        /// source path and destination path.
        /// </summary>
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

        /// <summary>
        /// Displays the interface for executing backup jobs.
        /// 
        /// Allows execution of a single job by ID
        /// or all jobs at once.
        /// </summary>
        private void ExecuteJobView()
        {
            ListJobsView();
            Console.Write($"\n{Strings.PromptExecuteId} ");

            string input = Console.ReadLine()?.Trim().ToLower();

            if (input == "all")
            {
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
