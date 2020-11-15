using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Commands.Services.LearnerMatchApi;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using SFA.DAS.EmployerIncentives.Domain.Factories;

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
                submissionData = new SubmissionData(learnerData.IlrSubmissionDate, learnerData.Training.Any(t => t.Reference == "ZPROG001"));
                submissionData.SetStartDate(learnerData.LearningStartDateForApprenticeship(learner.ApprenticeshipId));

                var firstPendingPayment = incentive.PendingPayments.Where(p => p.PaymentMadeDate == null).OrderBy(p => p.DueDate).FirstOrDefault();
                submissionData.SetIsInLearning(learnerData.InLearningForApprenticeship(learner.ApprenticeshipId, firstPendingPayment));

                submissionData.SetRawJson(learnerData.RawJson);
            }

            learner.SetSubmissionData(submissionData);

            await _learnerDomainRepository.Save(learner);
        }
    }
}
