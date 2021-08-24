using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Persistence;

namespace SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.SetSuccessfulLearnerMatchExecution
{
    public class SetSuccessfulLearnerMatchExecutionCommandHandler : ICommandHandler<SetSuccessfulLearnerMatchExecutionCommand>
    {
        private readonly IApprenticeshipIncentiveDomainRepository _incentiveDomainRepository;
        private readonly ILearnerDomainRepository _learnerDomainRepository;

        public SetSuccessfulLearnerMatchExecutionCommandHandler(
            IApprenticeshipIncentiveDomainRepository incentiveDomainRepository,
            ILearnerDomainRepository learnerDomainRepository)
        {
            _incentiveDomainRepository = incentiveDomainRepository;
            _learnerDomainRepository = learnerDomainRepository;
        }

        public async Task Handle(SetSuccessfulLearnerMatchExecutionCommand executionCommand, CancellationToken cancellationToken = default)
        {
            var incentive = await _incentiveDomainRepository.Find(executionCommand.ApprenticeshipIncentiveId);

            if (incentive == null) return;

            var learner = await _learnerDomainRepository.GetOrCreate(incentive);

            learner.SetSuccessfulLearnerMatchExecution(executionCommand.Succeeded);

            await _learnerDomainRepository.Save(learner);
        }
    }
}
