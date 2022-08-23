using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Commands.Services.EmploymentCheckApi;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;

namespace SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.SendEmploymentCheckRequests
{
    public class SendEmploymentCheckRequestsCommandHandler : ICommandHandler<SendEmploymentCheckRequestsCommand>
    {
        private readonly IApprenticeshipIncentiveDomainRepository _domainRepository;
        private readonly IEmploymentCheckService _employmentCheckService;
        
        public SendEmploymentCheckRequestsCommandHandler(IApprenticeshipIncentiveDomainRepository domainRepository, 
                                                         IEmploymentCheckService employmentCheckService)
        {
            _domainRepository = domainRepository;
            _employmentCheckService = employmentCheckService;
        }

        public async Task Handle(SendEmploymentCheckRequestsCommand command, CancellationToken cancellationToken = default)
        {
            var apprenticeshipIncentive = await _domainRepository.Find(command.ApprenticeshipIncentiveId);
            if (apprenticeshipIncentive == null)
            {
                return;
            }

            foreach (var employmentCheck in apprenticeshipIncentive.EmploymentChecks.Where(ec => ec.CheckType == command.CheckType))
            {
                var correlationId = await _employmentCheckService.RegisterEmploymentCheck(employmentCheck, apprenticeshipIncentive);
                employmentCheck.SetCorrelationId(correlationId);
            }

            await _domainRepository.Save(apprenticeshipIncentive);
        }
    }
}
