using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Persistence;

namespace SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.LearnerChangeOfCircumstance
{
    public class LearnerChangeOfCircumstanceCommandHandler : ICommandHandler<LearnerChangeOfCircumstanceCommand>
    {
        private readonly IApprenticeshipIncentiveDomainRepository _domainRepository;
        private readonly ILearnerDomainRepository _learnerDomainRepository;

        public LearnerChangeOfCircumstanceCommandHandler(IApprenticeshipIncentiveDomainRepository domainRepository, ILearnerDomainRepository learnerDomainRepository)
        {
            _domainRepository = domainRepository;
            _learnerDomainRepository = learnerDomainRepository;
        }

        public async Task Handle(LearnerChangeOfCircumstanceCommand command, CancellationToken cancellationToken = default)
        {
            var incentive = await _domainRepository.Find(command.ApprenticeshipIncentiveId);

            if (!incentive.HasPossibleChangeOfCircumstances)
            {
                return;
            }

            var learner = await _learnerDomainRepository.GetByApprenticeshipIncentiveId(incentive.Id);

            if(learner.SubmissionFound && learner.SubmissionData.StartDate.HasValue)
                incentive.SetActualStartDate(learner.SubmissionData.StartDate.Value);

            incentive.SetHasPossibleChangeOfCircumstances(false);

            await _domainRepository.Save(incentive);
        }
    }
}
