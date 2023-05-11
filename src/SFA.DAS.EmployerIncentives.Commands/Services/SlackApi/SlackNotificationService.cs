using SFA.DAS.EmployerIncentives.Commands.Types.Notification;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.Services.SlackApi
{
    public class SlackNotificationService : ISlackNotificationService
    {
        private readonly HttpClient _client;
        private readonly string _webhookUrl;

        public SlackNotificationService(
            HttpClient client,
            string webhookUrl)
        {
            _client = client;
            _webhookUrl = webhookUrl;
        }

        public async Task Send(SlackMessage slackMessage, CancellationToken cancellationToken = default)
        {
            var response = await _client.PostAsync(
               _webhookUrl,
               new StringContent(slackMessage.Content)
               );

            response.EnsureSuccessStatusCode();
        }
    }
}
