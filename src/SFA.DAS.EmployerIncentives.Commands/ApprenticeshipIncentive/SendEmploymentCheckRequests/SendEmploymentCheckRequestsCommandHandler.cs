using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Commands.Services.EmploymentCheckApi;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;
using SFA.DAS.EmployerIncentives.Infrastructure.Configuration;

namespace SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.SendEmploymentCheckRequests
{
    public class SendEmploymentCheckRequestsCommandHandler : ICommandHandler<SendEmploymentCheckRequestsCommand>
    {
        private readonly IApprenticeshipIncentiveDomainRepository _domainRepository;
        private readonly IEmploymentCheckService _employmentCheckService;
        private readonly ApplicationSettings _applicationSettings;

        public SendEmploymentCheckRequestsCommandHandler(IApprenticeshipIncentiveDomainRepository domainRepository, 
                                                         IEmploymentCheckService employmentCheckService,
                                                         IOptions<ApplicationSettings> applicationSettings)
        {
            _domainRepository = domainRepository;
            _employmentCheckService = employmentCheckService;
            _applicationSettings = applicationSettings.Value;
        }

        public async Task Handle(SendEmploymentCheckRequestsCommand command, CancellationToken cancellationToken = default)
        {
            if (!_applicationSettings.EmploymentCheckEnabled)
            {
                return;
            }

            var apprenticeshipIncentive = await _domainRepository.Find(command.ApprenticeshipIncentiveId);
            if (apprenticeshipIncentive == null)
            {
                return;
            }

            foreach (var employmentCheck in apprenticeshipIncentive.EmploymentChecks)
            {
                var correlationId = await _employmentCheckService.RegisterEmploymentCheck(employmentCheck, apprenticeshipIncentive);
                employmentCheck.SetCorrelationId(correlationId);
            }

            await _domainRepository.Save(apprenticeshipIncentive);
        }
    }
}
