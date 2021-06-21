using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.SetSuccessfulLearnerMatch
{
    public class SetSuccessfulLearnerMatchCommandHandler : ICommandHandler<SetSuccessfulLearnerMatchCommand>
    {
        private readonly IApprenticeshipIncentiveDomainRepository _incentiveDomainRepository;
        private readonly ILearnerDomainRepository _learnerDomainRepository;

        public SetSuccessfulLearnerMatchCommandHandler(
            IApprenticeshipIncentiveDomainRepository incentiveDomainRepository,
            ILearnerDomainRepository learnerDomainRepository)
        {
            _incentiveDomainRepository = incentiveDomainRepository;
            _learnerDomainRepository = learnerDomainRepository;
        }

        public async Task Handle(SetSuccessfulLearnerMatchCommand command, CancellationToken cancellationToken = default)
        {
            var incentive = await _incentiveDomainRepository.Find(command.ApprenticeshipIncentiveId);

            if (incentive == null) return;

            var learner = await _learnerDomainRepository.Get(incentive);
            learner.SetSuccessfulLearnerMatch(command.Succeeded);

            await _learnerDomainRepository.Save(learner);
        }
    }
}
