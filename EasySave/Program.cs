using EasySave.Models;
using EasySave.ViewModels;
using System;
using System.IO;

namespace EasySave
{
    class Program
    {
        static void Main(string[] args)
        {

            //FileInfo infoErreur = new FileInfo(destination);
            //long tailleOctetsErreur = infoErreur.Length;
            // --- 1. AFFICHAGE DU CHEMIN JSON (Pour que tu saches où regarder) ---
            string jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "jobs.json");
            Console.WriteLine($"📂 LE FICHIER JSON EST ICI : \n   👉 {jsonPath}\n");

            // --- 2. PRÉPARATION DE L'ENVIRONNEMENT ---
            string projectPath = AppDomain.CurrentDomain.BaseDirectory;
            string basePath = Path.Combine(projectPath, "EasySave_Tests");
            string sourceDir = Path.Combine(basePath, "Source_Commune");

            // On génère les fichiers s'ils n'existent pas
            if (!Directory.Exists(sourceDir))
            {
                PrepareTestFiles(sourceDir);
            }

            // --- 3. DÉMARRAGE DU VIEWMODEL ---
            var viewModel = new MainViewModel();
            Console.WriteLine($"📊 Jobs actuellement en mémoire : {viewModel.backupJobs.Count}");

            // --- 4. CRÉATION D'UN NOUVEAU JOB UNIQUE ---
            int nextId = viewModel.backupJobs.Count + 1;
            string jobName = $"Job_Auto_{nextId}";
            string destDir = Path.Combine(basePath, $"Backup_{jobName}");

            Console.WriteLine($"\n➕ Ajout du job : {jobName}");
            Console.WriteLine($"   Source : {sourceDir}");
            Console.WriteLine($"   Dest   : {destDir}");

            // ON CRÉE LE JOB (Cela doit écrire dans le JSON)
            viewModel.CreateJob(jobName, sourceDir, destDir, BackupType.Full);

            // --- 5. EXÉCUTION ---
            var newJob = viewModel.backupJobs[viewModel.backupJobs.Count - 1];

            Console.WriteLine($"\n🚀 Exécution du job ID {newJob.id}...");
            viewModel.ExecuteJob(newJob.id);

            // --- 6. RÉSULTAT ---
            Console.WriteLine($"\n✅ TERMINÉ !");
            Console.WriteLine($"   Job ajouté au fichier JSON.");
            Console.WriteLine($"   Tu devrais avoir {viewModel.backupJobs.Count} jobs dans le fichier.");
            Console.WriteLine("   Appuie sur une touche pour quitter.");
            Console.ReadKey();
        }

        static void PrepareTestFiles(string source)
        {
            Console.WriteLine("🛠️  Création des fichiers de test (TXT, PPTX, MP3)...");
            Directory.CreateDirectory(source);
            File.WriteAllText(Path.Combine(source, "Note.txt"), "Ceci est un test.");
            File.WriteAllBytes(Path.Combine(source, "Presentation.pptx"), new byte[100]);
            File.WriteAllBytes(Path.Combine(source, "Musique.mp3"), new byte[100]);

            string subFolder = Path.Combine(source, "Secret");
            Directory.CreateDirectory(subFolder);
            File.WriteAllText(Path.Combine(subFolder, "Mdp.txt"), "1234");
        }
    }
}