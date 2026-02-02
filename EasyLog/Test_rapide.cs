using System;

namespace EasyLog
{
    public static class TestRapide
    {
        public static void ExecuterTest()
        {
            var testLog = new LogEntry
            {
                NameSave = "Test_Debug",
                SourcePath = "C:\\SourceTest",
                DestinationPath = "D:\\DestTest",
                SizeFile = 500,
                TimeTransfer = 10,
                OperationName = "Modifier nom ",
                Status = "Fin"
            };

            Logger.Instance.WriteLog(testLog);
            Console.WriteLine("Log de test écrit avec succès !");
        }
    }
}