using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Queries.EarningsResilienceCheck;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.EarningsResilienceCheck
{
    public class EarningsResilienceApplicationsCheckCommandHandler : ICommandHandler<EarningsResilienceApplicationsCheckCommand>
    {
        private readonly IIncentiveApplicationDomainRepository _applicationDomainRepository;

        public EarningsResilienceApplicationsCheckCommandHandler(IIncentiveApplicationDomainRepository applicationDomainRepository)
        {
            _applicationDomainRepository = applicationDomainRepository;
        }

        public async Task Handle(EarningsResilienceApplicationsCheckCommand command, CancellationToken cancellationToken = default)
        {
            var applications = await _applicationDomainRepository.FindIncentiveApplicationsWithoutEarningsCalculations();
            foreach (var application in applications)
            {
                application.CalculateEarnings();

                await _applicationDomainRepository.Save(application);
            }
        }
    }
}
