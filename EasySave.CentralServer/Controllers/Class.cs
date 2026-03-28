using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace EasySave.CentralServer.Controllers
{
    [ApiController]
    [Route("api/logs")]
    public class LogsController : ControllerBase
    {
        private readonly string _logFilePath = "centralized_logs.json";

        [HttpPost]
        public async Task<IActionResult> PostLog([FromBody] JsonElement logEntry)
        {
            try
            {
                // On écrit le log reçu dans un fichier sur le serveur Docker
                string logLine = logEntry.GetRawText() + Environment.NewLine;
                await System.IO.File.AppendAllTextAsync(_logFilePath, logLine);

                // On l'affiche aussi dans la console pour débugger
                Console.WriteLine("Log reçu et stocké !");

                return Ok(new { message = "Log centralisé avec succès !" });
            }
            catch (Exception ex)
            {
                return BadRequest($"Erreur serveur : {ex.Message}");
            }
        }
    }
}