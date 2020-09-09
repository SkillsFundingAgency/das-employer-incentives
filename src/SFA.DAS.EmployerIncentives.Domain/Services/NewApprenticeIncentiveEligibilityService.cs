using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Data;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;
using SFA.DAS.EmployerIncentives.ValueObjects;

namespace SFA.DAS.EmployerIncentives.Domain.Services
{
    public class NewApprenticeIncentiveEligibilityService : INewApprenticeIncentiveEligibilityService
    {
        private readonly IUlnValidationService _ulnValidationService;

        public NewApprenticeIncentiveEligibilityService(IUlnValidationService ulnValidationService)
        {
            _ulnValidationService = ulnValidationService;
        }
        public async Task<bool> IsApprenticeshipEligible(Apprenticeship apprenticeship)
        {
            if (!Incentive.IsStartDateEligible(apprenticeship.StartDate))
            {
                return false;
            }

            if (await _ulnValidationService.UlnAlreadyOnSubmittedIncentiveApplication(apprenticeship.UniqueLearnerNumber))
            {
                return false;
            }

            return true;
        }
    }
}
