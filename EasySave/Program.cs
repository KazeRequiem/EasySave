using EasyLog; 

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
    Console.WriteLine("try writing log");
    Logger.Instance.WriteLog(testEntry); 
    Console.WriteLine("Success");
}
catch (Exception ex)
{
    Console.WriteLine($"Error : {ex.Message}");
}

Console.WriteLine("\nPress a key to leave...");
Console.ReadKey();