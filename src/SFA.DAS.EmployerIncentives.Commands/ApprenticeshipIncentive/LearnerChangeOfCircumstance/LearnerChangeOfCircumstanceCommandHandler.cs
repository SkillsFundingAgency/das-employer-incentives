using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Persistence;

namespace SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.LearnerChangeOfCircumstance
{
    public class LearnerChangeOfCircumstanceCommandHandler : ICommandHandler<LearnerChangeOfCircumstanceCommand>
    {
        private readonly IApprenticeshipIncentiveDomainRepository _domainRepository;
        private readonly ILearnerDomainRepository _learnerDomainRepository;
        private readonly ILogger<LearnerChangeOfCircumstanceCommandHandler> _logger;

        public LearnerChangeOfCircumstanceCommandHandler(IApprenticeshipIncentiveDomainRepository domainRepository, ILearnerDomainRepository learnerDomainRepository, ILogger<LearnerChangeOfCircumstanceCommandHandler> logger)
        {
            _domainRepository = domainRepository;
            _learnerDomainRepository = learnerDomainRepository;
            _logger = logger;
        }

        public async Task Handle(LearnerChangeOfCircumstanceCommand command, CancellationToken cancellationToken = default)
        {
            var incentive = await _domainRepository.Find(command.ApprenticeshipIncentiveId);

            _logger.LogInformation($"HasPossibleChangeOfCircumstances = {incentive.HasPossibleChangeOfCircumstances}");

            if (!incentive.HasPossibleChangeOfCircumstances)
            {
                return;
            }

            var learner = await _learnerDomainRepository.GetByApprenticeshipIncentiveId(incentive.Id);

            _logger.LogInformation($"Submission found = {learner.SubmissionFound}");
            _logger.LogInformation($"Start date = {learner.SubmissionData.StartDate}");

            if (learner.SubmissionFound && learner.SubmissionData.StartDate.HasValue)
                incentive.SetActualStartDate(learner.SubmissionData.StartDate.Value);

            incentive.SetHasPossibleChangeOfCircumstances(false);

            await _domainRepository.Save(incentive);
        }
    }
}
