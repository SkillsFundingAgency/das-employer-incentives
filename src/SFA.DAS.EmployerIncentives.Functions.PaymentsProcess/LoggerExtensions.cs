using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess
{
    public static class LoggerExtensions
    {
        public static Task LogInformationAsync(this ILogger logger, string message, params object[] args)
        {
            logger.LogInformation(message, args);
            return Task.CompletedTask;
        }
    }
}
