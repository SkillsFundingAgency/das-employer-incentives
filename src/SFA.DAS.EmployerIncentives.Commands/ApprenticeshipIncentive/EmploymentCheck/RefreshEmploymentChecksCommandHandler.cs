using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;
using SFA.DAS.EmployerIncentives.Domain.Interfaces;

namespace SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.EmploymentCheck
{
    public class RefreshEmploymentChecksCommandHandler : ICommandHandler<RefreshEmploymentChecksCommand>
    {
        private readonly IApprenticeshipIncentiveDomainRepository _incentiveDomainRepository;
        private readonly ICommandPublisher _commandPublisher;

        public RefreshEmploymentChecksCommandHandler(IApprenticeshipIncentiveDomainRepository incentiveDomainRepository, 
                                                     ICommandPublisher commandPublisher)
        {
            _incentiveDomainRepository = incentiveDomainRepository;
            _commandPublisher = commandPublisher;
        }
        public async Task Handle(RefreshEmploymentChecksCommand command, CancellationToken cancellationToken = default(CancellationToken))
        {
            var incentivesWithLearning = await _incentiveDomainRepository.FindIncentivesWithLearningFound();

            foreach(var incentive in incentivesWithLearning)
            {
                if (!incentive.HasEmploymentChecks)
                {
                    incentive.AddEmploymentChecks();
                    await _incentiveDomainRepository.Save(incentive);
                }

                await _commandPublisher.Publish(new SendEmploymentCheckRequestsCommand(incentive.Id));
            }
        }
    }

}
