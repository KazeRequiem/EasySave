using EasyLog; // Tout en haut

// Ton code de test
Console.WriteLine("==========================================");
Console.WriteLine("       BIENVENUE DANS EASY SAVE          ");
Console.WriteLine("==========================================");

LogEntry testEntry = new LogEntry
{
    NameSave = "Sauvegarde_Documents",
    SourcePath = @"C:\Users\Quentin\Documents",
    DestinationPath = @"D:\Backup\Documents",
    SizeFile = 102450,
    TimeTransfer = 120
};

try
{
    Console.WriteLine("Tentative d'écriture du log via le Singleton...");
    Logger.Instance.WriteLog(testEntry); // On utilise le Singleton
    Console.WriteLine("Succès ! Le fichier log a été mis à jour.");
}
catch (Exception ex)
{
    Console.WriteLine($"Erreur lors du test : {ex.Message}");
}

Console.WriteLine("\nAppuie sur une touche pour quitter...");
Console.ReadKey();