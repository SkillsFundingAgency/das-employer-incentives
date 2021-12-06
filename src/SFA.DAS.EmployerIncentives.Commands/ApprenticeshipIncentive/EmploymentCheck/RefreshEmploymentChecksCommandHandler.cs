using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Persistence;

namespace SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.EmploymentCheck
{
    public class RefreshEmploymentChecksCommandHandler : ICommandHandler<RefreshEmploymentChecksCommand>
    {
        private readonly IApprenticeshipIncentiveDomainRepository _incentiveDomainRepository;

        public RefreshEmploymentChecksCommandHandler(IApprenticeshipIncentiveDomainRepository incentiveDomainRepository)
        {
            _incentiveDomainRepository = incentiveDomainRepository;
        }
        public async Task Handle(RefreshEmploymentChecksCommand command, CancellationToken cancellationToken = default(CancellationToken))
        {
            var incentivesWithLearning = await _incentiveDomainRepository.FindIncentivesWithLearningFound();

            foreach(var incentive in incentivesWithLearning)
            {
                incentive.AddEmploymentChecks();
                await _incentiveDomainRepository.Save(incentive);
            }
        }
    }

}
