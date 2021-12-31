using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;
using SFA.DAS.EmployerIncentives.Infrastructure.Configuration;

namespace SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.SendEmploymentCheckRequests
{
    public class SendEmploymentCheckRequestsCommandHandlerWithEmploymentCheckToggle : ICommandHandler<SendEmploymentCheckRequestsCommand>
    {
        private readonly ICommandHandler<SendEmploymentCheckRequestsCommand> _handler;
        private readonly ApplicationSettings _applicationSettings;

        public SendEmploymentCheckRequestsCommandHandlerWithEmploymentCheckToggle(
            ICommandHandler<SendEmploymentCheckRequestsCommand> handler,
            IOptions<ApplicationSettings> applicationSettings)
        {
            _handler = handler;
            _applicationSettings = applicationSettings.Value;
        }

        public async Task Handle(SendEmploymentCheckRequestsCommand command, CancellationToken cancellationToken = default)
        {
            if (!_applicationSettings.EmploymentCheckEnabled)
            {
                return;
            }

            await _handler.Handle(command, cancellationToken);
        }
    }
}
