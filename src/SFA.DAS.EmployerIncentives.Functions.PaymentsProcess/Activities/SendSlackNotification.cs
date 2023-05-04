using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using SFA.DAS.EmployerIncentives.Commands.Services.SlackApi;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess
{
    public class SendSlackNotification
    {
        private readonly ISlackNotificationService _slackNotificationService;

        public SendSlackNotification(ISlackNotificationService slackNotificationService)
        {
            _slackNotificationService = slackNotificationService;
        }

        [FunctionName(nameof(SendSlackNotification))]
        public async Task Complete([ActivityTrigger] SendSlackNotificationInput input)
        {
            try
            {
                await _slackNotificationService.Send(input.Message);
            }
            catch { }
        }
    }
}
