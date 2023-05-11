using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Types.Notification;

namespace SFA.DAS.EmployerIncentives.Commands.Types.PaymentProcess
{
    public class SlackNotificationCommand : DomainCommand
    {
        public SlackMessage SlackMessage { get; private set; }

        public SlackNotificationCommand(SlackMessage slackMessage)
        {
            SlackMessage = slackMessage;
        }
    }
}
