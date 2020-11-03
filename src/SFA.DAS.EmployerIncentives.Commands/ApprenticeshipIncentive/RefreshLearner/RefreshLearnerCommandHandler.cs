using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Commands.Services.LearnerMatchApi;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.ValidatePendingPayment
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

            var learner = await _learnerDataRepository.GetByApprenticeshipIncentiveId(incentive.Id);

            if (learner == null)
            {
                learner = new Domain.ApprenticeshipIncentives.ValueTypes.Learner(
                    Guid.NewGuid(), 
                    incentive.Id, 
                    incentive.Apprenticeship.Id, 
                    incentive.Apprenticeship.Provider.Ukprn, 
                    incentive.Apprenticeship.UniqueLearnerNumber, 
                    DateTime.UtcNow
                    );
            }

            await _learnerService.Refresh(learner);

            await _learnerDataRepository.Save(learner);
        }
    }
}
