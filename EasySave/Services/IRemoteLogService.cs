using EasyLog;
using EasySave.Models;
using System.Threading.Tasks;

namespace EasySave.Services
{
    public interface IRemoteLogService
    {
        Task SendLogAsync(LogEntry logEntry);
    }
}