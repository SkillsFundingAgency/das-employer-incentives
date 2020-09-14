using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmployerIncentives.Data;
using SFA.DAS.EmployerIncentives.ValueObjects;

namespace SFA.DAS.EmployerIncentives.Domain.Services
{
    public class NewApprenticeIncentiveEligibilityService : INewApprenticeIncentiveEligibilityService
    {
        private readonly IUlnValidationService _ulnValidationService;
        private readonly ILogger<NewApprenticeIncentiveEligibilityService> _logger;

        public NewApprenticeIncentiveEligibilityService(IUlnValidationService ulnValidationService, 
                                                        ILogger<NewApprenticeIncentiveEligibilityService> logger)
        {
            _ulnValidationService = ulnValidationService;
            _logger = logger;
        }
        public async Task<bool> IsApprenticeshipEligible(Apprenticeship apprenticeship)
        {
            var incentive = new NewApprenticeIncentive();
            if (!incentive.IsApprenticeshipEligible(apprenticeship))
            {
                _logger.LogInformation($"Uln {apprenticeship.UniqueLearnerNumber} failed eligibility check: start date {apprenticeship.StartDate} approved {apprenticeship.IsApproved}");
                return false;
            }

            if (await _ulnValidationService.UlnAlreadyOnSubmittedIncentiveApplication(apprenticeship.UniqueLearnerNumber))
            {
                _logger.LogInformation($"Uln {apprenticeship.UniqueLearnerNumber} already on submitted incentive application");
                return false;
            }

            return true;
        }
    }
}
