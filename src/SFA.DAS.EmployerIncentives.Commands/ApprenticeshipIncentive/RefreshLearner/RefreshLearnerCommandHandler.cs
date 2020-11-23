using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Commands.Services.LearnerMatchApi;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using SFA.DAS.EmployerIncentives.Domain.Factories;
using System;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives;

namespace SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.RefreshLearner
{
    public class RefreshLearnerCommandHandler : ICommandHandler<RefreshLearnerCommand>
    {
        private readonly IApprenticeshipIncentiveDomainRepository _incentiveDomainRepository;
        private readonly ILearnerService _learnerService;
        private readonly ILearnerDomainRepository _learnerDomainRepository;
        private readonly ILearnerFactory _learnerFactory;

        public RefreshLearnerCommandHandler(
            IApprenticeshipIncentiveDomainRepository incentiveDomainRepository,
            ILearnerService learnerService,
            ILearnerDomainRepository learnerDomainRepository,
            ILearnerFactory learnerFactory)
        {
            _incentiveDomainRepository = incentiveDomainRepository;
            _learnerService = learnerService;
            _learnerDomainRepository = learnerDomainRepository;
            _learnerFactory = learnerFactory;
        }

        public async Task Handle(RefreshLearnerCommand command, CancellationToken cancellationToken = default)
        {
            var incentive = await _incentiveDomainRepository.Find(command.ApprenticeshipIncentiveId);

            var learner = await _learnerDomainRepository.GetByApprenticeshipIncentiveId(incentive.Id);

            if (learner == null)
            {
                learner = _learnerFactory.CreateNew(
                   Guid.NewGuid(),
                    incentive.Id,
                    incentive.Apprenticeship.Id,
                    incentive.Apprenticeship.Provider.Ukprn,
                    incentive.Apprenticeship.UniqueLearnerNumber,
                    DateTime.UtcNow);
            }

            SubmissionData submissionData = null;
            var learnerData = await _learnerService.Get(learner);
            if(learnerData != null)
            {
                if (LearnerAndEarningsHaveNotChanged(learnerData, learner, incentive))
                {
                    return;
                }

                submissionData = new SubmissionData(learnerData.IlrSubmissionDate);
                submissionData.SetStartDate(learnerData.LearningStartDate(incentive));
                submissionData.SetLearningFound(learnerData.LearningFound(incentive));
                submissionData.SetHasDataLock(learnerData.HasProviderDataLocks(incentive));                
                submissionData.SetIsInLearning(learnerData.IsInLearning(incentive));
                submissionData.SetRawJson(learnerData.RawJson);
            }

            learner.SetSubmissionData(submissionData);
            incentive.LearnerRefreshCompleted();

            await _learnerDomainRepository.Save(learner);
            await _incentiveDomainRepository.Save(incentive);
        }

        private bool LearnerAndEarningsHaveNotChanged(LearnerSubmissionDto learnerData, Learner learner, Domain.ApprenticeshipIncentives.ApprenticeshipIncentive incentive)
        {
            return learnerData.IlrSubmissionDate == learner.SubmissionData?.SubmissionDate &&
                   incentive.RefreshedLearnerForEarnings;
        }
    }
}
