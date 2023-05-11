using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmployerIncentives.Commands.Services.SlackApi;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess
{
    public class SendSlackNotification
    {
        private readonly ISlackNotificationService _slackNotificationService;
        private readonly ILogger<SendSlackNotification> _logger;

        public SendSlackNotification(
            ISlackNotificationService slackNotificationService,
            ILogger<SendSlackNotification> logger)
        {
            _slackNotificationService = slackNotificationService;
            _logger = logger;
        }

        [FunctionName(nameof(SendSlackNotification))]
        public async Task Complete([ActivityTrigger] SendSlackNotificationInput input)
        {
            try
            {
                await _slackNotificationService.Send(input.Message);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Unable to notify via Slack");
            }
        }
    }
}
