using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Commands.EarningsResilienceCheck;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.EarningsResilienceCheck
{
    public class EarningsResilienceIncentivesCheckCommandHandler : ICommandHandler<EarningsResilienceIncentivesCheckCommand>
    {
        private readonly IApprenticeshipIncentiveDomainRepository _incentiveDomainRepository;

        public EarningsResilienceIncentivesCheckCommandHandler(IApprenticeshipIncentiveDomainRepository incentiveDomainRepository)
        {
            _incentiveDomainRepository = incentiveDomainRepository;
        }

        public async Task Handle(EarningsResilienceIncentivesCheckCommand command, CancellationToken cancellationToken = default)
        {
            var incentives = await _incentiveDomainRepository.FindIncentivesWithoutPendingPayments();
            foreach (var incentive in incentives)
            {
                incentive.CalculatePayments();

                await _incentiveDomainRepository.Save(incentive);
            }
        }
    }
}
