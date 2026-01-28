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
                AddrSource = "C:\\SourceTest",
                AddrDestination = "D:\\DestTest",
                SizeFile = 500,
                TimeTransfer = 10
            };

            Logger.WriteLog(testLog);
            Console.WriteLine("Log de test écrit avec succès !");
        }
    }
}