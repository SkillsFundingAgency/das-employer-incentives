using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Commands.Services.LearnerMatchApi;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.RefreshLearner
{
    public class RefreshLearnerCommandHandler : ICommandHandler<RefreshLearnerCommand>
    {
        private readonly IApprenticeshipIncentiveDomainRepository _domainRepository;
        private readonly ILearnerService _learnerService;
        private readonly ILearnerDataRepository _learnerDataRepository;

        public RefreshLearnerCommandHandler(
            IApprenticeshipIncentiveDomainRepository domainRepository,
            ILearnerService learnerService,
            ILearnerDataRepository learnerDataRepository)
        {
            _domainRepository = domainRepository;
            _learnerService = learnerService;
            _learnerDataRepository = learnerDataRepository;
        }

        public async Task Handle(RefreshLearnerCommand command, CancellationToken cancellationToken = default)
        {
            var incentive = await _domainRepository.Find(command.ApprenticeshipIncentiveId);

            var learner = await _learnerDataRepository.Get(incentive);

            await _learnerService.Refresh(learner);

            await _learnerDataRepository.Save(learner);
        }
    }
}
