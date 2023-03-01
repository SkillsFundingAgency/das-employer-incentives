using SFA.DAS.EmployerIncentives.Commands.Types.Notification;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess
{
    public class SendSlackNotificationInput
    {
        public SlackMessage Message { get; private set; }

        public SendSlackNotificationInput(SlackMessage message)
        {
            Message = message;
        }
    }
}
