using SFA.DAS.EmployerIncentives.Commands.Types.Notification;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.Services.SlackApi
{
    public interface ISlackNotificationService
    {
        public Task Send(SlackMessage slackMessage, CancellationToken cancellationToken = default);
    }    
}
