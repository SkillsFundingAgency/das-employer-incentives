using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Commands.Services.LearnerMatchApi;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Domain.Interfaces;

namespace SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.RefreshLearner
{
    public class RefreshLearnerCommandHandler : ICommandHandler<RefreshLearnerCommand>
    {
        private readonly IApprenticeshipIncentiveDomainRepository _incentiveDomainRepository;
        private readonly LearnerService _learnerService;

        public RefreshLearnerCommandHandler(
            IApprenticeshipIncentiveDomainRepository incentiveDomainRepository,
            ILearnerSubmissionService learnerSubmissionService,
            ILearnerDomainRepository learnerDomainRepository,
            ICollectionCalendarService collectionCalendarService)
        {
            _incentiveDomainRepository = incentiveDomainRepository;
            _learnerService = new LearnerService(learnerSubmissionService, learnerDomainRepository, collectionCalendarService);
        }

        public async Task Handle(RefreshLearnerCommand command, CancellationToken cancellationToken = default)
        {
            var incentive = await _incentiveDomainRepository.Find(command.ApprenticeshipIncentiveId);
            var learner = await _learnerService.Refresh(incentive);
            
            incentive.RefreshLearner(learner);

            await _incentiveDomainRepository.Save(incentive);
        }
    }
}
