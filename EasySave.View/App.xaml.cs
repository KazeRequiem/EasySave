using System;
using System.Diagnostics;
using System.Windows;

namespace EasySave.View
{
    public partial class App : Application
    {
        private const string ContainerName = "easysave-seq";

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            ManageDockerContainer("start");
        }

        private void ManageDockerContainer(string action)
        {
            try
            {
                ProcessStartInfo procInfo = new ProcessStartInfo
                {
                    FileName = "docker",
                    Arguments = $"{action} {ContainerName}",
                    CreateNoWindow = true,
                    UseShellExecute = false
                };

                var process = Process.Start(procInfo);
                process?.WaitForExit();

                if (action == "start" && process?.ExitCode != 0)
                {
                    CreateAndRunContainer();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error Docker ({action}) : {ex.Message}");
            }
        }

        private void CreateAndRunContainer()
        {
            string runArgs = $"run -d --name {ContainerName} -p 5341:80 -v seq-data:/data -e \"ACCEPT_EULA=Y\" -e \"SEQ_FIRSTRUN_ADMINPASSWORD=admin\" datalust/seq:latest";

            ProcessStartInfo runInfo = new ProcessStartInfo
            {
                FileName = "docker",
                Arguments = runArgs,
                CreateNoWindow = true,
                UseShellExecute = false
            };
            Process.Start(runInfo)?.WaitForExit();
        }
    }
}