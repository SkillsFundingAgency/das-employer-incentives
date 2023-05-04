using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Services.SlackApi;
using SFA.DAS.EmployerIncentives.Commands.Types.PaymentProcess;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.SendSlack
{
    public class SlackNotificationCommandHandler : ICommandHandler<SlackNotificationCommand>
    {
        private readonly ISlackNotificationService _slackNotificationService;

        public SlackNotificationCommandHandler(ISlackNotificationService slackNotificationService)
        {
            _slackNotificationService = slackNotificationService;
        }

        public Task Handle(SlackNotificationCommand command, CancellationToken cancellationToken = default)
        {   
            return _slackNotificationService.Send(command.SlackMessage);
        }
    }
}
