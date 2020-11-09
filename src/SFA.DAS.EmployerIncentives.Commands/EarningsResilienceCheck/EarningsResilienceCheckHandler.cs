using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Exceptions;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Queries.EarningsResilienceCheck;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.EarningsResilienceCheck
{
    public class EarningsResilienceCheckHandler : ICommandHandler<EarningsResilienceCheckCommand>
    {
        private readonly IIncentiveApplicationDomainRepository _applicationDomainRepository;
        private readonly IApprenticeshipIncentiveDomainRepository _incentiveDomainRepository;

        public EarningsResilienceCheckHandler(IIncentiveApplicationDomainRepository applicationDomainRepository,
                                              IApprenticeshipIncentiveDomainRepository incentiveDomainRepository)
        {
            _applicationDomainRepository = applicationDomainRepository;
            _incentiveDomainRepository = incentiveDomainRepository;
        }

        public async Task Handle(EarningsResilienceCheckCommand command, CancellationToken cancellationToken = default)
        {
            await CalculateEarningsForApplications();
            await CalculatePaymentsForIncentives();
        }

        private async Task CalculateEarningsForApplications()
        {
            var applications = await _applicationDomainRepository.FindIncentiveApplicationsWithoutEarningsCalculations();
            foreach(var application in applications)
            {
                application.CalculateEarnings();

                await _applicationDomainRepository.Save(application);
            }
        }

        private async Task CalculatePaymentsForIncentives()
        {
            var incentives = await _incentiveDomainRepository.FindIncentivesWithoutPayments();
            foreach(var incentive in incentives)
            {
                incentive.CalculatePayments();

                await _incentiveDomainRepository.Save(incentive);
            }
        }
    }
}
