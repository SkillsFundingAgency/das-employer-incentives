using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using System.Threading;
using System.Threading.Tasks;

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

            var learner = await _learnerDomainRepository.GetOrCreate(incentive);

            if (learner.SubmissionData.SubmissionFound && learner.SubmissionData.LearningData.StartDate.HasValue)
            {
                incentive.SetStartDate(learner.SubmissionData.LearningData.StartDate.Value);
            }

            incentive.SetHasPossibleChangeOfCircumstances(false);

            await _domainRepository.Save(incentive);
        }
    }
}
