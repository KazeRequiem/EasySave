using EasyLog;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace EasySave.Services
{
    public class DockerLogService : IRemoteLogService
    {
        // static to avoid port exhaustion
        private static readonly HttpClient httpClient = new HttpClient();

        private readonly string dockerApiUrl;
        public DockerLogService(string apiUrl = "http://localhost:5341/api/events/raw")
        {
            this.dockerApiUrl = apiUrl;
        }

        public async Task SendLogAsync(LogEntry logEntry)
        {
            try
            {
                var seqPayload = new
                {
                    Events = new[]
                    {
                        new
                        {
                            Timestamp = DateTime.UtcNow,
                            MessageTemplate = "EasySave [{success_Error}] - Opération {operationName} sur {savetype}",
                            Properties = logEntry
                        }
                    }
                };

                string jsonPayload = JsonSerializer.Serialize(seqPayload);
                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await httpClient.PostAsync(dockerApiUrl, content);

                if (!response.IsSuccessStatusCode)
                {
                    string errorDetails = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[Avertissement] Le conteneur Docker a refusé le log. Code : {response.StatusCode}. Détails : {errorDetails}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Erreur Réseau] Impossible d'envoyer le log à Docker : {ex.Message}");
            }
        }
    }
}